# Scene cards for marcus

18 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 38 spoken lines in scenes, 22 dialogue lines and 15 player choice labels.

### marcus_heart2 · "Hold This Plank" · funny
event · npc_roster.json · map Carpenter · priority 6
plays when: `hearts:marcus>=2 && !weekday:wed && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(6, 3)
  · (marcus faces down)
  [marcus]: "Hold this plank a moment, [player]."  ✏ `event.marcus_heart2.0`
  · (move player to (6,3))
  · (player faces up)
  · (marcus emote ellipsis)
  [marcus]: "Level as a lake. We make a good team."  ✏ `event.marcus_heart2.1`
  [marcus]: "Take these. Left over from the big jobs."  ✏ `event.marcus_heart2.2`
  · effects: give:resource.wood,40, friend:marcus,40, flag:event.marcus_heart2

### marcus_heart5 · "The Boyhood Carving" · wholesome
event · npc_roster.json · map Carpenter · priority 7
plays when: `hearts:marcus>=5 && !weekday:wed && hour>=9 && hour<17 && flag:event.marcus_heart2`
beats may be ADDED or CUT in this scene
placed: player(6, 3)
  · (marcus faces down)
  [marcus]: "I carved this when I was a boy."  ✏ `event.marcus_heart5.0`
  · (move player to (6,3))
  · (player faces up)
  · (marcus emote ellipsis)
  [marcus]: "Silly, perhaps. But I want you to have it."  ✏ `event.marcus_heart5.1`
  [marcus]: "Keep your tools sharp, friend."  ✏ `event.marcus_heart5.2`
  · effects: give:fertilizer.speed,3, give:prop.carving, friend:marcus,60, flag:event.marcus_heart5

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
  [marcus]: "I checked the roof once more. Then twice. It is finished."  ✏ `event.hall_dance.0`
  · (marcus emote sparkle)
  [wren]: "Look at this room! Full, and the porch is full too. Somebody count the porch, I've lost track of my own party."  (emote sparkle)
  · (spawn prop artisan.wine at (7,3))
  [wren]: "[player]. I said I'd open it on the night. This is the night. You're first at the bar."  (expr happy)
  · (remove prop wine)
  [tilda]: "The roof's done, the hall's open, and nobody asked for credit. I'm writing it on the slate anyway."
  [piper]: "First verse, then I step off. I'm calm. I'm holding this fiddle like a hot loaf."  (emote sweat)
  · (music piper_theme)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall sings the chorus. Piper has stopped playing, and nobody minds."
  [bram]: "Hm. Hm-hm. I know the tune. Say nothing."  (emote note)
  [juno]: "The hall door hangs on hinges I made. My mark's on every pin, and it swings like it's showing off!"  (expr happy)
  [wren]: "The roof pool is closed. Seven winters of coins, and Tilda had the right year."
  [tilda]: "Into the hall box it goes. Marcus, take one coin for the ladder."
  [marcus]: "Hm. One coin. One dance. Nobody tell the ladder."  ✏ `event.hall_dance.11`
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

### story_pie_payoff · "A Slice for Odalys" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:pie_feud && (hearts:marcus>=3 || hearts:odalys>=3) && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(25, 18), player(25, 17)
  · (place marcus to (25,18))
  · (marcus faces down)
  [marcus]: "[player]. A word. You're good with a delivery, aren't you?"  ✏ `event.story_pie_payoff.intro`
  · (move player to (25,17))
  · (player faces up)
  · (marcus emote exclaim)
  ▶ DIALOGUE `story.pie.talk` (player-facing graph, plays here):
    n0 [marcus]: "One slice of my pie. Her door. Don't say who sent it. If she asks, say a stranger with excellent taste."  ✏ `dlg.story.pie.talk.0`   (→ n1)
    n1 [marcus]: "What do you say?"  ✏ `dlg.story.pie.talk.1`
       ? "Deliver it honestly." → r0  effects: flag:choice.pie.honest  ✏ `dlg.story.pie.talk.c0`
       ? "Improve it first." → r1  effects: flag:choice.pie.improve  ✏ `dlg.story.pie.talk.c1`
       ? "Eat it." → r2  effects: flag:choice.pie.eat  ✏ `dlg.story.pie.talk.c2`
    r0 [narration]: "You carry the slice to Odalys's door. She reads the note, sighs, and takes a bite."  ✏ `dlg.story.pie.talk.r0`   (→ r0b)
    r0b [odalys]: "Ambitious. ...It's rather good. Tell him nothing."   (expr thinking)
    r1 [narration]: "You add a pinch of cinnamon on the way. Odalys takes a bite and frowns for a long time."  ✏ `dlg.story.pie.talk.r1`   (→ r1b)
    r1b [odalys]: "Who touched this? It's better. I hate that."   (expr embarrassed)
    r2 [narration]: "You eat the slice on the road. It is excellent. You now have two angry customers."  ✏ `dlg.story.pie.talk.r2`   (→ r2b)
    r2b [odalys]: "Marcus owes me an apology. And a pie. And, apparently, you."
  [marcus]: "{if choice:pie.honest}She said 'good'. Out loud. Do not repeat it.{else}{if choice:pie.improve}She sent back a note: 'Cinnamon? Cheek.' I'm delighted.{else}You ate it. Fair. Odalys is baking a second. So am I.{/if}{/if}"  ✏ `event.story_pie_payoff.after`
  · effects: flag:event.story_pie_payoff, friend:marcus,20, friend:odalys,20, flag:storydone.pie_feud

### story_whistler_marcus · "The Whistler" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:mystery_whistler.marcus && hour>=18 && hour<21 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(25, 18), player(25, 17)
  · (place marcus to (25,18))
  · (marcus faces down)
  [marcus]: "(The whistling stops. Marcus puts a plank down, very carefully.) Ah. You've found me."  ✏ `event.story_whistler_marcus.intro`
  · (move player to (25,17))
  · (player faces up)
  · (marcus emote note)
  ▶ DIALOGUE `story.whistler.marcus` (player-facing graph, plays here):
    n0 [marcus]: "It's a family tune. One note went wrong in my grandfather's time and we kept it."  ✏ `dlg.story.whistler.marcus.0`   (→ n1)
    n1 [marcus]: "...Well?"  ✏ `dlg.story.whistler.marcus.1`
       ? "Call out a friendly hello." → r0  effects: flag:choice.whistler.marcus.hello  ✏ `dlg.story.whistler.marcus.c0`
       ? "Whistle the wrong note back." → r1  effects: flag:choice.whistler.marcus.back, friend:marcus,10  ✏ `dlg.story.whistler.marcus.c1`
       ? "Say nothing. Just listen." → r2  effects: flag:choice.whistler.marcus.listen, friend:marcus,10  ✏ `dlg.story.whistler.marcus.c2`
    r0 [marcus]: "Hello. Right. Good. Thank you for the normal approach."  ✏ `dlg.story.whistler.marcus.r0`
    r1 [marcus]: "You... got the wrong note exactly right. I'm moved and mortified."  ✏ `dlg.story.whistler.marcus.r1`   (expr embarrassed)
    r2 [marcus]: "You listened. To the whole thing. ...I may whistle again. Quietly."  ✏ `dlg.story.whistler.marcus.r2`   (expr happy)
  [marcus]: "Tradition. A wrong note is only wrong if you apologize for it."  ✏ `event.story_whistler_marcus.after`
  · effects: flag:event.story_whistler_marcus, friend:marcus,30, flag:storydone.mystery_whistler

### story_pumpkin_marcus · "The Missing Pumpkin" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:missing_pumpkin.marcus && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(25, 18), player(25, 17)
  · (place marcus to (25,18))
  · (marcus faces down)
  · (spawn prop prop.pumpkin at (26,18))
  [marcus]: "Ah. You've found out. I was carving a practice lantern. A very large practice lantern."  ✏ `event.story_pumpkin_marcus.intro`
  · (move player to (25,17))
  · (player faces up)
  · (marcus emote sweat)
  · (marcus shrug (background))
  ▶ DIALOGUE `story.pumpkin.marcus` (player-facing graph, plays here):
    n0 [narration]: "You find Marcus behind the store, with a very large pumpkin and a very guilty face."  ✏ `dlg.story.pumpkin.marcus.0`   (→ n1)
    n1 [tilda]: "Is that my prize pumpkin? The one the whole village has been blaming the goats for?"   (→ n2)
    n2 [marcus]: "...It might be."  ✏ `dlg.story.pumpkin.marcus.2`
       ? "Let them off kindly." → r0  effects: flag:choice.pumpkin.marcus.kind, friend:marcus,10  ✏ `dlg.story.pumpkin.marcus.c0`
       ? "Insist on a proper apology." → r1  effects: flag:choice.pumpkin.marcus.apology  ✏ `dlg.story.pumpkin.marcus.c1`
       ? "Offer to help carry it back." → r2  effects: flag:choice.pumpkin.marcus.help, friend:tilda,10  ✏ `dlg.story.pumpkin.marcus.c2`
    r0 [tilda]: "Kindly? Fine. Kindly. But he's on the slate for a pie."   (expr thinking)
    r1 [marcus]: "An apology. Yes. Very sorry. Truly. Pumpkin, I'm also sorry to you."  ✏ `dlg.story.pumpkin.marcus.r1`   (emote sweat)
    r2 [tilda]: "You carry the heavy end. I'll supervise. We'll call it civic duty."   (expr happy)
  [marcus]: "He's returning it with a face carved in. Tilda says it's an improvement. Marcus will not stop smiling."  ✏ `event.story_pumpkin_marcus.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_pumpkin_marcus, friend:marcus,30, friend:tilda,20, flag:storydone.missing_pumpkin

### marcus_heart8 · "The Hall Roof" · surprise
conversation · wave3_events.json · map Carpenter · priority 7
plays when: `hearts:marcus>=8 && flag:event.marcus_heart5 && !weekday:wed && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (marcus faces down)
  [marcus]: "Come here. I want you to look at the roof plan. Not many have. Measure twice, I tell myself. Then I ask you to look once."  ✏ `event.marcus_heart8.intro`
  · (move player to (6,3))
  · (player faces up)
  · (marcus wave (background))
  · (marcus emote ellipsis)
  ▶ DIALOGUE `marcus.roof` (player-facing graph, plays here):
    n0 [marcus]: "Here's the plan. Pitch, rafters, shingles. I've drawn it nine times. I'm afraid it still won't hold."  ✏ `dlg.marcus.roof.0`   (→ n1; expr thinking)
    n1 [marcus]: "What do you think I should do?"  ✏ `dlg.marcus.roof.1`
       ? "Help him measure." → r0  effects: friend:marcus,15, flag:choice.roof.measure  ✏ `dlg.marcus.roof.c0`
       ? "Suggest a new design." → r1  effects: friend:marcus,10, flag:choice.roof.design  ✏ `dlg.marcus.roof.c1`
       ? "Offer to hold the ladder." → r2  effects: friend:marcus,10, flag:choice.roof.ladder  ✏ `dlg.marcus.roof.c2`
    r0 [marcus]: "Measure with me. Yes. Hold the end of the tape. Twice. Then once more."  ✏ `dlg.marcus.roof.r0`   (expr happy)
    r1 [marcus]: "A new design. I'd stopped asking. ...Show me the angle. Slowly."  ✏ `dlg.marcus.roof.r1`
    r2 [marcus]: "The ladder. Plain, steady, necessary. That's the best offer I've had."  ✏ `dlg.marcus.roof.r2`   (expr happy)
  [marcus]: "{if choice:roof.measure}You measured with me. Twice, then a third time under your breath. It's the first time I haven't been alone with it.{else}{if choice:roof.design}A new pitch. I'd never have tried it. I'll draw it tonight. Not nine times. Perhaps four.{else}You held the ladder. That's half of any building. The roof's still nearly fixed. But less alone.{/if}{/if}"  ✏ `event.marcus_heart8.after`
  · effects: flag:event.marcus_heart8
  · effects: friend:marcus,70

### marcus_birthday · "Marcus's Birthday" · wholesome
conversation · wave4_birthdays.json · map Carpenter · priority 8 · repeatable
plays when: `hearts:marcus>=2 && birthday.in:marcus==0 && unseen:birthday.marcus && !weekday:wed && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (marcus faces down)
  [marcus]: "It's my birthday. I planed a plank for it. Nothing fancy. A straight board. Sit on the bench."  ✏ `event.marcus_birthday.intro`
  · (move player to (6,3))
  · (player faces up)
  · (marcus wave (background))
  · (marcus emote ellipsis)
  ▶ DIALOGUE `marcus.birthday` (player-facing graph, plays here):
    n0 [marcus]: "The bench holds two. What do you say?"  ✏ `dlg.marcus.birthday.0`
       ? "Wish him a happy birthday." → r0  effects: friend:marcus,10, flag:choice.bday.marcus.wish  ✏ `dlg.marcus.birthday.c0`
       ? "Admire the plank." → r1  effects: friend:marcus,10, flag:choice.bday.marcus.plank  ✏ `dlg.marcus.birthday.c1`
       ? "Offer to hold the other end." → r2  effects: friend:marcus,10, flag:choice.bday.marcus.hold  ✏ `dlg.marcus.birthday.c2`
    r0 [marcus]: "Thank you. I'll note it on the beam. Measure twice, thank twice."  ✏ `dlg.marcus.birthday.r0`   (expr happy)
    r1 [marcus]: "It's a good one. Oak. Straight grain. Thank you for noticing."  ✏ `dlg.marcus.birthday.r1`   (expr happy)
    r2 [marcus]: "The other end. Yes. That's the best birthday present: a second pair of hands."  ✏ `dlg.marcus.birthday.r2`   (expr happy)
  [marcus]: "{if choice:bday.marcus.wish}It's on the beam, in small letters. It will outlast us both.{else}{if choice:bday.marcus.plank}You noticed the plank. Nobody notices. It's hanging over the door now.{else}Two hands on a plank. A quiet, good birthday.{/if}{/if}"  ✏ `event.marcus_birthday.after`
  · effects: flag:event.marcus_birthday
  · effects: mark:birthday.marcus, friend:marcus,30, give:resource.wood,5, mail:marcus_bday_thanks

### overheard_marcus_dorian_1 · "The Straight Plank" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.dorian && hearts:marcus>=2 && hearts:dorian>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "Dorian. Hold the end. No, the other end. There."  ✏ `event.overheard_marcus_dorian_1.l1`
  [dorian]: "...Straight."
  · (dorian shrug (background))
  · (dorian emote ellipsis)
  · (marcus point (background))
  [marcus]: "Straight. That's the whole job. I couldn't have said it better."  ✏ `event.overheard_marcus_dorian_1.l3`
  [dorian]: "...You could have. You didn't. Good."
  · effects: flag:event.overheard_marcus_dorian_1

### overheard_marcus_dorian_2 · "Sawdust Silence" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.dorian && hearts:marcus>=4 && hearts:dorian>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_dorian_1`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "It's been an hour. You haven't said anything."  ✏ `event.overheard_marcus_dorian_2.l1`
  [dorian]: "...Neither have you."
  · (dorian shrug (background))
  · (dorian emote ellipsis)
  · (marcus wave (background))
  [marcus]: "I was getting worried. No, I wasn't. I was enjoying it."  ✏ `event.overheard_marcus_dorian_2.l3`
  [dorian]: "...Me too. Hand me the plane."
  · effects: flag:event.overheard_marcus_dorian_2

### overheard_marcus_dorian_3 · "The Forest Door" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.dorian && hearts:marcus>=6 && hearts:dorian>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_dorian_2`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "I made a door for your room. It's oak. It's heavier than it looks."  ✏ `event.overheard_marcus_dorian_3.l1`
  [dorian]: "...It's good. It's the first door I've owned that I'd close on purpose."
  · (dorian point (background))
  · (dorian emote ellipsis)
  · (marcus shrug (background))
  [marcus]: "That's the whole idea of a door."  ✏ `event.overheard_marcus_dorian_3.l3`
  [dorian]: "...Thank you. It's a very good door. I'll keep it open. Mostly."
  · effects: flag:event.overheard_marcus_dorian_3

### overheard_marcus_dorian_4 · "The Hut Window" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.dorian && hearts:marcus>=8 && hearts:dorian>=8 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_dorian_3`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "Dorian. I built a window for your room. Square. Oak."  ✏ `event.overheard_marcus_dorian_4.l1`
  [dorian]: "...A window. Facing the trees."
  · (dorian wave (background))
  · (dorian emote ellipsis)
  · (marcus wave (background))
  [marcus]: "Facing the owl. You can say hello."  ✏ `event.overheard_marcus_dorian_4.l3`
  [dorian]: "...Thank you. The owl will want a name."
  · effects: flag:event.overheard_marcus_dorian_4

### overheard_bram_marcus_1 · "The Two Trades" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.bram && flag:met.marcus && hearts:bram>=2 && hearts:marcus>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: bram(24, 18), marcus(26, 18)
  · (place bram to (24,18))
  · (place marcus to (26,18))
  · (bram faces right)
  · (marcus faces left)
  [bram]: "Hm. Your joint's tight."
  [marcus]: "Your weld's clean. Don't tell Juno."  ✏ `event.overheard_bram_marcus_1.l2`
  · (marcus shrug (background))
  · (marcus emote ellipsis)
  · (bram point (background))
  [bram]: "Hm. Wouldn't."
  [marcus]: "Good. Hand me the clamp."  ✏ `event.overheard_bram_marcus_1.l4`
  · effects: flag:event.overheard_bram_marcus_1

### overheard_bram_marcus_2 · "Iron and Oak" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.bram && flag:met.marcus && hearts:bram>=4 && hearts:marcus>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_bram_marcus_1`
text-only scene: rewrite lines, do not add or cut beats
placed: bram(24, 18), marcus(26, 18)
  · (place bram to (24,18))
  · (place marcus to (26,18))
  · (bram faces right)
  · (marcus faces left)
  [bram]: "Hm. Handle for the big hammer?"
  [marcus]: "Made you one. Oak. Measured twice. Your grip's in the shop."  ✏ `event.overheard_bram_marcus_2.l2`
  · (marcus shrug (background))
  · (marcus emote ellipsis)
  · (bram wave (background))
  [bram]: "Hm. It fits."
  [marcus]: "Good. A handle should vanish in the hand."  ✏ `event.overheard_bram_marcus_2.l4`
  · effects: flag:event.overheard_bram_marcus_2

### overheard_bram_marcus_3 · "Quiet Competition" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.bram && flag:met.marcus && hearts:bram>=6 && hearts:marcus>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_bram_marcus_2`
text-only scene: rewrite lines, do not add or cut beats
placed: bram(24, 18), marcus(26, 18)
  · (place bram to (24,18))
  · (place marcus to (26,18))
  · (bram faces right)
  · (marcus faces left)
  [bram]: "Hm. A hundred nails today."
  [marcus]: "A hundred and four. All straight."  ✏ `event.overheard_bram_marcus_3.l2`
  · (marcus point (background))
  · (marcus emote ellipsis)
  · (bram shrug (background))
  [bram]: "Hm. Mine were straighter."
  [marcus]: "Measure them. Then we'll know."  ✏ `event.overheard_bram_marcus_3.l4`
  · effects: flag:event.overheard_bram_marcus_3

### overheard_marcus_odalys_1 · "Bend the Knees" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.odalys && hearts:marcus>=2 && hearts:odalys>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), marcus(26, 18)
  · (place odalys to (24,18))
  · (place marcus to (26,18))
  · (odalys faces right)
  · (marcus faces left)
  [odalys]: "Marcus. You lifted that beam alone. I saw it from the clinic window."
  [marcus]: "Measured it twice first."  ✏ `event.overheard_marcus_odalys_1.l2`
  · (marcus shrug (background))
  · (marcus emote exclaim)
  · (odalys point (background))
  [odalys]: "Measuring is not lifting. Bend the knees. Doctor's orders."
  [marcus]: "Knees. Noted. Twice."  ✏ `event.overheard_marcus_odalys_1.l4`
  · effects: flag:event.overheard_marcus_odalys_1

### overheard_marcus_odalys_2 · "The Clinic Bench" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.odalys && hearts:marcus>=4 && hearts:odalys>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_odalys_1`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), odalys(26, 18)
  · (place marcus to (24,18))
  · (place odalys to (26,18))
  · (marcus faces right)
  · (odalys faces left)
  [marcus]: "Dr. Penn. The bench outside your clinic had a loose leg. Fixed it."  ✏ `event.overheard_marcus_odalys_2.l1`
  [odalys]: "I do not sit on that bench."
  · (odalys shrug (background))
  · (odalys emote ellipsis)
  · (marcus point (background))
  [marcus]: "That's why the leg was loose. Nobody tests it. Try it. Whenever you like."  ✏ `event.overheard_marcus_odalys_2.l3`
  [odalys]: "...Five minutes, then. It is a good bench. Do not tell Elara."
  · effects: flag:event.overheard_marcus_odalys_2

### overheard_marcus_odalys_3 · "Doctor's Own Orders" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.odalys && hearts:marcus>=6 && hearts:odalys>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_odalys_2`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), marcus(26, 18)
  · (place odalys to (24,18))
  · (place marcus to (26,18))
  · (odalys faces right)
  · (marcus faces left)
  [odalys]: "Your back, Marcus. How is it?"
  [marcus]: "Better. I've been resting it. The doctor said to."  ✏ `event.overheard_marcus_odalys_3.l2`
  · (marcus point (background))
  · (marcus emote heart)
  · (odalys shrug (background))
  [odalys]: "I did. You listened. You are the only patient who does."
  [marcus]: "Then take your own advice. Sundays. I'll build the chair."  ✏ `event.overheard_marcus_odalys_3.l4`
  · effects: flag:event.overheard_marcus_odalys_3
