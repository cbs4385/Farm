# Scene cards for bram

23 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 48 spoken lines in scenes, 22 dialogue lines and 15 player choice labels.

### bram_heart2 · "You Again" · funny
event · npc_roster_core.json · map Blacksmith · priority 6
plays when: `hearts:bram>=2 && !weekday:mon && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. You again. Come here."  ✏ `event.bram_heart2.0`
  · (move player to (6,3))
  · (player faces up)
  · (bram emote ellipsis)
  [bram]: "Watch the metal. Don't blink. See that? That's heat done right."  ✏ `event.bram_heart2.1`
  [bram]: "Now you know something. Don't tell anyone."  ✏ `event.bram_heart2.2`
  · effects: give:resource.copperbar,1, friend:bram,40, flag:event.bram_heart2

### bram_heart5 · "Words from Bram" · surprise
event · npc_roster_core.json · map Blacksmith · priority 6
plays when: `hearts:bram>=5 && !weekday:mon && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(6, 3)
  · (bram faces down)
  [bram]: "I don't say this often."  ✏ `event.bram_heart5.0`
  · (move player to (6,3))
  · (player faces up)
  · (bram emote ellipsis)
  [bram]: "You're the only one who treats a good tool like a friend. Here."  ✏ `event.bram_heart5.1`
  [bram]: "Look after it."  ✏ `event.bram_heart5.2`
  · effects: give:resource.ironbar,2, friend:bram,60, flag:event.bram_heart5

### next_door · "Next Door" · wholesome
event · spec_scenes.json · map Blacksmith · priority 6
plays when: `flag:event.juno_heart10 && flag:event.bram_heart10 && hearts:juno>=10 && hearts:bram>=10 && !weekday:mon && hour>=9 && hour<17 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: juno(2, 2), bram(6, 3), player(4, 2)
  · (place juno to (2,2))
  · (juno faces right)
  · (place bram to (6,3))
  · (bram faces left)
  · (move player to (4,2))
  · (player faces left)
  [juno]: "Okay, okay, okay. Don't look yet. Don't look. ...Now look."  (emote sparkle)
  · (spawn prop resource.copperbar at (3,2))
  [juno]: "My own piece. My own mark. It's a hinge, a plain one, and the mark is crooked and I love it!"  (expr happy)
  · (bram emote ellipsis)
  [bram]: "Hm."  ✏ `event.next_door.2`
  [bram]: "Shoulder's thin by a hair. Here. I brought the good tongs."  ✏ `event.next_door.3`
  ▶ DIALOGUE `next_door.talk` (player-facing graph, plays here):
    n0 [juno]: "Tongs or no tongs? Say something. Anything. I'll do the opposite if I'm nervous enough!"
       ? "Let Bram bring the tongs." → r0  effects: flag:choice.next_door.tongs, friend:juno,5
       ? "Trust your own hands." → r1  effects: flag:choice.next_door.own, friend:juno,5
    r0 [juno]: "Fine. Fine! One heat, together. ...Okay, that's better. It's still mine. The tongs only held it."
    r1 [juno]: "Own hands. Okay. Okay, okay. ...It's a hair thicker. I did that. Bram, I did that!"
  · (remove prop piece)
  · (bram faces down)
  [bram]: "I'm proud of you, Juno."  ✏ `event.next_door.4`  (emote heart)
  [juno]: "...He said it. Out loud, in front of a witness. I need to sit down. The anvil is fine."  (expr embarrassed)
  · effects: flag:event.next_door, friend:juno,40, friend:bram,40

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
  [tilda]: "The roof's done, the hall's open, and nobody asked for credit. I'm writing it on the slate anyway."
  [piper]: "First verse, then I step off. I'm calm. I'm holding this fiddle like a hot loaf."  (emote sweat)
  · (music piper_theme)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall sings the chorus. Piper has stopped playing, and nobody minds."
  [bram]: "Hm. Hm-hm. I know the tune. Say nothing."  ✏ `event.hall_dance.7`  (emote note)
  [juno]: "The hall door hangs on hinges I made. My mark's on every pin, and it swings like it's showing off!"  (expr happy)
  [wren]: "The roof pool is closed. Seven winters of coins, and Tilda had the right year."
  [tilda]: "Into the hall box it goes. Marcus, take one coin for the ladder."
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

### story_whistler_bram · "The Whistler" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:mystery_whistler.bram && hour>=18 && hour<21 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: bram(25, 18), player(25, 17)
  · (place bram to (25,18))
  · (bram faces down)
  [bram]: "{pause=0.5}...Hm. (A cheerful tune stops. Bram turns, very slowly.)"  ✏ `event.story_whistler_bram.intro`
  · (move player to (25,17))
  · (player faces up)
  · (bram emote ellipsis)
  ▶ DIALOGUE `story.whistler.bram` (player-facing graph, plays here):
    n0 [bram]: "Hm. I whistle. Don't. Tell. Anyone."  ✏ `dlg.story.whistler.bram.0`   (→ n1)
    n1 [bram]: "...Well?"  ✏ `dlg.story.whistler.bram.1`
       ? "Call out a friendly hello." → r0  effects: flag:choice.whistler.bram.hello  ✏ `dlg.story.whistler.bram.c0`
       ? "Whistle the wrong note back." → r1  effects: flag:choice.whistler.bram.back, friend:bram,10  ✏ `dlg.story.whistler.bram.c1`
       ? "Say nothing. Just listen." → r2  effects: flag:choice.whistler.bram.listen, friend:bram,10  ✏ `dlg.story.whistler.bram.c2`
    r0 [bram]: "Hello. Right. Good. Thank you for the normal approach."  ✏ `dlg.story.whistler.bram.r0`
    r1 [bram]: "You... got the wrong note exactly right. I'm moved and mortified."  ✏ `dlg.story.whistler.bram.r1`   (expr embarrassed)
    r2 [bram]: "You listened. To the whole thing. ...I may whistle again. Quietly."  ✏ `dlg.story.whistler.bram.r2`   (expr happy)
  [bram]: "Hm. That wrong note is my signature. I'll be hearing about it."  ✏ `event.story_whistler_bram.after`
  · effects: flag:event.story_whistler_bram, friend:bram,30, flag:storydone.mystery_whistler

### story_band_battle · "Battle of the Bands" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:competing_band && hearts:piper>=2 && hearts:juno>=2 && hour>=17 && hour<21 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: piper(25, 18), player(25, 17)
  · (place piper to (25,18))
  · (piper faces down)
  [piper]: "Two bands, one square, one very polite war. Judge us, [player]. Be honest, and honest means fiddle."
  · (move player to (25,17))
  · (player faces up)
  · (piper emote note)
  ▶ DIALOGUE `story.band.talk` (player-facing graph, plays here):
    n0 [piper]: "Exhibit A: a fiddle, a voice, a lifetime of practice. Watch."   (→ n1)
    n1 [juno]: "Exhibit B: three anvils, a wash bucket and Bram, who has agreed to hit one thing. Once."   (→ n2)
    n2 [bram]: "Hm."  ✏ `dlg.story.band.talk.2`   (→ n3)
    n3 [piper]: "Well? Who owns the square tonight?"
       ? "The fiddle." → r0  effects: flag:choice.band.piper
       ? "The forge band." → r1  effects: flag:choice.band.juno
       ? "Both, together." → r2  effects: flag:choice.band.both, friend:piper,10, friend:juno,10
    r0 [piper]: "Ha! Justice and good taste. Bow, everybody."   (expr happy)
    r1 [juno]: "Okay, okay, okay! Did you hear that, Bram? We won. Bram?"   (expr happy)
    r2 [piper]: "Together? ...That's either genius or a noise complaint. Let's find out."   (emote note)
  [piper]: "{if choice:band.piper}A fiddle wins! Of course. Juno took it well. She's inventing a louder anvil.{else}{if choice:band.juno}The forge band won! I'm thrilled. I'm furious. I'm writing a song about both.{else}A joint set! Fiddle, anvil, one very confused crowd. That's a chorus.{/if}{/if}"
  · effects: flag:event.story_band_battle, friend:piper,20, friend:juno,20, flag:storydone.competing_band

### bram_heart4 · "The Repair" · wholesome
conversation · slice_events.json · map Blacksmith · priority 7
plays when: `hearts:bram>=4 && flag:event.bram_heart2 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. Come here. I need a second pair of hands."  ✏ `event.bram_heart4.intro`
  · (move player to (6,3))
  · (player faces up)
  · (bram wave (background))
  · (bram emote ellipsis)
  ▶ DIALOGUE `bram.repair` (player-facing graph, plays here):
    n0 [bram]: "Hm. The handle's cracked. Hold, hammer, or watch?"  ✏ `dlg.bram.repair.0`
       ? "Hold the handle." → r0  effects: friend:bram,10, flag:choice.bram.repair.hold  ✏ `dlg.bram.repair.c0`
       ? "Take the hammer." → r1  effects: friend:bram,15, flag:choice.bram.repair.hammer  ✏ `dlg.bram.repair.c1`
       ? "Watch and learn." → r2  effects: friend:bram,10, flag:choice.bram.repair.watch  ✏ `dlg.bram.repair.c2`
    r0 [bram]: "Hm. Steady. Good."  ✏ `dlg.bram.repair.r0`
    r1 [bram]: "...Hm. Strike true. Again. Good."  ✏ `dlg.bram.repair.r1`   (expr happy)
    r2 [bram]: "Hm. Watching's a skill. Few have it."  ✏ `dlg.bram.repair.r2`
  [bram]: "Hm. Good enough. Better than good."  ✏ `event.bram_heart4.after`
  · effects: flag:event.bram_heart4
  · effects: friend:bram,50

### bram_heart6 · "One Long Sentence" · surprise
confession · slice_events.json · map Blacksmith · priority 7
plays when: `hearts:bram>=6 && flag:event.bram_heart5 && flag:event.bram_heart4 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. Sit. I've got something to say. It'll be long for me."  ✏ `event.bram_heart6.intro`
  · (move player to (6,3))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on bram)
  · (bram expression thinking)
  ▶ DIALOGUE `bram.long` (player-facing graph, plays here):
    n0 [bram]: "I worked alone a long time, and I decided I liked it, and I was wrong about that, and I'm saying so because you came in and sat down. I wrote my old teacher thirty letters. Sent none."  ✏ `dlg.bram.long.0`   (expr thinking)
       ? "Say nothing. Nod." → r0  effects: friend:bram,15, flag:choice.bram.long.nod  ✏ `dlg.bram.long.c0`
       ? "Tell him you're glad." → r1  effects: friend:bram,10, flag:choice.bram.long.glad  ✏ `dlg.bram.long.c1`
       ? "Answer with one word." → r2  effects: friend:bram,10, flag:choice.bram.long.word  ✏ `dlg.bram.long.c2`
    r0 [bram]: "...Hm. Good. You understood. Fine."  ✏ `dlg.bram.long.r0`
    r1 [bram]: "Hm. Glad. Hm. Yes. Me too."  ✏ `dlg.bram.long.r1`   (expr happy)
    r2 [bram]: "One word. ...Hm. Right size."  ✏ `dlg.bram.long.r2`
  · (bram expression neutral)
  [bram]: "Hm. That's all of it. Don't ask me to repeat it."  ✏ `event.bram_heart6.after`
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.bram_heart6
  · effects: friend:bram,60

### bram_heart8 · "Juno's Piece" · funny
conversation · slice_events.json · map Blacksmith · priority 7
plays when: `hearts:bram>=8 && flag:event.bram_heart6 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. Juno made something. I want you to look at it. Tell me true."  ✏ `event.bram_heart8.intro`
  · (move player to (6,3))
  · (player faces up)
  · (bram wave (background))
  · (bram emote ellipsis)
  ▶ DIALOGUE `bram.juno` (player-facing graph, plays here):
    n0 [bram]: "She made a horseshoe. Crooked. She's proud. What do I say?"  ✏ `dlg.bram.juno.0`
       ? "Say it's wonderful." → r0  effects: friend:bram,10, flag:choice.bram.juno.praise  ✏ `dlg.bram.juno.c0`
       ? "Tease the crookedness." → r1  effects: friend:bram,10, flag:choice.bram.juno.tease  ✏ `dlg.bram.juno.c1`
       ? "Ask what he sees in it." → r2  effects: friend:bram,15, flag:choice.bram.juno.ask  ✏ `dlg.bram.juno.c2`
    r0 [bram]: "Hm. Wonderful. It's crooked. Still wonderful."  ✏ `dlg.bram.juno.r0`
    r1 [bram]: "Ha. Hm. I'll tease her. Gently. With a nail."  ✏ `dlg.bram.juno.r1`   (expr happy)
    r2 [bram]: "...Hm. Me. When I started. That's what I see."  ✏ `dlg.bram.juno.r2`   (expr sad)
  [bram]: "Hm. Thanks. I'll tell her. Maybe."  ✏ `event.bram_heart8.after`
  · effects: flag:event.bram_heart8
  · effects: friend:bram,70

### bram_heart10 · "Soft Gold" · wholesome
heirloom · slice_events.json · map Blacksmith · priority 7
plays when: `hearts:bram>=10 && flag:event.bram_heart8 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. I made you something. Don't speak. Just take it."  ✏ `event.bram_heart10.intro`
  · (move player to (6,3))
  · (player faces up)
  · (spawn prop resource.goldbar at (6,4))
  · (bram emote heart)
  [bram]: "Gold. Soft. Not for work. For you. Hm."  ✏ `event.bram_heart10.give`
  · (sfx coin)
  · effects: give:resource.goldbar,1, flag:event.bram_heart10
  · (remove prop heirloom)
  [bram]: "{if choice:bram.juno.ask}You asked what I see. I see you. Hm.{else}Hm. It's not much. It's all I could say.{/if}"  ✏ `event.bram_heart10.thanks`
  · effects: friend:bram,80

### bram_friend · "The Bench" · wholesome
shared_activity · slice_events.json · map Blacksmith · priority 7
plays when: `hearts:bram>=3 && weekday:wed && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. Marcus is closed. We sit. Come on."  ✏ `event.bram_friend.intro`
  · (move player to (6,3))
  · (player faces up)
  · (bram emote ellipsis)
  · (bram nod)
  [bram]: "Hm. Good bench. Good silence. Don't spoil it."  ✏ `event.bram_friend.doing`
  [bram]: "Hm. That was nice. Don't tell anyone."  ✏ `event.bram_friend.wrap`
  · effects: flag:event.bram_friend
  · effects: friend:bram,15

### bram_birthday · "Bram's Birthday" · wholesome
conversation · wave4_birthdays.json · map Blacksmith · priority 8 · repeatable
plays when: `hearts:bram>=2 && birthday.in:bram==0 && unseen:birthday.bram && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. Birthday. Don't make a fuss. Juno made a card. Mostly diagrams. Hm."  ✏ `event.bram_birthday.intro`
  · (move player to (6,3))
  · (player faces up)
  · (bram wave (background))
  · (bram emote ellipsis)
  ▶ DIALOGUE `bram.birthday` (player-facing graph, plays here):
    n0 [bram]: "You came. Well?"  ✏ `dlg.bram.birthday.0`
       ? "Wish him a good year." → r0  effects: friend:bram,10, flag:choice.bday.bram.wish  ✏ `dlg.bram.birthday.c0`
       ? "Hand him a small gift." → r1  effects: friend:bram,10, flag:choice.bday.bram.gift  ✏ `dlg.bram.birthday.c1`
       ? "Sit on the bench with him." → r2  effects: friend:bram,10, flag:choice.bday.bram.bench  ✏ `dlg.bram.birthday.c2`
    r0 [bram]: "Hm. Good year. I'll try. Thank you."  ✏ `dlg.bram.birthday.r0`   (expr happy)
    r1 [bram]: "...Hm. For me? Hm. It's a good one. Thank you."  ✏ `dlg.bram.birthday.r1`   (expr happy)
    r2 [bram]: "Hm. Good. That's all I want. Sit."  ✏ `dlg.bram.birthday.r2`   (expr happy)
  [bram]: "{if choice:bday.bram.wish}A good year. Hm. I'll aim for one. Thanks.{else}{if choice:bday.bram.gift}Hm. The gift sits on the shelf. I look at it. Good.{else}A silent birthday, on a bench. The best kind. Hm.{/if}{/if}"  ✏ `event.bram_birthday.after`
  · effects: flag:event.bram_birthday
  · effects: mark:birthday.bram, friend:bram,30, give:resource.ironbar,1, mail:bram_bday_thanks

### overheard_juno_bram_1 · "Labeled Tongs" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.juno && flag:met.bram && hearts:juno>=2 && hearts:bram>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), bram(26, 18)
  · (place juno to (24,18))
  · (place bram to (26,18))
  · (juno faces right)
  · (bram faces left)
  [juno]: "Bram! I labeled the tongs. 'Tongs (Left)'. 'Tongs (Right)'. They're the same tongs."
  [bram]: "Hm. They've always known which is which."  ✏ `event.overheard_juno_bram_1.l2`
  · (bram shrug (background))
  · (bram emote sweat)
  · (juno point (background))
  [juno]: "That's what the labels are for! So everyone knows!"
  [bram]: "Hm. Everyone knows. Take the label off my hat."  ✏ `event.overheard_juno_bram_1.l4`
  · effects: flag:event.overheard_juno_bram_1

### overheard_juno_bram_2 · "The Second Label" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.juno && flag:met.bram && hearts:juno>=4 && hearts:bram>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_juno_bram_1`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), bram(26, 18)
  · (place juno to (24,18))
  · (place bram to (26,18))
  · (juno faces right)
  · (bram faces left)
  [juno]: "Bram. Did you add a label to my cake? It says 'Eat'."
  [bram]: "Hm. It was missing a verb."  ✏ `event.overheard_juno_bram_2.l2`
  · (bram shrug (background))
  · (bram emote sweat)
  · (juno wave (background))
  [juno]: "It's the nicest label anyone's made me, and I made the labels!"
  [bram]: "Hm. Don't cry on the clasp. It'll rust."  ✏ `event.overheard_juno_bram_2.l4`
  · effects: flag:event.overheard_juno_bram_2

### overheard_juno_bram_3 · "The Apprentice Bench" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.juno && flag:met.bram && hearts:juno>=6 && hearts:bram>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_juno_bram_2`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), bram(26, 18)
  · (place juno to (24,18))
  · (place bram to (26,18))
  · (juno faces right)
  · (bram faces left)
  [juno]: "Bram! I'm on your bench again, and I carved a small mistake into it. Don't look."
  [bram]: "Hm. Saw it. Best joint in the shop. Don't change it."  ✏ `event.overheard_juno_bram_3.l2`
  · (bram point (background))
  · (bram emote sweat)
  · (juno shrug (background))
  [juno]: "Wait. Was that a compliment? Bram, was that a compliment? I need a minute."
  [bram]: "Hm. Take the minute. Move over."  ✏ `event.overheard_juno_bram_3.l4`
  · effects: flag:event.overheard_juno_bram_3

### overheard_juno_bram_4 · "The Tiny Forge" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.juno && flag:met.bram && hearts:juno>=8 && hearts:bram>=8 && flag:event.juno_heart10 && flag:event.bram_heart10 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_juno_bram_3`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), bram(26, 18)
  · (place juno to (24,18))
  · (place bram to (26,18))
  · (juno faces right)
  · (bram faces left)
  [juno]: "Bram. Someday I'll open my own forge. A tiny one!"
  [bram]: "Hm. Good. I'd come visit."  ✏ `event.overheard_juno_bram_4.l2`
  · (bram wave (background))
  · (bram emote sweat)
  · (juno wave (background))
  [juno]: "You would? You'd sit on my bench?"
  [bram]: "Hm. Move over. I'll bring the good tongs."  ✏ `event.overheard_juno_bram_4.l4`
  · effects: flag:event.overheard_juno_bram_4

### overheard_bram_marcus_1 · "The Two Trades" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.bram && flag:met.marcus && hearts:bram>=2 && hearts:marcus>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: bram(24, 18), marcus(26, 18)
  · (place bram to (24,18))
  · (place marcus to (26,18))
  · (bram faces right)
  · (marcus faces left)
  [bram]: "Hm. Your joint's tight."  ✏ `event.overheard_bram_marcus_1.l1`
  [marcus]: "Your weld's clean. Don't tell Juno."
  · (marcus shrug (background))
  · (marcus emote ellipsis)
  · (bram point (background))
  [bram]: "Hm. Wouldn't."  ✏ `event.overheard_bram_marcus_1.l3`
  [marcus]: "Good. Hand me the clamp."
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
  [bram]: "Hm. Handle for the big hammer?"  ✏ `event.overheard_bram_marcus_2.l1`
  [marcus]: "Made you one. Oak. Measured twice. Your grip's in the shop."
  · (marcus shrug (background))
  · (marcus emote ellipsis)
  · (bram wave (background))
  [bram]: "Hm. It fits."  ✏ `event.overheard_bram_marcus_2.l3`
  [marcus]: "Good. A handle should vanish in the hand."
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
  [bram]: "Hm. A hundred nails today."  ✏ `event.overheard_bram_marcus_3.l1`
  [marcus]: "A hundred and four. All straight."
  · (marcus point (background))
  · (marcus emote ellipsis)
  · (bram shrug (background))
  [bram]: "Hm. Mine were straighter."  ✏ `event.overheard_bram_marcus_3.l3`
  [marcus]: "Measure them. Then we'll know."
  · effects: flag:event.overheard_bram_marcus_3

### overheard_wren_bram_1 · "The Fourth Word" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.bram && hearts:wren>=2 && hearts:bram>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), bram(26, 18)
  · (place wren to (24,18))
  · (place bram to (26,18))
  · (wren faces right)
  · (bram faces left)
  [wren]: "Bram! Another day of our bet. One cider, three words. Give me a fourth."
  [bram]: "Hm. Cider. Please. Thanks."  ✏ `event.overheard_wren_bram_1.l2`
  · (bram shrug (background))
  · (bram emote sweat)
  · (wren point (background))
  [wren]: "That's three. 'Hm' doesn't count. Between us, it never has."
  [bram]: "Hm."  ✏ `event.overheard_wren_bram_1.l4`
  · effects: flag:event.overheard_wren_bram_1

### overheard_wren_bram_2 · "The Unsent Letter" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.bram && hearts:wren>=4 && hearts:bram>=4 && flag:event.bram_heart6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_bram_1`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), bram(26, 18)
  · (place wren to (24,18))
  · (place bram to (26,18))
  · (wren faces right)
  · (bram faces left)
  [wren]: "You're writing a letter at my bar, Bram Hollis. Who's it for?"
  [bram]: "Old teacher. Won't send it."  ✏ `event.overheard_wren_bram_2.l2`
  · (bram shrug (background))
  · (bram emote ellipsis)
  · (wren point (background))
  [wren]: "Then let me read it. I'll improve it. That's my gift."
  [bram]: "Hm. No. ...Maybe the ending."  ✏ `event.overheard_wren_bram_2.l4`
  · effects: flag:event.overheard_wren_bram_2

### overheard_wren_bram_3 · "Five Words" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.bram && hearts:wren>=6 && hearts:bram>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_bram_2 && flag:event.wren_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), bram(26, 18)
  · (place wren to (24,18))
  · (place bram to (26,18))
  · (wren faces right)
  · (bram faces left)
  [wren]: "Bram. If I ever left Wetherell, would you notice?"
  [bram]: "Yes. Cider'd be worse."  ✏ `event.overheard_wren_bram_3.l2`
  · (bram shrug (background))
  · (bram emote exclaim)
  · (wren wave (background))
  [wren]: "Four words! Four! Felix owes me a fish."
  [bram]: "Hm. Don't go far."  ✏ `event.overheard_wren_bram_3.l4`
  · effects: flag:event.overheard_wren_bram_3

### overheard_tilda_bram_1 · "The Even Slate" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.bram && hearts:tilda>=3 && hearts:bram>=3 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), bram(26, 18)
  · (place tilda to (24,18))
  · (place bram to (26,18))
  · (tilda faces right)
  · (bram faces left)
  [tilda]: "Bram. There's a crust on your bench. For the hinges, the lamp, and the shelf you fixed unasked."
  [bram]: "Hm. Didn't need one."  ✏ `event.overheard_tilda_bram_1.l2`
  · (bram shrug (background))
  · (bram emote ellipsis)
  · (tilda point (background))
  [tilda]: "You never say thank you. I never say it back. That's how the slate stays even."
  [bram]: "...Hm. Even. Good."  ✏ `event.overheard_tilda_bram_1.l4`
  · effects: flag:event.overheard_tilda_bram_1
