using System.Linq;
using Farm.Core;

namespace Farm.Gameplay
{
    // The mailbox (T-039): letters arrive overnight (or at once through the `mail:` effect), wait in the mailbox, and are
    // read from it. Reading runs the letter's effects (gifts, a recipe, a quest) and keeps the letter in the journal.
    public static class Mail
    {
        // Puts every letter whose condition now holds into the mailbox (once per letter ever). Returns how many arrived.
        public static int Deliver(GameSession s)
        {
            if (!s.InGame || s.Story == null) return 0;
            var arrived = 0;
            foreach (var letter in s.Story.Letters.OrderBy(l => l.Id, System.StringComparer.Ordinal))
            {
                if (Known(s.State, letter.Id)) continue;
                if (Conditions.TryEvaluate(letter.Condition, s.World, out var ok) && ok) { s.State.Mailbox.Add(letter.Id); arrived++; }
            }
            return arrived;
        }

        // The `mail:` effect: a letter sent on purpose arrives now.
        public static bool Send(GameSession s, string letterId)
        {
            if (s.Story.Letter(letterId) == null || Known(s.State, letterId)) return false;
            s.State.Mailbox.Add(letterId);
            s.Toast(L.Get("mail.arrived"));
            AudioService.PlayIfAvailable(Sfx.Letter);
            return true;
        }

        public static bool Known(GameState state, string letterId) => state.Mailbox.Contains(letterId) || state.MailKept.Contains(letterId);

        // Takes the next letter out of the mailbox. The effects run when the player has read it (see Finish).
        public static LetterDefinition Next(GameSession s)
        {
            foreach (var id in s.State.Mailbox.ToList())
            {
                var letter = s.Story.Letter(id);
                if (letter != null) return letter;
                s.State.Mailbox.Remove(id);      // a letter whose data is gone (a removed module): drop it
            }
            return null;
        }

        // The letter has been read: keep it, and run what it does.
        public static void Finish(GameSession s, LetterDefinition letter)
        {
            if (!s.State.Mailbox.Remove(letter.Id)) return;
            if (!s.State.MailKept.Contains(letter.Id)) s.State.MailKept.Add(letter.Id);
            Effects.RunAll(s, letter.Effects);
        }
    }
}
