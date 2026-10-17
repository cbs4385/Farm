# Scene cards for bram

23 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 51 spoken lines in scenes, 22 dialogue lines and 15 player choice labels.

### bram_heart2 · "You Again" · funny
event · npc_roster_core.json · map Blacksmith · priority 6
plays when: `hearts:bram>=2 && !weekday:mon && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. You again. Come here. Stand where the floor's swept."  ✏ `event.bram_heart2.0`
  · (move player to (6,3))
  · (player faces up)
  · (bram emote ellipsis)
  [bram]: "Watch the color. Cherry, not yellow. Yellow is iron showing off."  ✏ `event.bram_heart2.1`
  [bram]: "Took me three winters to see that. My teacher saw it from the door."  ✏ `event.bram_heart2.4`  (expr thinking)
  [bram]: "Hm. Now it listens. Strike. Hear that? That's heat done right."  ✏ `event.bram_heart2.3`
  [bram]: "Here. Copper. Tuition runs backward in this shop."  ✏ `event.bram_heart2.2`
  · effects: give:resource.copperbar,1, friend:bram,40, flag:event.bram_heart2

### bram_heart5 · "Words from Bram" · surprise
event · npc_roster_core.json · map Blacksmith · priority 6
plays when: `hearts:bram>=5 && !weekday:mon && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Wait. I don't say things often. Today I'm saying one."  ✏ `event.bram_heart5.0`
  · (move player to (6,3))
  · (player faces up)
  · (bram emote ellipsis)
  [bram]: "You treat a good tool like a friend. Oil, rag, a dry shelf. Here."  ✏ `event.bram_heart5.1`
  [bram]: "My teacher kept his tools like that. Hm. I should tell him so."  ✏ `event.bram_heart5.3`  (expr thinking)
  [bram]: "Look after them. Iron keeps score."  ✏ `event.bram_heart5.2`
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
  [juno]: "Okay, okay, okay. Eyes shut. Not yet. Not yet... Now!"  (emote sparkle)
  · (spawn prop resource.copperbar at (3,2))
  [juno]: "My own piece, with my own mark. It's a hinge. A plain one. The mark's crooked, and I love it more than breakfast!"  (expr happy)
  · (bram emote ellipsis)
  [bram]: "...Hm."  ✏ `event.next_door.2`
  [bram]: "Shoulder's thin. A hair. The good tongs are on the bench. If you want them."  ✏ `event.next_door.3`
  ▶ DIALOGUE `next_door.talk` (player-facing graph, plays here):
    n0 [juno]: "He brought the good tongs. The good tongs! Do I take them? Say something quick, before I do the opposite!"
       ? "Let Bram help. One heat." → r0  effects: flag:choice.next_door.tongs, friend:juno,5
       ? "Your hands. Your hinge." → r1  effects: flag:choice.next_door.own, friend:juno,5
    r0 [juno]: "One heat, together. ...Okay. Better. It's still mine, though. The tongs only held it."
    r1 [juno]: "My hands. Okay, okay. ...A hair thicker at the shoulder. I did that! Bram, I did that!"
  · (remove prop piece)
  · (bram faces down)
  [bram]: "Juno. {pause=0.4}I'm proud of you."  ✏ `event.next_door.4`  (emote heart)
  [juno]: "...He said it. Out loud, with a witness. I need to sit. The sign hook next door is still empty. The sign's drawn. It's under the coal."  (expr embarrassed)
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
  [marcus]: "I checked the roof once. Twice. ...Three times, my father said. It is finished. Not nearly. Finished."
  · (marcus emote sparkle)
  [wren]: "Look at this room! Full, and the porch is full too. Somebody count the porch. I need a number for the story."  (emote sparkle)
  · (spawn prop artisan.wine at (7,3))
  [wren]: "[player]. I said I'd open it on the night, and this is the night. First glass is yours. I poured it before the doors."  (expr happy)
  · (remove prop wine)
  [tilda]: "Nobody asked for credit for any of this. So I'm giving it, in the good chalk, under every name in this room."
  [piper]: "Same song, bigger room. One verse, then I step off again. My bow hand still hasn't heard."  (emote sweat)
  · (music saloon)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall takes the chorus. Piper stops playing and sways, and the song goes on without her."
  [bram]: "Hm. Hm-hm. Hm. I sing it in my own language."  ✏ `event.hall_dance.7`  (emote note)
  [juno]: "Those door hinges are mine. My mark's on every pin. Listen to it swing. It's showing off, and I'm letting it!"  (expr happy)
  [wren]: "The roof pool is closed! Six winters of coins, and Tilda picked the right year. I'm demanding a recount."
  [tilda]: "Into the hall box, every coin. Except one. Marcus, that one's yours. Spend it on something that isn't wood."
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

### story_whistler_bram · "The Whistler" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:mystery_whistler.bram && hour>=18 && hour<21 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: bram(25, 18), player(25, 17)
  · (place bram to (25,18))
  · (bram faces down)
  [bram]: "{pause=0.5}...Hm. How long have you been standing there?"  ✏ `event.story_whistler_bram.intro`
  · (move player to (25,17))
  · (player faces up)
  · (bram emote ellipsis)
  ▶ DIALOGUE `story.whistler.bram` (player-facing graph, plays here):
    n0 [bram]: "I whistle. Don't tell Wren. She'd count the notes as words."  ✏ `dlg.story.whistler.bram.0`   (→ n1)
    n1 [bram]: "...Well?"  ✏ `dlg.story.whistler.bram.1`
       ? "Wave and say a cheerful hello." → r0  effects: flag:choice.whistler.bram.hello  ✏ `dlg.story.whistler.bram.c0`
       ? "Whistle the wrong note back." → r1  effects: flag:choice.whistler.bram.back, friend:bram,10  ✏ `dlg.story.whistler.bram.c1`
       ? "Say nothing. Just listen." → r2  effects: flag:choice.whistler.bram.listen, friend:bram,10  ✏ `dlg.story.whistler.bram.c2`
    r0 [bram]: "Hm. Hello. Yes. We'll blame the wind. It owes me."  ✏ `dlg.story.whistler.bram.r0`
    r1 [bram]: "Hm? ...That's my wrong note. Twenty years. Nobody's ever caught it."  ✏ `dlg.story.whistler.bram.r1`   (expr embarrassed)
    r2 [bram]: "...Hm. You listened. All of it. Most people leave before the end."  ✏ `dlg.story.whistler.bram.r2`   (expr happy)
  [bram]: "My teacher whistled that tune. Same wrong note. I kept it."  ✏ `event.story_whistler_bram.after`
  · effects: flag:event.story_whistler_bram, friend:bram,30, flag:storydone.mystery_whistler

### story_band_battle · "Battle of the Bands" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:competing_band && hearts:piper>=2 && hearts:juno>=2 && hour>=17 && hour<21 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: piper(25, 18), player(25, 17)
  · (place piper to (25,18))
  · (piper faces down)
  [piper]: "Two bands, one square, one extremely polite war. Judge us, [player]. Be honest. Honest means fiddle."
  · (move player to (25,17))
  · (player faces up)
  · (piper emote note)
  ▶ DIALOGUE `story.band.talk` (player-facing graph, plays here):
    n0 [piper]: "Exhibit A: one fiddle, one voice, a lifetime of practice. Try not to swoon."   (→ n1)
    n1 [juno]: "Exhibit B: three anvils, the wash bucket, and Bram, who has agreed to hit one thing. Once. Gently."   (→ n2)
    n2 [bram]: "Hm. One hit. I'm saving it for the end."  ✏ `dlg.story.band.talk.2`   (→ n3)
    n3 [piper]: "Well? Who owns the square tonight? Take your time. Pick the fiddle."
       ? "The fiddle, obviously." → r0  effects: flag:choice.band.piper
       ? "The forge band, honestly." → r1  effects: flag:choice.band.juno
       ? "Both, together. Loudly." → r2  effects: flag:choice.band.both, friend:piper,10, friend:juno,10
    r0 [piper]: "Justice! And good taste! Juno, you were wonderful. Just less fiddle."   (expr happy)
    r1 [juno]: "We won! Bram, we won! ...Bram? He's hitting the thing. Twice!"   (expr happy)
    r2 [piper]: "Together? ...That's either genius or a noise complaint. Bram, one hit. Make it count."   (emote note)
  [piper]: "{if choice:band.piper}The fiddle wins! Juno took it beautifully. She's already forging a louder anvil.{else}{if choice:band.juno}The forge band wins! I'm thrilled. I'm furious. I'm writing a song about both.{else}A joint set! Fiddle, anvils, and a whole square clapping on the beat. Mostly on the beat.{/if}{/if}"
  [piper]: "And Bram tapped his foot on the second verse. I saw it. He'll deny it until spring."  (emote note, expr happy)
  · effects: flag:event.story_band_battle, friend:piper,20, friend:juno,20, flag:storydone.competing_band

### bram_heart4 · "The Repair" · wholesome
conversation · slice_events.json · map Blacksmith · priority 7
plays when: `hearts:bram>=4 && flag:event.bram_heart2 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. You. Hands free? Good. I need two more."  ✏ `event.bram_heart4.intro`
  · (move player to (6,3))
  · (player faces up)
  · (bram wave (background))
  · (bram emote ellipsis)
  ▶ DIALOGUE `bram.repair` (player-facing graph, plays here):
    n0 [bram]: "Old hoe. Split socket, blade's still good. Hold it, strike it, or watch?"  ✏ `dlg.bram.repair.0`
       ? "Hold it steady for him." → r0  effects: friend:bram,10, flag:choice.bram.repair.hold  ✏ `dlg.bram.repair.c0`
       ? "Ask to swing the hammer." → r1  effects: friend:bram,15, flag:choice.bram.repair.hammer  ✏ `dlg.bram.repair.c1`
       ? "Watch from a safe distance." → r2  effects: friend:bram,10, flag:choice.bram.repair.watch  ✏ `dlg.bram.repair.c2`
    r0 [bram]: "Don't grip. Hold. ...Hm. Like that."  ✏ `dlg.bram.repair.r0`
    r1 [bram]: "Square on. Again. ...Hm. The iron believed you."  ✏ `dlg.bram.repair.r1`   (expr happy)
    r2 [bram]: "Hm. Watch the edge, not me. Juno watches me. Bad habit."  ✏ `dlg.bram.repair.r2`
  [bram]: "It'll hold. Juno would've used half the strikes. Don't tell her."  ✏ `event.bram_heart4.after`
  · effects: flag:event.bram_heart4
  · effects: friend:bram,50

### bram_heart6 · "One Long Sentence" · surprise
confession · slice_events.json · map Blacksmith · priority 7
plays when: `hearts:bram>=6 && flag:event.bram_heart5 && flag:event.bram_heart4 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. Sit. Not on the bucket. This'll be long. For me."  ✏ `event.bram_heart6.intro`
  · (move player to (6,3))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on bram)
  · (bram expression thinking)
  ▶ DIALOGUE `bram.long` (player-facing graph, plays here):
    n0 [bram]: "I worked alone for years, told myself I liked it, lied, and then you kept sitting down. I wrote my teacher thirty letters. Sent none."  ✏ `dlg.bram.long.0`   (expr thinking)
       ? "Say nothing. Nod." → r0  effects: friend:bram,15, flag:choice.bram.long.nod  ✏ `dlg.bram.long.c0`
       ? "Tell him you're glad he said it." → r1  effects: friend:bram,10, flag:choice.bram.long.glad  ✏ `dlg.bram.long.c1`
       ? "Answer with one word." → r2  effects: friend:bram,10, flag:choice.bram.long.word  ✏ `dlg.bram.long.c2`
    r0 [bram]: "...Hm. A nod. That's how my teacher said yes."  ✏ `dlg.bram.long.r0`
    r1 [bram]: "Glad. Hm. Good word. I'll keep it."  ✏ `dlg.bram.long.r1`   (expr happy)
    r2 [bram]: "One word. ...Hm. That's all I ever wanted back."  ✏ `dlg.bram.long.r2`
  · (bram expression neutral)
  [bram]: "That's all of it. The letters stay in the drawer. For now."  ✏ `event.bram_heart6.after`
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
  [bram]: "Hm. Juno made something. On her own. Look at it before I ruin it."  ✏ `event.bram_heart8.intro`
  · (move player to (6,3))
  · (player faces up)
  · (bram wave (background))
  · (bram emote ellipsis)
  ▶ DIALOGUE `bram.juno` (player-facing graph, plays here):
    n0 [bram]: "A horseshoe. Crooked by a thumb. She's out back, pretending not to wait. What do I say?"  ✏ `dlg.bram.juno.0`
       ? "Tell her it's wonderful." → r0  effects: friend:bram,10, flag:choice.bram.juno.praise  ✏ `dlg.bram.juno.c0`
       ? "Tease her about the crook." → r1  effects: friend:bram,10, flag:choice.bram.juno.tease  ✏ `dlg.bram.juno.c1`
       ? "Ask what he sees in it." → r2  effects: friend:bram,15, flag:choice.bram.juno.ask  ✏ `dlg.bram.juno.c2`
    r0 [bram]: "Wonderful. Hm. Crooked and wonderful. Both true. I can say both."  ✏ `dlg.bram.juno.r0`
    r1 [bram]: "Hm. Tease. I can do that. Teasing's just praise with a dent."  ✏ `dlg.bram.juno.r1`   (expr happy)
    r2 [bram]: "...Hm. Me. My first shoe was crooked the same way."  ✏ `dlg.bram.juno.r2`   (expr sad)
  [bram]: "I'll tell her. Today. Or near today."  ✏ `event.bram_heart8.after`
  · effects: flag:event.bram_heart8
  · effects: friend:bram,70

### bram_heart10 · "Soft Gold" · wholesome
heirloom · slice_events.json · map Blacksmith · priority 7
plays when: `hearts:bram>=10 && flag:event.bram_heart8 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. Stand still. Don't speak. It's easier."  ✏ `event.bram_heart10.intro`
  · (move player to (6,3))
  · (player faces up)
  · (spawn prop resource.goldbar at (6,4))
  · (bram emote heart)
  [bram]: "Gold. Soft. Not for work. For you. Hm."  ✏ `event.bram_heart10.give`
  · (sfx coin)
  · effects: give:resource.goldbar,1, flag:event.bram_heart10
  · (remove prop heirloom)
  [bram]: "{if choice:bram.juno.ask}You asked what I see. I see a friend, [player]. Rare metal.{else}...Hm. Saved it nine winters. Didn't know who for. Now I do. [player].{/if}"  ✏ `event.bram_heart10.thanks`
  · effects: friend:bram,80

### bram_friend · "The Bench" · wholesome
shared_activity · slice_events.json · map Blacksmith · priority 7
plays when: `hearts:bram>=3 && weekday:wed && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. Wednesday. Marcus comes for lunch. Late today. Take his half."  ✏ `event.bram_friend.intro`
  · (move player to (6,3))
  · (player faces up)
  · (bram emote ellipsis)
  · (bram nod)
  [bram]: "Bread's yours. Cheese is mine. Silence we split."  ✏ `event.bram_friend.doing`
  [bram]: "Good lunch. Marcus will find crumbs on his half. He'll count them. Twice."  ✏ `event.bram_friend.wrap`
  · effects: flag:event.bram_friend
  · effects: friend:bram,15

### bram_birthday · "Bram's Birthday" · wholesome
conversation · wave4_birthdays.json · map Blacksmith · priority 8 · repeatable
plays when: `hearts:bram>=2 && birthday.in:bram==0 && unseen:birthday.bram && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 3)
  · (bram faces down)
  [bram]: "Hm. Birthday. No fuss. Juno made a card, mostly diagrams. No other mail."  ✏ `event.bram_birthday.intro`
  · (move player to (6,3))
  · (player faces up)
  · (bram wave (background))
  · (bram emote ellipsis)
  ▶ DIALOGUE `bram.birthday` (player-facing graph, plays here):
    n0 [bram]: "You came. Well?"  ✏ `dlg.bram.birthday.0`
       ? "Wish him a good year." → r0  effects: friend:bram,10, flag:choice.bday.bram.wish  ✏ `dlg.bram.birthday.c0`
       ? "Hand him a small gift." → r1  effects: friend:bram,10, flag:choice.bday.bram.gift  ✏ `dlg.bram.birthday.c1`
       ? "Sit on the bench with him." → r2  effects: friend:bram,10, flag:choice.bday.bram.bench  ✏ `dlg.bram.birthday.c2`
    r0 [bram]: "A good year. Hm. I'll forge one. Slowly."  ✏ `dlg.bram.birthday.r0`   (expr happy)
    r1 [bram]: "...Hm. For me? It goes on the shelf. Next to the card."  ✏ `dlg.bram.birthday.r1`   (expr happy)
    r2 [bram]: "Hm. Left side. It's warmer. I checked."  ✏ `dlg.bram.birthday.r2`   (expr happy)
  [bram]: "{if choice:bday.bram.wish}Here. One bar. A good year needs something to start on.{else}{if choice:bday.bram.gift}Your turn. Iron. One bar. Fair trade.{else}Best birthday in years. Here. Iron. Don't make it a thing.{/if}{/if}"  ✏ `event.bram_birthday.after`
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
  [juno]: "Bram! I labeled the tongs! 'Tongs (Left)' and 'Tongs (Right)'. Same tongs."
  [bram]: "Hm. Tongs know which is which. Ask them."  ✏ `event.overheard_juno_bram_1.l2`
  · (bram shrug (background))
  · (bram emote sweat)
  · (juno point (background))
  [juno]: "That's what labels are for! So everyone else knows too!"
  [bram]: "Then peel the one off my apron. I know I'm Bram."  ✏ `event.overheard_juno_bram_1.l4`
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
  [juno]: "Bram. Did you put a second label on my cake? It just says 'Eat'."
  [bram]: "Hm. It was missing a verb."  ✏ `event.overheard_juno_bram_2.l2`
  · (bram shrug (background))
  · (bram emote sweat)
  · (juno wave (background))
  [juno]: "That's the nicest label anyone's ever made me, and I make all the labels!"
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
  [juno]: "Bram! I'm on your bench again. I fixed its wobbly leg myself. The joint's crooked. Don't look."
  [bram]: "Hm. Saw it. Keep it. Every bench needs one honest mistake."  ✏ `event.overheard_juno_bram_3.l2`
  · (bram point (background))
  · (bram emote sweat)
  · (juno shrug (background))
  [juno]: "Wait. Was that a compliment? Bram. Was that a compliment? I need a minute."
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
  [juno]: "Bram. Someday I'll have my own forge. A tiny one. Maybe next door."
  [bram]: "Good. Make it next door. I'll visit. Daily."  ✏ `event.overheard_juno_bram_4.l2`
  · (bram wave (background))
  · (bram emote sweat)
  · (juno wave (background))
  [juno]: "You would? You'd sit on my bench? Even the crooked one?"
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
  [marcus]: "Your weld's clean. Don't tell Juno I said so."
  · (marcus shrug (background))
  · (marcus emote ellipsis)
  · (bram point (background))
  [bram]: "Wouldn't. She'd frame it."  ✏ `event.overheard_bram_marcus_1.l3`
  [marcus]: "Good. Hand me the clamp. Wednesday, same bench."
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
  [marcus]: "Made you one. Oak. Measured your grip while you weren't looking."
  · (marcus shrug (background))
  · (marcus emote ellipsis)
  · (bram wave (background))
  [bram]: "...Hm. Fits like I grew it."  ✏ `event.overheard_bram_marcus_2.l3`
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
  [marcus]: "A hundred and four. All straight. I counted."
  · (marcus point (background))
  · (marcus emote ellipsis)
  · (bram shrug (background))
  [bram]: "Mine were straighter. Yours were more."  ✏ `event.overheard_bram_marcus_3.l3`
  [marcus]: "Measure them. Bring them Wednesday. Lunch goes to the straighter."
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
  [wren]: "Bram! One cider, three words, every night. Today I want a fourth. I've a fish riding on it."
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
  [tilda]: "Bram. There's a pie crust on your bench. For the hinges, the lamp, and the shelf."
  [bram]: "Hm. Hinges were squeaking. Bothered me. That's all."  ✏ `event.overheard_tilda_bram_1.l2`
  · (bram shrug (background))
  · (bram emote ellipsis)
  · (tilda point (background))
  [tilda]: "You never say thank you. I never say you're welcome. That's how the slate stays even."
  [bram]: "...Hm. Even. Good."  ✏ `event.overheard_tilda_bram_1.l4`
  · effects: flag:event.overheard_tilda_bram_1
