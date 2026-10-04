# Friendship pacing (T-084)

Rules (`FriendshipModel`): 250 points a heart, 20 for a daily talk, gifts at most one a day and two a week (liked 45, loved 80, a birthday gift eight times that, the same item again within 14 days half), neglect costs 2 a day after 3 days. Heart events add 40 to 80 points when they play.

Three players, from `FriendshipPacingTests` (a game year is 112 days):

| Player | Habits | Ten hearts |
|---|---|---|
| Engaged | talks daily, two liked gifts a week, sees every scene | about two months (40 to 90 days) |
| Casual | talks daily, no gifts | inside the first year |
| Rare | one liked gift a week, scenes only weekly | more than a year, but reachable |

Rules the model enforces: the first heart event (heart 4, after the existing heart 2) arrives inside a month for an engaged player, and no two heart steps from 4 to 10 are closer than a week.

Why it matters for streams: a scene a week keeps a viewer-watched run fresh; a scene a day would burn the set pieces, and a scene never would waste them. If scene rewards, gift points or the talk value change, run these tests and read the table again.

Not a substitute for a human playthrough: these are arithmetic bounds, not feel.
