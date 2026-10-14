# Villager relationships: the starting point

Status: **working draft for design review.** Nothing here changes shipped lines or data. It collects what the game already says about how the twelve villagers feel about one another and about the player, so the team can see the web as a whole, find the thin spots and decide what to build next.

**Sources.** `BIBLE.md` (Wren, Hazel, Bram), `BIBLE_WAVE2.md` (Tilda, Juno, Piper), `BIBLE_ENHANCED.md` (Marcus, Dr. Penn, Felix, Dorian, Elara, Ione), the eight storylines (`storylines/`), `mythos/LORE.md`, and the roster data (`NpcDefaults.cs`, `NpcRoster.cs`, `FriendshipModel.cs`). Where the sources are silent, this document says so (`·` in the matrix) rather than inventing an attitude; suggested fills are in section 6 and are proposals only.

**How to use it.** Section 2 is the cast at a glance. Section 3 is each villager's personality, wants and goals, and how they greet the player. Section 4 is the villager-to-villager matrix and the pair notes behind it. Sections 5 and 6 are the observations (clusters, gaps, uneven depth) and the questions worth discussing. Section 7 is the horror layer, kept apart because it changes what some of these attitudes really are (spoilers).

---

## 1. Reading the scale

**Between villagers** (how the *row* villager feels about the *column* villager; it is not always mutual):

| Mark | Meaning |
|---|---|
| **3** | Close bond: family-like, will do anything quietly for them |
| **2** | Warm: real fondness, regular exchange |
| **1** | Cordial: friendly, knows them well, no special tie |
| **0** | Polite distance: courteous, little contact |
| **R** | Affectionate rivalry: friction that is mostly play (or a feud the village enjoys) |
| **S** | Shy or avoidant: wants contact and keeps away, or leaves them alone on purpose |
| **·** | Not written yet |

**Toward the player.** Friendship is 0 to 10 hearts at 250 points a heart. Every villager starts at **0 points and not yet met** (`NpcState` defaults), so the starting attitude is *polite warmth*: the tone rule in `BIBLE.md` is "villagers start polite and become fond; they are never contemptuous of the player and never scold." What differs is the **flavor of the first impression** and **what each villager wants from the player**, given in section 3. Neglect costs 2 points a day after 3 days without contact, so attitude toward the player is never permanent.

**The player.** The new neighbor, at the farm on the village's edge, inheriting from a distant uncle (Edmund Fenn). Nobody knows the player; everybody knows the farm.

---

## 2. The cast at a glance

| Villager | Role | Birthday | Romance | Public face | Hidden want | Player's role to them |
|---|---|---|---|---|---|---|
| **Tilda Ashby** | General store | Spring 12 | No | Warm, motherly, brisk | A village that stays full | "My favorite new account" |
| **Bram Hollis** | Blacksmith | Fall 4 | No | Gruff, economical | Juno to flourish; a word from his old teacher | The one he finally calls by name (heart 10) |
| **Ione Fairweather** | Librarian | Winter 20 | Yes | Gentle, precise | More readers; the sea in winter | "The reader I hoped for" |
| **Marcus Dell** | Carpenter | Summer 9 | No | Solid, patient | A building that outlives him (the hall roof) | A helper for the roof |
| **Dr. Odalys Penn** | Village doctor | Winter 3 | No | Formal, brisk | A village that rests | "A patient I hope never to see, and a friend I hope to" |
| **Wren Calloway** | Saloon keeper | Fall 17 | Yes | The warm center of the village | To see the sea from somewhere else | The audience for her stories |
| **Felix Hartwell** | Fish stall | Summer 21 | Yes | Salty, slow, laconic | Someone to share the dawn | "My dawn partner" |
| **Juno Hale** | Blacksmith's apprentice | Spring 25 | Yes | Cheerful, fast, sooty | Her first original piece | "The reason I stayed in Wetherell" |
| **Hazel Brandt** | Library assistant | Fall 8 | Yes | Quiet, flustered, polite | To be heard once, properly | The listener she may eventually trust |
| **Piper Vance** | Musician | Spring 5 | Yes | Charismatic, a little vain | A song the village sings without her | "My front-row regular" |
| **Dorian Lake** | Forager | Winter 11 | Yes | Quiet, watchful, brief | A friend who needs no talk | The one who can sit in silence |
| **Elara Finch** | Clinic nurse | Spring 18 | Yes | Bright, bubbly | To see the sea from a boat | "My favorite recovery" |

Depth of writing is uneven: **Full** (Tilda, Wren, Bram, Juno, Hazel, Piper: heart events at 2, 4, 6, 8, 10 and a friend day) versus **Enhanced** (Marcus, Dr. Penn, Felix, Dorian, Elara, Ione: hearts 2, 5 and 8, one quest). See section 5.

---

## 3. The villagers

Each entry: **Personality**, **Wants and goals**, **Fears**, **First impression of the player**, **What they need from the player**, and the **hook to develop**.

### Tilda Ashby, the shopkeeper
- **Personality.** Warm, practical, wryly funny about prices and other people's secrets. Gives advice as instructions. Never gossips cruelly. Keeps a chalk slate of every small kindness in the village and settles accounts in favors, never gold. Does not like being thanked; likes being owed, because a debt means people come back.
- **Wants and goals.** A village that stays full. One customer who sits down for tea. To keep the shop (and its warm lamp) open.
- **Fears.** Closing for the last time. Being the only one who keeps count.
- **First impression of the player.** She is the first villager met and wrote the welcome letter. She sells the player the right seed before they ask. The player is an *account*: from the first day they are "in credit".
- **Needs from the player.** Proof the village can still grow: a regular, a supplier, someone who stays. Her heart 10 makes the farm a listed supplier on her sign.
- **Hook.** The slate is the village's memory. Anyone's slate entries about anyone else are free relationship content (the pie feud gag already uses it).

### Bram Hollis, the blacksmith
- **Personality.** Gruff, literal, deadpan. Speaks in fragments; "Hm." is his whole vocabulary. Shows love only by fixing things before being asked. Never lectures; his teasing is aimed at tools and weather.
- **Wants and goals.** Good work. Juno's first original piece. One word from his unseen old teacher.
- **Fears.** Being the reason Juno stays small. A tool he made failing someone.
- **First impression of the player.** Unimpressed on the surface; shows the forge and hands over a copper bar at heart 2. Rarely uses the player's name until heart 10.
- **Needs from the player.** An ally in saying the unsayable to Juno (heart 8: help him find the words).
- **Hook.** The village's long bets: Wren's daily fourth word, and what he quietly does for everyone (the free hook repair for Felix, early book returns).

### Ione Fairweather, the librarian
- **Personality.** Gentle, exact, quietly amused. Overdue is "a state of mind". Sets a book aside for every visitor and it is always the right one. Loves the sea and the first snow and would never say so.
- **Wants and goals.** More readers. To see the sea in winter.
- **Fears.** A library that stays a quiet room.
- **First impression of the player.** A potential reader, treated with careful, unhurried interest.
- **Needs from the player.** To be read to, borrowed from, visited. The player is "the reader I hoped for".
- **Hook.** The set-aside shelf of books for people who never came: a ready-made emotional engine, and a way to tie her to every other villager (what is set aside for whom?).

### Marcus Dell, the carpenter
- **Personality.** Measured, patient, craftsmanlike. Measures three times because his father did. Never rushes, never criticizes another's work.
- **Wants and goals.** The Community Hall roof finished, properly. A building that outlives him.
- **Fears.** A joint that gives way.
- **First impression of the player.** Courteous and slow to open; someone to be measured against a plank, not a stranger to be hurried.
- **Needs from the player.** Help finally finishing (or redesigning) the hall roof, the village's longest-running "nearly".
- **Hook.** The hall is the player's long cozy project, so Marcus is the natural co-owner of that arc, and the pie feud makes him a comedic lead too.

### Dr. Odalys Penn, the village doctor
- **Personality.** Formal, brisk, precise, a little intimidating; her sharpness is concern. Prescribes absurdly specific cozy remedies. Barely sleeps.
- **Wants and goals.** A village that rests. To carry less alone.
- **Fears.** Being the one who could not help.
- **First impression of the player.** Evaluative: she is checking whether the player looks after themselves. A patient she hopes never to see.
- **Needs from the player.** Someone who takes the prescription in the other direction and makes *her* rest (heart 8, the soup).
- **Hook.** She is the village's carer, so her relationships are mostly professional (patients) and could be made more personal.

### Wren Calloway, the saloon keeper
- **Personality.** Warm center of the village, raconteur and embellisher ("I never repeat gossip. I improve it."). Hates cooking, loves hosting. Lonely in a crowd.
- **Wants and goals.** A full room and a story to tell about it. To see the sea from somewhere else.
- **Fears.** An empty room. Being the person who stayed.
- **First impression of the player.** Instantly welcoming: "stranger" becomes "friend" becomes the player's name. A new story to collect.
- **Needs from the player.** A real audience, and an honest answer about the packed bag (heart 8: go, stay or listen).
- **Hook.** Her gossip network touches everyone, which makes her the natural narrator of village-wide events (and a Keeper, see section 7).

### Felix Hartwell, the fisher
- **Personality.** Salty, slow, laconic, a patient teller of tall tales; each fish grows with every telling.
- **Wants and goals.** A fishing partner. The sturgeon.
- **Fears.** A quiet dawn that stays quiet.
- **First impression of the player.** Mild and unhurried; assumes nothing, sells bait.
- **Needs from the player.** Company at dawn. The player becomes "my dawn partner".
- **Hook.** He has the oldest friendship in the village (Tilda) and few other ties in the writing: a gap worth filling.

### Juno Hale, the apprentice
- **Personality.** Exuberant, fast, exaggerating, self-deflating. Labels everything in the forge, and soon the player and the village. Jokes about her fears so the jokes get there first.
- **Wants and goals.** Her first original piece. For Bram to say it out loud.
- **Fears.** Outgrowing the forge. Bram being alone in it.
- **First impression of the player.** Instantly friendly; shows off a clasp.
- **Needs from the player.** A witness: she brings the crooked horseshoe to Bram and cannot look up (heart 8). The player is "the reason I stayed".
- **Hook.** Her best friend (Piper), aunt (Tilda) and mentor (Bram) already make her the village's most connected young person.

### Hazel Brandt, the library assistant
- **Personality.** Quiet, flustered, deadpan, sharp and funny once trusted. Precise numbers, parenthetical asides, book metaphors. Secretly writes poems and field-notes.
- **Wants and goals.** To be heard once, properly. For the library never to change.
- **Fears.** Reading aloud. Being known before she is ready.
- **First impression of the player.** Wary and polite; "Ask me again when we're friends." Trust is earned in small, silent steps (the swap of books).
- **Needs from the player.** A patient audience for the poem (hearts 6 to 10).
- **Hook.** She already knows more about the village than she says, which is also the key to her mythos role (section 7).

### Piper Vance, the musician
- **Personality.** Charismatic, quick, playful and a little vain, then deflates; rhymes that work "if you squint"; fills every silence.
- **Wants and goals.** A song the village sings without her.
- **Fears.** Silence. Being a good performer and a forgettable writer.
- **First impression of the player.** Treats the player as a ready-made audience: "front-row regular". Asks for requests and plays something else.
- **Needs from the player.** To co-write (heart 8) and to hear the saloon sing it back (heart 10).
- **Hook.** Her bandmate-best-friend ties (Wren, Juno) are strong; her ties to the quiet villagers are mostly "serenaded once, pretends it never happened".

### Dorian Lake, the forager
- **Personality.** Shy rather than cold, observant, very short sentences, nature words. Leaves small gifts and denies them ("Not me. The forest."). Keeps a secret notebook of mushroom drawings.
- **Wants and goals.** A friend who does not need him to talk.
- **Fears.** Being looked at.
- **First impression of the player.** Brief, watchful and polite from a distance. Tolerates the player more than most because the player does not crowd him.
- **Needs from the player.** Silence of the right length, and a reader for the notebook (heart 8).
- **Hook.** He lodges with Marcus and drinks tea in the saloon corner, so he is physically near more people than any other shy villager, and talks with almost none of them.

### Elara Finch, the nurse
- **Personality.** Bright, bouncy, gentle medical puns, never at a patient's expense. Knits increasingly absurd things in secret.
- **Wants and goals.** To see the sea from a boat. To be more than useful.
- **Fears.** Staying so useful she never leaves.
- **First impression of the player.** The first to knock on the farmhouse door (spring 2, the opening quest). Cheerful, a little too keen to help.
- **Needs from the player.** A travel companion in spirit: someone to pick a place from the list with (heart 8). The player is "my favorite recovery".
- **Hook.** The warm rivalry with Dr. Penn (joke versus frown) is one of the most reusable relationships in the cast.

---

## 4. Villager to villager

### 4.1 The matrix

Row = how that villager feels about the column villager. Order: Til, Bra, Ion, Mar, Pen, Wre, Fel, Jun, Haz, Pip, Dor, Ela.

| from \ to | Til | Bra | Ion | Mar | Pen | Wre | Fel | Jun | Haz | Pip | Dor | Ela |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Tilda** | | 2 | 1 | 1 | 1 | R | 3 | 3 | 1 | R | 1 | · |
| **Bram** | 2 | | 1 | 3 | 0 | 1 | 2 | 3 | 1 | 1 | 0 | 0 |
| **Ione** | · | 1 | | 1 | 2 | 1 | · | · | 3 | · | S | S |
| **Marcus** | 2 | 3 | 1 | | R | · | · | 2 | · | · | 1 | · |
| **Dr. Penn** | 1 | · | 2 | 2 | | · | · | · | 1 | · | · | R |
| **Wren** | R | 2 | 2 | 1 | 2 | | 2 | 3 | 3 | 3 | 1 | 2 |
| **Felix** | 3 | 2 | · | · | · | 2 | | · | · | S | · | · |
| **Juno** | 3 | 3 | · | 2 | · | 3 | · | | 2 | 3 | · | 2 |
| **Hazel** | 0 | 0 | 3 | 0 | 0 | 3 | 0 | 2 | | 1 | 2 | 2 |
| **Piper** | R | 1 | · | · | · | 3 | S | 3 | 1 | | S | 2 |
| **Dorian** | 1 | 0 | S | 1 | · | 1 | · | · | 2 | · | | S |
| **Elara** | · | · | S | · | R | 2 | · | 2 | 2 | 2 | S | |

(Ione's S toward Dorian and Elara, and Elara's and Dorian's S toward Ione, come from the Dorian entry: "Elara, Ione leave him alone, gently." It is a courteous leaving-alone, not coldness. Piper's S toward Felix and Dorian is the "serenaded once, pretends it did not happen" bit, an embarrassment, not a dislike.)

Counting the cells: of 132 possible directed pairs, about 94 have an attitude written and 38 are blank (Felix, Dr. Penn and Ione have the most blanks). Section 6 proposes how to fill the ones that matter.

### 4.2 The pairs that already exist (what to build on)

| Pair | Attitude | What is established | Engagement potential |
|---|---|---|---|
| **Bram and Juno** | 3 / 3 | Mentor and apprentice. He fixes her mistakes quietly; she pretends not to see; neither can say they are proud. Crooked horseshoe seen from both sides (hearts 8). | The village's favorite pairing. Needs the "will she leave?" tension to pay off with a visible, happy answer. |
| **Bram and Marcus** | 3 / 3 | The bench lunch in silence: "the best friendship in the village." | A friend-day scene exists (the bench). Open: what does the player learn if they sit and say nothing? |
| **Wren and Piper** | 3 / 3 | Bandmates; bickering about volume; the village's other long bet (who finishes whose sentence). | Heart 10 pairs with "Opening Night". |
| **Wren and Bram** | 2 / 1 | Daily bet: she wants a fourth word out of him. | Free recurring comedy; payoff when he finally says four. |
| **Wren and Tilda** | R / R | Rivals over who knows more; trade gossip for stock. Tilda's best gossip source, Wren's supplier. | Gossip arbitrage: the player carries rumors between them. |
| **Tilda and Felix** | 3 / 3 | Oldest friendship; they bicker about fish prices (she sells none). | Their shared history (what they were like young) is untouched. |
| **Tilda and Juno** | 3 / 3 | Surrogate aunt; Juno pays her slate in clasps. | Good for a "Juno's first commission" beat. |
| **Juno and Piper** | 3 / 3 | Same age; trade a bow for an anvil; cannot sit through each other's long stories. | The battle-of-the-bands storyline already plays on it. |
| **Marcus and Odalys** | R / 2 | Pie feud ("ambitious"). He obeys her about his back; she is "his most obedient patient". | A feud with real warmth under it. The resolution is an obvious late game moment. |
| **Odalys and Elara** | R / R | Warm rivalry (the joke and the frown) across the clinic. | Strongest reusable relationship; the clinic as a place where two ways of caring collide. |
| **Hazel and Ione** | 3 / 3 | Mentor and favorite person; Ione trusts her with the keys. | The library is the most settled relationship in the village. Little tension. |
| **Hazel and Wren** | 3 / 3 | "Hazel comes in with a book and leaves with a full notebook": Wren's stories are her raw material. | Hazel's poem and Wren's tales could meet. |
| **Hazel and Dorian** | 2 / 2 | Shared silences; they trade pressed leaves and mushroom notes for first editions. | A quiet pair who could be the village's first shy romance (if the player does not take either). |
| **Piper and Hazel** | 1 / 1 | Hazel is perplexed by Piper's loudness and secretly writes lyrics for her; she corrects Piper's scansion kindly. | A secret collaboration: unspoken, unknown to Piper. Strong dramatic irony. |
| **Dorian and Marcus** | 1 / 1 | Housemates with "about twelve words a week". | A silent comedy of two quiet men sharing a house. |
| **Elara and Piper** | 2 / 2 | Elara is the first to hear a new song. | Under-used. |
| **Elara and Juno** | 2 / 2 | She knitted Juno fireproof-ish gloves. | Under-used. |
| **Felix and Bram** | 2 / 2 | Bram repairs his hooks free and denies it. | The quiet-kindness theme again. |
| **Tilda and Piper** | R / R | Piper haggles in rhyme; Tilda keeps score on the slate. | Funny; could be extended to the rest of the village's trades. |

### 4.3 Storylines as relationship engines

The eight storylines (four drawn per new game) each put a small group together and make them act on each other. They are a reliable way to *change* an attitude during play; nothing in the starting matrix changes without one.

| Storyline | Who | What it does to the starting attitudes |
|---|---|---|
| Pie Feud | Marcus, Odalys (Wren, Hazel, Bram comment) | A live feud between two otherwise warm villagers; Wren runs the pool, Hazel files it, Bram would fix it with a hammer. |
| Anonymous Notes | Whole village | Strangers' kindness; nobody knows who; the reveal is small and human. |
| Lost Umbrella | Piper, Felix, Elara | Three villagers with little written between them are linked for the first time. |
| Rival Scarecrows | Tilda, Dorian | An odd couple (warm shopkeeper and the shyest villager) in a contest. |
| Mystery Whistler | The village at dusk (Ione, Juno) | A shared small mystery; the culprit varies per game. |
| Battle of the Bands | Piper, Juno, Wren, Bram | Strengthens an existing triangle; Bram agrees to hit one thing, once. |
| Missing Pumpkin | Tilda; culprit Dorian, Marcus or Felix | Puts Tilda at the center and one of three quiet men in the dock. |
| Five-Name Cat | The village | Each villager has named the same cat: a portrait of their personalities. |

---

## 5. Observations

1. **Strong clusters.** Three tight groups carry most of the writing: the **forge** (Bram, Juno, with Marcus and Tilda attached), the **saloon** (Wren, Piper, with Hazel's notebook and Bram's one cider), and the **library** (Ione, Hazel, with Dorian as a shy satellite). The clinic pair (Odalys and Elara) is a fourth, small and vivid.
2. **Thin ties.** **Felix** has two strong ties (Tilda, Bram) and almost nothing else. **Ione** has one dominant tie (Hazel). **Dr. Penn** and **Marcus** each have one pair. **Dorian** has many weak ties and no strong one. **Elara's** ties are fully warm but unwritten in the other direction.
3. **No true antagonism, by design.** Tone rule 1 is "warm first". The sharpest relationships are rivalries with an obvious warm floor. This is deliberate; the question is whether the lack of *any* real friction limits drama (see 6.1).
4. **Uneven depth.** Full villagers have five heart events; Enhanced villagers have two to three. The Enhanced six are also the least connected to each other, so the web is thinnest exactly where the writing is.
5. **Romance coverage.** Eight are romanceable (Wren, Felix, Juno, Hazel, Piper, Dorian, Elara, Ione). No villager-to-villager romance exists in the writing at all; the likeliest candidates, if wanted, are Hazel and Dorian (already a quiet pair) and Felix and Wren (both want the sea).
6. **Hidden asymmetry.** Some attitudes are one-way and secret: Hazel writes Piper's lyrics; Bram's fiddle-tune; Wren's packed bag; Elara's knitting from Dr. Penn. They are the best raw material for discovery-driven engagement because the player is the only one who can learn both sides.
7. **The player is the common factor.** In many entries the player is the *one* person who can see the hidden tie (Hazel's lyrics, Bram's letters, Wren's bag). That is the game's emotional structure and worth making explicit: the player *carries* attitudes between villagers.

---

## 6. Questions and proposed fills for discussion

### 6.1 Questions

1. **Friction.** Should anyone dislike anyone? A mild, non-malicious tension (Marcus and Wren's noise, Dr. Penn and Tilda's herb shelf, Bram and Piper's volume) could drive more varied scenes without breaking "warm first".
2. **Pairs with no tie.** Which blank cells matter? (See 6.2.)
3. **Should attitudes change in play?** Currently villager-to-villager attitudes are authored text, not data. A small numeric table (this matrix) could drive barks, schedule overlaps, "who is mad at whom" gossip and friend-day triggers.
4. **Should the player be able to change them?** The pie feud proves it is satisfying. Proposal: each villager has *one* relationship the player can repair or deepen (a "mend" action) and *one* the player can learn about (a secret).
5. **Romance between villagers.** Worth a quiet, optional subplot (Hazel and Dorian are the obvious pair), or kept out so it never competes with the player's romance?
6. **Spoken attitude.** Do villagers refer to one another at the right *hearts*? (A villager says something about Piper only if the player has met Piper.)

### 6.2 Proposed fills for the highest-value blanks (not yet in the game)

| Pair | Proposal | Why |
|---|---|---|
| Felix and Wren | **2 / 2**: she buys his best catch and his tall tales; he is one of her regulars, and her only audience that does not need an answer. | Gives Felix a second strong tie; both want to see the sea. |
| Felix and Ione | **1 / 2**: she sets a book about the sea aside for him each winter; he pretends he does not read. | Two sea-loving quiet people with no tie. |
| Ione and Juno | **2 / 1**: metals and maker's marks; Ione has the old guild book Juno needs for her mark. | Ties library to forge; supports Juno's heart 10. |
| Marcus and Wren | **1 / R**: Wren takes bets on the hall roof; Marcus says "nearly" at her. | Reuses the bet gag; gives Marcus a comic partner. |
| Marcus and Hazel | **0 / 2**: she shelved the almanac with the roof plans. | Connects the quiet pair at the hall. |
| Dr. Penn and Wren | **2 / 2**: she prescribes Wren a night off; Wren ignores it. | The village's two over-carriers. |
| Dr. Penn and Bram | **1 / 1** (Bram is currently 0 toward her): she nags about his hands and burns; he fixes her instruments. | Turns a silent "tolerated" into a real exchange. |
| Elara and Felix | **1 / 1**: she knitted a hat for his boat; they both want the sea. | Seeds a "go out on the water" hook shared with Wren. |
| Elara and Tilda | **2 / 1**: wool and bandages in the shop; Tilda's cold remedies. | Gives Elara a tie to the shop she is never written into. |
| Dorian and Juno | **1 / 1**: she labels his baskets; he leaves her a leaf. | Gives Juno (who knows everyone) one more shy friend. |
| Piper and Ione | **1 / 1**: Ione sets aside songbooks; Piper never returns them. | Fills a library/saloon bridge. |
| Piper and Odalys | **0 / 2**: she plays at the clinic for patients; Odalys tolerates it with a frown. | A fiddle in a clinic is a gentle joke. |

### 6.3 Ideas to drive engagement (for discussion, no commitment)

- **Carry-a-message** errands: the player takes a small kindness from one villager to another, which reveals how they feel about each other (and fills the matrix as a gameplay system).
- **Gossip tiers.** Tilda and Wren trade rumors; the player can learn which are true. Because the *Wren gossip bingo* gag already runs on contradictions, the player's job could be to catch the contradiction and report the truth.
- **Pair friend-days.** Today friend days are one villager and the player. A friend day for *two* villagers (the bench, the clinic, the saloon corner) would use the strong ties already present.
- **Seasonal alignments.** Each festival could expose a different tie (the dance pairs; the lantern feast seats everyone by tie).
- **A "village web" journal page** that fills in as the player learns each attitude. Because relationships are secrets in this game, learning them is a reward in itself.

---

## 7. The horror layer (spoilers; keep apart from the cozy writing)

Allegiance changes what several attitudes *really* are. At horror level 0 the table above is the whole truth. At levels 1 and 2 the layer adds hidden allegiance, which alters motive, not manner: a Keeper never behaves unkindly in cozy lines (see each bible entry's mythos note).

| Villager | Allegiance | How it recolors their attitude to the player |
|---|---|---|
| **Tilda** | Keeper | Wrote the welcome letter. Her warmth to the player is real, and so is her knowledge: the farm's owner is "the one they keep in reserve". |
| **Marcus** | Keeper | His patience and his "nearly fixed" hall (the Keepers quietly use the hall) take a second reading. |
| **Dr. Penn** | Keeper (leader) | Evaluating the player's health is also evaluating the player as an offering or asset; her "rest" is both medicine and watchfulness. |
| **Wren** | Keeper | Her saloon closes early on new-moon nights ("Quiet night. Bed early."); the packed bag becomes a stake. |
| **Dorian** | Keeper | His offerings on the player's step ("Not me. The forest.") become ambiguous. |
| **Hazel** | Resister | She has worked out the pattern in the village records. Her wariness of the player is also a test of whose side the player is on. |
| **Bram, Juno, Piper, Felix, Elara, Ione** | Unaware | Kind, ordinary, and the player's real human reaction to dread (Bram especially: stolid, practical, kind). |

Consequences for the relationship web:
1. **Five of twelve are Keepers** (Tilda, Marcus, Penn, Wren, Dorian). On the matrix, the Keepers' cells to each other are mostly **cordial or warm on the surface** (Marcus-Tilda 2, Wren-Penn 2) and carry a second, secret layer of shared purpose.
2. **Hazel is the odd one out**: she is polite-distant (0) to Tilda, Marcus and Dr. Penn, warm to Dorian (2) and closest to Wren (3). A Hazel and Wren friendship that is also her watching a Keeper is the richest secret in the cast.
3. **The unaware six** are the counterweight; they notice odd things (Bram's nails ordered on ritual days, Juno's cold iron) and are the player's confidants if horror is on.
4. **Dread speeds the decay** of villagers' attitude toward the player (the friendship-decay hook), so the player's *own* unease literally cools relationships.
5. Anything written about a Keeper's secret stays in `Farm.Mythos` and module data (`mythos.<id>.*`); do not add it to the core relationship text.

---

## 8. Open items to resolve before this becomes a design

- Confirm the matrix with the owner, especially every **R** and **S**, and the proposals in 6.2.
- Decide whether the matrix should become **data** (a `Relationships` module data file, readable by conditions) or stay a writing reference.
- Decide how the **Enhanced** six reach the same depth (T-150) and whether new ties are written with them.
- Decide how the **player-facing journal** might reveal the web.
- Check every line that names another villager against section 4 (the "never contradict another villager's entry" rule in `BIBLE.md` section 6).
