using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Farm.Core;

namespace Farm.Gameplay
{
    // A timing instruction inside a line: before the character at `Index` (visible characters, markup not counted) is
    // shown, wait `Pause` seconds and/or switch the typing speed to `Speed` (a multiplier; 0 = unchanged).
    public readonly struct TextCue
    {
        public readonly int Index;
        public readonly float Pause;
        public readonly float Speed;
        public TextCue(int index, float pause, float speed) { Index = index; Pause = pause; Speed = speed; }
    }

    public sealed class RichLine
    {
        public string Text = string.Empty;                 // ready for the text box (keeps TextMeshPro tags such as <b> and <i>)
        public readonly List<TextCue> Cues = new List<TextCue>();
    }

    // The inline markup writers use inside a line (T-094):
    //   {pause=0.4}                 wait 0.4 seconds here (0 to 5)
    //   {speed=slow}                typing speed from here on: slow (0.5x), normal, fast (2x), or a number from 0.1 to 4
    //   {Hello|Hi|Hey}              one of the options, chosen deterministically (the same one all day, a different one on other days)
    //   {if flag:x}...{else}...{/if}  text that depends on a condition (the else part is optional; ifs may nest)
    //   <b>bold</b> <i>italic</i>   passed through to the text box
    // Names such as [player] and [season] are replaced before this runs (StoryTokens). Pure and deterministic.
    public static class RichText
    {
        public static RichLine Process(string raw, IWorldQuery world, int seed)
        {
            var line = new RichLine();
            if (string.IsNullOrEmpty(raw)) return line;
            var sb = new StringBuilder(raw.Length);
            var stack = new List<(bool parentEmits, bool condition, bool inElse)>();
            var visible = 0;
            var inTag = false;

            bool Emitting()
            {
                if (stack.Count == 0) return true;
                var top = stack[stack.Count - 1];
                return top.parentEmits && (top.inElse ? !top.condition : top.condition);
            }

            for (var i = 0; i < raw.Length; i++)
            {
                var c = raw[i];
                if (c == '{')
                {
                    var close = raw.IndexOf('}', i + 1);
                    if (close > i)
                    {
                        var inner = raw.Substring(i + 1, close - i - 1);
                        if (Handle(inner, world, seed, i, sb, line, stack, ref visible, Emitting)) { i = close; continue; }
                    }
                }
                if (!Emitting()) continue;
                sb.Append(c);
                if (c == '<') inTag = true;
                if (!inTag && c != '\n' && c != '\r') visible++;
                if (c == '>') inTag = false;
            }
            line.Text = sb.ToString();
            return line;
        }

        static bool Handle(string inner, IWorldQuery world, int seed, int position, StringBuilder sb, RichLine line,
            List<(bool parentEmits, bool condition, bool inElse)> stack, ref int visible, Func<bool> emitting)
        {
            if (inner.StartsWith("if ", StringComparison.Ordinal))
            {
                var parent = emitting();
                var ok = false;
                if (parent && world != null) Conditions.TryEvaluate(inner.Substring(3).Trim(), world, out ok);
                stack.Add((parent, ok, false));
                return true;
            }
            if (inner == "else")
            {
                if (stack.Count == 0) return false;
                var top = stack[stack.Count - 1];
                stack[stack.Count - 1] = (top.parentEmits, top.condition, true);
                return true;
            }
            if (inner == "/if")
            {
                if (stack.Count == 0) return false;
                stack.RemoveAt(stack.Count - 1);
                return true;
            }
            if (inner.StartsWith("pause=", StringComparison.Ordinal))
            {
                if (!TryPause(inner.Substring(6), out var seconds)) return false;
                if (emitting()) line.Cues.Add(new TextCue(visible, seconds, 0f));
                return true;
            }
            if (inner.StartsWith("speed=", StringComparison.Ordinal))
            {
                if (!TrySpeed(inner.Substring(6), out var speed)) return false;
                if (emitting()) line.Cues.Add(new TextCue(visible, 0f, speed));
                return true;
            }
            if (inner.IndexOf('|') >= 0)
            {
                var options = inner.Split('|');
                if (emitting())
                {
                    var chosen = options[(int)(DialogueSet.Unit(seed + position * 17) * options.Length) % options.Length];
                    sb.Append(chosen);
                    visible += CountVisible(chosen);
                }
                return true;
            }
            return false;
        }

        static int CountVisible(string text)
        {
            var n = 0; var inTag = false;
            foreach (var c in text)
            {
                if (c == '<') inTag = true;
                if (!inTag && c != '\n' && c != '\r') n++;
                if (c == '>') inTag = false;
            }
            return n;
        }

        public static bool TryPause(string text, out float seconds) =>
            float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds) && seconds >= 0f && seconds <= 5f;

        public static bool TrySpeed(string text, out float speed)
        {
            switch (text)
            {
                case "slow": speed = 0.5f; return true;
                case "normal": speed = 1f; return true;
                case "fast": speed = 2f; return true;
            }
            return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out speed) && speed >= 0.1f && speed <= 4f;
        }

        // Writers' mistakes in a raw line, or an empty list when it is well formed.
        public static List<string> Validate(string raw)
        {
            var problems = new List<string>();
            if (string.IsNullOrEmpty(raw)) return problems;
            var depth = 0;
            var elseSeen = new List<bool>();
            for (var i = 0; i < raw.Length; i++)
            {
                if (raw[i] == '}') { problems.Add("a '}' with no '{'"); continue; }
                if (raw[i] != '{') continue;
                var close = raw.IndexOf('}', i + 1);
                if (close < 0) { problems.Add("a '{' is never closed"); break; }
                var inner = raw.Substring(i + 1, close - i - 1);
                i = close;
                if (inner.StartsWith("if ", StringComparison.Ordinal))
                {
                    depth++; elseSeen.Add(false);
                    if (!Conditions.Validate(inner.Substring(3).Trim(), out var error)) problems.Add($"{{{inner}}}: {error}");
                }
                else if (inner == "else")
                {
                    if (depth == 0) problems.Add("{else} outside an {if}");
                    else if (elseSeen[depth - 1]) problems.Add("two {else} in one {if}");
                    else elseSeen[depth - 1] = true;
                }
                else if (inner == "/if")
                {
                    if (depth == 0) problems.Add("{/if} with no {if}");
                    else { depth--; elseSeen.RemoveAt(elseSeen.Count - 1); }
                }
                else if (inner.StartsWith("pause=", StringComparison.Ordinal))
                {
                    if (!TryPause(inner.Substring(6), out _)) problems.Add($"{{{inner}}}: the pause must be 0 to 5 seconds");
                }
                else if (inner.StartsWith("speed=", StringComparison.Ordinal))
                {
                    if (!TrySpeed(inner.Substring(6), out _)) problems.Add($"{{{inner}}}: the speed must be slow, normal, fast or 0.1 to 4");
                }
                else if (inner.IndexOf('|') >= 0)
                {
                    foreach (var option in inner.Split('|'))
                        if (option.Length == 0) { problems.Add($"{{{inner}}}: an empty option"); break; }
                }
                else if (inner.Length > 0 && inner.All(char.IsDigit)) { /* a string.Format placeholder such as {0} */ }
                else problems.Add($"{{{inner}}}: not a known markup");
            }
            if (depth > 0) problems.Add("an {if} is never closed with {/if}");
            return problems;
        }

        // The words a reader sees, for length budgets: markup removed, the first option of a variation and the "then" part
        // of an {if} kept.
        public static string Plain(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            var sb = new StringBuilder(raw.Length);
            var inElseDepth = new List<bool>();
            for (var i = 0; i < raw.Length; i++)
            {
                if (raw[i] == '{')
                {
                    var close = raw.IndexOf('}', i + 1);
                    if (close > i)
                    {
                        var inner = raw.Substring(i + 1, close - i - 1);
                        if (inner.StartsWith("if ", StringComparison.Ordinal)) { inElseDepth.Add(false); i = close; continue; }
                        if (inner == "else" && inElseDepth.Count > 0) { inElseDepth[inElseDepth.Count - 1] = true; i = close; continue; }
                        if (inner == "/if" && inElseDepth.Count > 0) { inElseDepth.RemoveAt(inElseDepth.Count - 1); i = close; continue; }
                        if (inner.StartsWith("pause=", StringComparison.Ordinal) || inner.StartsWith("speed=", StringComparison.Ordinal)) { i = close; continue; }
                        if (inner.IndexOf('|') >= 0)
                        {
                            if (!inElseDepth.Contains(true)) sb.Append(inner.Split('|')[0]);
                            i = close; continue;
                        }
                    }
                }
                if (inElseDepth.Contains(true)) continue;
                sb.Append(raw[i]);
            }
            return System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "<[^>]+>", string.Empty);
        }
    }
}
