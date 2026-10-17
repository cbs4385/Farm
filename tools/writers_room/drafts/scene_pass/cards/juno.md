# Scene cards for juno

25 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 53 spoken lines in scenes, 36 dialogue lines and 23 player choice labels.

### juno_heart2 · "Hold the Tongs" · funny
event · npc_roster.json · map Blacksmith · priority 6
plays when: `hearts:juno>=2 && !weekday:mon && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Hold the tongs, [player]."  ✏ `event.juno_heart2.0`
  · (move player to (2,2))
  · (player faces up)
  · (juno emote sparkle)
  [juno]: "See the color? That's the moment."  ✏ `event.juno_heart2.1`
  [juno]: "Here. Your first bar."  ✏ `event.juno_heart2.2`
  · effects: give:resource.copperbar,2, friend:juno,40, flag:event.juno_heart2

### juno_heart5 · "Finally Finished" · wholesome
event · npc_roster.json · map Blacksmith · priority 7
plays when: `hearts:juno>=5 && !weekday:mon && hour>=9 && hour<17 && flag:event.juno_heart2`
beats may be ADDED or CUT in this scene
placed: player(2, 2)
  · (juno faces down)
  [juno]: "I finally finished it."  ✏ `event.juno_heart5.0`
  · (move player to (2,2))
  · (player faces up)
  · (juno emote sparkle)
  [juno]: "It's not much. It's yours."  ✏ `event.juno_heart5.1`
  [juno]: "Don't lose it!"  ✏ `event.juno_heart5.2`
  · effects: give:tool.sword_steel,1, friend:juno,60, flag:event.juno_heart5

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
  [juno]: "Okay, okay, okay. Don't look yet. Don't look. ...Now look."  ✏ `event.next_door.0`  (emote sparkle)
  · (spawn prop resource.copperbar at (3,2))
  [juno]: "My own piece. My own mark. It's a hinge, a plain one, and the mark is crooked and I love it!"  ✏ `event.next_door.1`  (expr happy)
  · (bram emote ellipsis)
  [bram]: "Hm."
  [bram]: "Shoulder's thin by a hair. Here. I brought the good tongs."
  ▶ DIALOGUE `next_door.talk` (player-facing graph, plays here):
    n0 [juno]: "Tongs or no tongs? Say something. Anything. I'll do the opposite if I'm nervous enough!"  ✏ `dlg.next_door.talk.0`
       ? "Let Bram bring the tongs." → r0  effects: flag:choice.next_door.tongs, friend:juno,5  ✏ `dlg.next_door.talk.c0`
       ? "Trust your own hands." → r1  effects: flag:choice.next_door.own, friend:juno,5  ✏ `dlg.next_door.talk.c1`
    r0 [juno]: "Fine. Fine! One heat, together. ...Okay, that's better. It's still mine. The tongs only held it."  ✏ `dlg.next_door.talk.r0`
    r1 [juno]: "Own hands. Okay. Okay, okay. ...It's a hair thicker. I did that. Bram, I did that!"  ✏ `dlg.next_door.talk.r1`
  · (remove prop piece)
  · (bram faces down)
  [bram]: "I'm proud of you, Juno."  (emote heart)
  [juno]: "...He said it. Out loud, in front of a witness. I need to sit down. The anvil is fine."  ✏ `event.next_door.5`  (expr embarrassed)
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
  [bram]: "Hm. Hm-hm. I know the tune. Say nothing."  (emote note)
  [juno]: "The hall door hangs on hinges I made. My mark's on every pin, and it swings like it's showing off!"  ✏ `event.hall_dance.8`  (expr happy)
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

### story_notes_juno · "The Note Writer" · wholesome
event · storyline_events.json · map Village · priority 7
plays when: `flag:notes.began && storyline:anonymous_notes.juno && hearts:juno>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: juno(25, 18), player(25, 17)
  · (place juno to (25,18))
  · (juno faces down)
  · (spawn prop prop.note at (26,18))
  [juno]: "Don't look! I mean, look. I can't stop myself. Encouragement leaks out of me."  ✏ `event.story_notes_juno.intro`
  · (move player to (25,17))
  · (player faces up)
  · (juno emote sparkle)
  · (juno shrug (background))
  ▶ DIALOGUE `story.notes.juno` (player-facing graph, plays here):
    n0 [narration]: "You catch Juno slipping a folded note under a door. Neither of you moves."  ✏ `dlg.story.notes.juno.0`   (→ n1)
    n1 [juno]: "...Well. Now you know."  ✏ `dlg.story.notes.juno.1`
       ? "Say nothing. Just smile." → r0  effects: flag:choice.notes.juno.smile  ✏ `dlg.story.notes.juno.c0`
       ? "Say thank you." → r1  effects: flag:choice.notes.juno.thanks, friend:juno,10  ✏ `dlg.story.notes.juno.c1`
       ? "Leave a note back." → r2  effects: flag:choice.notes.juno.note, friend:juno,15  ✏ `dlg.story.notes.juno.c2`
    r0 [juno]: "...You smiled. That's a lot of kindness for one smile."  ✏ `dlg.story.notes.juno.r0`   (expr embarrassed)
    r1 [juno]: "Oh. Thank you. That's the whole reward. I'll go and be flustered somewhere."  ✏ `dlg.story.notes.juno.r1`   (expr happy)
    r2 [juno]: "A note, for me? ...I'm keeping it. In a drawer I've just invented."  ✏ `dlg.story.notes.juno.r2`   (expr happy)
  [juno]: "I'll keep writing them. I tried to stop for a week. I cried at a turnip."  ✏ `event.story_notes_juno.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_notes_juno, friend:juno,40, flag:storydone.anonymous_notes

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
    n1 [juno]: "Exhibit B: three anvils, a wash bucket and Bram, who has agreed to hit one thing. Once."  ✏ `dlg.story.band.talk.1`   (→ n2)
    n2 [bram]: "Hm."   (→ n3)
    n3 [piper]: "Well? Who owns the square tonight?"
       ? "The fiddle." → r0  effects: flag:choice.band.piper
       ? "The forge band." → r1  effects: flag:choice.band.juno
       ? "Both, together." → r2  effects: flag:choice.band.both, friend:piper,10, friend:juno,10
    r0 [piper]: "Ha! Justice and good taste. Bow, everybody."   (expr happy)
    r1 [juno]: "Okay, okay, okay! Did you hear that, Bram? We won. Bram?"  ✏ `dlg.story.band.talk.r1`   (expr happy)
    r2 [piper]: "Together? ...That's either genius or a noise complaint. Let's find out."   (emote note)
  [piper]: "{if choice:band.piper}A fiddle wins! Of course. Juno took it well. She's inventing a louder anvil.{else}{if choice:band.juno}The forge band won! I'm thrilled. I'm furious. I'm writing a song about both.{else}A joint set! Fiddle, anvil, one very confused crowd. That's a chorus.{/if}{/if}"
  · effects: flag:event.story_band_battle, friend:piper,20, friend:juno,20, flag:storydone.competing_band

### juno_heart4 · "The Label Maker" · funny
prank · wave2_events.json · map Blacksmith · priority 7
plays when: `hearts:juno>=4 && flag:event.juno_heart2 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay, don't move! I have a surprise and a very sticky label."  ✏ `event.juno_heart4.setup`
  · (move player to (2,2))
  · (player faces up)
  · (juno jiggle (background))
  · (sfx click)
  · (camera shake)
  · (player emote exclaim)
  [juno]: "Done! You've been labeled. It says 'Guest (Excellent)'. I'm very proud."  ✏ `event.juno_heart4.prank`
  ▶ DIALOGUE `juno.label` (player-facing graph, plays here):
    n0 [juno]: "So? What do you say about it? I'll accept any answer except "Hm.""  ✏ `dlg.juno.label.0`
       ? "Accept the label." → r0  effects: friend:juno,10, flag:choice.label.accept  ✏ `dlg.juno.label.c0`
       ? "Relabel her." → r1  effects: friend:juno,10, flag:choice.label.relabel  ✏ `dlg.juno.label.c1`
       ? "Eat the label." → r2  effects: friend:juno,10, flag:choice.label.eat  ✏ `dlg.juno.label.c2`
    r0 [juno]: "Accepted! You're officially a fixture. I'll sweep around you."  ✏ `dlg.juno.label.r0`   (expr happy)
    r1 [juno]: "Relabel me! Oh, that's brilliant. "Apprentice (Chaotic)". Stick it on. Right here."  ✏ `dlg.juno.label.r1`   (expr happy)
    r2 [juno]: "You... you ate it. ...I'm labeling this moment "Do Not Repeat". And then I'll want it repeated."  ✏ `dlg.juno.label.r2`   (emote sweat)
  [juno]: "{if choice:label.accept}A label that stays! I'll put one on your hat tomorrow.{else}{if choice:label.relabel}Relabeled! 'Apprentice (Chaotic)'. That's fair, and now it's on my forehead.{else}You ate the label! Nobody's ever eaten a label! I'm thrilled and slightly worried.{/if}{/if}"  ✏ `event.juno_heart4.reaction`
  · effects: flag:event.juno_heart4
  · effects: friend:juno,50

### juno_heart6 · "First Strike" · wholesome
confession · wave2_events.json · map Blacksmith · priority 7
plays when: `hearts:juno>=6 && flag:event.juno_heart5 && flag:event.juno_heart4 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Don't come in. Okay, come in. Close the door. I made something. It's crooked. I'm about to put it in the quench barrel."  ✏ `event.juno_heart6.intro`
  · (move player to (2,2))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on juno)
  · (juno expression thinking)
  ▶ DIALOGUE `juno.strike` (player-facing graph, plays here):
    n0 [juno]: "It's my first piece that's not a copy. A little horseshoe. Look at it. It's lopsided, and I made it, and I hate that I love it."  ✏ `dlg.juno.strike.0`   (→ n1; expr sad)
    n1 [juno]: "Do I keep it, or does it go in the barrel?"  ✏ `dlg.juno.strike.1`
       ? "Keep it." → r0  effects: friend:juno,10, flag:choice.strike.keep  ✏ `dlg.juno.strike.c0`
       ? "Make it better together." → r1  effects: friend:juno,15, flag:choice.strike.together  ✏ `dlg.juno.strike.c1`
       ? "Laugh with her." → r2  effects: friend:juno,10, flag:choice.strike.laugh  ✏ `dlg.juno.strike.c2`
    r0 [juno]: "Keep it. Yes. Even crooked. Especially crooked."  ✏ `dlg.juno.strike.r0`   (expr happy)
    r1 [juno]: "Together! You hold the tongs, I'll tap the bend. Okay, okay, okay, let's go."  ✏ `dlg.juno.strike.r1`   (expr happy)
    r2 [juno]: "Okay, it's very funny. A horseshoe for a three-legged horse. A very proud three-legged horse."  ✏ `dlg.juno.strike.r2`   (expr happy)
  · (juno expression neutral)
  [juno]: "Thank you for stopping me. It's still crooked. It's also still mine. That's a new feeling."  ✏ `event.juno_heart6.after`
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.juno_heart6
  · effects: friend:juno,60

### juno_heart8 · "The Horseshoe" · surprise
conversation · wave2_events.json · map Blacksmith · priority 7
plays when: `hearts:juno>=8 && flag:event.juno_heart6 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay. Tonight I show it to Bram. The crooked horseshoe. Will you stand next to me? I'll sweat less."  ✏ `event.juno_heart8.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno wave (background))
  · (juno emote ellipsis)
  ▶ DIALOGUE `juno.shoe` (player-facing graph, plays here):
    n0 [narration]: "Bram stands at the far end of the forge. Juno holds out the horseshoe with both hands and does not look up."  ✏ `dlg.juno.shoe.0`   (→ n1)
    n1 [juno]: "I can't look. Tell me what's happening."  ✏ `dlg.juno.shoe.1`
       ? "Wait with her, quietly." → r0  effects: friend:juno,15, flag:choice.shoe.wait  ✏ `dlg.juno.shoe.c0`
       ? "Tell her it's wonderful." → r1  effects: friend:juno,10, flag:choice.shoe.praise  ✏ `dlg.juno.shoe.c1`
       ? "Hand her the hammer." → r2  effects: friend:juno,10, flag:choice.shoe.hammer  ✏ `dlg.juno.shoe.c2`
    r0 [juno]: "...Thank you. Quiet's good. The anvil's listening too."  ✏ `dlg.juno.shoe.r0`
    r1 [juno]: "Wonderful? It's crooked! ...But thank you. I'm going to look up now. Slowly."  ✏ `dlg.juno.shoe.r1`   (expr happy)
    r2 [juno]: "A hammer. Okay. One more tap. For luck. For me. For crooked things everywhere."  ✏ `dlg.juno.shoe.r2`   (expr happy)
  [juno]: "{if choice:shoe.wait}You waited. That meant more than any speech. He's holding it now. He hasn't said a word. It's the best word.{else}{if choice:shoe.praise}You said it was wonderful. He's frowning at it like he agrees. That's how Bram says yes.{else}You gave me the hammer. So I tapped the bend one last time. It's better, and it's still mine.{/if}{/if}"  ✏ `event.juno_heart8.after`
  · effects: flag:event.juno_heart8
  · effects: friend:juno,70

### juno_heart10 · "Her Mark" · wholesome
heirloom · wave2_events.json · map Blacksmith · priority 7
plays when: `hearts:juno>=10 && flag:event.juno_heart8 && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay. Hold out your hand. I made a mark. My own. Nobody else has it. I stamped this with it before I told anyone."  ✏ `event.juno_heart10.intro`
  · (move player to (2,2))
  · (player faces up)
  · (spawn prop resource.copperbar at (2,3))
  · (juno emote heart)
  [juno]: "It's the first thing with my name on it. A tiny copper bar. {if quest:juno_gold=done}Copper on purpose. Your gold is in the die. {/if}It isn't much. It's the most I've ever made."  ✏ `event.juno_heart10.give`
  · (sfx coin)
  · effects: give:resource.copperbar,1, flag:event.juno_heart10
  · (remove prop heirloom)
  [juno]: "{if choice:shoe.wait}Bram saw the mark, and waited, and nodded once. Once! You were there for the whole thing.{else}Bram saw the mark and said, 'Hm.' I'm having it framed.{/if}"  ✏ `event.juno_heart10.thanks`
  · effects: friend:juno,80

### juno_friend · "The Swimming Pool" · funny
shared_activity · wave2_events.json · map Blacksmith · priority 7
plays when: `hearts:juno>=3 && weekday:thu && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay, quick one. Come see the swimming pool. It's a bucket. Wren named it. I made the sign."  ✏ `event.juno_friend.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno emote sparkle)
  · (juno hop)
  [juno]: "See? 'Swimming Pool (Official)'. I'm very proud, and the bucket is very wet."  ✏ `event.juno_friend.doing`
  [juno]: "Best pool in Wetherell. Also the only pool. Also a bucket. I'll cherish it."  ✏ `event.juno_friend.wrap`
  · effects: flag:event.juno_friend
  · effects: friend:juno,15

### juno_birthday · "Juno's Birthday" · funny
conversation · wave4_birthdays.json · map Blacksmith · priority 8 · repeatable
plays when: `hearts:juno>=2 && birthday.in:juno==0 && unseen:birthday.juno && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay, it's my birthday and I'm not nervous! I labeled the cake. It says 'Cake (Birthday)'. Come in!"  ✏ `event.juno_birthday.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno wave (background))
  · (juno emote sparkle)
  ▶ DIALOGUE `juno.birthday` (player-facing graph, plays here):
    n0 [juno]: "So? What's the plan? There's a plan, right? Tell me there's a plan!"  ✏ `dlg.juno.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:juno,10, flag:choice.bday.juno.wish  ✏ `dlg.juno.birthday.c0`
       ? "Tease her about the cake label." → r1  effects: friend:juno,10, flag:choice.bday.juno.tease  ✏ `dlg.juno.birthday.c1`
       ? "Give her a tiny gift." → r2  effects: friend:juno,10, flag:choice.bday.juno.gift  ✏ `dlg.juno.birthday.c2`
    r0 [juno]: "Thank you! I'm having it framed. Don't tell Bram."  ✏ `dlg.juno.birthday.r0`   (expr happy)
    r1 [juno]: "It's a perfectly good label! ...Okay, it's too good. I'm blushing."  ✏ `dlg.juno.birthday.r1`   (expr embarrassed)
    r2 [juno]: "A gift! For me! I'm going to cry on a clasp. Thank you!"  ✏ `dlg.juno.birthday.r2`   (expr happy)
  [juno]: "{if choice:bday.juno.wish}It's in a frame, next to the first clasp that closed.{else}{if choice:bday.juno.tease}The cake label stays. Bram added a second label. It says 'Eat'.{else}A gift, on my birthday, from you. I put it on the anvil. It's the new center of everything.{/if}{/if}"  ✏ `event.juno_birthday.after`
  · effects: flag:event.juno_birthday
  · effects: mark:birthday.juno, friend:juno,30, give:resource.copperbar,2, mail:juno_bday_thanks

### juno_courtship · "Juno Asks" · wholesome
conversation · wave5_courtship.json · map Blacksmith · priority 8
plays when: `hearts:juno>=8 && flag:event.juno_heart8 && !flag:partners.juno && !flag:choice.courtship.juno.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay. I made a list. 'Reasons I like the farmer.' It has forty items. I'd read them but I'll faint. So. Would you court me?"  ✏ `event.juno_courtship.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno wave (background))
  · (juno emote sparkle)
  ▶ DIALOGUE `juno.courtship` (player-facing graph, plays here):
    n0 [juno]: "What do you say?"  ✏ `dlg.juno.courtship.0`
       ? "Yes! Forty items is a lot." → r0  effects: friend:juno,20, flag:choice.courtship.juno.yes  ✏ `dlg.juno.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:juno,15, flag:choice.courtship.juno.friends  ✏ `dlg.juno.courtship.c1`
       ? "Can I think about it?" → r2  effects: friend:juno,5, flag:choice.courtship.juno.later  ✏ `dlg.juno.courtship.c2`
    r0 [juno]: "YES. Okay. Oh my gosh. I'm going to need a minute and also a hammer. Courting! With you!"  ✏ `dlg.juno.courtship.r0`   (expr happy)
    r1 [juno]: "Friends! Oh, okay! Good! Great! The best! ...I'll label this one 'Friends (Very Good)'. Thank you for saying it honestly."  ✏ `dlg.juno.courtship.r1`   (expr happy)
    r2 [juno]: "Think! Yes! Take your time! I'll be right here, polishing a clasp, not looking at the door."  ✏ `dlg.juno.courtship.r2`   (expr thinking)
  [juno]: "{if flag:choice.courtship.juno.yes}Courting! Bram says I'm vibrating. I am! Thank you!{else}{if flag:choice.courtship.juno.friends}Friends, forever, labeled. It's on the shelf. Next to the first clasp.{else}Take your time. I'll finish item forty-one. It's 'patient'.{/if}{/if}"  ✏ `event.juno_courtship.after`
  · effects: flag:event.juno_courtship
  · effects: flag:courtship.asked.juno

### juno_courtship2 · "Juno Asks Again" · wholesome
conversation · wave5_courtship.json · map Blacksmith · priority 8
plays when: `hearts:juno>=10 && flag:choice.courtship.juno.later && !flag:event.juno_courtship2.done && !flag:partners.juno && !flag:choice.courtship.juno.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Okay, okay, okay. I made a list. 'Reasons I like the farmer.' It has forty items. I'd read them but I'll faint. So. Would you court me?"  ✏ `event.juno_courtship2.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno wave (background))
  · (juno emote sparkle)
  ▶ DIALOGUE `juno.courtship` (player-facing graph, plays here):
    n0 [juno]: "What do you say?"  ✏ `dlg.juno.courtship.0`
       ? "Yes! Forty items is a lot." → r0  effects: friend:juno,20, flag:choice.courtship.juno.yes  ✏ `dlg.juno.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:juno,15, flag:choice.courtship.juno.friends  ✏ `dlg.juno.courtship.c1`
       ? "Can I think about it?" → r2  effects: friend:juno,5, flag:choice.courtship.juno.later  ✏ `dlg.juno.courtship.c2`
    r0 [juno]: "YES. Okay. Oh my gosh. I'm going to need a minute and also a hammer. Courting! With you!"  ✏ `dlg.juno.courtship.r0`   (expr happy)
    r1 [juno]: "Friends! Oh, okay! Good! Great! The best! ...I'll label this one 'Friends (Very Good)'. Thank you for saying it honestly."  ✏ `dlg.juno.courtship.r1`   (expr happy)
    r2 [juno]: "Think! Yes! Take your time! I'll be right here, polishing a clasp, not looking at the door."  ✏ `dlg.juno.courtship.r2`   (expr thinking)
  [juno]: "{if flag:choice.courtship.juno.yes}Courting! Bram says I'm vibrating. I am! Thank you!{else}{if flag:choice.courtship.juno.friends}Friends, forever, labeled. It's on the shelf. Next to the first clasp.{else}Take your time. I'll finish item forty-one. It's 'patient'.{/if}{/if}"  ✏ `event.juno_courtship2.after`
  · effects: flag:event.juno_courtship2
  · effects: flag:courtship.asked.juno, flag:event.juno_courtship2.done

### juno_partners · "Juno, Together" · wholesome
conversation_prop · wave5_courtship.json · map Blacksmith · priority 8
plays when: `hearts:juno>=10 && flag:choice.courtship.juno.yes && !flag:partners.juno && !weekday:mon && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(2, 2)
  · (juno faces down)
  [juno]: "Hi! Hi, hi! I made you something. It took four hours and I'm not nervous. It's a tiny anvil. For your desk. It doesn't do anything!"  ✏ `event.juno_partners.intro`
  · (move player to (2,2))
  · (player faces up)
  · (juno wave (background))
  · (spawn prop prop.anvil at (3,2))
  · (juno emote heart)
  ▶ DIALOGUE `juno.partners` (player-facing graph, plays here):
    n0 [juno]: "Okay, okay, okay. Forge is cold, evening's free. What are we doing?"  ✏ `dlg.juno.partners.0`
       ? "Admire the tiny anvil." → r0  effects: friend:juno,10, flag:choice.courtship.juno.anvil  ✏ `dlg.juno.partners.c0`
       ? "Take her to the beach." → r1  effects: friend:juno,10, flag:choice.courtship.juno.beach  ✏ `dlg.juno.partners.c1`
       ? "Hold her hand at the forge." → r2  effects: friend:juno,10, flag:choice.courtship.juno.hand  ✏ `dlg.juno.partners.c2`
    r0 [juno]: "It's perfect, right? It's tiny! It rings if you hit it with a pin. I tested it. Twice. Once on Bram."  ✏ `dlg.juno.partners.r0`   (expr happy)
    r1 [juno]: "The beach! With sand! I'll bring a clasp for luck. And a sandwich. Not for the clasp."  ✏ `dlg.juno.partners.r1`   (expr happy)
    r2 [juno]: "Okay. Yes. Hands. I'm very warm, it's the forge, definitely. Don't let go."  ✏ `dlg.juno.partners.r2`   (expr embarrassed)
  [juno]: "{if choice:courtship.juno.anvil}The tiny anvil is on my bench. It's the most important tool I own. It doesn't do anything.{else}{if choice:courtship.juno.beach}The beach was perfect. I got sand in a clasp. I'm keeping it. It's a souvenir.{else}Hands, in the forge. Bram pretended not to see. He smiled. I saw.{/if}{/if}"  ✏ `event.juno_partners.after`
  · effects: flag:event.juno_partners
  · (remove prop scene_prop)
  · effects: flag:partners.juno, give:prop.anvil, friend:juno,50

### overheard_juno_bram_1 · "Labeled Tongs" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.juno && flag:met.bram && hearts:juno>=2 && hearts:bram>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), bram(26, 18)
  · (place juno to (24,18))
  · (place bram to (26,18))
  · (juno faces right)
  · (bram faces left)
  [juno]: "Bram! I labeled the tongs. 'Tongs (Left)'. 'Tongs (Right)'. They're the same tongs."  ✏ `event.overheard_juno_bram_1.l1`
  [bram]: "Hm. They've always known which is which."
  · (bram shrug (background))
  · (bram emote sweat)
  · (juno point (background))
  [juno]: "That's what the labels are for! So everyone knows!"  ✏ `event.overheard_juno_bram_1.l3`
  [bram]: "Hm. Everyone knows. Take the label off my hat."
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
  [juno]: "Bram. Did you add a label to my cake? It says 'Eat'."  ✏ `event.overheard_juno_bram_2.l1`
  [bram]: "Hm. It was missing a verb."
  · (bram shrug (background))
  · (bram emote sweat)
  · (juno wave (background))
  [juno]: "It's the nicest label anyone's made me, and I made the labels!"  ✏ `event.overheard_juno_bram_2.l3`
  [bram]: "Hm. Don't cry on the clasp. It'll rust."
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
  [juno]: "Bram! I'm on your bench again, and I carved a small mistake into it. Don't look."  ✏ `event.overheard_juno_bram_3.l1`
  [bram]: "Hm. Saw it. Best joint in the shop. Don't change it."
  · (bram point (background))
  · (bram emote sweat)
  · (juno shrug (background))
  [juno]: "Wait. Was that a compliment? Bram, was that a compliment? I need a minute."  ✏ `event.overheard_juno_bram_3.l3`
  [bram]: "Hm. Take the minute. Move over."
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
  [juno]: "Bram. Someday I'll open my own forge. A tiny one!"  ✏ `event.overheard_juno_bram_4.l1`
  [bram]: "Hm. Good. I'd come visit."
  · (bram wave (background))
  · (bram emote sweat)
  · (juno wave (background))
  [juno]: "You would? You'd sit on my bench?"  ✏ `event.overheard_juno_bram_4.l3`
  [bram]: "Hm. Move over. I'll bring the good tongs."
  · effects: flag:event.overheard_juno_bram_4

### overheard_piper_juno_1 · "Volume Control" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.piper && flag:met.juno && hearts:piper>=2 && hearts:juno>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), juno(26, 18)
  · (place piper to (24,18))
  · (place juno to (26,18))
  · (piper faces right)
  · (juno faces left)
  [piper]: "Juno! Could you hammer quieter? My ballad is drowning!"
  [juno]: "Quieter? Piper, it's a forge! It's meant to be felt!"  ✏ `event.overheard_piper_juno_1.l2`
  · (juno shrug (background))
  · (juno emote note)
  · (piper point (background))
  [piper]: "I felt it in my teeth! Through the stage!"
  [juno]: "Then the anvil's a fan! I'll take it!"  ✏ `event.overheard_piper_juno_1.l4`
  · effects: flag:event.overheard_piper_juno_1

### overheard_piper_juno_2 · "Forge Song" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.piper && flag:met.juno && hearts:piper>=4 && hearts:juno>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_piper_juno_1`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), juno(26, 18)
  · (place piper to (24,18))
  · (place juno to (26,18))
  · (piper faces right)
  · (juno faces left)
  [piper]: "Juno! I wrote a song to the rhythm of your hammer!"
  [juno]: "You did? Play it! No, wait, I'll hammer along!"  ✏ `event.overheard_piper_juno_2.l2`
  · (juno shrug (background))
  · (juno emote note)
  · (piper wave (background))
  [piper]: "One, two, clang! That's the chorus!"
  [juno]: "It's a banger! I'll label it 'Song (Forge)'!"  ✏ `event.overheard_piper_juno_2.l4`
  · effects: flag:event.overheard_piper_juno_2

### overheard_piper_juno_3 · "Stage and Anvil" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.piper && flag:met.juno && hearts:piper>=6 && hearts:juno>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_piper_juno_2`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), juno(26, 18)
  · (place piper to (24,18))
  · (place juno to (26,18))
  · (piper faces right)
  · (juno faces left)
  [piper]: "Juno, you made me a tiny stage out of a crate?"
  [juno]: "It says 'Stage (Piper)'! I labeled it! Play on it!"  ✏ `event.overheard_piper_juno_3.l2`
  · (juno point (background))
  · (juno emote note)
  · (piper shrug (background))
  [piper]: "I'll play my best set! You watch!"
  [juno]: "Okay! No, I'll hammer! No, watch! Both! Always both!"  ✏ `event.overheard_piper_juno_3.l4`
  · effects: flag:event.overheard_piper_juno_3

### overheard_tilda_juno_1 · "Paying the Slate" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.juno && hearts:tilda>=2 && hearts:juno>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: juno(24, 18), tilda(26, 18)
  · (place juno to (24,18))
  · (place tilda to (26,18))
  · (juno faces right)
  · (tilda faces left)
  [juno]: "Tilda, I'm paying my slate! Three clasps and a hinge with a smiley face."  ✏ `event.overheard_tilda_juno_1.l1`
  [tilda]: "That's two months of bread, and the hinge is a tip. I'll put it on the slate."
  · (tilda shrug (background))
  · (tilda emote sweat)
  · (juno wave (background))
  [juno]: "Is the smiley face worth extra?"  ✏ `event.overheard_tilda_juno_1.l3`
  [tilda]: "The smiley face is priceless. That's why I'm not charging for it."
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
  [tilda]: "You've grown, Juno. When you first came in, you couldn't reach the jar of hooks."
  [juno]: "I still can't. I stand on the flour sack now!"  ✏ `event.overheard_tilda_juno_2.l2`
  · (juno shrug (background))
  · (juno emote heart)
  · (tilda point (background))
  [tilda]: "I know. I moved it there for you. Years ago."
  [juno]: "Tilda! You've been on my slate the whole time."  ✏ `event.overheard_tilda_juno_2.l4`
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
  [juno]: "Tilda... if I ever had my own forge, somewhere small, would you still feed me?"  ✏ `event.overheard_tilda_juno_3.l1`
  [tilda]: "Open a forge in Wetherell and I'll stock your first anvil. Go farther, and I'll post you pie."
  · (tilda point (background))
  · (tilda emote ellipsis)
  · (juno shrug (background))
  [juno]: "In Wetherell. Definitely. Next door to Bram, probably."  ✏ `event.overheard_tilda_juno_3.l3`
  [tilda]: "Good. The street could use the noise."
  · effects: flag:event.overheard_tilda_juno_3
