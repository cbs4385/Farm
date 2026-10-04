# Narrative bible, wave 2: Tilda, Juno and Piper

Status: **draft for owner review** (T-081, wave 2 of `../NPC_DIALOGUE_PLAN.md`). Nothing here changes shipped lines; once approved it governs what is written next. The template, tone and village facts are in `BIBLE.md`; the three slice villagers (Wren, Hazel, Bram) are already approved and locked. Everything below was checked against the shipped roster data (`NpcDefaults`, `NpcRoster`), the existing talk lines of each villager, and `mythos/LORE.md`.

Decisions this draft needs from you are collected in section 4.

## 1. Tilda Ashby (general store)

**Role and place.** Runs the general store: seeds, backpacks, gossip. Open nine to five, closed Sundays (she goes to the beach and the saloon on Sundays, and stays upstairs with a book on rainy ones). Birthday: Spring 12. Not romanceable. The first villager the player meets.

**Public face.** Warm, motherly, brisk. She sells you the right seed before you ask and tells you the weather before you notice it.

**Real self.** Quietly afraid the village is shrinking and the shop will one day have no one to serve. She keeps the lamp on after closing for anyone who needs company and calls it "doing the books". She does not like being thanked; she likes being owed, because debts mean people will come back.

**Voice rules.**
- Warm and practical. Shop language is her metaphor system: stock, slate, margin, closing time.
- Sentences up to about 20 words; at most one exclamation mark a line; "dear" at most once in twenty lines.
- Gives advice as instructions ("Plant parsnips first. They pay for everything else.").
- Never gossips cruelly: her gossip is always affectionate, about food, weather and who is courting whom.
- Never says "I don't mind" about herself.

**Humour type.** Wry shopkeeper's arithmetic: understatement about prices, margins and other people's secrets.

**Signature bit: *The slate*.** She keeps a chalk slate of every small kindness in the village ("I've put you down for a pie" / "Marcus is two planks ahead of Odalys"), and settles accounts with favors, never gold. The player learns their slate balance is always "in credit". Catchphrase: **"I'll put it on the slate."** Sound: the shop bell, two notes.

**Wants / fears.** Wants: a village that stays full; one customer who sits down for tea. Fears: closing for the last time; being the only one who keeps count.

**Relationships.**
- *Bram:* trades iron for goods; she sends a repair or a pie crust back as thanks.
- *Felix:* the oldest friendship; they bicker pleasantly about fish prices (she does not sell fish and has opinions).
- *Wren:* her best source of gossip and her best customer for lemons she never has.
- *Hazel, Ione:* orders books she never reads.
- *Marcus, Dr. Penn, Dorian:* trusted and slightly formal; she is the one who remembers their birthdays.
- *Juno:* a surrogate aunt; Piper: "the only person who haggles in rhyme".
- *The player:* "my favorite new account."

**Tastes.** Existing: loves strawberry and elderflower; likes cauliflower, potato and raspberry; dislikes stone, clam and all fish. Proposed: **loved** `crop.strawberry`, `forage.elderflower`, `food.pumpkin_pie`; **liked** `crop.cauliflower`, `crop.potato`, `forage.raspberry`; **disliked** `resource.stone`, `forage.clam`, `resource.slime`; category dislike: Fish. (The pie as a third loved item ties into the pie feud storyline.)

**Arc.**
- *Heart 2 (exists):* first deliveries and the slate introduced.
- *Heart 4, "The slate":* the player sees the slate; choice of what to add under their own name (*a thank-you*, *a joke*, *a promise*).
- *Heart 6, "After hours":* she is still in the shop with the lights half off; she admits she worries the village is shrinking; choices *reassure*, *offer a plan*, *joke gently*.
- *Heart 8, "Inventory":* the player helps with the yearly stocktake and finds her mother's recipe card behind the shelf; she has never baked from it.
- *Heart 10, "The sign":* she hangs a new shop sign with the farm's name under hers as a "supplier"; she hands over the original two-note bell.
- *Friend day, "Stocktake Saturday":* a helping scene (she needs three of something to finish the window).

**Year.** Spring: seed rush; summer: the awning goes up; fall: pie week; winter: she stays open late because the shop is warm.

**Moments (seeds).** *Funny:* the slate keeps score of pettiness ("Marcus owes Odalys an apology and a pie"); the lemons she never has. *Wholesome:* the lamp after closing. *Surprise:* the recipe card. *Quotable:* "Kindness is the only stock that goes up when you give it away."

**Mythos note.** Tilda is a **Keeper** (LORE.md). Her cozy lines must never hint at it: no talk of nights, wood or watching. Her mythos lines (`mythos.tilda.*`) live in `Farm.Mythos`; her business hours around ritual nights are handled by the schedule hooks, not by cozy text. At horror level 0 she is exactly the cozy shopkeeper above.

**Samples that sound right.** "Plant parsnips first. They pay for everything else." / "I'll put it on the slate." / "Mind the step. The cat has opinions." / "You're in credit, [player]. Don't make that face."
**Samples that do not.** "Sell me your soul for a discount." (horror leak) / "Dear, dear, dear, dearest!" (over the dear limit) / A cruel joke about a neighbor.

## 2. Juno Hale (blacksmith's apprentice)

**Role and place.** Bram's apprentice at the forge; does the fine work (clasps, hinges, jewelry). Works nine to six, closed Mondays with Bram. Birthday: Spring 25. Romanceable.

**Public face.** Cheerful, fast, covered in soot and enthusiasm; talks while she hammers.

**Real self.** Terrified of two opposite things: that she will never make anything original, and that when she does she will have to leave the forge and Bram. She jokes about both because the jokes get there first.

**Voice rules.**
- Fast, run-on, delighted. Often opens with "Okay, okay, okay" and breathes after a dash.
- Exclamations allowed (one a line); exaggerations ("the most perfect hinge in recorded history").
- Craft vocabulary used loosely and lovingly: temper, quench, grain, burr.
- Teases Bram affectionately; never criticizes his work.
- Never says "whatever".

**Humour type.** Exuberant exaggeration with a self-deflating tail.

**Signature bit: *The labels*.** She labels everything in the forge ("TONGS (NOT A BACK SCRATCHER)", "BRAM'S CHAIR (DO NOT)") and starts labeling the player and the village. Labels appear in her lines and as props in the forge. Catchphrase: **"Okay, okay, okay..."** Sound: three quick hammer taps.

**Wants / fears.** Wants: her first original piece; Bram to say it out loud. Fears: outgrowing the forge; Bram being alone in it.

**Relationships.**
- *Bram:* teacher and favorite person to annoy; the village's favorite gentle pairing. He fixes her mistakes quietly; she pretends not to see.
- *Tilda:* the aunt who feeds her; Juno pays her slate in clasps.
- *Wren:* her biggest cheerleader and the source of her nickname for Bram ("the Hm").
- *Piper:* closest in age; they trade an anvil for a fiddle bow on the beach; best friends who cannot sit through each other's long stories.
- *Hazel:* lends her books on metals she then mislabels.
- *The player:* "the reason I stayed in Wetherell" (existing line; becomes a late heart payoff).

**Tastes.** Existing: loves gold bar and truffle; likes copper bar and potato; dislikes kale and dandelion. Proposed: **loved** `resource.goldbar`, `forage.truffle`, `crop.pepper`; **liked** `resource.copperbar`, `crop.potato`, `resource.coal`; **disliked** `crop.kale`, `forage.dandelion`, `crop.cucumber` (too cold).

**Arc.**
- *Heart 2 (exists):* shows the player a clasp.
- *Heart 4, "The label maker":* she labels the player; a prank scene; choices *accept the label*, *relabel her*, *eat the label*.
- *Heart 6, "First strike":* her first original piece comes out crooked; she nearly throws it in the quench barrel; choices *keep it*, *make it better together*, *laugh with her*.
- *Heart 8, "The horseshoe":* the same night seen from her side as Bram's heart 8: she brings the crooked horseshoe to him and cannot look up; the player waits with her.
- *Heart 10, "Her mark":* she stamps her maker's mark for the first time and hands the player the first piece stamped with it.
- *Friend day, "The swimming pool":* a shared activity at the forge's quench bucket.

**Year.** Spring: hinges and hooks; summer: the forge as a sauna; fall: gift commissions; winter: she tells stories by the fire.

**Moments (seeds).** *Funny:* the labels; her review of Bram's "Hm." as dialogue. *Wholesome:* the crooked horseshoe. *Surprise:* the maker's mark. *Quotable:* "Technically it's art. Technically it's also a doorstop."

**Mythos note.** Juno is **unaware**. At horror levels above 0 she may notice odd things at the forge (cold iron on certain nights) and say so; her cozy lines never explain anything.

**Samples that sound right.** "Okay, okay, okay, look at this clasp!" / "It's labeled 'Bram's Chair (Do Not)'. He sat in it anyway." / "Hammer, tongs, patience. In that order, mostly." / "Don't tell him I cried at a hinge."
**Samples that do not.** "Whatever." / A long calm speech about feelings before heart 6. / Any criticism of Bram's work.

## 3. Piper Vance (musician)

**Role and place.** Plays the fiddle at the saloon most evenings from about seven until closing (Tuesdays off with Wren, Sundays by choice). Beach and village square in the mornings. Birthday: Spring 5. Romanceable.

**Public face.** Charismatic, quick, a little vain about her playing and entirely open about it.

**Real self.** She fills every silence because the quiet makes her wonder if the songs are any good. She has never written one that people sing when she is not in the room.

**Voice rules.**
- Playful, rhythmic sentences with a beat; she often finishes a thought as a rhyme.
- Exclamations allowed; musical vocabulary used as puns (key, pitch, tune, bridge, rest).
- Self-aggrandizing, then deflates in the next clause.
- Never mocks another musician or anyone's singing.
- Never says "I can't" about music.

**Humour type.** Musical wordplay and mock-grandeur.

**Signature bit: *The verse*.** She turns whatever the player just said into a couplet, and the rhyme is always slightly off ("It rhymes if you squint"). Late hearts, a verse is genuinely good, and she is stunned. Catchphrase: **"That's a chorus."** Sound: a rising fiddle slide.

**Wants / fears.** Wants: a song the village sings without her. Fears: silence; being a good performer and a forgettable writer.

**Relationships.**
- *Wren:* bandmate, bookkeeper and the loudest fan; the village's other long bet ("who finishes whose sentence").
- *Juno:* her best friend; they trade lessons (fiddle for filing).
- *Bram:* a fiddle tune once rescued his bad day; neither has ever said so.
- *Tilda:* haggles in rhyme with her; Tilda keeps score on the slate.
- *Hazel:* finds her lyrics in the library's margins and corrects the scansion, kindly.
- *Dorian, Elara, Felix:* each has been serenaded once; each pretends it did not happen.
- *The player:* "my front-row regular."

**Tastes.** Existing: loves sunflower and jam; likes strawberry and raspberry; dislikes coal and slime. Proposed: **loved** `crop.sunflower`, `artisan.jam`, `artisan.juice`; **liked** `crop.strawberry`, `forage.raspberry`, `crop.corn`; **disliked** `resource.coal`, `resource.slime`, `resource.bat_wing`.

**Arc.**
- *Heart 2 (exists):* requests and a first song about parsnips.
- *Heart 4, "Request line":* the player requests songs; each choice is a different tune and sets what she plays at heart 10.
- *Heart 6, "Stage fright":* she is hiding in the cellar before a gig; choices *talk it through*, *sit in silence*, *play along on a bucket*.
- *Heart 8, "The chorus":* the player helps write a song; each line is a choice; she writes the chorus around them.
- *Heart 10, "The song everyone knows":* the saloon sings it back; she steps off the stage and lets them (the performance template).
- *Friend day, "Parsnip ballad":* a shared activity on the beach with the parsnip song.

**Year.** Spring: new songs; summer: beach sets; fall: harvest dances; winter: indoors, wool socks, long ballads.

**Moments (seeds).** *Funny:* the off rhymes; the parsnip masterpiece. *Wholesome:* the song everyone sings. *Surprise:* the first verse that is actually good. *Quotable:* "Music is just noise that learned manners." (existing line)

**Mythos note.** Piper is **unaware**. Her songs may pick up a faint, unexplained refrain at horror levels above 0 (written in `Farm.Mythos`); her cozy lines never refer to it.

**Samples that sound right.** "That's a chorus." / "You said 'turnip'. I heard 'return it'. It rhymes if you squint." / "Request anything. I'll probably play something else." / "Don't clap. Sing. I'll pretend I'm surprised."
**Samples that do not.** "I can't play that." / Any mockery of a villager's voice. / A heartfelt speech before heart 6.

## 4. Decisions needed

1. **Surnames (corrected 2026-10-04).** The shipped data already names them Tilda Ashby, Juno Hale and Piper Vance (the dialogue box shows them). The first draft of this bible proposed Reyes and Vale by mistake; it now uses the shipped names.
2. **Tilda's real self** (afraid the village is shrinking, the slate as her way of keeping people), Juno's (afraid of never making something original and of outgrowing Bram) and Piper's (fills silence, wants a song the village sings).
3. **The slate** as Tilda's signature bit and **"I'll put it on the slate"** as her catchphrase; Juno's **labels**; Piper's **verse**. These shape every line written next.
4. **Cross-cast scenes:** Juno's heart 8 as the other side of Bram's heart 8; Piper's heart 10 as the saloon's song (a companion to Wren's "Opening Night"); Tilda's heart 4 slate gag naming Marcus and Odalys (the pie feud).
5. **Tastes** (the proposed third loved or disliked items above), and **Tilda's pie** (a loved pumpkin pie) tying to the pie feud.
6. **Sounds:** Tilda's two-note bell, Juno's three hammer taps, Piper's rising slide (the procedural voice synth already hashes a default for each; these would be hand-tuned like Wren's, Hazel's and Bram's).
