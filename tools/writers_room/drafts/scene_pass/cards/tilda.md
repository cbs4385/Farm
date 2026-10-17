# Scene cards for tilda

26 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 48 spoken lines in scenes, 37 dialogue lines and 19 player choice labels.

### tilda_heart5 · "Too Many Scones" · funny
event · npc_roster_core.json · map GeneralStore · priority 6
plays when: `hearts:tilda>=5 && !weekday:sun && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Sit a moment, [player]. I made too many scones."  ✏ `event.tilda_heart5.0`
  · (move player to (7,4))
  · (player faces up)
  · (tilda emote ellipsis)
  [tilda]: "I've never had a neighbor who ate my cooking without flinching."  ✏ `event.tilda_heart5.1`
  [tilda]: "Take a few home."  ✏ `event.tilda_heart5.2`
  · effects: give:food.mashed_potato,2, friend:tilda,60, flag:event.tilda_heart5

### tilda_heart2 · "Mother's Gratin" · wholesome
event · quests_and_mail.json · map GeneralStore · priority 5
plays when: `hearts:tilda>=2 && !weekday:sun && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Oh, [player]! Just the person I wanted to see."  ✏ `event.tilda_heart2.0`
  · (move player to (7,4))
  · (player faces up)
  · (tilda emote ellipsis)
  [tilda]: "Here, a recipe my mother taught me. Cauliflower gratin. You've earned it."  ✏ `event.tilda_heart2.1`
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
  [marcus]: "I checked the roof once more. Then twice. It is finished."
  · (marcus emote sparkle)
  [wren]: "Look at this room! Full, and the porch is full too. Somebody count the porch, I've lost track of my own party."  (emote sparkle)
  · (spawn prop artisan.wine at (7,3))
  [wren]: "[player]. I said I'd open it on the night. This is the night. You're first at the bar."  (expr happy)
  · (remove prop wine)
  [tilda]: "The roof's done, the hall's open, and nobody asked for credit. I'm writing it on the slate anyway."  ✏ `event.hall_dance.3`
  [piper]: "First verse, then I step off. I'm calm. I'm holding this fiddle like a hot loaf."  (emote sweat)
  · (music piper_theme)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall sings the chorus. Piper has stopped playing, and nobody minds."
  [bram]: "Hm. Hm-hm. I know the tune. Say nothing."  (emote note)
  [juno]: "The hall door hangs on hinges I made. My mark's on every pin, and it swings like it's showing off!"  (expr happy)
  [wren]: "The roof pool is closed. Seven winters of coins, and Tilda had the right year."
  [tilda]: "Into the hall box it goes. Marcus, take one coin for the ladder."  ✏ `event.hall_dance.10`
  [marcus]: "Hm. One coin. One dance. Nobody tell the ladder."
  · (marcus dance (background))
  · (tilda dance (background))
  · (wren emote heart)
  · (juno emote note)
  · (wait 2.5s)
  [narration]: "Marcus takes his one dance, and Tilda lets him lead. The lamps burn low, and nobody goes home."
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
  [tilda]: "Ah. Caught. I tuck them in with the change. A kind word weighs nothing."  ✏ `event.story_notes_tilda.intro`
  · (move player to (25,17))
  · (player faces up)
  · (tilda emote sparkle)
  · (tilda shrug (background))
  ▶ DIALOGUE `story.notes.tilda` (player-facing graph, plays here):
    n0 [narration]: "You catch Tilda slipping a folded note under a door. Neither of you moves."  ✏ `dlg.story.notes.tilda.0`   (→ n1)
    n1 [tilda]: "...Well. Now you know."  ✏ `dlg.story.notes.tilda.1`
       ? "Say nothing. Just smile." → r0  effects: flag:choice.notes.tilda.smile  ✏ `dlg.story.notes.tilda.c0`
       ? "Say thank you." → r1  effects: flag:choice.notes.tilda.thanks, friend:tilda,10  ✏ `dlg.story.notes.tilda.c1`
       ? "Leave a note back." → r2  effects: flag:choice.notes.tilda.note, friend:tilda,15  ✏ `dlg.story.notes.tilda.c2`
    r0 [tilda]: "...You smiled. That's a lot of kindness for one smile."  ✏ `dlg.story.notes.tilda.r0`   (expr embarrassed)
    r1 [tilda]: "Oh. Thank you. That's the whole reward. I'll go and be flustered somewhere."  ✏ `dlg.story.notes.tilda.r1`   (expr happy)
    r2 [tilda]: "A note, for me? ...I'm keeping it. In a drawer I've just invented."  ✏ `dlg.story.notes.tilda.r2`   (expr happy)
  [tilda]: "I don't mean to be mysterious. It's just that signing them felt like showing off."  ✏ `event.story_notes_tilda.after`
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
  [tilda]: "[player]! Just in time. You are my judge. Please be fair, which means pick mine."  ✏ `event.story_scarecrows.intro`
  · (move player to (25,17))
  · (player faces up)
  · (tilda emote exclaim)
  · (tilda point (background))
  ▶ DIALOGUE `story.scarecrow.talk` (player-facing graph, plays here):
    n0 [tilda]: "Mine has a smile. Smiles scare crows. That is simply known."  ✏ `dlg.story.scarecrow.talk.0`   (→ n1)
    n1 [dorian]: "Mine is quiet. Quiet is unsettling. I did my research."   (→ n2)
    n2 [tilda]: "Well? Which scarecrow rules the fields?"  ✏ `dlg.story.scarecrow.talk.2`
       ? "Tilda's." → r0  effects: flag:choice.scarecrow.tilda  ✏ `dlg.story.scarecrow.talk.c0`
       ? "Dorian's." → r1  effects: flag:choice.scarecrow.dorian  ✏ `dlg.story.scarecrow.talk.c1`
       ? "A tie." → r2  effects: flag:choice.scarecrow.tie  ✏ `dlg.story.scarecrow.talk.c2`
       ? "Ask the crows." → r3  effects: flag:choice.scarecrow.crows  ✏ `dlg.story.scarecrow.talk.c3`
    r0 [tilda]: "Ha! A person of taste. Take this one home. It's the runner-up. It's still excellent."  ✏ `dlg.story.scarecrow.talk.r0`   (expr happy)
    r1 [dorian]: "...Oh. Thank you. Take the runner-up. It's very quiet company."   (expr happy)
    r2 [tilda]: "A tie. You coward. Take one anyway. Both of us will be wounded."  ✏ `dlg.story.scarecrow.talk.r2`   (emote sweat)
    r3 [dorian]: "The crows? ...Brave. They landed on both. A tie, then. Take this."   (expr thinking)
  [tilda]: "{if choice:scarecrow.tilda}I won. I'm not smug. I'm radiant.{else}{if choice:scarecrow.dorian}He won. I'll say it once. Never again.{else}{if choice:scarecrow.tie}A tie. I'm not speaking to you, with love.{else}The crows chose. Wisely. They're furious with both of us.{/if}{/if}{/if}"  ✏ `event.story_scarecrows.after`
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
  [dorian]: "...The pumpkin was perfect for a mushroom experiment. I meant to ask. I forgot to ask."
  · (move player to (25,17))
  · (player faces up)
  · (dorian emote sweat)
  · (dorian shrug (background))
  ▶ DIALOGUE `story.pumpkin.dorian` (player-facing graph, plays here):
    n0 [narration]: "You find Dorian behind the store, with a very large pumpkin and a very guilty face."   (→ n1)
    n1 [tilda]: "Is that my prize pumpkin? The one the whole village has been blaming the goats for?"  ✏ `dlg.story.pumpkin.dorian.1`   (→ n2)
    n2 [dorian]: "...It was for scale. Next to the carp."
       ? "Let them off kindly." → r0  effects: flag:choice.pumpkin.dorian.kind, friend:dorian,10
       ? "Insist on a proper apology." → r1  effects: flag:choice.pumpkin.dorian.apology
       ? "Offer to help carry it back." → r2  effects: flag:choice.pumpkin.dorian.help, friend:tilda,10
    r0 [tilda]: "Kindly? Fine. Kindly. But he's on the slate for a pie."  ✏ `dlg.story.pumpkin.dorian.r0`   (expr thinking)
    r1 [dorian]: "An apology. Yes. Very sorry. Truly. Pumpkin, I'm also sorry to you."   (emote sweat)
    r2 [tilda]: "You carry the heavy end. I'll supervise. We'll call it civic duty."  ✏ `dlg.story.pumpkin.dorian.r2`   (expr happy)
  [dorian]: "He's returning it with a mushroom stew inside. Tilda forgives him on the spot. The pumpkin does not."
  · (remove prop story_prop_0)
  · effects: flag:event.story_pumpkin_dorian, friend:dorian,30, friend:tilda,20, flag:storydone.missing_pumpkin

### story_pumpkin_marcus · "The Missing Pumpkin" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:missing_pumpkin.marcus && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(25, 18), player(25, 17)
  · (place marcus to (25,18))
  · (marcus faces down)
  · (spawn prop prop.pumpkin at (26,18))
  [marcus]: "Ah. You've found out. I was carving a practice lantern. A very large practice lantern."
  · (move player to (25,17))
  · (player faces up)
  · (marcus emote sweat)
  · (marcus shrug (background))
  ▶ DIALOGUE `story.pumpkin.marcus` (player-facing graph, plays here):
    n0 [narration]: "You find Marcus behind the store, with a very large pumpkin and a very guilty face."   (→ n1)
    n1 [tilda]: "Is that my prize pumpkin? The one the whole village has been blaming the goats for?"  ✏ `dlg.story.pumpkin.marcus.1`   (→ n2)
    n2 [marcus]: "...It might be."
       ? "Let them off kindly." → r0  effects: flag:choice.pumpkin.marcus.kind, friend:marcus,10
       ? "Insist on a proper apology." → r1  effects: flag:choice.pumpkin.marcus.apology
       ? "Offer to help carry it back." → r2  effects: flag:choice.pumpkin.marcus.help, friend:tilda,10
    r0 [tilda]: "Kindly? Fine. Kindly. But he's on the slate for a pie."  ✏ `dlg.story.pumpkin.marcus.r0`   (expr thinking)
    r1 [marcus]: "An apology. Yes. Very sorry. Truly. Pumpkin, I'm also sorry to you."   (emote sweat)
    r2 [tilda]: "You carry the heavy end. I'll supervise. We'll call it civic duty."  ✏ `dlg.story.pumpkin.marcus.r2`   (expr happy)
  [marcus]: "He's returning it with a face carved in. Tilda says it's an improvement. Marcus will not stop smiling."
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
  [felix]: "Oh no. Don't tell the fish. I wanted a picture of it next to the biggest carp. For comparison."
  · (move player to (25,17))
  · (player faces up)
  · (felix emote sweat)
  · (felix shrug (background))
  ▶ DIALOGUE `story.pumpkin.felix` (player-facing graph, plays here):
    n0 [narration]: "You find Felix behind the store, with a very large pumpkin and a very guilty face."   (→ n1)
    n1 [tilda]: "Is that my prize pumpkin? The one the whole village has been blaming the goats for?"  ✏ `dlg.story.pumpkin.felix.1`   (→ n2)
    n2 [felix]: "...It might be."
       ? "Let them off kindly." → r0  effects: flag:choice.pumpkin.felix.kind, friend:felix,10
       ? "Insist on a proper apology." → r1  effects: flag:choice.pumpkin.felix.apology
       ? "Offer to help carry it back." → r2  effects: flag:choice.pumpkin.felix.help, friend:tilda,10
    r0 [tilda]: "Kindly? Fine. Kindly. But he's on the slate for a pie."  ✏ `dlg.story.pumpkin.felix.r0`   (expr thinking)
    r1 [felix]: "An apology. Yes. Very sorry. Truly. Pumpkin, I'm also sorry to you."   (emote sweat)
    r2 [tilda]: "You carry the heavy end. I'll supervise. We'll call it civic duty."  ✏ `dlg.story.pumpkin.felix.r2`   (expr happy)
  [felix]: "He's returning it with a photograph. Tilda frames it. The carp has not forgiven anyone."
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
  [tilda]: "Everyone has a lantern, dear. Hold it up. Don't light it yet. Wait for the bell."  ✏ `event.festival_winter_lanterns.0`
  [hazel]: "(One lantern each.) (A small light, and a small wish.) Please keep yours private."
  · (hazel emote sparkle)
  · (sfx lantern)
  [felix]: "It's this big. The wish, I mean. I'll say no more. The lantern will do the telling."
  [ione]: "Now. Together, and slowly. Let them go. The dark will do the rest."
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
  [tilda]: "Come around the counter. I want you to see something. Mind the cat."  ✏ `event.tilda_heart4.intro`
  · (move player to (7,4))
  · (player faces up)
  · (tilda wave (background))
  · (tilda emote ellipsis)
  ▶ DIALOGUE `tilda.slate` (player-facing graph, plays here):
    n0 [tilda]: "This is the slate. Every small kindness in Wetherell, in chalk. Look. Marcus is two planks ahead of Odalys."  ✏ `dlg.tilda.slate.0`   (→ n1)
    n1 [tilda]: "There's room under your name. What would you like me to write?"  ✏ `dlg.tilda.slate.1`
       ? "A thank-you." → r0  effects: friend:tilda,10, flag:choice.slate.thanks  ✏ `dlg.tilda.slate.c0`
       ? "A joke." → r1  effects: friend:tilda,10, flag:choice.slate.joke  ✏ `dlg.tilda.slate.c1`
       ? "A promise." → r2  effects: friend:tilda,15, flag:choice.slate.promise  ✏ `dlg.tilda.slate.c2`
    r0 [tilda]: "A thank-you. Plain, honest and very hard to cash. My favorite currency."  ✏ `dlg.tilda.slate.r0`   (expr happy)
    r1 [tilda]: "A joke! On my slate! ...Oh, that's good. I'm laughing in a shop. Don't tell anyone."  ✏ `dlg.tilda.slate.r1`   (expr happy)
    r2 [tilda]: "A promise. I'll write it small and keep it large."  ✏ `dlg.tilda.slate.r2`
  [tilda]: "{if choice:slate.thanks}A thank-you. In chalk. It looks very well on a slate.{else}{if choice:slate.joke}A joke on the slate! Nobody's ever dared. I'll leave it up.{else}A promise. I'll hold you to it. I hold everyone to everything, kindly.{/if}{/if}"  ✏ `event.tilda_heart4.after`
  · effects: flag:event.tilda_heart4
  · effects: friend:tilda,50

### tilda_heart6 · "After Hours" · surprise
confession · wave2_events.json · map GeneralStore · priority 7
plays when: `hearts:tilda>=6 && flag:event.tilda_heart5 && flag:event.tilda_heart4 && !weekday:sun && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Turn the sign to Closed, would you? I do the books when it's quiet. Sit."  ✏ `event.tilda_heart6.intro`
  · (move player to (7,4))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on tilda)
  · (tilda expression thinking)
  ▶ DIALOGUE `tilda.hours` (player-facing graph, plays here):
    n0 [tilda]: "I count the stock every night. And every night the street is quieter than it was. It's an odd thing for a shopkeeper to notice."  ✏ `dlg.tilda.hours.0`   (→ n1; expr thinking)
    n1 [tilda]: "I tell myself the village isn't shrinking. Then I count again. What do you think?"  ✏ `dlg.tilda.hours.1`
       ? "It isn't shrinking." → r0  effects: friend:tilda,10, flag:choice.hours.reassure  ✏ `dlg.tilda.hours.c0`
       ? "Let's fill the shop together." → r1  effects: friend:tilda,15, flag:choice.hours.plan  ✏ `dlg.tilda.hours.c1`
       ? "Count again, then. Gently." → r2  effects: friend:tilda,10, flag:choice.hours.joke  ✏ `dlg.tilda.hours.c2`
    r0 [tilda]: "It isn't. ...Say it again, slower. I'd like to put it on the slate in your handwriting."  ✏ `dlg.tilda.hours.r0`
    r1 [tilda]: "Together. Now that's a plan I'd stock. Bring ideas. And tea."  ✏ `dlg.tilda.hours.r1`   (expr happy)
    r2 [tilda]: "Gently. Yes. Recounting with kindness. I'll try that, and the cat can supervise."  ✏ `dlg.tilda.hours.r2`
  · (tilda expression neutral)
  [tilda]: "Thank you for letting me say it out loud. It weighs less than it did. Mind the step on the way out."  ✏ `event.tilda_heart6.after`
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
  [tilda]: "Stocktake day. Help me move the flour? There's something behind the shelf I've never had the nerve to look at."  ✏ `event.tilda_heart8.intro`
  · (move player to (7,4))
  · (player faces up)
  · (tilda wave (background))
  · (tilda emote ellipsis)
  ▶ DIALOGUE `tilda.card` (player-facing graph, plays here):
    n0 [narration]: "Behind the flour sacks, in a very old tin, lies a recipe card in faded ink."  ✏ `dlg.tilda.card.0`   (→ n1)
    n1 [tilda]: "My mother's pumpkin pie. I've never baked from it. I was afraid I'd get it wrong. What should I do?"  ✏ `dlg.tilda.card.1`
       ? "Bake it." → r0  effects: friend:tilda,10, flag:choice.card.bake  ✏ `dlg.tilda.card.c0`
       ? "Keep it safe." → r1  effects: friend:tilda,10, flag:choice.card.keep  ✏ `dlg.tilda.card.c1`
       ? "Share it with the village." → r2  effects: friend:tilda,15, flag:choice.card.share  ✏ `dlg.tilda.card.c2`
    r0 [tilda]: "Bake it. Yes. With you in the kitchen, so I can't back out."  ✏ `dlg.tilda.card.r0`   (expr happy)
    r1 [tilda]: "Safe. It's been safe for forty years. That's not a bad record."  ✏ `dlg.tilda.card.r1`
    r2 [tilda]: "The whole village. ...The pie feud would hate that. I love it."  ✏ `dlg.tilda.card.r2`   (expr happy)
  [tilda]: "{if choice:card.bake}Baking from it, then. Flour, butter and my mother's handwriting. I'll need a bigger oven and a smaller heart.{else}{if choice:card.keep}Keep it safe. A card doesn't need to be a pie to matter.{else}Sharing it. I'll write it out for the whole village. Mother would have scolded me, then smiled.{/if}{/if}"  ✏ `event.tilda_heart8.after`
  · effects: flag:event.tilda_heart8
  · effects: friend:tilda,70

### tilda_heart10 · "The Sign" · wholesome
heirloom · wave2_events.json · map GeneralStore · priority 7
plays when: `hearts:tilda>=10 && flag:event.tilda_heart8 && !weekday:sun && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Close your eyes. No, really. I've done something on the front of the shop and I'm too nervous to watch."  ✏ `event.tilda_heart10.intro`
  · (move player to (7,4))
  · (player faces up)
  · (spawn prop food.pumpkin_pie at (7,5))
  · (tilda emote heart)
  [tilda]: "I baked it from her card. Take it. It's the first one. The sign outside says you're a supplier, and so you are."  ✏ `event.tilda_heart10.give`
  · (sfx coin)
  · effects: give:food.pumpkin_pie,1, flag:event.tilda_heart10
  · (remove prop heirloom)
  [tilda]: "{if choice:card.share}I copied the recipe for the whole village, and put your name at the bottom, in chalk.{else}{if choice:card.bake}The pie came out right. It tasted like June, and my mother, and you.{else}The card's still safe. The pie's yours. Both are the best things in the shop.{/if}{/if}"  ✏ `event.tilda_heart10.thanks`
  · effects: friend:tilda,80

### tilda_friend · "Stocktake Saturday" · funny
helping_scene · wave2_events.json · map GeneralStore · priority 7 · repeatable
plays when: `!flag:event.tilda_friend && hearts:tilda>=3 && weekday:sat && !weekday:sun && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "Saturday stocktake, and I'm three sunflowers short of finishing the window. Do you have any?"  ✏ `event.tilda_friend.ask`
  · (move player to (7,4))
  · (player faces up)
  · (tilda emote question)
  [tilda]: "Three sunflowers, if you find them? The window's half dressed and very embarrassed."  ✏ `event.tilda_friend.fallback`
  [tilda]: "Three sunflowers! The window's finished. It looks like noon. Thank you, dear. I'll put it on the slate."  ✏ `event.tilda_friend.success`
  · effects: flag:event.tilda_friend
  · effects: friend:tilda,15

### tilda_birthday · "Tilda's Birthday" · wholesome
conversation · wave4_birthdays.json · map GeneralStore · priority 8 · repeatable
plays when: `hearts:tilda>=2 && birthday.in:tilda==0 && unseen:birthday.tilda && !weekday:sun && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (tilda faces down)
  [tilda]: "It's my birthday. I don't make a fuss, I make a large pie. Come in, dear, and do not look at the slate."  ✏ `event.tilda_birthday.intro`
  · (move player to (7,4))
  · (player faces up)
  · (tilda wave (background))
  · (tilda emote heart)
  ▶ DIALOGUE `tilda.birthday` (player-facing graph, plays here):
    n0 [tilda]: "You looked at the slate. I saw. Go on, then. What do you say?"  ✏ `dlg.tilda.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:tilda,10, flag:choice.bday.tilda.wish  ✏ `dlg.tilda.birthday.c0`
       ? "Bring her a flower." → r1  effects: friend:tilda,10, flag:choice.bday.tilda.flower  ✏ `dlg.tilda.birthday.c1`
       ? "Offer to mind the shop for an hour." → r2  effects: friend:tilda,10, flag:choice.bday.tilda.mind  ✏ `dlg.tilda.birthday.c2`
    r0 [tilda]: "Thank you. I'll put it on the slate in capital letters, where I can see it."  ✏ `dlg.tilda.birthday.r0`   (expr happy)
    r1 [tilda]: "A flower, in my shop! It goes in the window, beside the good pumpkin. Thank you."  ✏ `dlg.tilda.birthday.r1`   (expr happy)
    r2 [tilda]: "You'd mind it? Oh, that's the best present. Take the stool. Mind the cat."  ✏ `dlg.tilda.birthday.r2`   (expr happy)
  [tilda]: "{if choice:bday.tilda.wish}Capital letters, on the slate. It looks very well from the street.{else}{if choice:bday.tilda.flower}The flower's in the window. Half the village asked who sent it. I said a friend.{else}An hour off. I read a chapter. It was marvelous. The shop was fine.{/if}{/if}"  ✏ `event.tilda_birthday.after`
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
  [tilda]: "Felix. Your carp is eight coins. Last week it was six."  ✏ `event.overheard_tilda_felix_1.l1`
  [felix]: "It's a bigger carp. It's grown. It's grown in the stall, overnight, out of pride."
  · (felix shrug (background))
  · (felix emote ellipsis)
  · (tilda point (background))
  [tilda]: "It's been in your bucket since dawn. It hasn't grown. You've inflated it."  ✏ `event.overheard_tilda_felix_1.l3`
  [felix]: "Fine. Six. But I'm adding a story to the price. It's a good story."
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
  [felix]: "Tilda. You remember the winter the pond froze, and I fell in?"
  [tilda]: "I remember the winter you fell in, and also the winter you fell in again."  ✏ `event.overheard_tilda_felix_2.l2`
  · (tilda shrug (background))
  · (tilda emote ellipsis)
  · (felix wave (background))
  [felix]: "It was the same winter. It was a long one. You brought me soup."
  [tilda]: "I brought you soup, and a blanket, and I chalked 'Thin Ice' on the slate. You ignored the slate."  ✏ `event.overheard_tilda_felix_2.l4`
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
  [felix]: "Tilda. I wrote the first fish of the year on your slate. In capital letters."
  [tilda]: "Oh, Felix. It's a very good fish. I'll leave it up until the spring thaw."  ✏ `event.overheard_tilda_felix_3.l2`
  · (tilda point (background))
  · (tilda emote ellipsis)
  · (felix shrug (background))
  [felix]: "It was this big. Bigger, since I began. Don't tell the pond."
  [tilda]: "The pond knows, dear. The pond always knows. Sit. I'll put the kettle on."  ✏ `event.overheard_tilda_felix_3.l4`
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
  [felix]: "Tilda, I brought a pie. For once. A fish pie. Don't judge."
  [tilda]: "Felix, I'll judge it kindly. And salt it."  ✏ `event.overheard_tilda_felix_4.l2`
  · (tilda wave (background))
  · (tilda emote ellipsis)
  · (felix wave (background))
  [felix]: "It's a fair sentence. Pass the fork."
  [tilda]: "To an old friend, dear. The crust is mine. The fish is the cat's."  ✏ `event.overheard_tilda_felix_4.l4`
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
  [tilda]: "Wren. Your chalkboard looks like my slate."  ✏ `event.overheard_tilda_wren_1.l1`
  [wren]: "Imitation is flattery, Tilda, with better lighting."
  · (wren shrug (background))
  · (wren emote note)
  · (tilda point (background))
  [tilda]: "Flattery earns a ribbon, dear."  ✏ `event.overheard_tilda_wren_1.l3`
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
  [tilda]: "I heard something. About a certain cat."  ✏ `event.overheard_tilda_wren_2.l1`
  [wren]: "I heard it first, my darling. Three days ago."
  · (wren shrug (background))
  · (wren emote note)
  · (tilda wave (background))
  [tilda]: "Then we'll trade. You tell me yours."  ✏ `event.overheard_tilda_wren_2.l3`
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
  [tilda]: "Wren, I've stood behind that counter eight hours a day all week. Save me a seat tonight?"  ✏ `event.overheard_tilda_wren_3.l1`
  [wren]: "Tilda, you've earned the good chair. Come at six. Cider's on me."
  · (wren point (background))
  · (wren emote note)
  · (tilda shrug (background))
  [tilda]: "Just one cup. Then the slate."  ✏ `event.overheard_tilda_wren_3.l3`
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
  [juno]: "Tilda, I'm paying my slate! Three clasps and a hinge with a smiley face."
  [tilda]: "That's two months of bread, and the hinge is a tip. I'll put it on the slate."  ✏ `event.overheard_tilda_juno_1.l2`
  · (tilda shrug (background))
  · (tilda emote sweat)
  · (juno wave (background))
  [juno]: "Is the smiley face worth extra?"
  [tilda]: "The smiley face is priceless. That's why I'm not charging for it."  ✏ `event.overheard_tilda_juno_1.l4`
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
  [tilda]: "You've grown, Juno. When you first came in, you couldn't reach the jar of hooks."  ✏ `event.overheard_tilda_juno_2.l1`
  [juno]: "I still can't. I stand on the flour sack now!"
  · (juno shrug (background))
  · (juno emote heart)
  · (tilda point (background))
  [tilda]: "I know. I moved it there for you. Years ago."  ✏ `event.overheard_tilda_juno_2.l3`
  [juno]: "Tilda! You've been on my slate the whole time."
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
  [juno]: "Tilda... if I ever had my own forge, somewhere small, would you still feed me?"
  [tilda]: "Open a forge in Wetherell and I'll stock your first anvil. Go farther, and I'll post you pie."  ✏ `event.overheard_tilda_juno_3.l2`
  · (tilda point (background))
  · (tilda emote ellipsis)
  · (juno shrug (background))
  [juno]: "In Wetherell. Definitely. Next door to Bram, probably."
  [tilda]: "Good. The street could use the noise."  ✏ `event.overheard_tilda_juno_3.l4`
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
  [tilda]: "Bram. There's a crust on your bench. For the hinges, the lamp, and the shelf you fixed unasked."  ✏ `event.overheard_tilda_bram_1.l1`
  [bram]: "Hm. Didn't need one."
  · (bram shrug (background))
  · (bram emote ellipsis)
  · (tilda point (background))
  [tilda]: "You never say thank you. I never say it back. That's how the slate stays even."  ✏ `event.overheard_tilda_bram_1.l3`
  [bram]: "...Hm. Even. Good."
  · effects: flag:event.overheard_tilda_bram_1
