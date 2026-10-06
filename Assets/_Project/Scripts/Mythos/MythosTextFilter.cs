using System.Text;
using Farm.Core;
using Farm.Gameplay;

namespace Farm.Mythos
{
    // Text distortion at high dread (X-008): at full intensity, once dread passes a threshold, a few words in what villagers say, in events
    // and in letters stumble or scramble. It never blocks reading: names, numbers, {placeholders}, [tokens], clue lines and the player's own
    // choices are left alone, and the same line shows the same way all day. Mild and off never distort.
    public static class MythosTextFilter
    {
        public const int Threshold = 50;                // dread (0..100) where it starts
        public const float MaxShare = 0.2f;             // share of eligible words affected at dread 100
        const int MinWordLength = 4;

        static GameSession _session;
        static readonly System.Func<string, string, string> Filter = Apply;

        public static void Install(GameSession session)
        {
            _session = session;
            L.AddFilter(Filter);
        }

        public static void UninstallForTests()
        {
            _session = null;
            L.RemoveFilter(Filter);
        }

        static string Apply(string key, string text)
        {
            var s = _session;
            if (s == null || !s.InGame || !MythosLevel.Full(s)) return text;
            var dread = s.GetVar(MythosIds.Vars.Dread);
            if (dread < Threshold || !Eligible(key)) return text;
            return Distort(key, text, dread, s.Clock.Now.TotalDays);
        }

        // Story speech only; never interface text, clue lines, the layer's own strings or the player's choices.
        public static bool Eligible(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            if (!(key.StartsWith("dlg.") || key.StartsWith("event.") || key.StartsWith("letter."))) return false;
            if (key.Contains(".clue") || key.StartsWith("dlg.mythos")) return false;
            var dot = key.LastIndexOf('.');
            return !IsChoiceTail(key, dot + 1);
        }

        static bool IsChoiceTail(string key, int start)
        {
            if (start >= key.Length || key[start] != 'c' || start + 1 >= key.Length) return false;
            for (var i = start + 1; i < key.Length; i++) if (!char.IsDigit(key[i])) return false;
            return true;
        }

        // Pure and deterministic: the same (key, text, dread, day) always gives the same result.
        public static string Distort(string key, string text, int dread, int day)
        {
            if (string.IsNullOrEmpty(text) || dread < Threshold) return text;
            var share = MaxShare * (System.Math.Min(100, dread) - Threshold) / (100f - Threshold);
            var sb = new StringBuilder(text.Length + 8);
            var word = 0;
            for (var i = 0; i < text.Length;)
            {
                var c = text[i];
                var close = c == '{' ? '}' : c == '[' ? ']' : c == '<' ? '>' : '\0';
                if (close != '\0')
                {
                    var end = text.IndexOf(close, i + 1);
                    if (end > i) { sb.Append(text, i, end - i + 1); i = end + 1; continue; }      // a token, a group or a tag: copied as it is
                }
                if (!char.IsLetter(c)) { sb.Append(c); i++; continue; }
                var j = i;
                while (j < text.Length && char.IsLetter(text[j])) j++;
                var w = text.Substring(i, j - i);
                var h = Hash(key, day, word++);
                var capital = char.IsUpper(w[0]);
                if (w.Length >= MinWordLength && !capital && (h % 1000) / 1000f < share) sb.Append(Scramble(w, h / 1000));
                else sb.Append(w);
                i = j;
            }
            return sb.ToString();
        }

        static string Scramble(string w, uint h)
        {
            if (h % 2 == 0)
            {
                var first = w[0];
                return first + "-" + first + "-" + w;                      // a stammer
            }
            var chars = w.ToCharArray();
            var at = 1 + (int)(h / 2 % (uint)(chars.Length - 2));         // swap two inner letters, the first and last stay
            if (at + 1 < chars.Length)
            {
                var t = chars[at]; chars[at] = chars[at + 1]; chars[at + 1] = t;
            }
            return new string(chars);
        }

        static uint Hash(string key, int day, int word)
        {
            unchecked
            {
                var h = 2166136261u;
                foreach (var ch in key) h = (h ^ ch) * 16777619u;
                h = (h ^ (uint)day) * 16777619u;
                h = (h ^ (uint)word) * 16777619u;
                return h;
            }
        }
    }
}
