# NPC dialogue analysis: Farm versus highly rated cozy games

Status: analysis, 2026-10-02. No code or content was changed. Author: Claude (automated read of the repository).

## 1. Scope and method

**What was examined** (all in `Assets/_Project/`):

| Source | Content |
|---|---|
| `Resources/Story/npc_dialogue.json`, `npc_roster.json` | Dialogue graphs and talk sets for all 12 NPCs, 18 heart events |
| `Resources/Story/npc_roster_core.json`, `quests_and_mail.json`, `hall_and_festivals.json` | 5 more heart events, `bram.ask/remind/turnin`, festival narration |
| `Resources/Mythos/mythos_story.json` | Horror overlay: clue dialogues, altar, set entries gated on `horror:1` |
| `Resources/Localization/en.json` | The actual text (keys `dlg.*`, `event.*`, `mythos.*`) |
| `Data/Npcs/*.asset` | Birthdays, romanceable flag, gift tastes |
| `Scripts/Gameplay/Story/Dialogue.cs`, `FriendshipModel.cs`, `Npc/NpcInteractions.cs`, `UI/DialogueScreen.cs` | How dialogue is picked, run and shown |
| `docs/01-GameDesign.md`, `docs/adr/0003-dialogue-and-story-content.md` | Stated intent |

**Method.** I parsed every dialogue file with a script, resolved each text key against `en.json`, and counted lines, words, choices, conditions and set entries. I read all 204 NPC lines and the mythos clue lines in full. Figures in section 3 are measured. Priority-shadowing findings (section 4) come from reading `DialogueSet.Pick`, not from playing the game, so treat them as **verified by reading, not reproduced in play**.

**Benchmark caveat.** Comparisons to other games (section 5) are from my general knowledge of those games' publicly known design, not from fresh research in this session, and I have not re-checked exact line counts. Treat them as design patterns rather than citations. I name patterns, not copyrighted text, in keeping with the project's originality rule.

**Benchmark set:** Stardew Valley, Animal Crossing: New Horizons, Spiritfarer, Story of Seasons / Harvest Moon line, Disney Dreamlight Valley, Coral Island, Cozy Grove, Wylde Flowers.

## 2. How the system works (so findings make sense)

- A villager's `talk` set is a list of entries `{condition, priority, dialogue}`. `Pick` keeps only entries whose condition holds, takes the **highest priority present**, and chooses one deterministically from `today*7919 + hash(npcId)`. Lower-priority entries are ignored entirely while a higher one is valid.
- Every talk set has the same 9 entries: `first` (priority 100 until `met.<npc>`), `rain1` (1, `weather:rain`), `chat1-3` (0), `friend1-2` (2, hearts >= 3), `close1-2` (3, hearts >= 6).
- Gifts play `npc.<id>.gift.<birthday|loved|liked|neutral|disliked>`, falling back to generic `gift.*` narration.
- Friendship: 250 points per heart, +20 for the first talk each day, gifts 80 / 45 / 20 / -20 (x8 on birthdays), max 1 gift a day and 2 a week, slow decay after 3 days of neglect.
- Choice nodes are supported, with per-choice conditions and effects. Dread/horror content already uses them.
- Presentation: portrait, name, typewriter text at 90 characters per second, choice buttons. Speaker-less lines render as italic narration.
- Conditions already available for dialogue: `season`, `weekday`, `weather`, `hour`, `day`, `year`, `moon`, `var`, `flag`, `hearts`, `has`, `quest`, `knows`, plus mythos atoms. **The engine is richer than the content that uses it.**

## 3. Measured inventory

### 3.1 Content volume

| Item | Count | Notes |
|---|---|---|
| Villagers | 12 | 8 romanceable (Dorian, Elara, Felix, Hazel, Ione, Juno, Piper, Wren), 4 not (Bram, Marcus, Odalys, Tilda). Matches GDD. |
| Authored NPC lines (base game) | 204 | Exactly 17 per NPC, no variation between characters |
| Distinct "daily talk" lines per NPC | 7 | `chat1-3`, `rain1`, `friend1-2`, `close1-2` (8 counting rain) |
| Heart events | 24 | Exactly 2 per NPC (hearts 2 and 5; Tilda's heart-2 scene lives in `quests_and_mail.json`). GDD asks for **3-4 per NPC**. No event at 8 or 10 hearts. |
| Dialogue graphs with a **choice** | 13 base (12 first-meetings + `bram.ask`) | Every other talk line is a single non-interactive node |
| NPCs with a dialogue-driven quest | 1 of 12 (Bram) | Others rely on the board and mail |
| Average line length | 8.0 words | Range by NPC: Tilda 12.2, Ione 10.6 ... Hazel 6.3, Dorian 5.8 |
| Horror-layer clue dialogues | 18 (6 NPCs x 3) plus altar and reactions | All gated on `horror:1`, so the cozy game is unaffected at "off" |

### 3.2 Lines a player can actually hear, by friendship tier

| Hearts | Candidate talk lines (after the first meeting) | What the player hears |
|---|---|---|
| 0-2 | `chat1-3`, plus `rain1` on rainy days | 3 lines on dry days, **1 line on every rainy day** |
| 3-5 | `friend1-2` only | **2 lines, alternating** |
| 6-10 | `close1-2` only | **2 lines, alternating, for the rest of the game** |

Because higher priorities fully override lower ones, the content that exists for early tiers is not heard later. Rain lines are unreachable after 3 hearts, and chat lines after 3 hearts. The 6-10 heart range is 1,000 friendship points, and a player who talks daily and gifts regularly will spend many in-game weeks there hearing the same two sentences.

### 3.3 Friendship pacing (arithmetic from `FriendshipModel`)

- Talking alone: 20 points a day means 12.5 days per heart, **about 125 days to reach 10 hearts**, which is more than one in-game year (112 days). Talking is meaningful early but cannot be the whole loop.
- Two loved gifts a week add 160 points a week; a loved birthday gift is worth 640 (2.5 hearts), so birthdays are the strongest single lever. The reaction a player gets for that moment is one line (section 4.4).
- A first-meeting "nice" choice grants 10 points, which is 4% of a heart.

### 3.4 Gift reactions

Each NPC has five gift reactions (`loved`, `liked`, `neutral`, `disliked`, `birthday`), each a single line. Neutral reactions are close to interchangeable: "Thanks." (3 NPCs), "Thank you." (2), "Thanks!" (2), plus five longer variants. A loved gift to Dorian is "Hm. Thank you. Really."; to Hazel, "Oh... it's lovely. Thank you." Tastes themselves are well designed and individual (Felix loves fish, Bram loves truffles, Wren loves wine and hops, and so on), but each NPC has 2 loved, 2-3 liked and 1-2 disliked items, short of the GDD's "3 loved / liked / disliked."

## 4. Findings

Severity scale: **A** blocks the cozy promise or breaks a stated rule, **B** noticeable to most players, **C** polish.

### 4.1 Strengths (keep these)

1. **Distinct voices at a glance.** Bram (terse, "Hm."), Juno (chatty, jokes), Dorian (laconic, nature), Wren (bar-keeper's patter), Piper (musical metaphors), Ione and Hazel (bookish, gentle) are each recognisable in one line. Tilda's warmth and Felix's patience are clear.
2. **Voice carries through the gift lines.** Bram's "...What am I supposed to do with this?" and Felix's "Ah. Fishermen aren't picky, but... no." are character moments, not generic.
3. **Kind relationship arcs.** Lines escalate from professional to personal ("I look forward to your visits", "You're the reason I stayed in Wetherell") in a believable, all-ages way, matching the design guidelines.
4. **Relationships between villagers.** Juno/Bram, Hazel/Ione, Elara/Dr. Penn and Dorian lodging with Marcus make the village feel networked. Hazel and Elara both ask the player to keep a small secret.
5. **Gentle foreshadowing in cozy lines** ("Everyone sounds braver after dark", "The moon does strange things to the pond fish", "The forest likes you") that pays off in the horror layer without being needed for it.
6. **Choice design in the horror layer is the best-written dialogue in the project.** The clue1 scene (a flinch, a two-way choice that trades standing for dread, a reply that stays ambiguous) is exactly the "meaningful but small" choice that the cozy lines lack.
7. **Clean architecture.** Conditions, effects, deterministic daily pick, and failure-isolated mythos overlay mean every recommendation below is content work, not engine work.

### 4.2 Content gaps

| # | Sev | Finding | Evidence |
|---|---|---|---|
| G1 | A | **No seasonal, weekday, time-of-day or location awareness.** Every base-game line is unconditional apart from rain and hearts. The engine supports `season`, `weekday`, `hour`, `moon`, but none is used in a talk set. | All 12 sets have the identical 9-entry shape |
| G2 | B | **Time-bound lines can appear at the wrong time.** Tilda: "Business is slow before the planting rush" (heard in autumn). Ione: "The almanac says the winter will be gentle" (heard in summer). Wren: "Everyone sounds braver after dark" (heard at noon). Bram recommends "the early evening" at 9:00. | `tilda.chat1`, `ione.chat1`, `wren.chat1`, `bram.chat3` |
| G3 | A | **Dangling invitations with no way to answer.** Odalys: "I could use a hand gathering elderflower. Interested?" Piper: "Wanna hear?" Elara: "Stay a minute?" Marcus: "Come by Sunday." Felix: "Take my spot tomorrow." Dorian: "Come with me tomorrow." None has a choice or a follow-up, and none changes anything. These are the lines that sound most like a story starting and are where the player is most likely to feel the game is hollow. | `*.friend2` for those NPCs |
| G4 | B | **A promise that is never paid.** Hazel: "Too many. Ask me again when we're friends." There is no favourite-book line at any heart level. Wren's "Oh, sit down. Where do I start..." after "Any gossip?" ends the conversation with no gossip. | `hazel.first.r1_0`, `wren.first.r1_0` |
| G5 | B | **Late-game repetition.** Hearts 6-10 hear two lines for the rest of the game (see 3.2). Rain, chat and friend lines are shadowed. | `DialogueSet.Pick` priority rule |
| G6 | B | **Heart events stop at 5.** Nothing marks 8 or 10 hearts, the point where a player has invested most. GDD target is 3-4; delivered 2. Event text is three short lines each; the reward is always an item and friendship points. | 24 events |
| G7 | B | **Choices are decorative.** The first-meeting choice is "friendly" (+10 points) versus "ask about the shop" (information, 0 points). Friendly always wins, nothing remembers the choice, and no later line references it. Outside the first meeting, no cozy dialogue branches. | 12 `first` graphs |
| G8 | C | **Gift reactions are one line and not item-aware.** Giving a loved crop and a loved forage both produce the same line. Neutral reactions are near-identical across NPCs. | `npc.*.gift.*` |
| G9 | C | **Personalisation is thin.** `[player]` appears in 3 of Tilda's 17 lines but only 1 of most others, and 0 for Wren. `[farm]` appears in 6 lines in total. No line mentions what the player actually grew, the season's crop, the pet, the player's progress or recent events. | token counts |
| G10 | C | **Idle lines do not reflect world state.** Villagers do not comment on a festival coming up, the player's farm, a recent harvest or help-wanted job, or the moon (except Felix once). | none exist |
| G11 | C | **Shop/service NPCs never reference their business** after the first meeting, although a weekly stock change or upgrade in progress is a natural hook ("Your pickaxe will be ready tomorrow"). | none exist |

### 4.3 Mechanics and rule observations

| # | Sev | Finding |
|---|---|---|
| M1 | B | **Priority shadowing can hide a quest offer** (verified by reading, not reproduced). `bram.ask` has priority 2. At 3-5 hearts it is pooled randomly with `friend1-2` (about 1 day in 3), and at 6+ hearts `close1-2` (priority 3) outrank it entirely. A player who declines the offer and builds Bram's friendship past 6 hearts will not be offered `bram_stone` through dialogue. The generic fix is to give quest offers a priority above the friendship tiers. |
| M2 | C | The rain entry (priority 1) beats chat entries (0) but loses to friend/close entries (2/3), so the rain-day experience differs from the intended "weather flavour" for any NPC above 3 hearts. |
| M3 | C | Same daily pick for the same day by design (talk twice, hear the same thing). Good, but with 2-line pools the player sees each line every other day. |
| M4 | C | `DialogueScreen` shows one NPC portrait per speaker with no expression variants. Cozy peers use 2-4 expressions to carry tone cheaply (see 5.3). |
| M5 | C | Cancel skips ahead to the next choice. Effects run on the way, which is correct, but a skipped `first` conversation still sets `met.*`, so accidental skipping of an introduction cannot be undone. Acceptable; noted for QA. |

### 4.4 Emotional weight

The single most valuable moments in the friendship system are birthdays and heart events. Birthdays are worth 8x points (up to 640) and get a single line. A loved birthday gift to Tilda produces "You remembered my birthday! You've made my whole year." That is a good line, and it is the only acknowledgement. Cozy peers invest most heavily here (see 5.1, 5.2).

## 5. Comparison with highly rated cozy games

### 5.1 Pattern table

Ratings are my qualitative judgment: **Strong**, **Adequate**, **Thin**, **Absent**. "Peers" means the benchmark set in section 1, where the pattern is widely known.

| Pattern | What peers do | Farm today | Rating |
|---|---|---|---|
| Distinct character voice | Each villager is recognisable by diction and obsession | Clear in most NPCs | **Strong** |
| Daily variety | Many lines per villager, varied by day, season, weather, place and heart level (Stardew-style); fresh daily dialogue plus catchphrases (Animal Crossing) | 3 lines per day on dry days at low hearts, 2 lines from 3 hearts | **Thin** |
| Season and calendar awareness | Villagers comment on the season, upcoming festivals, the day of the week | None | **Absent** |
| World-state reactivity | Comments on weather, the player's farm, recent events, achievements | Weather only (and only below 3 hearts) | **Thin** |
| Heart events or story beats | 3-5+ scripted scenes per villager, some with choices and consequences, with late-game ones as emotional peaks | 2 per NPC, no choices, none past 5 hearts | **Thin** |
| Meaningful choices | Choices change relationships, unlock scenes, or are remembered | Only a trivial first-meeting choice and the horror clues | **Thin** (cozy) / **Strong** (horror) |
| Gift feedback | Reaction per taste tier, often item- or context-specific; birthdays are a big moment | 5 single-line reactions per NPC | **Adequate** |
| Personal quests and arcs | Each villager has a personal storyline or request chain (Spiritfarer's and Dreamlight Valley's friendship quests are the clearest examples) | 1 of 12 | **Thin** |
| Mutual relationships | Villagers talk about and to each other, with cross-NPC events | Light cross-references in text, no scenes | **Adequate** |
| Tone and safety | Warm, low-stakes, no punishing choices | Matches | **Strong** |
| Presentation | Portraits with expressions, a typewriter effect, a voice blip or sound per speaker | Portraits (single), typewriter, no sound | **Adequate** |
| Accessibility and localisation | Text speed, skip, localisation-ready | Skip and localisation keys present; text speed fixed at 90 cps | **Adequate** |
| Cohesive optional darkness | Rare in the cozy genre; when done (Wylde Flowers' witch theme, Cozy Grove's ghosts) it is woven through the same cast | Strong design, with an off switch | **Strong, distinctive** |

### 5.2 What the best peers do that Farm does not (yet)

1. **Make the player feel *known*.** Stardew-style villagers vary lines by what the player has done and where they are standing, so the same person feels responsive. Farm's NPCs never refer to the player's actions, which is the biggest distance between the two.
2. **Give invitations somewhere to go.** Dreamlight Valley and Spiritfarer turn a villager's request into a small playable moment and a reward. Farm has the language of invitations (G3) but no destination.
3. **Spend the emotional budget at the end of a relationship, not the start.** The strongest cozy scenes arrive at 8-10 hearts and are often the reason players finish a villager's arc. Farm stops at 5.
4. **Use time as texture.** Animal Crossing's day-by-day freshness and its seasonal event chatter give a reason to talk daily for a long time. Farm's daily talk is the pillar of friendship points (section 3.3), so freshness matters directly.
5. **Treat birthdays as a set piece.** A short special scene, a small gift back, mail the next morning, a seasonal calendar marker.
6. **Let the cast gossip.** In the better peers, villagers react to each other, so befriending one changes what another says.

### 5.3 Where Farm is already ahead of the genre

- **Condition-driven everything**, including schedules, so a friendly neighbour can hold a hidden night schedule. Very few cozy games have this.
- **Choices that remove options under dread**, and clue scenes whose tone shifts while staying all-ages.
- **A hard "off" switch for the horror layer** that guarantees base behaviour.
- **Deterministic daily pick** that avoids the "reroll until you hear something new" behaviour.

## 6. Recommendations

Ordered by value for effort. All fit existing data formats and condition atoms (no assembly changes), and horror content stays in `Farm.Mythos` and data per ADR 0002. Every item should also add or extend a validator/test (T-040 style) so missing keys and unreachable lines are caught automatically.

### P1 - Fix before release (small, high value)

| ID | Change | Effort | Resolves |
|---|---|---|---|
| R1 | **Raise quest-offer priority** above friendship tiers (for example 4 for offers, keep 4-5 for remind and turn-in), and add a regression test that a 6-heart NPC with a `new` quest still offers it. | XS | M1 |
| R2 | **Add a "reachability" validator test**: for each talk set, simulate the hearts 0-10 x weather x season matrix and fail if an entry can never be selected or an NPC has fewer than N distinct lines at any tier. Would have caught G5 and M2. | S | G5, M2 |
| R3 | **Fix time-bound lines** by adding `season:` and `hour` conditions (for example Tilda's planting rush to `season:spring`, Ione's almanac line to autumn/winter, Wren's "after dark" to `hour>=18`). | XS | G2 |
| R4 | **Resolve dangling invitations**: make each `friend2` a two-option node where the second option politely declines, and wire the accept path to something small (see R8). At minimum, rewrite lines that promise something into plain observations. | S-M | G3 |
| R5 | **Pay off or remove promises**: add Hazel's favourite-book line at 4+ hearts and give Wren's gossip a real answer (the first of a rotating gossip pool, R6). | XS | G4 |

### P2 - Make daily talk feel alive (medium)

| ID | Change | Effort | Resolves |
|---|---|---|---|
| R6 | **Add tiered pools and per-season lines.** Targets per NPC: 4 chat lines per season (16 total), 4 weekday/location-flavoured lines, 3 rain lines, 6 friend-tier and 6 close-tier lines. At roughly 8 words a line this is about 40 new lines per NPC, or 480 across the cast: a few days of writing, and it multiplies the perceived content of the whole game. | M | G1, G5 |
| R7 | **Stop priority from hiding lower tiers**: let `friend`/`close` entries share a pool with `chat` entries (same priority, different hearts conditions), so a 7-heart player still hears seasonal lines and rain lines in addition to close-tier ones. | XS | G5, M2 |
| R8 | **Reactive lines** (about 1 per NPC per category): comments on a harvest, a festival within 3 days (`day` conditions), the moon, a recently finished quest or the player's progress (`var:` stats already exist). | M | G9, G10 |
| R9 | **Shop-aware lines**: "your upgrade is ready tomorrow" style hooks for Bram, Tilda, Marcus, Felix, Odalys. | S | G11 |
| R10 | **Rotate gossip**: Wren's gossip pool and cross-NPC references (a line about Juno from Bram conditional on `hearts:juno>=N`). | S | G4, cross-NPC |

### P3 - Spend the emotional budget (medium-large)

| ID | Change | Effort | Resolves |
|---|---|---|---|
| R11 | **Add heart events at 8 and 10 for all 12 NPCs** , each with at least one meaningful choice whose outcome is remembered through a flag and referenced in later `close` lines. | L | G6, G7 |
| R12 | **Make heart-event choices matter lightly**: warm versus honest options that change a flag and a follow-up line, never a penalty (cozy, all-ages). | M | G7 |
| R13 | **Birthday set piece**: a short scene plus a next-morning mail from the NPC, using existing events and mail. | M | 4.4 |
| R14 | **Personal request chains** (3-step quests) for the 8 romanceable NPCs and the remaining 3 non-romance NPCs, using the invitation lines from R4 as the entry points. Reuse board and quest systems. | L | G3, quests |
| R15 | **Item-aware gift lines** for loved items only (2 each), and unique neutral lines for every NPC. | S | G8 |

### P4 - Presentation polish

| ID | Change | Effort |
|---|---|---|
| R16 | Add 2-3 portrait expressions per NPC (neutral, smile, concerned/embarrassed) and an optional `expression` field on nodes. | M (art) |
| R17 | Add a text-speed setting (accessibility) and a soft per-speaker voice blip. | S |
| R18 | Show a small "heart gained" flourish at the end of the conversation rather than a toast over it. | XS |

### Suggested sequence

1. R1, R2, R3, R5 together as one bug-fix task (a day).
2. R7, R6 for the 4 NPCs the player meets first (Tilda, Bram, Ione, Wren), then the rest.
3. R4/R14 together so that every invitation has somewhere to go.
4. R11-R13 as the Milestone 5 content push.
5. R16-R18 when art for T-060 lands.

## 7. Risks and cautions

- **Scope.** R6, R11 and R14 are authoring-heavy. The scope caps in the GDD are binding, so decide targets (lines per NPC, events per NPC) there first rather than expanding implicitly.
- **Localisation and ids.** New keys must go in `en.json` and dialogue ids are save-compatibility surfaces once shipped (ADR 0003). Never rename the existing ids.
- **Horror layer.** Keep new cozy content free of cult references. Foreshadowing lines (4.1.5) should stay ambiguous so they read as cozy at horror level 0. The mythos overlay already outranks base lines through priorities 8-10, so adding base lines of priority 0-4 cannot shadow it.
- **Originality.** All new text must be original. Do not borrow lines or character beats from the benchmark games.
- **Verification.** Section 4.3 findings come from reading code. A test that fails first (project rule) should confirm M1 before the fix.

## 8. Quick scorecard

| Dimension | Score (1-5) | Comment |
|---|---|---|
| Character voice | 4 | Distinct, with room to deepen Elara, Hazel, Odalys |
| Variety | 2 | 2-3 lines per day, none seasonal |
| Reactivity | 1.5 | Weather only |
| Emotional payoff | 2.5 | Good tone, events stop at 5 |
| Player agency | 2 | Strong in horror, absent in cozy |
| Systems and tooling | 4.5 | Conditions, effects and overlay are ahead of the genre |
| Cozy tone and safety | 5 | Consistent |
| **Overall (cozy layer)** | **~2.8 / 5** | A solid, well-voiced foundation that currently reads as a first pass. Most of the gap is content volume and conditions, not engineering. |

## 9. Implementation status (2026-10-02)

Verified by automation only (563 EditMode and 107 PlayMode tests green); nobody has played the new lines yet.

| ID | Status | What changed |
|---|---|---|
| R1 | done | `bram.ask` priority 2 to 4; `BramQuestOffer_IsNeverHiddenByFriendship` |
| R2 | done | `DialogueVarietyTests`: every talk entry is reachable, minimum distinct lines per tier, rain lines for close friends |
| R3 | done | `season:`/`hour` conditions on Tilda, Ione, Wren, Bram and Piper lines; `TimeBoundLines_OnlyPlayAtTheirTime` |
| R4 | done | Odalys, Piper, Elara, Marcus, Felix, Dorian: invitation is now an accept/decline choice; accepting sets `flag:invite.<npc>`, gives +15 friendship (some give a small item), and later swaps in a follow-up line (`<npc>.friend2b`) |
| R5 | done | `hazel.book` plays once at 4 hearts; Wren's gossip answer added |
| R6 | partly | Per NPC: 4 seasonal lines, 2 friend lines, 2 close lines, 1 extra rain line (about 108 lines). Not done: weekday/location lines and the full ~40 lines per NPC target. Tilda, Ione, Wren and Bram also gained one general chat line |
| R7 | done | friend/close entries now share one pool with chat lines (priority 0); rain stays at 1 |
| R10 | partly | Cross-villager references written into lines (Bram/Juno, Hazel/Ione, Marcus/Dorian, Wren/Piper, Odalys/Elara); no conditions on other villagers' hearts |
| R15 | partly | Distinct neutral gift reactions for every villager. Item-aware loved lines not done |
| R8, R9 | not done | Need new condition atoms or game-state queries (festival countdown, upgrade-ready, recent harvest) |
| R11-R14 | not done | Heart events at 8 and 10, choice-driven events, birthday scenes and personal request chains are large authoring tasks. Event scenes already have a `dialogue` step, so choices inside heart events need no engine work (cutscene polish such as camera and emotes does; see `NPC_DIALOGUE_PLAN.md`) |
| R16-R18 | not done | Portrait expressions need art (T-060); text speed and heart flourish are small UI tasks |

Notes:
- Correction to section 3.1: Tilda does have a heart-2 event (in `quests_and_mail.json`), so every villager has exactly 2 heart events.
- Side effect of R1: a player who declines Bram's offer hears `bram.ask` on every visit until they accept, which hides his friendship lines for that player. This mirrors `bram.remind`.
