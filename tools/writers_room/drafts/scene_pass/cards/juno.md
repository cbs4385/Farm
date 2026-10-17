# Scene cards for juno

25 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 58 spoken lines in scenes, 36 dialogue lines and 23 player choice labels.

### juno_heart2 · "Hold the Tongs" · funny
event · npc_roster.json · map Blacksmith · priority 6
plays when: `(hearts:juno>=2 && !weekday:mon && hour>=9 && hour<17) && !flag:event.juno_heart2`
beats may be ADDED or CUT in this scene
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay, hold the tongs, [player]. Both hands. They bite if you're shy."  ✏ `event.juno_heart2.0`
  · (move player to (2,2))
  · (player faces up)
  · (juno emote sparkle)
  [juno]: "See that color? Cherry going to straw. That's the moment. Bram calls it 'now'. I call it 'NOW!'"  ✏ `event.juno_heart2.1`
  [juno]: "Quench it! ...Hear that hiss? That's the bar saying thank you. Or ow. Same word, in iron."  ✏ `event.juno_heart2.3`  (expr happy)
  [juno]: "Bram taught me the hiss trick my first week. I screamed. Professionally."  ✏ `event.juno_heart2.5`  (expr embarrassed)
  [juno]: "Here. Your first bars. Two, because one looked lonely."  ✏ `event.juno_heart2.2`
  [juno]: "Bram's mark is on every bar in here. Mine is thirty sketches under the coal bin. One of them is right. I haven't met it yet."  ✏ `event.juno_heart2.4`  (expr thinking)
  · effects: give:resource.copperbar,2, friend:juno,40, flag:event.juno_heart2

### juno_heart5 · "Finally Finished" · wholesome
event · npc_roster.json · map Blacksmith · priority 7
plays when: `(hearts:juno>=5 && !weekday:mon && hour>=9 && hour<17 && flag:event.juno_heart2) && !flag:event.juno_heart5`
beats may be ADDED or CUT in this scene
placed: player(2, 2)
  · (juno faces down)
  [juno]: "I finished it! I finished it! Eleven evenings. Bram fixed one rivet, and I pretended not to see."  ✏ `event.juno_heart5.0`
  · (move player to (2,2))
  · (player faces up)
  · (juno emote sparkle)
  [juno]: "A steel sword. For the mines, or the weeds, or looking very serious on your porch. It's yours."  ✏ `event.juno_heart5.1`
  [juno]: "Swing it once? Here, where I can see. ...There. That's the sound. That's the exact sound I drew."  ✏ `event.juno_heart5.4`  (emote sparkle, expr happy)
  [juno]: "I polished the flat until it showed my nose. Then my whole face. Then Bram behind me, nodding at it."  ✏ `event.juno_heart5.3`  (expr embarrassed)
  [juno]: "Don't lose it! ...It's still Bram's pattern. Every line. The next one will be mine, once I find out what mine looks like."  ✏ `event.juno_heart5.2`
  · effects: give:tool.sword_steel,1, friend:juno,60, flag:event.juno_heart5

### next_door · "Next Door" · wholesome
event · spec_scenes.json · map Blacksmith · priority 6
plays when: `(flag:event.juno_heart10 && flag:event.bram_heart10 && hearts:juno>=10 && hearts:bram>=10 && !weekday:mon && hour>=9 && hour<17 && !festival.in:==0) && !flag:event.next_door`
beats may be ADDED or CUT in this scene
placed: juno(2, 2), bram(6, 3), player(4, 2)
  · (place juno to (2,2))
  · (juno faces right)
  · (place bram to (6,3))
  · (bram faces left)
  · (move player to (4,2))
  · (player faces left)
  [juno]: "Okay, okay, okay. Eyes shut. Not yet. Not yet... Now!"  ✏ `event.next_door.0`  (emote sparkle)
  · (spawn prop resource.copperbar at (3,2))
  [juno]: "My own piece, with my own mark. It's a hinge. A plain one. The mark's crooked, and I love it more than breakfast!"  ✏ `event.next_door.1`  (expr happy)
  · (bram emote ellipsis)
  [bram]: "...Hm."
  [bram]: "Shoulder's thin. A hair. The good tongs are on the bench. If you want them."
  ▶ DIALOGUE `next_door.talk` (player-facing graph, plays here):
    n0 [juno]: "He brought the good tongs. The good tongs! Do I take them? Say something quick, before I do the opposite!"  ✏ `dlg.next_door.talk.0`
       ? "Let Bram help. One heat." → r0  effects: flag:choice.next_door.tongs, friend:juno,5  ✏ `dlg.next_door.talk.c0`
       ? "Your hands. Your hinge." → r1  effects: flag:choice.next_door.own, friend:juno,5  ✏ `dlg.next_door.talk.c1`
    r0 [juno]: "One heat, together. ...Okay. Better. It's still mine, though. The tongs only held it."  ✏ `dlg.next_door.talk.r0`
    r1 [juno]: "My hands. Okay, okay. ...A hair thicker at the shoulder. I did that! Bram, I did that!"  ✏ `dlg.next_door.talk.r1`
  · (remove prop piece)
  · (bram faces down)
  [bram]: "Juno. {pause=0.4}I'm proud of you."  (emote heart)
  [juno]: "...He said it. Out loud, with a witness. I need to sit on something official. The sign for next door is already drawn. It's under the coal, with everything else I'm not ready for."  ✏ `event.next_door.5`  (expr embarrassed)
  · effects: flag:event.next_door, friend:juno,40, friend:bram,40

### hall_dance · "Opening Night" · wholesome
event · spec_scenes.json · map CommunityHall · priority 8
plays when: `(flag:hall.restored && flag:hall.roof && quest:wren_full_house=done && flag:event.piper_heart10 && flag:event.juno_heart10 && hour>=18 && hour<23 && !festival.in:==0) && !flag:event.hall_dance`
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
  [wren]: "[player]. I saved this bottle for a night the hall was full. First glass is yours. Poured before the doors, so nobody could argue."  (expr happy)
  · (remove prop wine)
  [tilda]: "Nobody asked for credit for any of this. So I'm giving it, in the good chalk, under every name in this room."
  [piper]: "Same song, bigger room. One verse, then I step off again. My bow hand still hasn't heard."  (emote sweat)
  · (music piper_theme)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall takes the chorus. Piper stops playing and sways, and the song goes on without her."
  [bram]: "Hm. Hm-hm. Hm. I sing it in my own language."  (emote note)
  [juno]: "Those door hinges are mine. My mark's on every pin. Listen to it swing. It's showing off, and I'm letting it!"  ✏ `event.hall_dance.8`  (expr happy)
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

### story_notes_juno · "The Note Writer" · wholesome
event · storyline_events.json · map Village · priority 7
plays when: `(flag:notes.began && storyline:anonymous_notes.juno && hearts:juno>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.story_notes_juno`
beats may be ADDED or CUT in this scene
placed: juno(25, 18), player(25, 17)
  · (place juno to (25,18))
  · (juno faces down)
  · (spawn prop prop.note at (26,18))
  [juno]: "Don't look! ...Okay, you looked. Fine. Encouragement leaks out of me. I can't plug it."  ✏ `event.story_notes_juno.intro`
  · (move player to (25,17))
  · (player faces up)
  · (juno emote sparkle)
  · (juno shrug (background))
  ▶ DIALOGUE `story.notes.juno` (player-facing graph, plays here):
    n0 [narration]: "Juno has a folded note halfway under a door. She stops. So do you."  ✏ `dlg.story.notes.juno.0`   (→ n1)
    n1 [juno]: "...So. Now you know. Fifty-one notes, all me. Pencil, so anyone can pretend they don't know."  ✏ `dlg.story.notes.juno.1`
       ? "Say nothing. Just smile." → r0  effects: flag:choice.notes.juno.smile  ✏ `dlg.story.notes.juno.c0`
       ? "Thank her for them." → r1  effects: flag:choice.notes.juno.thanks, friend:juno,10  ✏ `dlg.story.notes.juno.c1`
       ? "Write her one back." → r2  effects: flag:choice.notes.juno.note, friend:juno,15  ✏ `dlg.story.notes.juno.c2`
    r0 [juno]: "...You smiled. Okay. That's a lot of kindness for one face to hold."  ✏ `dlg.story.notes.juno.r0`   (expr embarrassed)
    r1 [juno]: "Oh. Oh no, that's the whole reward. I'm going to go be flustered behind the rain barrel."  ✏ `dlg.story.notes.juno.r1`   (expr happy)
    r2 [juno]: "A note? For me? ...I'm keeping it. In a drawer I'm inventing right now. Label pending."  ✏ `dlg.story.notes.juno.r2`   (expr happy)
  [juno]: "Bram's had six. He keeps every one in his apron pocket. He thinks they're from Tilda. Let him."  ✏ `event.story_notes_juno.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_notes_juno, friend:juno,40, flag:storydone.anonymous_notes

### story_band_battle · "Battle of the Bands" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `(storyline:competing_band && hearts:piper>=2 && hearts:juno>=2 && hour>=17 && hour<21 && !festival.in:==0) && !flag:event.story_band_battle`
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
    n1 [juno]: "Exhibit B: three anvils, the wash bucket, and Bram, who has agreed to hit one thing. Once. Gently."  ✏ `dlg.story.band.talk.1`   (→ n2)
    n2 [bram]: "Hm. One hit. I'm saving it for the end."   (→ n3)
    n3 [piper]: "Well? Who owns the square tonight? Take your time. Pick the fiddle."
       ? "The fiddle, obviously." → r0  effects: flag:choice.band.piper
       ? "The forge band, honestly." → r1  effects: flag:choice.band.juno
       ? "Both, together. Loudly." → r2  effects: flag:choice.band.both, friend:piper,10, friend:juno,10
    r0 [piper]: "Justice! And good taste! Juno, you were wonderful. Just less fiddle."   (expr happy)
    r1 [juno]: "We won! Bram, we won! ...Bram? He's hitting the thing. Twice!"  ✏ `dlg.story.band.talk.r1`   (expr happy)
    r2 [piper]: "Together? ...That's either genius or a noise complaint. Bram, one hit. Make it count."   (emote note)
  [piper]: "{if choice:band.piper}The fiddle wins! Juno took it beautifully. She's already forging a louder anvil.{else}{if choice:band.juno}The forge band wins! I'm thrilled. I'm furious. I'm writing a song about both.{else}A joint set! Fiddle, anvils, and a whole square clapping on the beat. Mostly on the beat.{/if}{/if}"
  [piper]: "And Bram tapped his foot on the second verse. I saw it. He'll deny it until spring."  (emote note, expr happy)
  · effects: flag:event.story_band_battle, friend:piper,20, friend:juno,20, flag:storydone.competing_band

### juno_heart4 · "The Label Maker" · funny
prank · wave2_events.json · map Blacksmith · priority 7
plays when: `(hearts:juno>=4 && flag:event.juno_heart2 && !weekday:mon && hour>=9 && hour<17) && !flag:event.juno_heart4`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Freeze! Right there. Don't move. I have a surprise, and it's very sticky."  ✏ `event.juno_heart4.setup`
  · (move player to (2,2))
  · (player faces up)
  · (juno jiggle (background))
  · (sfx click)
  · (camera shake)
  · (player emote exclaim)
  [juno]: "Done! You're labeled! 'Guest (Excellent)'. I agonized over the parentheses. They're correct."  ✏ `event.juno_heart4.prank`
  ▶ DIALOGUE `juno.label` (player-facing graph, plays here):
    n0 [juno]: "So? Say something. Any answer except 'Hm.' I get enough of that at work."  ✏ `dlg.juno.label.0`
       ? "Wear it proudly." → r0  effects: friend:juno,10, flag:choice.label.accept  ✏ `dlg.juno.label.c0`
       ? "Relabel her right back." → r1  effects: friend:juno,10, flag:choice.label.relabel  ✏ `dlg.juno.label.c1`
       ? "Eat the label." → r2  effects: friend:juno,10, flag:choice.label.eat  ✏ `dlg.juno.label.c2`
    r0 [juno]: "Accepted! You're officially a fixture. I'll sweep around you and dust you on Fridays."  ✏ `dlg.juno.label.r0`   (expr happy)
    r1 [juno]: "Relabel me? Brilliant. Here's the pencil. ...'Apprentice (Chaotic)'. Fair. Stick it right here."  ✏ `dlg.juno.label.r1`   (expr happy)
    r2 [juno]: "You... ate it. You ate my label. ...I'm labeling this moment 'Do Not Repeat'. Please repeat it."  ✏ `dlg.juno.label.r2`   (emote sweat)
  [juno]: "{if choice:label.accept}A label that stays! Next I'm doing the whole village. The hall roof gets 'Nearly (Fixed)'.{else}{if choice:label.relabel}'Apprentice (Chaotic)', on my forehead, all day. Bram read it and moved the tongs out of my reach.{else}Nobody's ever eaten a label! I'm thrilled. I'm a little worried. I'm making a drawer that says 'Not Food'.{/if}{/if}"  ✏ `event.juno_heart4.reaction`
  · effects: flag:event.juno_heart4
  · effects: friend:juno,50

### juno_heart6 · "First Strike" · wholesome
confession · wave2_events.json · map Blacksmith · priority 7
plays when: `(hearts:juno>=6 && flag:event.juno_heart5 && flag:event.juno_heart4 && !weekday:mon && hour>=9 && hour<17) && !flag:event.juno_heart6`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Don't come in. Okay, come in. Shut the door. I made something, it's crooked, and the quench barrel is looking at it."  ✏ `event.juno_heart6.intro`
  · (move player to (2,2))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on juno)
  · (juno expression thinking)
  ▶ DIALOGUE `juno.strike` (player-facing graph, plays here):
    n0 [juno]: "My first piece that isn't a copy of anything. A little horseshoe. It's lopsided. I made it. I hate how much I love it."  ✏ `dlg.juno.strike.0`   (→ n1; expr sad)
    n1 [juno]: "So. Keep it, or the barrel? Quick, before my arm decides."  ✏ `dlg.juno.strike.1`
       ? "Keep it. It's yours." → r0  effects: friend:juno,10, flag:choice.strike.keep  ✏ `dlg.juno.strike.c0`
       ? "Let's fix the bend together." → r1  effects: friend:juno,15, flag:choice.strike.together  ✏ `dlg.juno.strike.c1`
       ? "Laugh about it with her." → r2  effects: friend:juno,10, flag:choice.strike.laugh  ✏ `dlg.juno.strike.c2`
    r0 [juno]: "Keep it. Yes. Even crooked. ...Especially crooked."  ✏ `dlg.juno.strike.r0`   (expr happy)
    r1 [juno]: "Together? You hold the tongs, like the first day. I tap the bend. Ready? Go!"  ✏ `dlg.juno.strike.r1`   (expr happy)
    r2 [juno]: "It's very funny! A horseshoe for a three-legged horse. A very proud three-legged horse."  ✏ `dlg.juno.strike.r2`   (expr happy)
  · (juno expression neutral)
  [juno]: "It's still crooked. It's also still mine. That's new. ...Don't tell Bram yet. I have to pick the right day."  ✏ `event.juno_heart6.after`
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.juno_heart6
  · effects: friend:juno,60

### juno_heart8 · "The Horseshoe" · surprise
conversation · wave2_events.json · map Blacksmith · priority 7
plays when: `(hearts:juno>=8 && flag:event.juno_heart6 && !weekday:mon && hour>=9 && hour<17) && !flag:event.juno_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay. Today's the right day. I'm showing Bram the horseshoe. Stand next to me? I sweat less with company."  ✏ `event.juno_heart8.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno wave (background))
  · (juno emote ellipsis)
  ▶ DIALOGUE `juno.shoe` (player-facing graph, plays here):
    n0 [narration]: "Bram stands at the far end of the forge. Juno holds the horseshoe out in both hands and keeps her eyes on the floor."  ✏ `dlg.juno.shoe.0`   (→ n1)
    n1 [juno]: "I can't look. What's his face doing? Is it the 'Hm' face or the other 'Hm' face?"  ✏ `dlg.juno.shoe.1`
       ? "Wait with her, quietly." → r0  effects: friend:juno,15, flag:choice.shoe.wait  ✏ `dlg.juno.shoe.c0`
       ? "Tell her it's wonderful." → r1  effects: friend:juno,10, flag:choice.shoe.praise  ✏ `dlg.juno.shoe.c1`
       ? "Hand her the hammer." → r2  effects: friend:juno,10, flag:choice.shoe.hammer  ✏ `dlg.juno.shoe.c2`
    r0 [juno]: "...Okay. Quiet's good. The anvil's being quiet too. Everyone is being so quiet."  ✏ `dlg.juno.shoe.r0`
    r1 [juno]: "Wonderful? It's crooked! ...But okay. I'm looking up now. Slowly. Very slowly."  ✏ `dlg.juno.shoe.r1`   (expr happy)
    r2 [juno]: "The hammer? Okay. One more tap. For luck. For me. For crooked things everywhere."  ✏ `dlg.juno.shoe.r2`   (expr happy)
  [juno]: "{if choice:shoe.wait}You waited. He's holding it now. He turned it over twice and hasn't said a word. It's the best word.{else}{if choice:shoe.praise}You said wonderful, and he's frowning at it like he agrees. That's how Bram says yes.{else}One last tap on the bend. It's better, and it's still mine. He hasn't put it down.{/if}{/if}"  ✏ `event.juno_heart8.after`
  · effects: flag:event.juno_heart8
  · effects: friend:juno,70

### juno_heart10 · "Her Mark" · wholesome
heirloom · wave2_events.json · map Blacksmith · priority 7
plays when: `(hearts:juno>=10 && flag:event.juno_heart8 && !weekday:mon && hour>=9 && hour<17) && !flag:event.juno_heart10`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Hold out your hands. Both. I cut my own mark last night. A little anvil with a J hiding in it. You're first."  ✏ `event.juno_heart10.intro`
  · (move player to (2,2))
  · (player faces up)
  · (spawn prop resource.copperbar at (2,3))
  · (juno emote heart)
  [juno]: "First piece with my mark. A copper bar. {if quest:juno_gold=done}Copper on purpose: your gold went into the die. {/if}It weighs nothing. It's the heaviest thing I've ever handed anyone."  ✏ `event.juno_heart10.give`
  · (sfx coin)
  · effects: give:resource.copperbar,1, flag:event.juno_heart10
  · (remove prop heirloom)
  [juno]: "{if choice:shoe.wait}Bram saw the mark. He waited, like you did, then nodded once. Once! Next door has an empty sign hook.{else}Bram saw the mark and said 'Hm.' The good 'Hm.' I'd know. Next door has an empty sign hook.{/if}"  ✏ `event.juno_heart10.thanks`
  · effects: friend:juno,80

### juno_friend · "The Swimming Pool" · funny
shared_activity · wave2_events.json · map Blacksmith · priority 7
plays when: `(hearts:juno>=3 && weekday:thu && !weekday:mon && hour>=9 && hour<17) && !flag:event.juno_friend`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Quick, quick! Come see the swimming pool. It's a bucket. Wren named it. I made the sign."  ✏ `event.juno_friend.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno emote sparkle)
  · (juno hop)
  [juno]: "See? 'Swimming Pool (Official)'. Quench tried it once. Quench has opinions now."  ✏ `event.juno_friend.doing`
  [juno]: "Best pool in Wetherell. Only pool in Wetherell. Still a bucket. Thursdays are swim day, if you want a lane."  ✏ `event.juno_friend.wrap`
  · effects: flag:event.juno_friend
  · effects: friend:juno,15

### juno_birthday · "Juno's Birthday" · funny
conversation · wave4_birthdays.json · map Blacksmith · priority 8 · repeatable
plays when: `hearts:juno>=2 && birthday.in:juno==0 && unseen:birthday.juno && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay, it's my birthday and I'm not nervous! I labeled the cake 'Cake (Birthday)', in case of confusion. Come in!"  ✏ `event.juno_birthday.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno wave (background))
  · (juno emote sparkle)
  ▶ DIALOGUE `juno.birthday` (player-facing graph, plays here):
    n0 [juno]: "So? What's the plan? There's a plan, right? Tell me there's a plan!"  ✏ `dlg.juno.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:juno,10, flag:choice.bday.juno.wish  ✏ `dlg.juno.birthday.c0`
       ? "'Cake (Birthday)'? Really?" → r1  effects: friend:juno,10, flag:choice.bday.juno.tease  ✏ `dlg.juno.birthday.c1`
       ? "Slip her a tiny gift." → r2  effects: friend:juno,10, flag:choice.bday.juno.gift  ✏ `dlg.juno.birthday.c2`
    r0 [juno]: "Thank you! I'm framing that. The actual words. I'll need a very thin frame."  ✏ `dlg.juno.birthday.r0`   (expr happy)
    r1 [juno]: "It's a perfectly good label! ...Okay, it's too good. I'm going red. That's the forge."  ✏ `dlg.juno.birthday.r1`   (expr embarrassed)
    r2 [juno]: "A gift! For me! On purpose! I'm going to cry on a clasp. Again."  ✏ `dlg.juno.birthday.r2`   (expr happy)
  [juno]: "{if choice:bday.juno.wish}It's framed now, next to the first clasp that ever closed. Click is very proud.{else}{if choice:bday.juno.tease}The cake label stays. I'm adding a second one: 'Yes, Really'. Bram saw and backed away slowly.{else}A gift, from you, on my birthday. It's on the anvil. Everything else in here orbits it now.{/if}{/if}"  ✏ `event.juno_birthday.after`
  · effects: flag:event.juno_birthday
  · effects: mark:birthday.juno, friend:juno,30, give:resource.copperbar,2, mail:juno_bday_thanks

### juno_courtship · "Juno Asks" · wholesome
conversation · wave5_courtship.json · map Blacksmith · priority 8
plays when: `(hearts:juno>=8 && flag:event.juno_heart8 && !flag:partners.juno && !flag:choice.courtship.juno.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:mon && hour>=9 && hour<17) && !flag:event.juno_courtship`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay. I made a list. 'Reasons I Like the Farmer.' Forty items. I'd read it, but I'd faint. So. Would you court me?"  ✏ `event.juno_courtship.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno wave (background))
  · (juno emote sparkle)
  ▶ DIALOGUE `juno.courtship` (player-facing graph, plays here):
    n0 [juno]: "My ears are hot. Say something before I label them."  ✏ `dlg.juno.courtship.0`
       ? "Yes! Read me the list." → r0  effects: friend:juno,20, flag:choice.courtship.juno.yes  ✏ `dlg.juno.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:juno,15, flag:choice.courtship.juno.friends  ✏ `dlg.juno.courtship.c1`
       ? "Can I think about it?" → r2  effects: friend:juno,5, flag:choice.courtship.juno.later  ✏ `dlg.juno.courtship.c2`
    r0 [juno]: "Yes? YES. Okay. Item one is your muddy hat. I can't read item two. My hands are shaking. Courting!"  ✏ `dlg.juno.courtship.r0`   (expr happy)
    r1 [juno]: "Friends! Oh. Okay! Good! The best kind! ...I'll label it 'Friends (Very Good)'. Thank you for saying it straight."  ✏ `dlg.juno.courtship.r1`   (expr happy)
    r2 [juno]: "Think! Yes! Take your time! I'll be right here, polishing one clasp very slowly, not watching the door."  ✏ `dlg.juno.courtship.r2`   (expr thinking)
  [juno]: "{if flag:choice.courtship.juno.yes}Bram says I'm vibrating. I am. The tongs are too. Item two is your laugh, by the way.{else}{if flag:choice.courtship.juno.friends}Friends, labeled, on the shelf next to the first clasp. That's the good shelf.{else}Take your time. I'll start item forty-one. It's 'patient'.{/if}{/if}"  ✏ `event.juno_courtship.after`
  · effects: flag:event.juno_courtship
  · effects: flag:courtship.asked.juno

### juno_courtship2 · "Juno Asks Again" · wholesome
conversation · wave5_courtship.json · map Blacksmith · priority 8
plays when: `hearts:juno>=10 && flag:choice.courtship.juno.later && !flag:event.juno_courtship2.done && !flag:partners.juno && !flag:choice.courtship.juno.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "The list has forty-one items now. Forty-one is 'patient'. I've been so patient! So. Would you court me?"  ✏ `event.juno_courtship2.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno wave (background))
  · (juno emote sparkle)
  ▶ DIALOGUE `juno.courtship` (player-facing graph, plays here):
    n0 [juno]: "My ears are hot. Say something before I label them."  ✏ `dlg.juno.courtship.0`
       ? "Yes! Read me the list." → r0  effects: friend:juno,20, flag:choice.courtship.juno.yes  ✏ `dlg.juno.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:juno,15, flag:choice.courtship.juno.friends  ✏ `dlg.juno.courtship.c1`
       ? "Can I think about it?" → r2  effects: friend:juno,5, flag:choice.courtship.juno.later  ✏ `dlg.juno.courtship.c2`
    r0 [juno]: "Yes? YES. Okay. Item one is your muddy hat. I can't read item two. My hands are shaking. Courting!"  ✏ `dlg.juno.courtship.r0`   (expr happy)
    r1 [juno]: "Friends! Oh. Okay! Good! The best kind! ...I'll label it 'Friends (Very Good)'. Thank you for saying it straight."  ✏ `dlg.juno.courtship.r1`   (expr happy)
    r2 [juno]: "Think! Yes! Take your time! I'll be right here, polishing one clasp very slowly, not watching the door."  ✏ `dlg.juno.courtship.r2`   (expr thinking)
  [juno]: "{if flag:choice.courtship.juno.yes}Finally! Bram says I'm vibrating. I am. I'm crossing out 'patient'. It was never my best item.{else}{if flag:choice.courtship.juno.friends}Friends, labeled, on the shelf next to the first clasp. That's the good shelf.{else}Take your time. Item forty-two is 'still patient'. It's underlined.{/if}{/if}"  ✏ `event.juno_courtship2.after`
  · effects: flag:event.juno_courtship2
  · effects: flag:courtship.asked.juno, flag:event.juno_courtship2.done

### juno_partners · "Juno, Together" · wholesome
conversation_prop · wave5_courtship.json · map Blacksmith · priority 8
plays when: `(hearts:juno>=10 && flag:choice.courtship.juno.yes && !flag:partners.juno && !weekday:mon && hour>=9 && hour<17) && !flag:event.juno_partners`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Hi! Hi, hi! I made you something. Four hours, zero nerves. It's a tiny anvil for your desk. It does nothing!"  ✏ `event.juno_partners.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno wave (background))
  · (spawn prop prop.anvil at (3,2))
  · (juno emote heart)
  ▶ DIALOGUE `juno.partners` (player-facing graph, plays here):
    n0 [juno]: "Bram banked the forge and shooed me out. The afternoon's ours. What are we doing?"  ✏ `dlg.juno.partners.0`
       ? "Admire the tiny anvil." → r0  effects: friend:juno,10, flag:choice.courtship.juno.anvil  ✏ `dlg.juno.partners.c0`
       ? "Take her to the beach." → r1  effects: friend:juno,10, flag:choice.courtship.juno.beach  ✏ `dlg.juno.partners.c1`
       ? "Hold her hand at the forge." → r2  effects: friend:juno,10, flag:choice.courtship.juno.hand  ✏ `dlg.juno.partners.c2`
    r0 [juno]: "It's perfect, right? It rings if you tap it with a pin. I tested it twice. Once on Bram."  ✏ `dlg.juno.partners.r0`   (expr happy)
    r1 [juno]: "The beach! With sand! I'll bring a clasp for luck, and a sandwich. Not for the clasp."  ✏ `dlg.juno.partners.r1`   (expr happy)
    r2 [juno]: "Okay. Yes. Hands. I'm very warm. That's the forge. Definitely the forge. Don't let go."  ✏ `dlg.juno.partners.r2`   (expr embarrassed)
  [juno]: "{if choice:courtship.juno.anvil}The tiny anvil's on my bench now. Most important tool I own. Still does nothing. Perfectly.{else}{if choice:courtship.juno.beach}The beach was perfect. I got sand in a clasp. It crunches now. I'm keeping it.{else}Hands, at the forge. Bram pretended not to see. Then he smiled at the coal. I saw.{/if}{/if}"  ✏ `event.juno_partners.after`
  · effects: flag:event.juno_partners
  · (remove prop scene_prop)
  · effects: flag:partners.juno, give:prop.anvil, friend:juno,50

### overheard_juno_bram_1 · "Labeled Tongs" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.juno && flag:met.bram && hearts:juno>=2 && hearts:bram>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_juno_bram_1`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), bram(26, 18)
  · (place juno to (24,18))
  · (place bram to (26,18))
  · (juno faces right)
  · (bram faces left)
  [juno]: "Bram! I labeled the tongs! 'Tongs (Left)' and 'Tongs (Right)'. Same tongs."  ✏ `event.overheard_juno_bram_1.l1`
  [bram]: "Hm. Tongs know which is which. Ask them."
  · (bram shrug (background))
  · (bram emote sweat)
  · (juno point (background))
  [juno]: "That's what labels are for! So everyone else knows too!"  ✏ `event.overheard_juno_bram_1.l3`
  [bram]: "Then peel the one off my apron. I'm Bram. Mostly."
  · effects: flag:event.overheard_juno_bram_1

### overheard_juno_bram_2 · "The Second Label" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.juno && flag:met.bram && hearts:juno>=4 && hearts:bram>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_juno_bram_1) && !flag:event.overheard_juno_bram_2`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), bram(26, 18)
  · (place juno to (24,18))
  · (place bram to (26,18))
  · (juno faces right)
  · (bram faces left)
  [juno]: "Bram. Did you put a second label on my cake? It just says 'Eat'."  ✏ `event.overheard_juno_bram_2.l1`
  [bram]: "Hm. It was missing a verb."
  · (bram shrug (background))
  · (bram emote sweat)
  · (juno wave (background))
  [juno]: "That's the nicest label anyone's ever made me, and I make all the labels!"  ✏ `event.overheard_juno_bram_2.l3`
  [bram]: "Hm. Don't cry on the clasp. It'll rust."
  · effects: flag:event.overheard_juno_bram_2

### overheard_juno_bram_3 · "The Apprentice Bench" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.juno && flag:met.bram && hearts:juno>=6 && hearts:bram>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_juno_bram_2) && !flag:event.overheard_juno_bram_3`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), bram(26, 18)
  · (place juno to (24,18))
  · (place bram to (26,18))
  · (juno faces right)
  · (bram faces left)
  [juno]: "Bram! I'm on your bench again. I fixed its wobbly leg myself. The joint's crooked. Don't look."  ✏ `event.overheard_juno_bram_3.l1`
  [bram]: "Hm. Saw it. Keep it. Every bench needs one honest mistake."
  · (bram point (background))
  · (bram emote sweat)
  · (juno shrug (background))
  [juno]: "Wait. Was that a compliment? Bram. Was that a compliment? I need a minute."  ✏ `event.overheard_juno_bram_3.l3`
  [bram]: "Hm. Take the minute. Move over."
  · effects: flag:event.overheard_juno_bram_3

### overheard_juno_bram_4 · "The Tiny Forge" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.juno && flag:met.bram && hearts:juno>=8 && hearts:bram>=8 && flag:event.juno_heart10 && flag:event.bram_heart10 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_juno_bram_3) && !flag:event.overheard_juno_bram_4`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), bram(26, 18)
  · (place juno to (24,18))
  · (place bram to (26,18))
  · (juno faces right)
  · (bram faces left)
  [juno]: "Bram. Someday I'll have my own forge. A tiny one. Maybe next door."  ✏ `event.overheard_juno_bram_4.l1`
  [bram]: "Good. Make it next door. I measured the wall. I'll visit. Daily."
  · (bram wave (background))
  · (bram emote sweat)
  · (juno wave (background))
  [juno]: "You would? You'd sit on my bench? Even the crooked one?"  ✏ `event.overheard_juno_bram_4.l3`
  [bram]: "Hm. Move over. I'll bring the good tongs."
  · effects: flag:event.overheard_juno_bram_4

### overheard_piper_juno_1 · "Volume Control" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.piper && flag:met.juno && hearts:piper>=2 && hearts:juno>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_piper_juno_1`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), juno(26, 18)
  · (place piper to (24,18))
  · (place juno to (26,18))
  · (piper faces right)
  · (juno faces left)
  [piper]: "Juno! Hammer softer! My ballad's drowning!"
  [juno]: "Quieter? Piper, it's a forge! You're supposed to feel it!"  ✏ `event.overheard_piper_juno_1.l2`
  · (juno shrug (background))
  · (juno emote note)
  · (piper point (background))
  [piper]: "My slow verse has a backbeat now! I felt it in my teeth!"
  [juno]: "Then the anvil has a fan! Tell your teeth I said thanks!"  ✏ `event.overheard_piper_juno_1.l4`
  · effects: flag:event.overheard_piper_juno_1

### overheard_piper_juno_2 · "Forge Song" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.piper && flag:met.juno && hearts:piper>=4 && hearts:juno>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_piper_juno_1) && !flag:event.overheard_piper_juno_2`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), juno(26, 18)
  · (place piper to (24,18))
  · (place juno to (26,18))
  · (piper faces right)
  · (juno faces left)
  [piper]: "Juno! I wrote a song to the rhythm of your hammer! Don't stop!"
  [juno]: "You did? Play it! No, wait, I'll keep time! Okay, go!"  ✏ `event.overheard_piper_juno_2.l2`
  · (juno shrug (background))
  · (juno emote note)
  · (piper wave (background))
  [piper]: "One, two, clang! That's a chorus!"
  [juno]: "That's a hit! I'm labeling it 'Song (Forge)' and hammering it all week!"  ✏ `event.overheard_piper_juno_2.l4`
  · effects: flag:event.overheard_piper_juno_2

### overheard_piper_juno_3 · "Stage and Anvil" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.piper && flag:met.juno && hearts:piper>=6 && hearts:juno>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_piper_juno_2) && !flag:event.overheard_piper_juno_3`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), juno(26, 18)
  · (place piper to (24,18))
  · (place juno to (26,18))
  · (piper faces right)
  · (juno faces left)
  [piper]: "Juno, did you build me a tiny stage? Out of a crate?"
  [juno]: "It says 'Stage (Piper)'! Official! Play on it! I sanded the splinters!"  ✏ `event.overheard_piper_juno_3.l2`
  · (juno point (background))
  · (juno emote note)
  · (piper shrug (background))
  [piper]: "Then it gets my best set! Front row's yours, Juno. Sit!"
  [juno]: "Okay! No, I'll hammer! No, I'll watch! Both! Always both!"  ✏ `event.overheard_piper_juno_3.l4`
  · effects: flag:event.overheard_piper_juno_3

### overheard_tilda_juno_1 · "Paying the Slate" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.tilda && flag:met.juno && hearts:tilda>=2 && hearts:juno>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_tilda_juno_1`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), tilda(26, 18)
  · (place juno to (24,18))
  · (place tilda to (26,18))
  · (juno faces right)
  · (tilda faces left)
  [juno]: "Tilda! Slate payment! Three clasps, one hinge, one smiley face. Stamped, not drawn."  ✏ `event.overheard_tilda_juno_1.l1`
  [tilda]: "Three clasps is two months of bread. The hinge is a tip. I'll put it on the slate."
  · (tilda shrug (background))
  · (tilda emote sweat)
  · (juno wave (background))
  [juno]: "Is the smiley face worth extra? It took me an hour."  ✏ `event.overheard_tilda_juno_1.l3`
  [tilda]: "The smiley face is priceless. That's why I can't charge for it."
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
  [tilda]: "You've grown, Juno. You used to stand under the jar of hooks and glare at it."
  [juno]: "I still can't! I stand on the flour sack now."  ✏ `event.overheard_tilda_juno_2.l2`
  · (juno shrug (background))
  · (juno emote heart)
  · (tilda point (background))
  [tilda]: "I know. I put the sack there your first week, years ago. Nobody's moved it since."
  [juno]: "Years? Tilda, that's going on my slate. Under 'Owed (Forever)'."  ✏ `event.overheard_tilda_juno_2.l4`
  · effects: flag:event.overheard_tilda_juno_2

### overheard_tilda_juno_3 · "Next Door" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.tilda && flag:met.juno && hearts:tilda>=6 && hearts:juno>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_juno_2 && flag:event.juno_heart6) && !flag:event.overheard_tilda_juno_3`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), tilda(26, 18)
  · (place juno to (24,18))
  · (place tilda to (26,18))
  · (juno faces right)
  · (tilda faces left)
  [juno]: "Tilda... if I ever had my own forge, a small one, would you still feed me?"  ✏ `event.overheard_tilda_juno_3.l1`
  [tilda]: "Open one in Wetherell and I'll stock your first shelf. Go farther, and I'll post you pie."
  · (tilda point (background))
  · (tilda emote ellipsis)
  · (juno shrug (background))
  [juno]: "In Wetherell. Definitely. Next door to Bram. Probably. Okay, definitely."  ✏ `event.overheard_tilda_juno_3.l3`
  [tilda]: "Good. This street could use more noise."
  · effects: flag:event.overheard_tilda_juno_3
