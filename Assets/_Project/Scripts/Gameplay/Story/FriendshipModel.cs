using System;

namespace Farm.Gameplay
{
    public enum GiftTaste { Loved, Liked, Neutral, Disliked }

    // Friendship rules (GDD 3.6), pure and tested: 0-10 hearts at 250 points each, a little for talking every day,
    // gifts at most once a day and twice a week, birthdays count eight times, and neglect slowly costs points.
    public static class FriendshipModel
    {
        public const int PointsPerHeart = 250;
        public const int MaxHearts = 10;
        public const int MaxPoints = PointsPerHeart * MaxHearts;
        public const int TalkPoints = 20;
        public const int BirthdayMultiplier = 8;
        public const int MaxGiftsPerDay = 1;
        public const int MaxGiftsPerWeek = 2;
        public const int BaseDecayPerDay = 2;     // points lost per day of neglect, before modifiers
        public const int DecayAfterDays = 3;      // days without contact before the decay starts
        public const int RepeatGiftDays = 14;     // giving the same item to the same villager again within this many days...
        public const int RepeatGiftPercent = 50;  // ...is worth this much of the usual points (never on a birthday, never for a dislike)

        public static int Hearts(int points) => Math.Max(0, Math.Min(MaxHearts, points / PointsPerHeart));

        public static int Clamp(int points) => Math.Max(0, Math.Min(MaxPoints, points));

        public static int GiftPoints(GiftTaste taste, bool birthday)
        {
            int basePoints;
            switch (taste)
            {
                case GiftTaste.Loved: basePoints = 80; break;
                case GiftTaste.Liked: basePoints = 45; break;
                case GiftTaste.Disliked: basePoints = -20; break;
                default: basePoints = 20; break;
            }
            return birthday ? basePoints * BirthdayMultiplier : basePoints;
        }

        public static int RepeatGiftPoints(int points, bool repeat, bool birthday) =>
            repeat && !birthday && points > 0 ? points * RepeatGiftPercent / 100 : points;

        // Points lost this morning for a neighbour the player has ignored. `rate` is the (possibly modified) daily loss.
        public static int DecayFor(int daysSinceContact, float rate) =>
            daysSinceContact >= DecayAfterDays ? (int)Math.Round(rate) : 0;

        public static int WeekOf(int totalDays) => totalDays / 7;
    }
}
