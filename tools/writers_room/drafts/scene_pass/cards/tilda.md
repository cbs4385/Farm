# Scene cards for tilda

26 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 51 spoken lines in scenes, 37 dialogue lines and 19 player choice labels.

### tilda_heart5 · "Too Many Scones" · funny
event · npc_roster_core.json · map GeneralStore · priority 6
plays when: `hearts:tilda>=5 && !weekday:sun && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Sit a moment, [player]. I baked a dozen scones for a village that buys six."  ✏ `event.tilda_heart5.0`
  · (move player to (7,4))
  · (player faces up)
  · (tilda emote ellipsis)
  [tilda]: "You're eating it without flinching. Felix flinches. He says it's the butter. It isn't the butter."  ✏ `event.tilda_heart5.1`
  [tilda]: "Nobody sits in a shop. They lean, they pay, the bell goes twice. You sat."  ✏ `event.tilda_heart5.3`  (expr embarrassed)
  [tilda]: "Take the mashed potato home, too. I made that for nobody in particular either."  ✏ `event.tilda_heart5.2`
  [tilda]: "Same chair on Thursday? I'll bake fewer scones. That's a lie, but a polite one."  ✏ `event.tilda_heart5.4`  (expr happy)
  · effects: give:food.mashed_potato,2, friend:tilda,60, flag:event.tilda_heart5

### tilda_heart2 · "Mother's Gratin" · wholesome
event · quests_and_mail.json · map GeneralStore · priority 5
plays when: `hearts:tilda>=2 && !weekday:sun && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Oh, [player]! Just the account I wanted. Come here, I've something to hand over."  ✏ `event.tilda_heart2.0`
  · (move player to (7,4))
  · (player faces up)
  · (tilda emote ellipsis)
  [tilda]: "My mother's cauliflower gratin. She taught it by hand, never wrote it down. Don't thank me. Bring me a cauliflower someday."  ✏ `event.tilda_heart2.1`
  [tilda]: "She wrote down one recipe in her whole life. I know roughly where it is. I haven't looked."  ✏ `event.tilda_heart2.2`  (emote ellipsis, expr thinking)
  · effects: learn:cook_gratin, friend:tilda,50, flag:event.tilda_heart2

### hall_dance · "Opening Night" · wholesome
event · spec_scenes.json · map CommunityHall · priority 8
plays when: `flag:hall.restored && flag:hall.roof && quest:wren_full_house=done && flag:event.piper_heart10 && flag:event.juno_heart10 && hour>=18 && hour<23 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(8, 6), tilda(5, 5), wren(7, 5), piper(6, 4), bram(9, 5), juno(8, 4), player(7, 2)
  · (letterbox on)
  · (lighting warm)
  · (place marcus to (8,6))
  · (marcus faces up)
  · (place tilda to (5,5))
  · (tilda faces right)
  · (place wren to (7,5))
  · (wren faces down)
  · (place piper to (6,4))
  · (piper faces down)
  · (place bram to (9,5))
  · (bram faces left)
  · (place juno to (8,4))
  · (juno faces down)
  · (move player to (7,2))
  · (player faces up)
  [marcus]: "I checked the roof once. Twice. ...Three times, my father said. It is finished. Not nearly. Finished."
  · (marcus emote sparkle)
  [wren]: "Look at this room! Full, and the porch is full too. Somebody count the porch. I need a number for the story."  (emote sparkle)
  · (spawn prop artisan.wine at (7,3))
  [wren]: "[player]. I said I'd open it on the night, and this is the night. First glass is yours. I poured it before the doors."  (expr happy)
  · (remove prop wine)
  [tilda]: "Nobody asked for credit for any of this. So I'm giving it, in the good chalk, under every name in this room."  ✏ `event.hall_dance.3`
  [piper]: "Same song, bigger room. One verse, then I step off again. My bow hand still hasn't heard."  (emote sweat)
  · (music saloon)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall takes the chorus. Piper stops playing and sways, and the song goes on without her."
  [bram]: "Hm. Hm-hm. Hm. I sing it in my own language."  (emote note)
  [juno]: "Those door hinges are mine. My mark's on every pin. Listen to it swing. It's showing off, and I'm letting it!"  (expr happy)
  [wren]: "The roof pool is closed! Six winters of coins, and Tilda picked the right year. I'm demanding a recount."
  [tilda]: "Into the hall box, every coin. Except one. Marcus, that one's yours. Spend it on something that isn't wood."  ✏ `event.hall_dance.10`
  [marcus]: "The ladder can wait one night. One dance. I built this floor to take it."
  · (marcus dance (background))
  · (tilda dance (background))
  · (wren emote heart)
  · (juno emote note)
  · (wait 2.5s)
  [narration]: "Marcus takes his one dance, and Tilda takes hers beside him. The lamps burn low, and nobody goes home."
  · (music stop)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.hall_dance, friend:marcus,40, friend:tilda,40, friend:wren,40, friend:piper,40, friend:bram,40, friend:juno,40

### story_notes_tilda · "The Note Writer" · wholesome
event · storyline_events.json · map Village · priority 7
plays when: `flag:notes.began && storyline:anonymous_notes.tilda && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: tilda(25, 18), player(25, 17)
  · (place tilda to (25,18))
  · (tilda faces down)
  · (spawn prop prop.note at (26,18))
  [tilda]: "Oh. Oh, it's you. Stand there a moment and look innocent with me."  ✏ `event.story_notes_tilda.intro`
  · (move player to (25,17))
  · (player faces up)
  · (tilda emote sparkle)
  · (tilda shrug (background))
  ▶ DIALOGUE `story.notes.tilda` (player-facing graph, plays here):
    n0 [narration]: "Tilda slides a folded note under a door and straightens up. Neither of you moves."  ✏ `dlg.story.notes.tilda.0`   (→ n1)
    n1 [tilda]: "...Well. Now you know who's been doing it. Wren mustn't find out. She'd want a cut."  ✏ `dlg.story.notes.tilda.1`
       ? "Say nothing. Just smile." → r0  effects: flag:choice.notes.tilda.smile  ✏ `dlg.story.notes.tilda.c0`
       ? "Thank her for the notes." → r1  effects: flag:choice.notes.tilda.thanks, friend:tilda,10  ✏ `dlg.story.notes.tilda.c1`
       ? "Slip a note back. Unsigned, obviously." → r2  effects: flag:choice.notes.tilda.note, friend:tilda,15  ✏ `dlg.story.notes.tilda.c2`
    r0 [tilda]: "...A smile. No chalk, no receipt. That's the hardest kind to put on a slate."  ✏ `dlg.story.notes.tilda.r0`   (expr embarrassed)
    r1 [tilda]: "Don't thank me, I'd rather you owed me. ...Oh, now I'm flustered. I'll go rearrange something."  ✏ `dlg.story.notes.tilda.r1`   (expr happy)
    r2 [tilda]: "A note, for me? ...I'm keeping it. In a drawer I've just invented."  ✏ `dlg.story.notes.tilda.r2`   (expr happy)
  [tilda]: "Signing a kindness is just sending a bill. ...Two more doors before closing. Don't follow me."  ✏ `event.story_notes_tilda.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_notes_tilda, friend:tilda,40, flag:storydone.anonymous_notes

### story_scarecrows · "The Scarecrow Contest" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:rival_scarecrows && season:fall && weekday:sat && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: tilda(25, 18), player(25, 17)
  · (place tilda to (25,18))
  · (tilda faces down)
  · (spawn prop prop.scarecrow at (26,18))
  [tilda]: "[player]! You're judging. Be completely fair, by which I mean pick mine."  ✏ `event.story_scarecrows.intro`
  · (move player to (25,17))
  · (player faces up)
  · (tilda emote exclaim)
  · (tilda point (background))
  ▶ DIALOGUE `story.scarecrow.talk` (player-facing graph, plays here):
    n0 [tilda]: "Mine is smiling. Crows can't stand a cheerful face. That's simply known."  ✏ `dlg.story.scarecrow.talk.0`   (→ n1)
    n1 [dorian]: "Mine says nothing. Crows hate that. I watched them for a week."   (→ n2)
    n2 [tilda]: "Well? Whose scarecrow is guarding the fields this fall?"  ✏ `dlg.story.scarecrow.talk.2`
       ? "Tilda's, obviously." → r0  effects: flag:choice.scarecrow.tilda  ✏ `dlg.story.scarecrow.talk.c0`
       ? "Dorian's. Sorry, Tilda." → r1  effects: flag:choice.scarecrow.dorian  ✏ `dlg.story.scarecrow.talk.c1`
       ? "It's a tie." → r2  effects: flag:choice.scarecrow.tie  ✏ `dlg.story.scarecrow.talk.c2`
       ? "Let the crows decide." → r3  effects: flag:choice.scarecrow.crows  ✏ `dlg.story.scarecrow.talk.c3`
    r0 [tilda]: "Ha! Taste! Take the runner-up home. It'll stand in your field being thoughtful at the crows."  ✏ `dlg.story.scarecrow.talk.r0`   (expr happy)
    r1 [dorian]: "...Mine. Oh. Take the runner-up home. It doesn't talk either."   (expr happy)
    r2 [tilda]: "A tie. Coward. Take one anyway, and we'll both be wounded together."  ✏ `dlg.story.scarecrow.talk.r2`   (emote sweat)
    r3 [dorian]: "The crows. ...They sat on both. Equally. A tie, then. Take one."   (expr thinking)
  [tilda]: "{if choice:scarecrow.tilda}I won. I'm not smug. I'm radiant. Rematch next fall.{else}{if choice:scarecrow.dorian}He won. I'll say it once, quietly. Rematch next fall.{else}{if choice:scarecrow.tie}A tie. I'm not speaking to you, with love. Rematch next fall.{else}The crows chose both. Wise birds. They're on the slate for diplomacy.{/if}{/if}{/if}"  ✏ `event.story_scarecrows.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_scarecrows, give:machine.scarecrow,1, friend:tilda,20, friend:dorian,20, flag:storydone.rival_scarecrows

### story_pumpkin_dorian · "The Missing Pumpkin" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:missing_pumpkin.dorian && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: dorian(25, 18), player(25, 17)
  · (place dorian to (25,18))
  · (dorian faces down)
  · (spawn prop prop.pumpkin at (26,18))
  [dorian]: "...It was the right size. For spores. I meant to ask. Then I didn't."
  · (move player to (25,17))
  · (player faces up)
  · (dorian emote sweat)
  · (dorian shrug (background))
  ▶ DIALOGUE `story.pumpkin.dorian` (player-facing graph, plays here):
    n0 [narration]: "Behind the store sits a very large pumpkin. Dorian wears a very guilty face."   (→ n1)
    n1 [tilda]: "Dorian. Is that my prize pumpkin? I've been blaming the goats all week, out loud!"  ✏ `dlg.story.pumpkin.dorian.1`   (→ n2)
    n2 [dorian]: "...Borrowed. The mushrooms needed a roof. It was coming back."
       ? "Let him off gently." → r0  effects: flag:choice.pumpkin.dorian.kind, friend:dorian,10
       ? "Ask for a proper apology." → r1  effects: flag:choice.pumpkin.dorian.apology
       ? "Grab the heavy end. Carry it back." → r2  effects: flag:choice.pumpkin.dorian.help, friend:tilda,10
    r0 [tilda]: "Kindly, then. But he's on the slate for a pie. Mushrooms optional."  ✏ `dlg.story.pumpkin.dorian.r0`   (expr thinking)
    r1 [dorian]: "Sorry, Tilda. Truly. ...Sorry, pumpkin. You were very patient."   (emote sweat)
    r2 [tilda]: "You take the heavy end, Dorian takes the guilt. I'll carry the conversation."  ✏ `dlg.story.pumpkin.dorian.r2`   (expr happy)
  [dorian]: "Next fall I'll grow my own. Tilda may forgive me. The pumpkin won't."
  · (remove prop story_prop_0)
  · effects: flag:event.story_pumpkin_dorian, friend:dorian,30, friend:tilda,20, flag:storydone.missing_pumpkin

### story_pumpkin_marcus · "The Missing Pumpkin" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:missing_pumpkin.marcus && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(25, 18), player(25, 17), tilda(24, 18)
  · (place marcus to (25,18))
  · (marcus faces down)
  · (spawn prop prop.pumpkin at (26,18))
  [marcus]: "You've caught me. A practice lantern. I measured the face. I never asked whose pumpkin."
  · (move player to (25,17))
  · (player faces up)
  · (marcus emote sweat)
  · (marcus shrug (background))
  · (place tilda to (24,18))
  · (tilda faces right)
  ▶ DIALOGUE `story.pumpkin.marcus` (player-facing graph, plays here):
    n0 [narration]: "Tilda comes around the corner, stops, and stares at the pumpkin's new face."   (→ n1)
    n1 [tilda]: "Marcus! Is that my prize pumpkin? I blamed the goats. I apologized to a goat."  ✏ `dlg.story.pumpkin.marcus.1`   (→ n2)
    n2 [marcus]: "...It might be. It has a nose now."
       ? "Let him off gently." → r0  effects: flag:choice.pumpkin.marcus.kind, friend:marcus,10
       ? "He owes a proper apology." → r1  effects: flag:choice.pumpkin.marcus.apology
       ? "Help carry the evidence back." → r2  effects: flag:choice.pumpkin.marcus.help, friend:tilda,10
    r0 [tilda]: "Kindly. Fine. But he owes me a pie, and I've seen him measure. It'll be exact."  ✏ `dlg.story.pumpkin.marcus.r0`   (expr thinking)
    r1 [marcus]: "An apology. Yes. I'm sorry, Tilda. Truly. And sorry to you, pumpkin. You deserved a plan."   (emote sweat)
    r2 [tilda]: "You and Marcus take the ends. I'll walk ahead and tell everyone it was the goats."  ✏ `dlg.story.pumpkin.marcus.r2`   (expr happy)
  [marcus]: "It goes back with a face carved in. Tilda calls it an improvement. Next year, I ask first. Probably."
  · (remove prop story_prop_0)
  · effects: flag:event.story_pumpkin_marcus, friend:marcus,30, friend:tilda,20, flag:storydone.missing_pumpkin

### story_pumpkin_felix · "The Missing Pumpkin" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:missing_pumpkin.felix && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: felix(25, 18), player(25, 17)
  · (place felix to (25,18))
  · (felix faces down)
  · (spawn prop prop.pumpkin at (26,18))
  [felix]: "Ah. Caught. I only borrowed it. I wanted it beside my biggest carp. For scale."
  · (move player to (25,17))
  · (player faces up)
  · (felix emote sweat)
  · (felix shrug (background))
  ▶ DIALOGUE `story.pumpkin.felix` (player-facing graph, plays here):
    n0 [narration]: "Behind the store, Felix stands beside a very large pumpkin, wearing a very guilty face."   (→ n1)
    n1 [tilda]: "Felix. Is that my prize pumpkin? I blamed the goats! You don't even like vegetables!"  ✏ `dlg.story.pumpkin.felix.1`   (→ n2)
    n2 [felix]: "...It might be. In my defense, I didn't eat it. I only admired it."
       ? "Let Felix off kindly." → r0  effects: flag:choice.pumpkin.felix.kind, friend:felix,10
       ? "Ask for a proper apology." → r1  effects: flag:choice.pumpkin.felix.apology
       ? "Offer to carry the heavy end." → r2  effects: flag:choice.pumpkin.felix.help, friend:tilda,10
    r0 [tilda]: "Kindly. Fine. He owes me a pie, and if there's a fish in it, I'll know."  ✏ `dlg.story.pumpkin.felix.r0`   (expr thinking)
    r1 [felix]: "An apology. Yes. I'm sorry, Tilda. Truly. ...And sorry to you, pumpkin."   (emote sweat)
    r2 [tilda]: "You take the heavy end. Felix can carry the excuses. They weigh more."  ✏ `dlg.story.pumpkin.felix.r2`   (expr happy)
  [felix]: "Back it goes. Beside it, my biggest carp looked like bait. I'll not be telling that story."
  · (remove prop story_prop_0)
  · effects: flag:event.story_pumpkin_felix, friend:felix,30, friend:tilda,20, flag:storydone.missing_pumpkin

### festival_winter_lanterns · "The Lantern Release" · wholesome
event · wave7_festival_moments.json · map Village · priority 6 · repeatable
plays when: `season:winter && day==25 && flag:festival.winter.joined && unseen:festival.winter.lanterns && hour>=17`
beats may be ADDED or CUT in this scene
placed: hazel(23, 18), ione(24, 18), tilda(26, 18), felix(27, 18)
  · (place hazel to (23,18))
  · (hazel faces up)
  · (place ione to (24,18))
  · (ione faces up)
  · (place tilda to (26,18))
  · (tilda faces up)
  · (place felix to (27,18))
  · (felix faces up)
  · (spawn prop prop.lantern at (23,17))
  · (spawn prop prop.lantern at (24,17))
  · (spawn prop prop.lantern at (26,17))
  · (spawn prop prop.lantern at (27,17))
  [tilda]: "Lanterns up, everyone. Not lit yet. Wait for the bell. Felix, I mean you."  ✏ `event.festival_winter_lanterns.0`
  [hazel]: "One lantern, one wish. Please keep yours folded up. (Last winter four people said theirs aloud.)"
  · (hazel emote sparkle)
  · (sfx lantern)
  [felix]: "My wish is this big. Last winter it was this big. Wishes grow, like fish."
  [ione]: "That's the bell. Together now, and slowly. Let them go. ...Mine is drifting toward the sea. I didn't steer it."
  · (ione emote heart)
  · (remove prop story_prop_3)
  · (remove prop story_prop_2)
  · (remove prop story_prop_1)
  · (remove prop story_prop_0)
  · effects: mark:festival.winter.lanterns, flag:event.festival_winter_lanterns, friend:hazel,10, friend:ione,10, friend:tilda,10, friend:felix,10

### tilda_heart4 · "The Slate" · wholesome
conversation · wave2_events.json · map GeneralStore · priority 7
plays when: `hearts:tilda>=4 && flag:event.tilda_heart2 && !weekday:sun && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Come around the counter. Nobody comes back here but me. Mind the cat, it's on duty."  ✏ `event.tilda_heart4.intro`
  · (move player to (7,4))
  · (player faces up)
  · (tilda wave (background))
  · (tilda emote ellipsis)
  ▶ DIALOGUE `tilda.slate` (player-facing graph, plays here):
    n0 [tilda]: "This is the slate. Every small kindness in Wetherell, in chalk. Marcus is two planks ahead of Odalys. She doesn't know yet."  ✏ `dlg.tilda.slate.0`   (→ n1)
    n1 [tilda]: "And this blank bit is yours. What shall I write under your name?"  ✏ `dlg.tilda.slate.1`
       ? "A thank-you, in big letters." → r0  effects: friend:tilda,10, flag:choice.slate.thanks  ✏ `dlg.tilda.slate.c0`
       ? "A joke. A good one." → r1  effects: friend:tilda,10, flag:choice.slate.joke  ✏ `dlg.tilda.slate.c1`
       ? "A promise. I'll keep it." → r2  effects: friend:tilda,15, flag:choice.slate.promise  ✏ `dlg.tilda.slate.c2`
    r0 [tilda]: "A thank-you. Hard to cash, and I'll never spend it. My favorite currency."  ✏ `dlg.tilda.slate.r0`   (expr happy)
    r1 [tilda]: "A joke! On my slate! ...Oh, that's good. Don't tell anyone I laughed in the shop."  ✏ `dlg.tilda.slate.r1`   (expr happy)
    r2 [tilda]: "A promise. I'll write it small. I'll hold it large."  ✏ `dlg.tilda.slate.r2`
  [tilda]: "{if choice:slate.thanks}A thank-you, in chalk. I'll never wipe it, so don't make me.{else}{if choice:slate.joke}A joke on the slate. Nobody's dared. Wren will be furious she didn't think of it.{else}A promise. I hold everyone to everything, kindly. Mostly kindly.{/if}{/if} Now you're on it, you have to come back."  ✏ `event.tilda_heart4.after`
  · effects: flag:event.tilda_heart4
  · effects: friend:tilda,50

### tilda_heart6 · "After Hours" · surprise
confession · wave2_events.json · map GeneralStore · priority 7
plays when: `hearts:tilda>=6 && flag:event.tilda_heart5 && flag:event.tilda_heart4 && !weekday:sun && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Turn the sign to Closed, would you? Just for an hour. I do the books when it's quiet. Your chair's free."  ✏ `event.tilda_heart6.intro`
  · (move player to (7,4))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on tilda)
  · (tilda expression thinking)
  ▶ DIALOGUE `tilda.hours` (player-facing graph, plays here):
    n0 [tilda]: "I count the stock at closing time. Lately the count comes out right and the street comes out short."  ✏ `dlg.tilda.hours.0`   (→ n1; expr thinking)
    n1 [tilda]: "I tell myself Wetherell isn't shrinking. Then the bell goes four times before lunch. What do you think?"  ✏ `dlg.tilda.hours.1`
       ? "It isn't shrinking. I'm staying." → r0  effects: friend:tilda,10, flag:choice.hours.reassure  ✏ `dlg.tilda.hours.c0`
       ? "Let's fill the shop together." → r1  effects: friend:tilda,15, flag:choice.hours.plan  ✏ `dlg.tilda.hours.c1`
       ? "Count again, then. Gently." → r2  effects: friend:tilda,10, flag:choice.hours.joke  ✏ `dlg.tilda.hours.c2`
    r0 [tilda]: "Staying. ...Say it once more, slower. I'd like it on the slate in your handwriting."  ✏ `dlg.tilda.hours.r0`
    r1 [tilda]: "Together. Now that's a plan I'd stock. Bring ideas. I'll bring the tea, and the good cups."  ✏ `dlg.tilda.hours.r1`   (expr happy)
    r2 [tilda]: "Gently. Recounting with kindness. I'll try it. The cat can check my sums."  ✏ `dlg.tilda.hours.r2`
  · (tilda expression neutral)
  [tilda]: "Said out loud, it weighs less. Turn the sign back, would you? And not a word to Felix. He'd worry, then bring fish."  ✏ `event.tilda_heart6.after`
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.tilda_heart6
  · effects: friend:tilda,60

### tilda_heart8 · "The Recipe Card" · surprise
conversation · wave2_events.json · map GeneralStore · priority 7
plays when: `hearts:tilda>=8 && flag:event.tilda_heart6 && !weekday:sun && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Yearly stocktake, and I need strong arms. Shift the flour? There's a tin back there I've been carefully not looking at."  ✏ `event.tilda_heart8.intro`
  · (move player to (7,4))
  · (player faces up)
  · (tilda wave (background))
  · (tilda emote ellipsis)
  ▶ DIALOGUE `tilda.card` (player-facing graph, plays here):
    n0 [narration]: "Behind the flour sacks, in a very old tin, lies a recipe card in faded ink."  ✏ `dlg.tilda.card.0`   (→ n1)
    n1 [tilda]: "My mother's pumpkin pie. The only thing she ever wrote down. I've never baked it. What if I get it wrong?"  ✏ `dlg.tilda.card.1`
       ? "Bake it. I'll stand right there." → r0  effects: friend:tilda,10, flag:choice.card.bake  ✏ `dlg.tilda.card.c0`
       ? "Keep it safe. It's enough." → r1  effects: friend:tilda,10, flag:choice.card.keep  ✏ `dlg.tilda.card.c1`
       ? "Share it. The pie feud can cope." → r2  effects: friend:tilda,15, flag:choice.card.share  ✏ `dlg.tilda.card.c2`
    r0 [tilda]: "Bake it. Yes. With you by the oven, so I can't back out."  ✏ `dlg.tilda.card.r0`   (expr happy)
    r1 [tilda]: "Safe. It's been safe for forty years. That's not a bad record."  ✏ `dlg.tilda.card.r1`
    r2 [tilda]: "The whole village. ...The pie feud will be livid. I love it."  ✏ `dlg.tilda.card.r2`   (expr happy)
  [tilda]: "{if choice:card.bake}Flour, butter, and her handwriting. Saturday, before opening. Don't be late, or I'll lose my nerve.{else}{if choice:card.keep}Safe, then. A card doesn't need to be a pie to matter. Back in the tin.{else}Sharing it, then. A copy for every kitchen. Mother would have scolded me, then smiled.{/if}{/if}"  ✏ `event.tilda_heart8.after`
  · effects: flag:event.tilda_heart8
  · effects: friend:tilda,70

### tilda_heart10 · "The Sign" · wholesome
heirloom · wave2_events.json · map GeneralStore · priority 7
plays when: `hearts:tilda>=10 && flag:event.tilda_heart8 && !weekday:sun && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Close your eyes. No, really. I've done something to the front of the shop and I can't watch you see it."  ✏ `event.tilda_heart10.intro`
  · (move player to (7,4))
  · (player faces up)
  · (spawn prop food.pumpkin_pie at (7,5))
  · (tilda emote heart)
  [tilda]: "Pumpkin pie, still warm. Take it. And yes, that's your farm on my sign. Supplier. In paint, not chalk."  ✏ `event.tilda_heart10.give`
  · (sfx coin)
  · effects: give:food.pumpkin_pie,1, flag:event.tilda_heart10
  · (remove prop heirloom)
  [tilda]: "{if choice:card.share}I copied the recipe for every kitchen, and put your name at the bottom. In chalk, so it can grow.{else}{if choice:card.bake}That Saturday you stood by the oven so I couldn't back out. This is the second one. Easier.{else}The card's still in its tin, where it's happiest. This one's from memory. Close enough to count.{/if}{/if}"  ✏ `event.tilda_heart10.thanks`
  · effects: friend:tilda,80

### tilda_friend · "Stocktake Saturday" · funny
helping_scene · wave2_events.json · map GeneralStore · priority 7 · repeatable
plays when: `!flag:event.tilda_friend && hearts:tilda>=3 && weekday:sat && !weekday:sun && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Saturday stocktake, and the window's three sunflowers short of finished. Have you any?"  ✏ `event.tilda_friend.ask`
  · (move player to (7,4))
  · (player faces up)
  · (tilda emote question)
  [tilda]: "Three sunflowers, if you find them. The window's half dressed and mortified."  ✏ `event.tilda_friend.fallback`
  [tilda]: "Three sunflowers! The window looks like noon. You're on the slate, under 'saved the display.'"  ✏ `event.tilda_friend.success`
  · effects: flag:event.tilda_friend
  · effects: friend:tilda,15

### tilda_birthday · "Tilda's Birthday" · wholesome
conversation · wave4_birthdays.json · map GeneralStore · priority 8 · repeatable
plays when: `hearts:tilda>=2 && birthday.in:tilda==0 && unseen:birthday.tilda && !weekday:sun && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "It's my birthday. I don't make a fuss, I make a large pie. Come in, and whatever you do, don't read the slate."  ✏ `event.tilda_birthday.intro`
  · (move player to (7,4))
  · (player faces up)
  · (tilda wave (background))
  · (tilda emote heart)
  ▶ DIALOGUE `tilda.birthday` (player-facing graph, plays here):
    n0 [tilda]: "You looked at the slate. I saw. Go on, then. What do you say?"  ✏ `dlg.tilda.birthday.0`
       ? "Happy birthday, Tilda." → r0  effects: friend:tilda,10, flag:choice.bday.tilda.wish  ✏ `dlg.tilda.birthday.c0`
       ? "Give her a flower." → r1  effects: friend:tilda,10, flag:choice.bday.tilda.flower  ✏ `dlg.tilda.birthday.c1`
       ? "Mind the shop. Go read." → r2  effects: friend:tilda,10, flag:choice.bday.tilda.mind  ✏ `dlg.tilda.birthday.c2`
    r0 [tilda]: "Oh. Well. That goes at the top of the slate, where I'll see it first thing every morning."  ✏ `dlg.tilda.birthday.r0`   (expr happy)
    r1 [tilda]: "A flower, in my shop! It goes in the window, beside the good pumpkin."  ✏ `dlg.tilda.birthday.r1`   (expr happy)
    r2 [tilda]: "You'd mind it? That's the best present. Take the stool. The cat bites coins, not customers."  ✏ `dlg.tilda.birthday.r2`   (expr happy)
  [tilda]: "{if choice:bday.tilda.wish}Top of the slate. It reads very well from the street.{else}{if choice:bday.tilda.flower}The flower's in the window. Half the village asked who sent it. I said an account in good standing.{else}An hour off! I read a whole chapter upstairs. The shop survived. I'm slightly offended.{/if}{/if}"  ✏ `event.tilda_birthday.after`
  · effects: flag:event.tilda_birthday
  · effects: mark:birthday.tilda, friend:tilda,30, give:food.pumpkin_pie,1, mail:tilda_bday_thanks

### overheard_tilda_felix_1 · "Fish Prices" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.felix && hearts:tilda>=2 && hearts:felix>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), felix(26, 18)
  · (place tilda to (24,18))
  · (place felix to (26,18))
  · (tilda faces right)
  · (felix faces left)
  [tilda]: "Felix. Your carp is eight coins today. On Monday it was six."  ✏ `event.overheard_tilda_felix_1.l1`
  [felix]: "It's a bigger carp. It grew. In the stall, overnight, out of pride."
  · (felix shrug (background))
  · (felix emote ellipsis)
  · (tilda point (background))
  [tilda]: "It's been in a bucket since dawn. It hasn't grown. Your prices have."  ✏ `event.overheard_tilda_felix_1.l3`
  [felix]: "Fine. Six. The story's two more. It's a very good story."
  · effects: flag:event.overheard_tilda_felix_1

### overheard_tilda_felix_2 · "The Oldest Friend" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.felix && hearts:tilda>=4 && hearts:felix>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_felix_1`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), tilda(26, 18)
  · (place felix to (24,18))
  · (place tilda to (26,18))
  · (felix faces right)
  · (tilda faces left)
  [felix]: "Tilda. Remember the winter the pond froze, and I fell in?"
  [tilda]: "I remember the winter you fell in. And the Tuesday you fell in again."  ✏ `event.overheard_tilda_felix_2.l2`
  · (tilda shrug (background))
  · (tilda emote ellipsis)
  · (felix wave (background))
  [felix]: "Same winter. A long one. You brought me soup. ...Both times."
  [tilda]: "Soup, a blanket, and 'Thin Ice' on the slate in capitals. You walked straight past it."  ✏ `event.overheard_tilda_felix_2.l4`
  · effects: flag:event.overheard_tilda_felix_2

### overheard_tilda_felix_3 · "Slate and Line" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.felix && hearts:tilda>=6 && hearts:felix>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_felix_2`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), tilda(26, 18)
  · (place felix to (24,18))
  · (place tilda to (26,18))
  · (felix faces right)
  · (tilda faces left)
  [felix]: "Tilda. I chalked the year's first fish on your slate. Capital letters."
  [tilda]: "Oh, Felix. A very good fish. It stays up until the thaw."  ✏ `event.overheard_tilda_felix_3.l2`
  · (tilda point (background))
  · (tilda emote ellipsis)
  · (felix shrug (background))
  [felix]: "I chalked it life-size. Then I added a hand. Don't tell the pond."
  [tilda]: "The pond always knows. Sit down, Felix. I've made tea for two again."  ✏ `event.overheard_tilda_felix_3.l4`
  · effects: flag:event.overheard_tilda_felix_3

### overheard_tilda_felix_4 · "The Last Pie" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.felix && hearts:tilda>=8 && hearts:felix>=8 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_felix_3`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), tilda(26, 18)
  · (place felix to (24,18))
  · (place tilda to (26,18))
  · (felix faces right)
  · (tilda faces left)
  [felix]: "Tilda. I brought a pie, for once. A fish pie. Don't judge it."
  [tilda]: "I'll judge it kindly, Felix. Then I'll salt it."  ✏ `event.overheard_tilda_felix_4.l2`
  · (tilda wave (background))
  · (tilda emote ellipsis)
  · (felix wave (background))
  [felix]: "A fair sentence. Pass the fork. ...And the salt."
  [tilda]: "To old friends, dear. The crust is mine. The fish goes to the cat."  ✏ `event.overheard_tilda_felix_4.l4`
  · effects: flag:event.overheard_tilda_felix_4

### overheard_tilda_wren_1 · "Slate and Chalkboard" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.wren && hearts:tilda>=2 && hearts:wren>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), wren(26, 18)
  · (place tilda to (24,18))
  · (place wren to (26,18))
  · (tilda faces right)
  · (wren faces left)
  [tilda]: "Wren. Your chalkboard looks remarkably like my slate."  ✏ `event.overheard_tilda_wren_1.l1`
  [wren]: "Imitation is flattery, Tilda, with better lighting."
  · (wren shrug (background))
  · (wren emote note)
  · (tilda point (background))
  [tilda]: "Flattery earns a ribbon. Imitation earns a bill."  ✏ `event.overheard_tilda_wren_1.l3`
  [wren]: "A ribbon! Pin it to the stool."
  · effects: flag:event.overheard_tilda_wren_1

### overheard_tilda_wren_2 · "Gossip Exchange" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.wren && hearts:tilda>=4 && hearts:wren>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_wren_1`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), wren(26, 18)
  · (place tilda to (24,18))
  · (place wren to (26,18))
  · (tilda faces right)
  · (wren faces left)
  [tilda]: "I've heard something. About a certain cat, and a certain windowsill."  ✏ `event.overheard_tilda_wren_2.l1`
  [wren]: "I heard it first. Three days ago. It's grown since."
  · (wren shrug (background))
  · (wren emote note)
  · (tilda wave (background))
  [tilda]: "Then we'll trade. Yours first. I have a reputation to protect."  ✏ `event.overheard_tilda_wren_2.l3`
  [wren]: "Mine's better. It has a pickle in it."
  · effects: flag:event.overheard_tilda_wren_2

### overheard_tilda_wren_3 · "Closing the Books" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.wren && hearts:tilda>=6 && hearts:wren>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_wren_2`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), wren(26, 18)
  · (place tilda to (24,18))
  · (place wren to (26,18))
  · (tilda faces right)
  · (wren faces left)
  [tilda]: "Six days at that counter this week. Save me a seat later?"  ✏ `event.overheard_tilda_wren_3.l1`
  [wren]: "You've earned the good stool, Tilda. Come at seven. Cider's on me."
  · (wren point (background))
  · (wren emote note)
  · (tilda shrug (background))
  [tilda]: "One cup. Then back to the slate."  ✏ `event.overheard_tilda_wren_3.l3`
  [wren]: "The slate will wait. It's good at that."
  · effects: flag:event.overheard_tilda_wren_3

### overheard_tilda_juno_1 · "Paying the Slate" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.juno && hearts:tilda>=2 && hearts:juno>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), tilda(26, 18)
  · (place juno to (24,18))
  · (place tilda to (26,18))
  · (juno faces right)
  · (tilda faces left)
  [juno]: "Tilda! Slate payment! Three clasps and one hinge with a smiley face stamped on."
  [tilda]: "Three clasps is two months of bread. The hinge is a tip. I'll put it on the slate."  ✏ `event.overheard_tilda_juno_1.l2`
  · (tilda shrug (background))
  · (tilda emote sweat)
  · (juno wave (background))
  [juno]: "Is the smiley face worth extra? It took me an hour."
  [tilda]: "The smiley face is priceless. That's why I can't charge for it."  ✏ `event.overheard_tilda_juno_1.l4`
  · effects: flag:event.overheard_tilda_juno_1

### overheard_tilda_juno_2 · "The Flour Sack" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.juno && hearts:tilda>=4 && hearts:juno>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_juno_1`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), juno(26, 18)
  · (place tilda to (24,18))
  · (place juno to (26,18))
  · (tilda faces right)
  · (juno faces left)
  [tilda]: "You've grown, Juno. You used to stand under the jar of hooks and glare at it."  ✏ `event.overheard_tilda_juno_2.l1`
  [juno]: "I still can't! I stand on the flour sack now."
  · (juno shrug (background))
  · (juno emote heart)
  · (tilda point (background))
  [tilda]: "I know. I put the sack there your first week, years ago. Nobody's moved it since."  ✏ `event.overheard_tilda_juno_2.l3`
  [juno]: "Years? Tilda, that's going on my slate. Under 'Owed (Forever)'."
  · effects: flag:event.overheard_tilda_juno_2

### overheard_tilda_juno_3 · "Next Door" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.juno && hearts:tilda>=6 && hearts:juno>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_juno_2 && flag:event.juno_heart6`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), tilda(26, 18)
  · (place juno to (24,18))
  · (place tilda to (26,18))
  · (juno faces right)
  · (tilda faces left)
  [juno]: "Tilda... if I ever had my own forge, a small one, would you still feed me?"
  [tilda]: "Open one in Wetherell and I'll stock your first shelf. Go farther, and I'll post you pie."  ✏ `event.overheard_tilda_juno_3.l2`
  · (tilda point (background))
  · (tilda emote ellipsis)
  · (juno shrug (background))
  [juno]: "In Wetherell. Definitely. Next door to Bram. Probably. Okay, definitely."
  [tilda]: "Good. This street could use more noise."  ✏ `event.overheard_tilda_juno_3.l4`
  · effects: flag:event.overheard_tilda_juno_3

### overheard_tilda_bram_1 · "The Even Slate" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.bram && hearts:tilda>=3 && hearts:bram>=3 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), bram(26, 18)
  · (place tilda to (24,18))
  · (place bram to (26,18))
  · (tilda faces right)
  · (bram faces left)
  [tilda]: "Bram. There's a pie crust on your bench. For the hinges, the lamp, and the shelf."  ✏ `event.overheard_tilda_bram_1.l1`
  [bram]: "Hm. Hinges were squeaking. Bothered me. That's all."
  · (bram shrug (background))
  · (bram emote ellipsis)
  · (tilda point (background))
  [tilda]: "You never say thank you. I never say you're welcome. That's how the slate stays even."  ✏ `event.overheard_tilda_bram_1.l3`
  [bram]: "...Hm. Even. Good."
  · effects: flag:event.overheard_tilda_bram_1
