# Scene cards for piper

24 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 57 spoken lines in scenes, 34 dialogue lines and 24 player choice labels.

### piper_heart2 · "Learn a Chord" · funny
event · npc_roster.json · map Saloon · priority 6
plays when: `(hearts:piper>=2 && !weekday:tue && hour>=18 && hour<24) && !flag:event.piper_heart2`
beats may be ADDED or CUT in this scene
placed: player(11, 6)
  · (piper faces down)
  [piper]: "[player]! Up here. Learn one chord with me. Just one. It's the friendly one."  ✏ `event.piper_heart2.0`
  · (move player to (11,6))
  · (player faces up)
  · (piper emote note)
  [piper]: "Thumb there, two fingers there, now pull. ...There. You're a musician now. That's the whole secret."  ✏ `event.piper_heart2.1`
  [piper]: "That chord took me a week. You took a minute. I'm fine. I'm completely fine about it."  ✏ `event.piper_heart2.3`  (emote sweat, expr embarrassed)
  [piper]: "Here, a sunflower for your first gig. Don't quit the farm. Come back Friday, and I'll teach you the second chord."  ✏ `event.piper_heart2.2`
  · effects: give:crop.sunflower,1, friend:piper,40, flag:event.piper_heart2

### piper_heart5 · "A Song for You" · wholesome
event · npc_roster.json · map Saloon · priority 7
plays when: `(hearts:piper>=5 && !weekday:tue && hour>=18 && hour<24 && flag:event.piper_heart2) && !flag:event.piper_heart5`
beats may be ADDED or CUT in this scene
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Sit. This one's yours. Nobody requested it, which is how you know it's special."  ✏ `event.piper_heart5.0`
  · (move player to (11,6))
  · (player faces up)
  · (piper emote note)
  [piper]: "First time out of the case. Elara hasn't heard it yet, and she hears everything first."  ✏ `event.piper_heart5.1`
  [piper]: "If it's bad, clap anyway. If it's good, clap louder. I'm flexible."  ✏ `event.piper_heart5.3`  (expr happy)
  [piper]: "It ends on four notes I hum when nobody's around. I don't know where I got them. I keep meaning to find out."  ✏ `event.piper_heart5.2`
  · effects: give:artisan.jam,2, friend:piper,60, flag:event.piper_heart5

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
  [wren]: "[player]. I said I'd open it on the night, and this is the night. First glass is yours. I poured it before the doors."  (expr happy)
  · (remove prop wine)
  [tilda]: "Nobody asked for credit for any of this. So I'm giving it, in the good chalk, under every name in this room."
  [piper]: "Same song, bigger room. One verse, then I step off again. My bow hand still hasn't heard."  ✏ `event.hall_dance.4`  (emote sweat)
  · (music piper_theme)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  ✏ `event.hall_dance.5`  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall takes the chorus. Piper stops playing and sways, and the song goes on without her."
  [bram]: "Hm. Hm-hm. Hm. I sing it in my own language."  (emote note)
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

### story_umbrella · "The Red Umbrella" · funny
event · storyline_events.json · map Village · priority 7
plays when: `(storyline:lost_umbrella && weather:rain && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.story_umbrella`
beats may be ADDED or CUT in this scene
placed: piper(25, 18), player(25, 17)
  · (place piper to (25,18))
  · (piper faces down)
  · (spawn prop prop.umbrella at (26,18))
  [piper]: "Wait. Stop. Is that my umbrella? On a fence? Being useful without me?"  ✏ `event.story_umbrella.intro`
  · (move player to (25,17))
  · (player faces up)
  · (piper emote question)
  · (piper point (background))
  ▶ DIALOGUE `story.umbrella.talk` (player-facing graph, plays here):
    n0 [narration]: "A large red umbrella hangs open on the fence post, dripping with great dignity. Piper points at it."  ✏ `dlg.story.umbrella.talk.0`   (→ n1)
    n1 [piper]: "Third time it's wandered off. It always turns up drier than I do. What do we do?"  ✏ `dlg.story.umbrella.talk.1`
       ? "Give it back to her." → r0  effects: flag:choice.umbrella.return  ✏ `dlg.story.umbrella.talk.c0`
       ? "Leave it on the fence." → r1  effects: flag:choice.umbrella.hang  ✏ `dlg.story.umbrella.talk.c1`
       ? "Keep it. It suits me." → r2  effects: flag:choice.umbrella.keep  ✏ `dlg.story.umbrella.talk.c2`
    r0 [piper]: "Reunited! I'd write it a song, but it would wander off mid-verse."  ✏ `dlg.story.umbrella.talk.r0`   (expr happy)
    r1 [piper]: "Leave it? ...Yes. A little red landmark. Somebody out in this rain needs it more."  ✏ `dlg.story.umbrella.talk.r1`
    r2 [piper]: "You're keeping it? That's theft with excellent manners. ...It does suit you."  ✏ `dlg.story.umbrella.talk.r2`   (emote sweat)
  [piper]: "{if choice:umbrella.return}Back where it belongs. But someone keeps hanging it up so neatly. Who tidies a stranger's umbrella?{else}{if choice:umbrella.hang}A landmark. I'll look for it every rain. And whoever keeps hanging it there, I'll catch them humming.{else}Shared custody. I accept, under protest. Bring it Fridays so it hears me play.{/if}{/if}"  ✏ `event.story_umbrella.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_umbrella, friend:piper,30, flag:storydone.lost_umbrella

### story_band_battle · "Battle of the Bands" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `(storyline:competing_band && hearts:piper>=2 && hearts:juno>=2 && hour>=17 && hour<21 && !festival.in:==0) && !flag:event.story_band_battle`
beats may be ADDED or CUT in this scene
placed: piper(25, 18), player(25, 17)
  · (place piper to (25,18))
  · (piper faces down)
  [piper]: "Two bands, one square, one extremely polite war. Judge us, [player]. Be honest. Honest means fiddle."  ✏ `event.story_band_battle.intro`
  · (move player to (25,17))
  · (player faces up)
  · (piper emote note)
  ▶ DIALOGUE `story.band.talk` (player-facing graph, plays here):
    n0 [piper]: "Exhibit A: one fiddle, one voice, a lifetime of practice. Try not to swoon."  ✏ `dlg.story.band.talk.0`   (→ n1)
    n1 [juno]: "Exhibit B: three anvils, the wash bucket, and Bram, who has agreed to hit one thing. Once. Gently."   (→ n2)
    n2 [bram]: "Hm. One hit. I'm saving it for the end."   (→ n3)
    n3 [piper]: "Well? Who owns the square tonight? Take your time. Pick the fiddle."  ✏ `dlg.story.band.talk.3`
       ? "The fiddle, obviously." → r0  effects: flag:choice.band.piper  ✏ `dlg.story.band.talk.c0`
       ? "The forge band, honestly." → r1  effects: flag:choice.band.juno  ✏ `dlg.story.band.talk.c1`
       ? "Both, together. Loudly." → r2  effects: flag:choice.band.both, friend:piper,10, friend:juno,10  ✏ `dlg.story.band.talk.c2`
    r0 [piper]: "Justice! And good taste! Juno, you were wonderful. Just less fiddle."  ✏ `dlg.story.band.talk.r0`   (expr happy)
    r1 [juno]: "We won! Bram, we won! ...Bram? He's hitting the thing. Twice!"   (expr happy)
    r2 [piper]: "Together? ...That's either genius or a noise complaint. Bram, one hit. Make it count."  ✏ `dlg.story.band.talk.r2`   (emote note)
  [piper]: "{if choice:band.piper}The fiddle wins! Juno took it beautifully. She's already forging a louder anvil.{else}{if choice:band.juno}The forge band wins! I'm thrilled. I'm furious. I'm writing a song about both.{else}A joint set! Fiddle, anvils, and a whole square clapping on the beat. Mostly on the beat.{/if}{/if}"  ✏ `event.story_band_battle.after`
  [piper]: "And Bram tapped his foot on the second verse. I saw it. He'll deny it until spring."  ✏ `event.story_band_battle.0`  (emote note, expr happy)
  · effects: flag:event.story_band_battle, friend:piper,20, friend:juno,20, flag:storydone.competing_band

### piper_heart4 · "Request Line" · funny
conversation · wave2_events.json · map Saloon · priority 7
plays when: `(hearts:piper>=4 && flag:event.piper_heart2 && !weekday:tue && hour>=18 && hour<24) && !flag:event.piper_heart4`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Last set, and I'm out of ideas. You pick. Anything. I'll play it like I wrote it."  ✏ `event.piper_heart4.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote note)
  ▶ DIALOGUE `piper.request` (player-facing graph, plays here):
    n0 [piper]: "The room's waiting. A ballad, a jig, or my parsnip masterpiece? Choose wisely. Choose parsnips."  ✏ `dlg.piper.request.0`
       ? "A ballad, please." → r0  effects: friend:piper,10, flag:choice.request.ballad  ✏ `dlg.piper.request.c0`
       ? "A jig! Get them dancing." → r1  effects: friend:piper,10, flag:choice.request.jig  ✏ `dlg.piper.request.c1`
       ? "The parsnip song. Obviously." → r2  effects: friend:piper,10, flag:choice.request.parsnip  ✏ `dlg.piper.request.c2`
    r0 [piper]: "A ballad. Sad in the middle, happy at the end. Like a good harvest. Gather round."  ✏ `dlg.piper.request.r0`   (emote note)
    r1 [piper]: "A jig! Mind your boots. The good stool's been waiting all night to dance."  ✏ `dlg.piper.request.r1`   (expr happy)
    r2 [piper]: "The parsnip song! You have taste. Questionable, magnificent taste."  ✏ `dlg.piper.request.r2`   (expr happy)
  [piper]: "{if choice:request.ballad}A ballad, slow, with one high note I'm saving. I'm writing down that you asked. It matters later.{else}{if choice:request.jig}A jig! Feet, take cover. I'm writing your request down. Don't ask why yet.{else}The parsnip song, with feeling and a straight face. I'm keeping this request. Someday I'll play it back.{/if}{/if}"  ✏ `event.piper_heart4.after`
  · effects: flag:event.piper_heart4
  · effects: friend:piper,50

### piper_heart6 · "Stage Fright" · surprise
confession · wave2_events.json · map Saloon · priority 7
plays when: `(hearts:piper>=6 && flag:event.piper_heart5 && flag:event.piper_heart4 && !weekday:tue && hour>=18 && hour<24) && !flag:event.piper_heart6`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Down here. The cellar. I'm not hiding. I'm resting my nerves among the barrels. They're very supportive."  ✏ `event.piper_heart6.intro`
  · (move player to (11,6))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on piper)
  · (piper expression thinking)
  ▶ DIALOGUE `piper.fright` (player-facing graph, plays here):
    n0 [piper]: "I can play for a hundred people and never blink. Tonight I think they'll finally notice. I'm not as good as I say."  ✏ `dlg.piper.fright.0`   (→ n1; expr sad)
    n1 [piper]: "So. What do I do with that?"  ✏ `dlg.piper.fright.1`
       ? "Talk it through with her." → r0  effects: friend:piper,10, flag:choice.fright.talk  ✏ `dlg.piper.fright.c0`
       ? "Sit beside her, quietly." → r1  effects: friend:piper,15, flag:choice.fright.sit  ✏ `dlg.piper.fright.c1`
       ? "Drum along on a bucket." → r2  effects: friend:piper,10, flag:choice.fright.bucket  ✏ `dlg.piper.fright.c2`
    r0 [piper]: "Talk. Okay. I'm scared of quiet. Of being a good player and a forgettable writer. There. It's on the barrel now."  ✏ `dlg.piper.fright.r0`
    r1 [piper]: "...You didn't fill it. Nobody ever leaves the quiet alone around me. It's not so bad, shared."  ✏ `dlg.piper.fright.r1`   (expr happy)
    r2 [piper]: "A bucket? You're drumming on a bucket? ...Okay. Okay! In four. One, two, three..."  ✏ `dlg.piper.fright.r2`   (expr happy)
  · (piper expression neutral)
  [piper]: "The nerves are still here, just smaller. They fit in the fiddle case. Someday I want a song they sing without me. Tonight, I'll go up."  ✏ `event.piper_heart6.after`
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.piper_heart6
  · effects: friend:piper,60

### piper_heart8 · "The Chorus" · wholesome
conversation · wave2_events.json · map Saloon · priority 7
plays when: `(hearts:piper>=8 && flag:event.piper_heart6 && !weekday:tue && hour>=18 && hour<24) && !flag:event.piper_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Sit by the stage. I'm writing a song, and I need you on every line. It's the only way it'll be good."  ✏ `event.piper_heart8.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote note)
  ▶ DIALOGUE `piper.chorus` (player-facing graph, plays here):
    n0 [piper]: "The verse is about someone arriving. The chorus is about where they end up. What is it?"  ✏ `dlg.piper.chorus.0`
       ? "Home." → r0  effects: friend:piper,15, flag:choice.chorus.home  ✏ `dlg.piper.chorus.c0`
       ? "The road." → r1  effects: friend:piper,10, flag:choice.chorus.road  ✏ `dlg.piper.chorus.c1`
       ? "This room." → r2  effects: friend:piper,10, flag:choice.chorus.room  ✏ `dlg.piper.chorus.c2`
    r0 [piper]: "Home. ...Oh. That's it. I've been missing that word for three months. It was right here."  ✏ `dlg.piper.chorus.r0`   (expr happy)
    r1 [piper]: "The road. Wide and honest. It rhymes with 'code', if you squint."  ✏ `dlg.piper.chorus.r1`
    r2 [piper]: "This room! A song about the saloon. The good stool is going to be unbearable."  ✏ `dlg.piper.chorus.r2`   (expr happy)
  [piper]: "{if choice:chorus.home}'Home' in the chorus. Of course. Now it needs a room full of people to sing it without me.{else}{if choice:chorus.road}'The road' in the chorus. Wanderers love a road. Now it needs a room to sing it without me.{else}'This room' in the chorus. Cheeky and right. Now the room has to sing it without me.{/if}{/if}"  ✏ `event.piper_heart8.after`
  · effects: flag:event.piper_heart8
  · effects: friend:piper,70

### piper_heart10 · "The Song They Know" · wholesome
performance · wave2_events.json · map Saloon · priority 7
plays when: `(hearts:piper>=10 && flag:event.piper_heart8 && !weekday:tue && hour>=18 && hour<24) && !flag:event.piper_heart10`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Tonight we play the song we wrote. I'll start it. After the first verse, I step off. Don't let me climb back on."  ✏ `event.piper_heart10.intro`
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
  [piper]: "You asked for a ballad at the harvest. I saved the sad middle for tonight. Hear it hold? That's what you ordered."  ✏ `event.piper_heart10.req_ballad` [only if flag:choice.request.ballad]
  [piper]: "You asked for a jig that night. Tonight's has one in the last verse. Watch the floor. It remembers."  ✏ `event.piper_heart10.req_jig` [only if flag:choice.request.jig]
  [piper]: "You asked for the parsnip song once. There's a parsnip in the second verse. Nobody's caught it. Don't tell."  ✏ `event.piper_heart10.req_parsnip` [only if flag:choice.request.parsnip]
  [piper]: "{if choice:chorus.home}They sang 'home' without me. All of them. I played one verse and put the bow down. ...So that's what quiet sounds like.{else}{if choice:chorus.road}They took the road and walked it without me. I stood there with my bow down. Best seat I've ever had.{else}The room sang itself back to the room. Even the good stool joined in. I only had to stand there.{/if}{/if}"  ✏ `event.piper_heart10.outro`
  · effects: flag:event.piper_heart10
  · effects: friend:piper,80

### piper_friend · "Parsnip Ballad" · funny
shared_activity · wave2_events.json · map Saloon · priority 7
plays when: `(hearts:piper>=3 && weekday:fri && !weekday:tue && hour>=18 && hour<24) && !flag:event.piper_friend`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Friday! Come hear the parsnip ballad. Four verses, and one's about the gray cat. It hasn't approved its verse."  ✏ `event.piper_friend.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper emote note)
  · (piper sway)
  [piper]: "'Oh parsnip, pale and patient root...' Wait. It rhymes with 'boot'. If you squint. I'm very close."  ✏ `event.piper_friend.doing`
  [piper]: "That's a chorus! A bad one. Mine. Come back next Friday. Verse five is about you."  ✏ `event.piper_friend.wrap`
  · effects: flag:event.piper_friend
  · effects: friend:piper,15

### piper_birthday · "Piper's Birthday" · funny
conversation · wave4_birthdays.json · map Saloon · priority 8 · repeatable
plays when: `hearts:piper>=2 && birthday.in:piper==0 && unseen:birthday.piper && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "My birthday! I wrote myself a song. It's a very modest masterpiece. You'll want a good seat."  ✏ `event.piper_birthday.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote note)
  ▶ DIALOGUE `piper.birthday` (player-facing graph, plays here):
    n0 [piper]: "Front row's yours. Now give me your best line!"  ✏ `dlg.piper.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:piper,10, flag:choice.bday.piper.wish  ✏ `dlg.piper.birthday.c0`
       ? "Sing her song back to her." → r1  effects: friend:piper,10, flag:choice.bday.piper.sing  ✏ `dlg.piper.birthday.c1`
       ? "Ask what the next song is." → r2  effects: friend:piper,10, flag:choice.bday.piper.ask  ✏ `dlg.piper.birthday.c2`
    r0 [piper]: "Happy birthday to me, from you! Short, sweet, and in tune. It's going in the encore."  ✏ `dlg.piper.birthday.r0`   (expr happy)
    r1 [piper]: "You sang my song! Back to me! ...I'm keeping your key. It's mine now."  ✏ `dlg.piper.birthday.r1`   (expr happy)
    r2 [piper]: "The next one's yours. It isn't finished. It's a birthday present on a musician's schedule."  ✏ `dlg.piper.birthday.r2`   (expr happy)
  [piper]: "{if choice:bday.piper.wish}Your wish made the encore. I rhymed 'birthday' with 'worth the day'. Don't check.{else}{if choice:bday.piper.sing}My song, your voice, and the room clapped for you. I'm only a tiny bit jealous.{else}A song's coming with your name in it. Don't ask when. It will rhyme. Eventually.{/if}{/if}"  ✏ `event.piper_birthday.after`
  · effects: flag:event.piper_birthday
  · effects: mark:birthday.piper, friend:piper,30, give:artisan.juice,2, mail:piper_bday_thanks

### piper_courtship · "Piper Asks" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `(hearts:piper>=8 && flag:event.piper_heart8 && !flag:partners.piper && !flag:choice.courtship.piper.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:tue && hour>=18 && hour<24) && !flag:event.piper_courtship`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "So. I wrote a love song and a coward's song. This is the coward's. Would you court me? It scans if you say yes."  ✏ `event.piper_courtship.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote note)
  ▶ DIALOGUE `piper.courtship` (player-facing graph, plays here):
    n0 [piper]: "What do you say?"  ✏ `dlg.piper.courtship.0`
       ? "Yes. Sing me the love song." → r0  effects: friend:piper,20, flag:choice.courtship.piper.yes  ✏ `dlg.piper.courtship.c0`
       ? "I'd rather we stay friends." → r1  effects: friend:piper,15, flag:choice.courtship.piper.friends  ✏ `dlg.piper.courtship.c1`
       ? "Give me a verse or two." → r2  effects: friend:piper,5, flag:choice.courtship.piper.later  ✏ `dlg.piper.courtship.c2`
    r0 [piper]: "Yes! Then the love song's yours. It rhymes! It has a bridge! I'm going to be insufferable."  ✏ `dlg.piper.courtship.r0`   (expr happy)
    r1 [piper]: "Friends. That's a good song too. I'll still play your requests first. Thanks for the honest note."  ✏ `dlg.piper.courtship.r1`   (expr happy)
    r2 [piper]: "A verse or two. Fair. It's a long song. I'll leave the last line open."  ✏ `dlg.piper.courtship.r2`   (expr thinking)
  [piper]: "{if flag:choice.courtship.piper.yes}A love song, rhymed and bridged. You'll hear it Friday. It's unbearable. You'll love it.{else}{if flag:choice.courtship.piper.friends}Friends. The best duet going. Wren will still charge us both for the good stool.{else}A verse or two, then. Take your time. I'll hum while I wait. Quietly. Mostly.{/if}{/if}"  ✏ `event.piper_courtship.after`
  · effects: flag:event.piper_courtship
  · effects: flag:courtship.asked.piper

### piper_courtship2 · "Piper Asks Again" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `hearts:piper>=10 && flag:choice.courtship.piper.later && !flag:event.piper_courtship2.done && !flag:partners.piper && !flag:choice.courtship.piper.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:tue && hour>=18 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Me again. I finished the coward's song. It has a second verse now. Same question. Still scans if you say yes."  ✏ `event.piper_courtship2.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote note)
  ▶ DIALOGUE `piper.courtship` (player-facing graph, plays here):
    n0 [piper]: "What do you say?"  ✏ `dlg.piper.courtship.0`
       ? "Yes. Sing me the love song." → r0  effects: friend:piper,20, flag:choice.courtship.piper.yes  ✏ `dlg.piper.courtship.c0`
       ? "I'd rather we stay friends." → r1  effects: friend:piper,15, flag:choice.courtship.piper.friends  ✏ `dlg.piper.courtship.c1`
       ? "Give me a verse or two." → r2  effects: friend:piper,5, flag:choice.courtship.piper.later  ✏ `dlg.piper.courtship.c2`
    r0 [piper]: "Yes! Then the love song's yours. It rhymes! It has a bridge! I'm going to be insufferable."  ✏ `dlg.piper.courtship.r0`   (expr happy)
    r1 [piper]: "Friends. That's a good song too. I'll still play your requests first. Thanks for the honest note."  ✏ `dlg.piper.courtship.r1`   (expr happy)
    r2 [piper]: "A verse or two. Fair. It's a long song. I'll leave the last line open."  ✏ `dlg.piper.courtship.r2`   (expr thinking)
  [piper]: "{if flag:choice.courtship.piper.yes}Finally! That love song's been in the case since you said 'a verse or two'. Tonight it comes out.{else}{if flag:choice.courtship.piper.friends}Friends, then. Good. Some songs are better as duets. I'm keeping the second verse anyway.{else}Still a verse or two? Then the last line stays blank. Some songs end better late.{/if}{/if}"  ✏ `event.piper_courtship2.after`
  · effects: flag:event.piper_courtship2
  · effects: flag:courtship.asked.piper, flag:event.piper_courtship2.done

### piper_partners · "Piper, Together" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `(hearts:piper>=10 && flag:choice.courtship.piper.yes && !flag:partners.piper && !weekday:tue && hour>=18 && hour<24) && !flag:event.piper_partners`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 6)
  · (piper faces down)
  [piper]: "Hey, you. Front row. I wrote you a song, and it's the first one I haven't hidden behind a joke. Mostly."  ✏ `event.piper_partners.intro`
  · (move player to (11,6))
  · (player faces up)
  · (piper wave (background))
  · (piper emote heart)
  ▶ DIALOGUE `piper.partners` (player-facing graph, plays here):
    n0 [piper]: "No gig tonight. Just us. What's the set list?"  ✏ `dlg.piper.partners.0`
       ? "Hear the whole song." → r0  effects: friend:piper,10, flag:choice.courtship.piper.song  ✏ `dlg.piper.partners.c0`
       ? "Sing the chorus with her." → r1  effects: friend:piper,10, flag:choice.courtship.piper.duet  ✏ `dlg.piper.partners.c1`
       ? "Walk her home after the set." → r2  effects: friend:piper,10, flag:choice.courtship.piper.walk  ✏ `dlg.piper.partners.c2`
    r0 [piper]: "Every note? Oh, you star. If I wobble on the high part, blame the lamps."  ✏ `dlg.piper.partners.r0`   (expr happy)
    r1 [piper]: "A duet! You know the words! ...I wrote them yesterday. How do you know the words?"  ✏ `dlg.piper.partners.r1`   (expr happy)
    r2 [piper]: "A walk home after. The night's quiet. I'm loud. Let's see who wins."  ✏ `dlg.piper.partners.r2`   (expr happy)
  [piper]: "{if choice:courtship.piper.song}Your name's in the bridge. The bridge is the best part. That's not an accident.{else}{if choice:courtship.piper.duet}We sang the chorus together, and the room joined in. I nearly forgot the bridge.{else}A walk home under the stars. I hummed. You hummed back. Best accompaniment I've ever had.{/if}{/if}"  ✏ `event.piper_partners.after`
  · effects: flag:event.piper_partners
  · effects: flag:partners.piper, friend:piper,50

### overheard_wren_piper_1 · "Sound Check" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.piper && hearts:wren>=2 && hearts:piper>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_wren_piper_1`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "You're flat on the third bar. Fabulously flat. Do it again."
  [piper]: "I'm not flat, the stool's tilted! It's a stage stool, Wren. It has opinions!"  ✏ `event.overheard_wren_piper_1.l2`
  · (piper shrug (background))
  · (piper emote note)
  · (wren point (background))
  [wren]: "The stool doesn't have opinions. I do. Again. From the top."
  [piper]: "Fine. From the top. But I'm taking the bridge up a key, out of spite."  ✏ `event.overheard_wren_piper_1.l4`
  · effects: flag:event.overheard_wren_piper_1

### overheard_wren_piper_2 · "The Shared Verse" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.piper && hearts:wren>=4 && hearts:piper>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_1) && !flag:event.overheard_wren_piper_2`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "I wrote a verse. It's for the saloon. It's about you, and I'm sorry."
  [piper]: "Wren! You wrote a verse? Read it! No, sing it! No, read it slowly!"  ✏ `event.overheard_wren_piper_2.l2`
  · (piper shrug (background))
  · (piper emote note)
  · (wren wave (background))
  [wren]: "'Piper sings and the rafters shake, and every glass in town awake.' That's all I've got."
  [piper]: "It's perfect. I'm stealing it for my next opening. You get credit. Tiny letters. Huge feelings."  ✏ `event.overheard_wren_piper_2.l4`
  · effects: flag:event.overheard_wren_piper_2

### overheard_wren_piper_3 · "Closing Time" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.piper && hearts:wren>=6 && hearts:piper>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_2) && !flag:event.overheard_wren_piper_3`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "Today's last song, Piper. The room will be tired. So will I. Make it a soft one."
  [piper]: "Soft, I can do. No words. Just a hum for you, the stool and the last of the sun."  ✏ `event.overheard_wren_piper_3.l2`
  · (piper point (background))
  · (piper emote note)
  · (wren shrug (background))
  [wren]: "That's the best verse you've ever said out loud. Write it down, in pencil. Ink makes you shy."
  [piper]: "Pencil it is. Somebody keeps fixing my pencil anyway. Save me the last glass."  ✏ `event.overheard_wren_piper_3.l4`
  · effects: flag:event.overheard_wren_piper_3

### overheard_wren_piper_4 · "The Headliner" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.piper && hearts:wren>=8 && hearts:piper>=8 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_3) && !flag:event.overheard_wren_piper_4`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "Piper. Next month, you headline. Top of my chalkboard. I bought new chalk."
  [piper]: "Headline? Me? Your stage? Wren! I'll cry! Then write a song about crying!"  ✏ `event.overheard_wren_piper_4.l2`
  · (piper wave (background))
  · (piper emote note)
  · (wren wave (background))
  [wren]: "Do both. Cry first, song after. I'll sell handkerchiefs at the door."
  [piper]: "Deal! ...Actually, make it a duet. Your stage, your song too."  ✏ `event.overheard_wren_piper_4.l4`
  · effects: flag:event.overheard_wren_piper_4

### overheard_piper_juno_1 · "Volume Control" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.piper && flag:met.juno && hearts:piper>=2 && hearts:juno>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_piper_juno_1`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), juno(26, 18)
  · (place piper to (24,18))
  · (place juno to (26,18))
  · (piper faces right)
  · (juno faces left)
  [piper]: "Juno! Hammer softer! My ballad's drowning!"  ✏ `event.overheard_piper_juno_1.l1`
  [juno]: "Quieter? Piper, it's a forge! You're supposed to feel it!"
  · (juno shrug (background))
  · (juno emote note)
  · (piper point (background))
  [piper]: "I felt it in my teeth! Through the whole stage!"  ✏ `event.overheard_piper_juno_1.l3`
  [juno]: "Then the anvil has a fan! Tell your teeth I said thanks!"
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
  [piper]: "Juno! I wrote a song to the rhythm of your hammer! Don't stop!"  ✏ `event.overheard_piper_juno_2.l1`
  [juno]: "You did? Play it! No, wait, I'll keep time! Okay, go!"
  · (juno shrug (background))
  · (juno emote note)
  · (piper wave (background))
  [piper]: "One, two, clang! That's a chorus!"  ✏ `event.overheard_piper_juno_2.l3`
  [juno]: "That's a hit! I'm labeling it 'Song (Forge)' and hammering it all week!"
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
  [piper]: "Juno, did you build me a tiny stage? Out of a crate?"  ✏ `event.overheard_piper_juno_3.l1`
  [juno]: "It says 'Stage (Piper)'! Official! Play on it! I sanded the splinters!"
  · (juno point (background))
  · (juno emote note)
  · (piper shrug (background))
  [piper]: "Then it gets my best set! Front row's yours, Juno. Sit!"  ✏ `event.overheard_piper_juno_3.l3`
  [juno]: "Okay! No, I'll hammer! No, I'll watch! Both! Always both!"
  · effects: flag:event.overheard_piper_juno_3

### overheard_hazel_piper_1 · "Pencil Rhymes" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.hazel && flag:met.piper && hearts:hazel>=2 && hearts:piper>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_hazel_piper_1`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), hazel(26, 18)
  · (place piper to (24,18))
  · (place hazel to (26,18))
  · (piper faces right)
  · (hazel faces left)
  [piper]: "Hazel! Someone's fixing the rhymes in my songbook. In very neat pencil."  ✏ `event.overheard_hazel_piper_1.l1`
  [hazel]: "(Shh.) An outdoor voice is still a voice. ...Also, it wasn't me."
  · (hazel shrug (background))
  · (hazel emote exclaim)
  · (piper point (background))
  [piper]: "It was you! 'Moon' doesn't rhyme with 'spoon' now. It rhymes with 'soon'. It's better!"  ✏ `event.overheard_hazel_piper_1.l3`
  [hazel]: "(It always wanted 'soon'.) You're welcome. ...I mean, whoever it was."
  · effects: flag:event.overheard_hazel_piper_1

### overheard_hazel_piper_2 · "Eight Syllables" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.hazel && flag:met.piper && hearts:hazel>=4 && hearts:piper>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_hazel_piper_1) && !flag:event.overheard_hazel_piper_2`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), hazel(26, 18)
  · (place piper to (24,18))
  · (place hazel to (26,18))
  · (piper faces right)
  · (hazel faces left)
  [piper]: "Help. My new chorus has nine syllables, and the tune only has eight."  ✏ `event.overheard_hazel_piper_2.l1`
  [hazel]: "(Cut 'really'.) Nobody has ever needed 'really'."
  · (hazel point (background))
  · (hazel emote question)
  · (piper shrug (background))
  [piper]: "...Oh! That's it! Where did you learn songs, Hazel?"  ✏ `event.overheard_hazel_piper_2.l3`
  [hazel]: "(Books.) And you. Every Thursday, back corner, behind a very large book."
  · effects: flag:event.overheard_hazel_piper_2

### overheard_hazel_piper_3 · "The Hidden Verse" · surprise
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.hazel && flag:met.piper && hearts:hazel>=6 && hearts:piper>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_hazel_piper_2 && flag:event.piper_heart8) && !flag:event.overheard_hazel_piper_3`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), hazel(26, 18)
  · (place piper to (24,18))
  · (place hazel to (26,18))
  · (piper faces right)
  · (hazel faces left)
  [piper]: "Hazel. Write me one verse. Just one. Your words, my tune."  ✏ `event.overheard_hazel_piper_3.l1`
  [hazel]: "(I can't.) ...I already did. It's in your fiddle case, under the rosin."
  · (hazel shrug (background))
  · (hazel emote sparkle)
  · (piper wave (background))
  [piper]: "That's where it came from? It's the best verse I own! It should have your name on it."  ✏ `event.overheard_hazel_piper_3.l3`
  [hazel]: "(Not my name.) Just play it. Loudly. (It was always meant to be loud.)"
  · effects: flag:event.overheard_hazel_piper_3
