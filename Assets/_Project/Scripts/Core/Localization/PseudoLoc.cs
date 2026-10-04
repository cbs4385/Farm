using System.Text;

namespace Farm.Core
{
    // Pseudo-localisation (T-139): turns English into an accented, longer, bracketed lookalike so layout problems (text that does not fit,
    // strings that bypass the table, joined sentences) show up before a real translation exists. Placeholders, [tokens], {markup} and
    // <tags> are left untouched, so a pseudo-localised line must still run through the same code as the English one. Pure and deterministic.
    public static class PseudoLoc
    {
        public const float Expansion = 0.35f;         // German and French often run a third longer than English

        const string From = "aeiouyAEIOUYcnCNsSzZ";
        const string To = "áéíóúýÁÉÍÓÚÝçñÇÑšŠžŽ";

        public static string Apply(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var sb = new StringBuilder(text.Length * 2);
            var visible = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var close = c == '{' ? '}' : c == '[' ? ']' : c == '<' ? '>' : '\0';
                if (close != '\0')
                {
                    var end = text.IndexOf(close, i + 1);
                    if (end > i) { sb.Append(text, i, end - i + 1); i = end; continue; }      // a token, a markup group or a tag: copied as it is
                }
                var k = From.IndexOf(c);
                sb.Append(k >= 0 ? To[k] : c);
                if (!char.IsWhiteSpace(c)) visible++;
            }
            var padding = (int)(visible * Expansion);
            sb.Append(' ').Append('~', padding > 0 ? padding : 0);
            return "⟦" + sb.ToString().TrimEnd() + "⟧";
        }

        // True when `text` looks like something Apply produced.
        public static bool IsPseudo(string text) => !string.IsNullOrEmpty(text) && text.StartsWith("⟦") && text.EndsWith("⟧");
    }
}
