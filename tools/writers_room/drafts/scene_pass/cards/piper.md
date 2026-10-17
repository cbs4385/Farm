# Scene cards for piper

24 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 51 spoken lines in scenes, 34 dialogue lines and 24 player choice labels.

### piper_heart2 · "Learn a Chord" · funny
event · npc_roster.json · map Saloon · priority 6
plays when: `hearts:piper>=2 && !weekday:tue && hour>=18 && hour<24`
beats may be ADDED or CUT in this scene
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Learn a chord, [player]."  ✏ `event.piper_heart2.0`
  · (move player to (11,6))
  · (player faces up)
  · (piper emote note)
  [piper]: "There. You're a musician now."  ✏ `event.piper_heart2.1`
  [piper]: "Don't quit your day job."  ✏ `event.piper_heart2.2`
  · effects: give:crop.sunflower,1, friend:piper,40, flag:event.piper_heart2

### piper_heart5 · "A Song for You" · wholesome
event · npc_roster.json · map Saloon · priority 7
plays when: `hearts:piper>=5 && !weekday:tue && hour>=18 && hour<24 && flag:event.piper_heart2`
beats may be ADDED or CUT in this scene
placed: player(11, 6)
  · (piper faces down)
  [piper]: "This one's for you."  ✏ `event.piper_heart5.0`
  · (move player to (11,6))
  · (player faces up)
  · (piper emote note)
  [piper]: "I've never played it for anyone else."  ✏ `event.piper_heart5.1`
  [piper]: "Thank you for listening."  ✏ `event.piper_heart5.2`
  · effects: give:artisan.jam,2, friend:piper,60, flag:event.piper_heart5

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
  [piper]: "First verse, then I step off. I'm calm. I'm holding this fiddle like a hot loaf."  ✏ `event.hall_dance.4`  (emote sweat)
  · (music piper_theme)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  ✏ `event.hall_dance.5`  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall sings the chorus. Piper has stopped playing, and nobody minds."
  [bram]: "Hm. Hm-hm. I know the tune. Say nothing."  (emote note)
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

### story_umbrella · "The Red Umbrella" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:lost_umbrella && weather:rain && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: piper(25, 18), player(25, 17)
  · (place piper to (25,18))
  · (piper faces down)
  · (spawn prop prop.umbrella at (26,18))
  [piper]: "Wait. Stop. Is that my umbrella?"  ✏ `event.story_umbrella.intro`
  · (move player to (25,17))
  · (player faces up)
  · (piper emote question)
  · (piper point (background))
  ▶ DIALOGUE `story.umbrella.talk` (player-facing graph, plays here):
    n0 [narration]: "A large red umbrella hangs from the fence post, dripping with great dignity. Piper stares at it."  ✏ `dlg.story.umbrella.talk.0`   (→ n1)
    n1 [piper]: "It's red. It's enormous. It cannot simply vanish and reappear, and yet. What do you suggest?"  ✏ `dlg.story.umbrella.talk.1`
       ? "Return it to Piper." → r0  effects: flag:choice.umbrella.return  ✏ `dlg.story.umbrella.talk.c0`
       ? "Hang it where it was found." → r1  effects: flag:choice.umbrella.hang  ✏ `dlg.story.umbrella.talk.c1`
       ? "Keep it." → r2  effects: flag:choice.umbrella.keep  ✏ `dlg.story.umbrella.talk.c2`
    r0 [piper]: "Mine. At last. I was starting to take it personally."  ✏ `dlg.story.umbrella.talk.r0`   (expr happy)
    r1 [piper]: "Leave it? ...A little red landmark. Yes. I like that."  ✏ `dlg.story.umbrella.talk.r1`
    r2 [piper]: "You'll keep it? That's theft, with excellent manners."  ✏ `dlg.story.umbrella.talk.r2`   (emote sweat)
  [piper]: "{if choice:umbrella.return}Back where it belongs. It will be lost again by Thursday.{else}{if choice:umbrella.hang}A landmark. I'll look for it every rain.{else}Shared custody. I accept. Under protest.{/if}{/if}"  ✏ `event.story_umbrella.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_umbrella, friend:piper,30, flag:storydone.lost_umbrella

### story_band_battle · "Battle of the Bands" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:competing_band && hearts:piper>=2 && hearts:juno>=2 && hour>=17 && hour<21 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: piper(25, 18), player(25, 17)
  · (place piper to (25,18))
  · (piper faces down)
  [piper]: "Two bands, one square, one very polite war. Judge us, [player]. Be honest, and honest means fiddle."  ✏ `event.story_band_battle.intro`
  · (move player to (25,17))
  · (player faces up)
  · (piper emote note)
  ▶ DIALOGUE `story.band.talk` (player-facing graph, plays here):
    n0 [piper]: "Exhibit A: a fiddle, a voice, a lifetime of practice. Watch."  ✏ `dlg.story.band.talk.0`   (→ n1)
    n1 [juno]: "Exhibit B: three anvils, a wash bucket and Bram, who has agreed to hit one thing. Once."   (→ n2)
    n2 [bram]: "Hm."   (→ n3)
    n3 [piper]: "Well? Who owns the square tonight?"  ✏ `dlg.story.band.talk.3`
       ? "The fiddle." → r0  effects: flag:choice.band.piper  ✏ `dlg.story.band.talk.c0`
       ? "The forge band." → r1  effects: flag:choice.band.juno  ✏ `dlg.story.band.talk.c1`
       ? "Both, together." → r2  effects: flag:choice.band.both, friend:piper,10, friend:juno,10  ✏ `dlg.story.band.talk.c2`
    r0 [piper]: "Ha! Justice and good taste. Bow, everybody."  ✏ `dlg.story.band.talk.r0`   (expr happy)
    r1 [juno]: "Okay, okay, okay! Did you hear that, Bram? We won. Bram?"   (expr happy)
    r2 [piper]: "Together? ...That's either genius or a noise complaint. Let's find out."  ✏ `dlg.story.band.talk.r2`   (emote note)
  [piper]: "{if choice:band.piper}A fiddle wins! Of course. Juno took it well. She's inventing a louder anvil.{else}{if choice:band.juno}The forge band won! I'm thrilled. I'm furious. I'm writing a song about both.{else}A joint set! Fiddle, anvil, one very confused crowd. That's a chorus.{/if}{/if}"  ✏ `event.story_band_battle.after`
  · effects: flag:event.story_band_battle, friend:piper,20, friend:juno,20, flag:storydone.competing_band

### piper_heart4 · "Request Line" · funny
conversation · wave2_events.json · map Saloon · priority 7
plays when: `hearts:piper>=4 && flag:event.piper_heart2 && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Last set of the night, and I'm out of ideas. You pick one. Anything. I'll pretend it's mine."  ✏ `event.piper_heart4.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote note)
  ▶ DIALOGUE `piper.request` (player-facing graph, plays here):
    n0 [piper]: "The room's waiting. A ballad, a jig, or my masterpiece about parsnips?"  ✏ `dlg.piper.request.0`
       ? "A ballad." → r0  effects: friend:piper,10, flag:choice.request.ballad  ✏ `dlg.piper.request.c0`
       ? "A jig." → r1  effects: friend:piper,10, flag:choice.request.jig  ✏ `dlg.piper.request.c1`
       ? "The parsnip song." → r2  effects: friend:piper,10, flag:choice.request.parsnip  ✏ `dlg.piper.request.c2`
    r0 [piper]: "A ballad. A sad one. A sad one with a happy ending, at least. Gather round."  ✏ `dlg.piper.request.r0`   (emote note)
    r1 [piper]: "A jig! Get your boots! The stool in the corner is going to dance, I can feel it."  ✏ `dlg.piper.request.r1`   (expr happy)
    r2 [piper]: "The parsnip song! That's a chorus. A bad one. Mine."  ✏ `dlg.piper.request.r2`   (expr happy)
  [piper]: "{if choice:request.ballad}A ballad! I'll play it slowly and cry with my elbows.{else}{if choice:request.jig}A jig! Feet, take cover. I'm warming up the bow.{else}The parsnip song! My masterpiece! I'll sing it with feeling and a straight face.{/if}{/if}"  ✏ `event.piper_heart4.after`
  · effects: flag:event.piper_heart4
  · effects: friend:piper,50

### piper_heart6 · "Stage Fright" · surprise
confession · wave2_events.json · map Saloon · priority 7
plays when: `hearts:piper>=6 && flag:event.piper_heart5 && flag:event.piper_heart4 && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Down here. The cellar. It's quiet. I'm not hiding. I'm resting my nerves among the barrels."  ✏ `event.piper_heart6.intro`
  · (move player to (11,6))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on piper)
  · (piper expression thinking)
  ▶ DIALOGUE `piper.fright` (player-facing graph, plays here):
    n0 [piper]: "I can play for a hundred people and never blink. But tonight, I think they'll know. That I'm not as good as I say I am."  ✏ `dlg.piper.fright.0`   (→ n1; expr sad)
    n1 [piper]: "What do I do with that?"  ✏ `dlg.piper.fright.1`
       ? "Talk it through." → r0  effects: friend:piper,10, flag:choice.fright.talk  ✏ `dlg.piper.fright.c0`
       ? "Sit in silence with her." → r1  effects: friend:piper,15, flag:choice.fright.sit  ✏ `dlg.piper.fright.c1`
       ? "Play along on a bucket." → r2  effects: friend:piper,10, flag:choice.fright.bucket  ✏ `dlg.piper.fright.c2`
    r0 [piper]: "Talk. Okay. I'm scared of silence, and of being quiet. And of being forgotten. There. Now it's on the barrel."  ✏ `dlg.piper.fright.r0`
    r1 [piper]: "...Thank you. Silence is easier when someone shares it."  ✏ `dlg.piper.fright.r1`   (expr happy)
    r2 [piper]: "A bucket! You're going to drum on a bucket? ...Okay. Okay! Count me in, in four. One, two, three..."  ✏ `dlg.piper.fright.r2`   (expr happy)
  · (piper expression neutral)
  [piper]: "Thank you. The nerves are still here. But they're smaller. They fit in the fiddle case."  ✏ `event.piper_heart6.after`
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.piper_heart6
  · effects: friend:piper,60

### piper_heart8 · "The Chorus" · wholesome
conversation · wave2_events.json · map Saloon · priority 7
plays when: `hearts:piper>=8 && flag:event.piper_heart6 && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Come sit by the stage. I'm writing a song, and I need your help with every line. It's the only way it'll be good."  ✏ `event.piper_heart8.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote note)
  ▶ DIALOGUE `piper.chorus` (player-facing graph, plays here):
    n0 [piper]: "The verse is about someone arriving. The chorus is about where they end up. What is it?"  ✏ `dlg.piper.chorus.0`
       ? "Home." → r0  effects: friend:piper,15, flag:choice.chorus.home  ✏ `dlg.piper.chorus.c0`
       ? "The road." → r1  effects: friend:piper,10, flag:choice.chorus.road  ✏ `dlg.piper.chorus.c1`
       ? "This room." → r2  effects: friend:piper,10, flag:choice.chorus.room  ✏ `dlg.piper.chorus.c2`
    r0 [piper]: "Home. ...Oh. Oh, that's it. I've been missing that word for three months."  ✏ `dlg.piper.chorus.r0`   (expr happy)
    r1 [piper]: "The road. Restless, wide and honest. It rhymes with "code". If you squint."  ✏ `dlg.piper.chorus.r1`
    r2 [piper]: "This room! A song about the saloon. The stools will cry."  ✏ `dlg.piper.chorus.r2`   (expr happy)
  [piper]: "{if choice:chorus.home}'Home' in the chorus. Of course. That's the word that was missing.{else}{if choice:chorus.road}'The road' in the chorus. Yes. Wanderers love a road.{else}'The saloon' in the chorus. A song about the room itself. Brilliant, and a bit cheeky.{/if}{/if}"  ✏ `event.piper_heart8.after`
  · effects: flag:event.piper_heart8
  · effects: friend:piper,70

### piper_heart10 · "The Song They Know" · wholesome
performance · wave2_events.json · map Saloon · priority 7
plays when: `hearts:piper>=10 && flag:event.piper_heart8 && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Tonight I play the song we wrote. I'll start it. After the first verse, I'm stepping off the stage."  ✏ `event.piper_heart10.intro`
  · (move player to (11,6))
  · (player faces up)
  · (lighting warm)
  · (letterbox on)
  · (camera focus on piper)
  · (music piper_theme)
  · (together: piper dance; piper emote note)
  · (sfx coin)
  · (music stop)
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  [piper]: "{if choice:chorus.home}They sang 'home' back to me. All of them. I didn't play a note.{else}{if choice:chorus.road}They sang the road back to me. Every verse. I only stood and listened.{else}They sang the room back to the room. Even the stool joined in.{/if}{/if}"  ✏ `event.piper_heart10.outro`
  · effects: flag:event.piper_heart10
  · effects: friend:piper,80

### piper_friend · "Parsnip Ballad" · funny
shared_activity · wave2_events.json · map Saloon · priority 7
plays when: `hearts:piper>=3 && weekday:fri && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Friday! Come hear the parsnip ballad. It's four verses, and one of them is about the cat."  ✏ `event.piper_friend.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper emote note)
  · (piper sway)
  [piper]: "'Oh, parsnip, parsnip, in the soil...' I'm not done. I'm very close. I'm very dramatic."  ✏ `event.piper_friend.doing`
  [piper]: "That's a chorus! A bad one. Mine. Tell nobody, and come back for the encore."  ✏ `event.piper_friend.wrap`
  · effects: flag:event.piper_friend
  · effects: friend:piper,15

### piper_birthday · "Piper's Birthday" · funny
conversation · wave4_birthdays.json · map Saloon · priority 8 · repeatable
plays when: `hearts:piper>=2 && birthday.in:piper==0 && unseen:birthday.piper && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "It's my birthday, and I wrote myself a song. It's a very modest masterpiece. Sit in the front row."  ✏ `event.piper_birthday.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote note)
  ▶ DIALOGUE `piper.birthday` (player-facing graph, plays here):
    n0 [piper]: "Front row's yours. Now give me your best line!"  ✏ `dlg.piper.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:piper,10, flag:choice.bday.piper.wish  ✏ `dlg.piper.birthday.c0`
       ? "Sing it back to her." → r1  effects: friend:piper,10, flag:choice.bday.piper.sing  ✏ `dlg.piper.birthday.c1`
       ? "Ask for the next song." → r2  effects: friend:piper,10, flag:choice.bday.piper.ask  ✏ `dlg.piper.birthday.c2`
    r0 [piper]: "That's a chorus! Thank you. I'll use it in the encore."  ✏ `dlg.piper.birthday.r0`   (expr happy)
    r1 [piper]: "You sang my song! Badly! Perfectly! I'm stealing your key."  ✏ `dlg.piper.birthday.r1`   (expr happy)
    r2 [piper]: "The next one's for you. It's not finished. It will be a birthday present, on a late schedule."  ✏ `dlg.piper.birthday.r2`   (expr happy)
  [piper]: "{if choice:bday.piper.wish}It's in the encore. It rhymes with 'year', if you squint.{else}{if choice:bday.piper.sing}My song, your voice. The room clapped for you. I'm only a little jealous.{else}A song's coming, with your name in it. Don't ask when. It will rhyme.{/if}{/if}"  ✏ `event.piper_birthday.after`
  · effects: flag:event.piper_birthday
  · effects: mark:birthday.piper, friend:piper,30, give:artisan.juice,2, mail:piper_bday_thanks

### piper_courtship · "Piper Asks" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `hearts:piper>=8 && flag:event.piper_heart8 && !flag:partners.piper && !flag:choice.courtship.piper.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "So. I've written a love song and a coward's song. This is the coward's. Would you court me? It scans if you say yes."  ✏ `event.piper_courtship.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote note)
  ▶ DIALOGUE `piper.courtship` (player-facing graph, plays here):
    n0 [piper]: "What do you say?"  ✏ `dlg.piper.courtship.0`
       ? "Yes. Sing me the love song." → r0  effects: friend:piper,20, flag:choice.courtship.piper.yes  ✏ `dlg.piper.courtship.c0`
       ? "Let's keep it as friends." → r1  effects: friend:piper,15, flag:choice.courtship.piper.friends  ✏ `dlg.piper.courtship.c1`
       ? "Give me a verse or two." → r2  effects: friend:piper,5, flag:choice.courtship.piper.later  ✏ `dlg.piper.courtship.c2`
    r0 [piper]: "Yes! The love song's for you. It rhymes! It has a bridge! I'm going to be insufferable."  ✏ `dlg.piper.courtship.r0`   (expr happy)
    r1 [piper]: "Friends. That's a good chorus, too. I'll sing it with feeling. Thank you for the honest note."  ✏ `dlg.piper.courtship.r1`   (expr happy)
    r2 [piper]: "A verse or two. Fair. It's a long song. I'll leave the last line unwritten."  ✏ `dlg.piper.courtship.r2`   (expr thinking)
  [piper]: "{if flag:choice.courtship.piper.yes}A love song, rhymed and bridged. You'll hear it Friday. It's unbearable. You'll love it.{else}{if flag:choice.courtship.piper.friends}Friends. A great duet. We'll get a spotlight and a better stool.{else}A verse or two, and a bridge. Take your time. I'll hum while I wait.{/if}{/if}"  ✏ `event.piper_courtship.after`
  · effects: flag:event.piper_courtship
  · effects: flag:courtship.asked.piper

### piper_courtship2 · "Piper Asks Again" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `hearts:piper>=10 && flag:choice.courtship.piper.later && !flag:event.piper_courtship2.done && !flag:partners.piper && !flag:choice.courtship.piper.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "So. I've written a love song and a coward's song. This is the coward's. Would you court me? It scans if you say yes."  ✏ `event.piper_courtship2.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote note)
  ▶ DIALOGUE `piper.courtship` (player-facing graph, plays here):
    n0 [piper]: "What do you say?"  ✏ `dlg.piper.courtship.0`
       ? "Yes. Sing me the love song." → r0  effects: friend:piper,20, flag:choice.courtship.piper.yes  ✏ `dlg.piper.courtship.c0`
       ? "Let's keep it as friends." → r1  effects: friend:piper,15, flag:choice.courtship.piper.friends  ✏ `dlg.piper.courtship.c1`
       ? "Give me a verse or two." → r2  effects: friend:piper,5, flag:choice.courtship.piper.later  ✏ `dlg.piper.courtship.c2`
    r0 [piper]: "Yes! The love song's for you. It rhymes! It has a bridge! I'm going to be insufferable."  ✏ `dlg.piper.courtship.r0`   (expr happy)
    r1 [piper]: "Friends. That's a good chorus, too. I'll sing it with feeling. Thank you for the honest note."  ✏ `dlg.piper.courtship.r1`   (expr happy)
    r2 [piper]: "A verse or two. Fair. It's a long song. I'll leave the last line unwritten."  ✏ `dlg.piper.courtship.r2`   (expr thinking)
  [piper]: "{if flag:choice.courtship.piper.yes}A love song, rhymed and bridged. You'll hear it Friday. It's unbearable. You'll love it.{else}{if flag:choice.courtship.piper.friends}Friends. A great duet. We'll get a spotlight and a better stool.{else}A verse or two, and a bridge. Take your time. I'll hum while I wait.{/if}{/if}"  ✏ `event.piper_courtship2.after`
  · effects: flag:event.piper_courtship2
  · effects: flag:courtship.asked.piper, flag:event.piper_courtship2.done

### piper_partners · "Piper, Together" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `hearts:piper>=10 && flag:choice.courtship.piper.yes && !flag:partners.piper && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Hey, you. Front row. I wrote a song for you and it's the first one I haven't hidden. Please clap anyway. Gently."  ✏ `event.piper_partners.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote heart)
  ▶ DIALOGUE `piper.partners` (player-facing graph, plays here):
    n0 [piper]: "No gig tonight. Just us. What's the set list?"  ✏ `dlg.piper.partners.0`
       ? "Listen to the whole song." → r0  effects: friend:piper,10, flag:choice.courtship.piper.song  ✏ `dlg.piper.partners.c0`
       ? "Sing the chorus with her." → r1  effects: friend:piper,10, flag:choice.courtship.piper.duet  ✏ `dlg.piper.partners.c1`
       ? "Walk her home after the set." → r2  effects: friend:piper,10, flag:choice.courtship.piper.walk  ✏ `dlg.piper.partners.c2`
    r0 [piper]: "Every note. Oh, you star. I might tear up on the high part, and I'll blame the stage lights."  ✏ `dlg.piper.partners.r0`   (expr happy)
    r1 [piper]: "A duet! You know the words! ...You know the words. I wrote them yesterday. You're a wonder."  ✏ `dlg.piper.partners.r1`   (expr happy)
    r2 [piper]: "A walk after the set. The night's quiet. I'm loud. Let's see who wins."  ✏ `dlg.piper.partners.r2`   (expr happy)
  [piper]: "{if choice:courtship.piper.song}The song has your name in the bridge. The bridge is the best part. It's yours.{else}{if choice:courtship.piper.duet}We sang the chorus together. The room joined in. I almost forgot the bridge.{else}A walk home under the stars. I hummed. You hummed back. The best accompaniment.{/if}{/if}"  ✏ `event.piper_partners.after`
  · effects: flag:event.piper_partners
  · effects: flag:partners.piper, friend:piper,50

### overheard_wren_piper_1 · "Sound Check" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.piper && hearts:wren>=2 && hearts:piper>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "You're flat on the third bar. Fabulously flat. Do it again."
  [piper]: "I'm not flat, the stool's tilted! It's a stage stool, Wren, it has opinions!"  ✏ `event.overheard_wren_piper_1.l2`
  · (piper shrug (background))
  · (piper emote note)
  · (wren point (background))
  [wren]: "The stool doesn't have opinions. I do. Again. From the top."
  [piper]: "Fine, fine, from the top. But I'm taking the bridge up a key out of spite."  ✏ `event.overheard_wren_piper_1.l4`
  · effects: flag:event.overheard_wren_piper_1

### overheard_wren_piper_2 · "The Shared Verse" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.piper && hearts:wren>=4 && hearts:piper>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_1`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "I wrote a verse. It's for the saloon. It's about you, and I'm sorry."
  [piper]: "Wren! You wrote a verse? Read it! Read it! No, wait. Sing it. Sing it badly. I'll fix it."  ✏ `event.overheard_wren_piper_2.l2`
  · (piper shrug (background))
  · (piper emote note)
  · (wren wave (background))
  [wren]: "'Piper sings and the rafters shake, and every glass in town awake.' That's all I've got."
  [piper]: "It's perfect. I'm stealing it. It's the opening of my next song, and you're getting credit in tiny letters."  ✏ `event.overheard_wren_piper_2.l4`
  · effects: flag:event.overheard_wren_piper_2

### overheard_wren_piper_3 · "Closing Time" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.piper && hearts:wren>=6 && hearts:piper>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_2`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "Today's last song, Piper. The room will be tired. So will I. Make it a soft one."
  [piper]: "Soft, I can do. I'll hum. No words. Just you and the stool and the sunlight."  ✏ `event.overheard_wren_piper_3.l2`
  · (piper point (background))
  · (piper emote note)
  · (wren shrug (background))
  [wren]: "My darling, that's the best verse you've written. Maybe write it down. In pencil."
  [piper]: "Pencil it is. It'll live in the room. Save me the last glass."  ✏ `event.overheard_wren_piper_3.l4`
  · effects: flag:event.overheard_wren_piper_3

### overheard_wren_piper_4 · "The Headliner" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.piper && hearts:wren>=8 && hearts:piper>=8 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_3`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "Piper, my darling. Next month, you headline. I mean it."
  [piper]: "Wren! Headline! Me! On your stage! I'll cry! Then write a song about crying!"  ✏ `event.overheard_wren_piper_4.l2`
  · (piper wave (background))
  · (piper emote note)
  · (wren wave (background))
  [wren]: "Do both. Cry first. Song after. The stool will survive."
  [piper]: "It's a deal! Actually, it's a duet!"  ✏ `event.overheard_wren_piper_4.l4`
  · effects: flag:event.overheard_wren_piper_4

### overheard_piper_juno_1 · "Volume Control" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.piper && flag:met.juno && hearts:piper>=2 && hearts:juno>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), juno(26, 18)
  · (place piper to (24,18))
  · (place juno to (26,18))
  · (piper faces right)
  · (juno faces left)
  [piper]: "Juno! Could you hammer quieter? My ballad is drowning!"  ✏ `event.overheard_piper_juno_1.l1`
  [juno]: "Quieter? Piper, it's a forge! It's meant to be felt!"
  · (juno shrug (background))
  · (juno emote note)
  · (piper point (background))
  [piper]: "I felt it in my teeth! Through the stage!"  ✏ `event.overheard_piper_juno_1.l3`
  [juno]: "Then the anvil's a fan! I'll take it!"
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
  [piper]: "Juno! I wrote a song to the rhythm of your hammer!"  ✏ `event.overheard_piper_juno_2.l1`
  [juno]: "You did? Play it! No, wait, I'll hammer along!"
  · (juno shrug (background))
  · (juno emote note)
  · (piper wave (background))
  [piper]: "One, two, clang! That's the chorus!"  ✏ `event.overheard_piper_juno_2.l3`
  [juno]: "It's a banger! I'll label it 'Song (Forge)'!"
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
  [piper]: "Juno, you made me a tiny stage out of a crate?"  ✏ `event.overheard_piper_juno_3.l1`
  [juno]: "It says 'Stage (Piper)'! I labeled it! Play on it!"
  · (juno point (background))
  · (juno emote note)
  · (piper shrug (background))
  [piper]: "I'll play my best set! You watch!"  ✏ `event.overheard_piper_juno_3.l3`
  [juno]: "Okay! No, I'll hammer! No, watch! Both! Always both!"
  · effects: flag:event.overheard_piper_juno_3

### overheard_hazel_piper_1 · "Pencil Rhymes" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.hazel && flag:met.piper && hearts:hazel>=2 && hearts:piper>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), hazel(26, 18)
  · (place piper to (24,18))
  · (place hazel to (26,18))
  · (piper faces right)
  · (hazel faces left)
  [piper]: "Hazel! Someone's been fixing the rhymes in my songbook. In very neat pencil."  ✏ `event.overheard_hazel_piper_1.l1`
  [hazel]: "(Shh.) Outdoor voice is still a voice. ...And it wasn't me."
  · (hazel shrug (background))
  · (hazel emote exclaim)
  · (piper point (background))
  [piper]: "It was you! 'Moon' doesn't rhyme with 'spoon' anymore. It rhymes with 'soon'. It's better!"  ✏ `event.overheard_hazel_piper_1.l3`
  [hazel]: "(It was always better.) You're welcome."
  · effects: flag:event.overheard_hazel_piper_1

### overheard_hazel_piper_2 · "Eight Syllables" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.hazel && flag:met.piper && hearts:hazel>=4 && hearts:piper>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_hazel_piper_1`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), hazel(26, 18)
  · (place piper to (24,18))
  · (place hazel to (26,18))
  · (piper faces right)
  · (hazel faces left)
  [piper]: "Help. My new chorus has nine syllables and the tune only has eight."  ✏ `event.overheard_hazel_piper_2.l1`
  [hazel]: "(Cut 'really'.) Nobody needs 'really'."
  · (hazel point (background))
  · (hazel emote question)
  · (piper shrug (background))
  [piper]: "...Oh. Oh, that's it! Who taught you songs?"  ✏ `event.overheard_hazel_piper_2.l3`
  [hazel]: "(Books.) And you. Every Thursday, from the back row."
  · effects: flag:event.overheard_hazel_piper_2

### overheard_hazel_piper_3 · "The Hidden Verse" · surprise
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.hazel && flag:met.piper && hearts:hazel>=6 && hearts:piper>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_hazel_piper_2 && flag:event.piper_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), hazel(26, 18)
  · (place piper to (24,18))
  · (place hazel to (26,18))
  · (piper faces right)
  · (hazel faces left)
  [piper]: "Hazel. Write me one verse. Just one. Your words, my tune."  ✏ `event.overheard_hazel_piper_3.l1`
  [hazel]: "(I can't.) ...I already did. It's in your fiddle case."
  · (hazel shrug (background))
  · (hazel emote sparkle)
  · (piper wave (background))
  [piper]: "That's where it came from? It's the best verse I own! I'll play it with your name on it."  ✏ `event.overheard_hazel_piper_3.l3`
  [hazel]: "(Not my name.) Just play it. That's plenty."
  · effects: flag:event.overheard_hazel_piper_3
