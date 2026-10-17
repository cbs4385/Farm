# Scene cards for marcus

18 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 41 spoken lines in scenes, 22 dialogue lines and 15 player choice labels.

### marcus_heart2 · "Hold This Plank" · funny
event · npc_roster.json · map Carpenter · priority 6
plays when: `hearts:marcus>=2 && !weekday:wed && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(6, 3)
  · (marcus faces down)
  [marcus]: "Hold this end a moment, [player]. Don't level it. Just hold it."  ✏ `event.marcus_heart2.0`
  · (move player to (6,3))
  · (player faces up)
  · (marcus emote ellipsis)
  [marcus]: "Measure once. Twice. ...Three times, my father said. Level as a lake."  ✏ `event.marcus_heart2.1`
  [marcus]: "Most hands drift. Yours stayed put. That's rarer than good oak."  ✏ `event.marcus_heart2.3`  (expr happy)
  [marcus]: "Take these. Leftovers from the big jobs. None long enough for the hall roof. I checked."  ✏ `event.marcus_heart2.2`
  · effects: give:resource.wood,40, friend:marcus,40, flag:event.marcus_heart2

### marcus_heart5 · "The Boyhood Carving" · wholesome
event · npc_roster.json · map Carpenter · priority 7
plays when: `hearts:marcus>=5 && !weekday:wed && hour>=9 && hour<17 && flag:event.marcus_heart2`
beats may be ADDED or CUT in this scene
placed: player(6, 3)
  · (marcus faces down)
  [marcus]: "I carved this when I was nine. A barn, about the size of a fist."  ✏ `event.marcus_heart5.0`
  · (move player to (6,3))
  · (player faces up)
  · (marcus emote ellipsis)
  [marcus]: "Silly, perhaps. The doors don't open. I glued them shut in a hurry. I want you to have it."  ✏ `event.marcus_heart5.1`
  [marcus]: "Edmund had a door that never opened, too. His cellar. He wouldn't let me oil it."  ✏ `event.marcus_heart5.4`  (expr neutral)
  [marcus]: "My father said a thing done fast stays done wrong. He kept it on his shelf anyway."  ✏ `event.marcus_heart5.3`  (expr thinking)
  [marcus]: "Keep your tools sharp, friend. The bag is for your fields. Some things are allowed to hurry."  ✏ `event.marcus_heart5.2`
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
  [marcus]: "I checked the roof once. Twice. ...Three times, my father said. It is finished. Not nearly. Finished."  ✏ `event.hall_dance.0`
  · (marcus emote sparkle)
  [wren]: "Look at this room! Full, and the porch is full too. Somebody count the porch. I need a number for the story."  (emote sparkle)
  · (spawn prop artisan.wine at (7,3))
  [wren]: "[player]. I said I'd open it on the night, and this is the night. First glass is yours. I poured it before the doors."  (expr happy)
  · (remove prop wine)
  [tilda]: "Nobody asked for credit for any of this. So I'm giving it, in the good chalk, under every name in this room."
  [piper]: "First verse, then I step off. I'm perfectly calm. My bow hand hasn't heard."  (emote sweat)
  · (music piper_theme)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall takes the chorus. Piper stops playing and sways, and the song goes on without her."
  [bram]: "Hm. Hm-hm. Hm. I sing it in my own language."  (emote note)
  [juno]: "Those door hinges are mine. My mark's on every pin. Listen to it swing. It's showing off, and I'm letting it!"  (expr happy)
  [wren]: "The roof pool is closed! Six winters of coins, and Tilda picked the right year. I'm demanding a recount."
  [tilda]: "Into the hall box, every coin. Except one. Marcus, that one's for the ladder."
  [marcus]: "One coin. One dance. I measured this floor for it. Twice."  ✏ `event.hall_dance.11`
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

### story_pie_payoff · "A Slice for Odalys" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:pie_feud && (hearts:marcus>=3 || hearts:odalys>=3) && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(25, 18), player(25, 17)
  · (place marcus to (25,18))
  · (marcus faces down)
  [marcus]: "[player]. A word, quietly. You know the way to the clinic, don't you?"  ✏ `event.story_pie_payoff.intro`
  · (move player to (25,17))
  · (player faces up)
  · (marcus emote exclaim)
  ▶ DIALOGUE `story.pie.talk` (player-facing graph, plays here):
    n0 [marcus]: "One slice of my pie. Her door. No name. If she asks, a stranger with excellent taste."  ✏ `dlg.story.pie.talk.0`   (→ n1)
    n1 [marcus]: "Well? You can say no. I'll just stand here holding it."  ✏ `dlg.story.pie.talk.1`
       ? "Deliver it, no tricks." → r0  effects: flag:choice.pie.honest  ✏ `dlg.story.pie.talk.c0`
       ? "I'll improve it on the way." → r1  effects: flag:choice.pie.improve  ✏ `dlg.story.pie.talk.c1`
       ? "It might not survive the walk." → r2  effects: flag:choice.pie.eat  ✏ `dlg.story.pie.talk.c2`
    r0 [narration]: "You leave the slice on Odalys's step. She reads the note, sighs, and takes a bite."  ✏ `dlg.story.pie.talk.r0`   (→ r0b)
    r0b [odalys]: "A stranger with excellent taste and sawdust on the note. ...It's rather good. Tell him nothing."   (expr thinking)
    r1 [narration]: "You add a pinch of cinnamon on the way. Odalys tastes it, then frowns at it for a long time."  ✏ `dlg.story.pie.talk.r1`   (→ r1b)
    r1b [odalys]: "Someone added cinnamon. It's better now. I resent that, professionally."   (expr embarrassed)
    r2 [narration]: "You eat the slice on the road. It is excellent. Two bakers will hear about this."  ✏ `dlg.story.pie.talk.r2`   (→ r2b)
    r2b [odalys]: "An empty plate and a crumb on your sleeve. I'll bill Marcus for one pie. You, for the crumb."
  [marcus]: "{if choice:pie.honest}'Rather good.' She said 'rather good.' Don't repeat it. I'll repeat it enough.{else}{if choice:pie.improve}She sent a note back: 'Cinnamon? Cheek.' Best review I've ever had.{else}You ate it. Fair. Now she's baking a second, and so am I. Ambitiously.{/if}{/if}"  ✏ `event.story_pie_payoff.after`
  · effects: flag:event.story_pie_payoff, friend:marcus,20, friend:odalys,20, flag:storydone.pie_feud

### story_whistler_marcus · "The Whistler" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:mystery_whistler.marcus && hour>=18 && hour<21 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(25, 18), player(25, 17)
  · (place marcus to (25,18))
  · (marcus faces down)
  [marcus]: "Ah. You've found me. I whistle when the grain runs true. It's running very true tonight."  ✏ `event.story_whistler_marcus.intro`
  · (move player to (25,17))
  · (player faces up)
  · (marcus emote note)
  ▶ DIALOGUE `story.whistler.marcus` (player-facing graph, plays here):
    n0 [marcus]: "It's a family tune. My grandfather hit one note wrong, and we've all kept it since."  ✏ `dlg.story.whistler.marcus.0`   (→ n1)
    n1 [marcus]: "...Well? Go on. Everybody has an opinion about the note."  ✏ `dlg.story.whistler.marcus.1`
       ? "Say hello, like a normal neighbor." → r0  effects: flag:choice.whistler.marcus.hello  ✏ `dlg.story.whistler.marcus.c0`
       ? "Whistle the wrong note back." → r1  effects: flag:choice.whistler.marcus.back, friend:marcus,10  ✏ `dlg.story.whistler.marcus.c1`
       ? "Say nothing. Wait for the encore." → r2  effects: flag:choice.whistler.marcus.listen, friend:marcus,10  ✏ `dlg.story.whistler.marcus.c2`
    r0 [marcus]: "Hello. Yes. Hello. Thank you for the normal approach. It's rarer than you'd think."  ✏ `dlg.story.whistler.marcus.r0`
    r1 [marcus]: "You got the wrong note exactly right. I'm moved. Also mortified."  ✏ `dlg.story.whistler.marcus.r1`   (expr embarrassed)
    r2 [marcus]: "You listened. All of it. ...Grandfather would have liked you. I may whistle again. Quietly."  ✏ `dlg.story.whistler.marcus.r2`   (expr happy)
  [marcus]: "A wrong note is only wrong if you apologize for it. Wednesdays, I whistle on the bench. Bram pretends not to hear."  ✏ `event.story_whistler_marcus.after`
  · effects: flag:event.story_whistler_marcus, friend:marcus,30, flag:storydone.mystery_whistler

### story_pumpkin_marcus · "The Missing Pumpkin" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:missing_pumpkin.marcus && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(25, 18), player(25, 17)
  · (place marcus to (25,18))
  · (marcus faces down)
  · (spawn prop prop.pumpkin at (26,18))
  [marcus]: "Ah. You've found out. A practice lantern. I measured the face twice. Never asked whose pumpkin."  ✏ `event.story_pumpkin_marcus.intro`
  · (move player to (25,17))
  · (player faces up)
  · (marcus emote sweat)
  · (marcus shrug (background))
  ▶ DIALOGUE `story.pumpkin.marcus` (player-facing graph, plays here):
    n0 [narration]: "Behind the store, Marcus stands beside a very large pumpkin with a very guilty face."  ✏ `dlg.story.pumpkin.marcus.0`   (→ n1)
    n1 [tilda]: "Marcus! Is that my prize pumpkin? I blamed the goats. I apologized to a goat."   (→ n2)
    n2 [marcus]: "...It might be. It has a nose now."  ✏ `dlg.story.pumpkin.marcus.2`
       ? "Let him off gently." → r0  effects: flag:choice.pumpkin.marcus.kind, friend:marcus,10  ✏ `dlg.story.pumpkin.marcus.c0`
       ? "He owes a proper apology." → r1  effects: flag:choice.pumpkin.marcus.apology  ✏ `dlg.story.pumpkin.marcus.c1`
       ? "Help carry the evidence back." → r2  effects: flag:choice.pumpkin.marcus.help, friend:tilda,10  ✏ `dlg.story.pumpkin.marcus.c2`
    r0 [tilda]: "Kindly. Fine. But he owes me a pie, and I've seen him measure. It'll be exact."   (expr thinking)
    r1 [marcus]: "An apology. Yes. I'm sorry, Tilda. Truly. And sorry to you, pumpkin. You deserved a plan."  ✏ `dlg.story.pumpkin.marcus.r1`   (emote sweat)
    r2 [tilda]: "You and Marcus take the ends. I'll walk ahead and tell everyone it was the goats."   (expr happy)
  [marcus]: "It goes back with a face carved in. Tilda calls it an improvement. I've stopped trying not to smile."  ✏ `event.story_pumpkin_marcus.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_pumpkin_marcus, friend:marcus,30, friend:tilda,20, flag:storydone.missing_pumpkin

### marcus_heart8 · "The Hall Roof" · surprise
conversation · wave3_events.json · map Carpenter · priority 7
plays when: `hearts:marcus>=8 && flag:event.marcus_heart5 && !weekday:wed && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (marcus faces down)
  [marcus]: "Come in. I want you to see the roof plan. Not many have. I measure twice. You look once."  ✏ `event.marcus_heart8.intro`
  · (move player to (6,3))
  · (player faces up)
  · (marcus wave (background))
  · (marcus emote ellipsis)
  ▶ DIALOGUE `marcus.roof` (player-facing graph, plays here):
    n0 [marcus]: "Pitch, rafters, shingles. Drawn nine times. Every time it looks right. Every time I'm afraid it won't hold."  ✏ `dlg.marcus.roof.0`   (→ n1; expr thinking)
    n1 [marcus]: "Six winters, it's been nearly fixed. ...What would you do?"  ✏ `dlg.marcus.roof.1`
       ? "Measure it with you, properly." → r0  effects: friend:marcus,15, flag:choice.roof.measure  ✏ `dlg.marcus.roof.c0`
       ? "Try a different pitch." → r1  effects: friend:marcus,10, flag:choice.roof.design  ✏ `dlg.marcus.roof.c1`
       ? "I'll hold the ladder. I'm good at holding." → r2  effects: friend:marcus,10, flag:choice.roof.ladder  ✏ `dlg.marcus.roof.c2`
    r0 [marcus]: "Measure with me. Yes. You take the end of the tape. Once. Twice. ...Three times."  ✏ `dlg.marcus.roof.r0`   (expr happy)
    r1 [marcus]: "A new pitch. I'd stopped asking that question. ...Show me the angle. Slowly."  ✏ `dlg.marcus.roof.r1`
    r2 [marcus]: "The ladder. Plain, steady, necessary. Best offer I've had in six winters."  ✏ `dlg.marcus.roof.r2`   (expr happy)
  [marcus]: "{if choice:roof.measure}You measured with me. Three times, the last under your breath. First time I haven't been alone with it.{else}{if choice:roof.design}A steeper pitch. I'd never have tried it. I'll draw it tonight. Not nine times. Perhaps four.{else}You held the ladder. That's half of any building. The roof's still nearly fixed. Less nearly, now.{/if}{/if}"  ✏ `event.marcus_heart8.after`
  · effects: flag:event.marcus_heart8
  · effects: friend:marcus,70

### marcus_birthday · "Marcus's Birthday" · wholesome
conversation · wave4_birthdays.json · map Carpenter · priority 8 · repeatable
plays when: `hearts:marcus>=2 && birthday.in:marcus==0 && unseen:birthday.marcus && !weekday:wed && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (marcus faces down)
  [marcus]: "It's my birthday. I planed a plank for it, like every year. A straight board. Sit, if you like."  ✏ `event.marcus_birthday.intro`
  · (move player to (6,3))
  · (player faces up)
  · (marcus wave (background))
  · (marcus emote ellipsis)
  ▶ DIALOGUE `marcus.birthday` (player-facing graph, plays here):
    n0 [marcus]: "The bench holds two. What do you say?"  ✏ `dlg.marcus.birthday.0`
       ? "Happy birthday, Marcus." → r0  effects: friend:marcus,10, flag:choice.bday.marcus.wish  ✏ `dlg.marcus.birthday.c0`
       ? "That's a beautiful plank." → r1  effects: friend:marcus,10, flag:choice.bday.marcus.plank  ✏ `dlg.marcus.birthday.c1`
       ? "Need someone on the other end?" → r2  effects: friend:marcus,10, flag:choice.bday.marcus.hold  ✏ `dlg.marcus.birthday.c2`
    r0 [marcus]: "Thank you. I'll write it on the beam, small. Measure twice, thank twice."  ✏ `dlg.marcus.birthday.r0`   (expr happy)
    r1 [marcus]: "Oak. Straight grain, no knots. Thank you for noticing. Most people see a board."  ✏ `dlg.marcus.birthday.r1`   (expr happy)
    r2 [marcus]: "The other end. Yes. A second pair of hands is the best present there is."  ✏ `dlg.marcus.birthday.r2`   (expr happy)
  [marcus]: "{if choice:bday.marcus.wish}It's on the beam now, in small letters. That beam will outlast us both.{else}{if choice:bday.marcus.plank}You noticed the plank. Nobody notices. It's going up over the door.{else}Two hands on one plank. A quiet birthday. A good one.{/if}{/if}"  ✏ `event.marcus_birthday.after`
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
  [marcus]: "Dorian. Hold the end. No, the other end. ...There."  ✏ `event.overheard_marcus_dorian_1.l1`
  [dorian]: "...Straight. Like a birch."
  · (dorian shrug (background))
  · (dorian emote ellipsis)
  · (marcus point (background))
  [marcus]: "Straight. That's the whole job. I couldn't have said it shorter."  ✏ `event.overheard_marcus_dorian_1.l3`
  [dorian]: "...You could have. You left it for me."
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
  [marcus]: "An hour, and you haven't said a word."  ✏ `event.overheard_marcus_dorian_2.l1`
  [dorian]: "...Neither have you."
  · (dorian shrug (background))
  · (dorian emote ellipsis)
  · (marcus wave (background))
  [marcus]: "I nearly got worried. Then I didn't. Best hour of the week."  ✏ `event.overheard_marcus_dorian_2.l3`
  [dorian]: "...Me too. Plane, please. That's my words for the week."
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
  [marcus]: "I hung a new door on your room. Oak. Heavier than it looks, like most things."  ✏ `event.overheard_marcus_dorian_3.l1`
  [dorian]: "...Oak. Good. First door I'd close on purpose."
  · (dorian point (background))
  · (dorian emote ellipsis)
  · (marcus shrug (background))
  [marcus]: "That's all a door is for. Closing on purpose."  ✏ `event.overheard_marcus_dorian_3.l3`
  [dorian]: "...It's a very good door. I'll leave it open. Mostly."
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
  [marcus]: "Dorian. I built a window for your room. Square. Oak. Measured three times."  ✏ `event.overheard_marcus_dorian_4.l1`
  [dorian]: "...Toward the trees? Good. The light comes in green there."
  · (dorian wave (background))
  · (dorian emote ellipsis)
  · (marcus wave (background))
  [marcus]: "Facing the owl. You two can finally be introduced."  ✏ `event.overheard_marcus_dorian_4.l3`
  [dorian]: "...The owl will want a name. I'll wait till it tells me."
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
  [marcus]: "Your weld's clean. Don't tell Juno I said so."  ✏ `event.overheard_bram_marcus_1.l2`
  · (marcus shrug (background))
  · (marcus emote ellipsis)
  · (bram point (background))
  [bram]: "Wouldn't. She'd frame it."
  [marcus]: "Good. Hand me the clamp. Wednesday, same bench."  ✏ `event.overheard_bram_marcus_1.l4`
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
  [marcus]: "Made you one. Oak. Measured your grip twice while you weren't looking."  ✏ `event.overheard_bram_marcus_2.l2`
  · (marcus shrug (background))
  · (marcus emote ellipsis)
  · (bram wave (background))
  [bram]: "...Hm. Fits like I grew it."
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
  [marcus]: "A hundred and four. All straight. Counted twice."  ✏ `event.overheard_bram_marcus_3.l2`
  · (marcus point (background))
  · (marcus emote ellipsis)
  · (bram shrug (background))
  [bram]: "Mine were straighter. Yours were more."
  [marcus]: "Measure them. Bring them Wednesday. Lunch goes to the straighter."  ✏ `event.overheard_bram_marcus_3.l4`
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
  [odalys]: "Marcus. You lifted that beam alone. I watched from the clinic window and wrote it down."
  [marcus]: "I measured it twice first."  ✏ `event.overheard_marcus_odalys_1.l2`
  · (marcus shrug (background))
  · (marcus emote exclaim)
  · (odalys point (background))
  [odalys]: "Measure twice, bend the knees once. That one isn't carpentry. Doctor's orders."
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
  [marcus]: "Dr. Penn. The bench outside your clinic had a loose leg. It's fixed."  ✏ `event.overheard_marcus_odalys_2.l1`
  [odalys]: "I don't sit on that bench. I've never had the time."
  · (odalys shrug (background))
  · (odalys emote ellipsis)
  · (marcus point (background))
  [marcus]: "That's why it worked loose. Nobody sits on it. Five minutes would do it good."  ✏ `event.overheard_marcus_odalys_2.l3`
  [odalys]: "...Five minutes, then. It's a good bench. Elara is not to hear about this."
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
  [odalys]: "Marcus. Your back. Honestly, this time."
  [marcus]: "Better. I rest it Wednesdays. The doctor told me to."  ✏ `event.overheard_marcus_odalys_3.l2`
  · (marcus point (background))
  · (marcus emote heart)
  · (odalys shrug (background))
  [odalys]: "I did say that. You listened. You're the only patient who does, and it ruins my averages."
  [marcus]: "Then take your own advice. Saturdays. I'll build the chair."  ✏ `event.overheard_marcus_odalys_3.l4`
  · effects: flag:event.overheard_marcus_odalys_3
