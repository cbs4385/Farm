# Scene cards for elara

15 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 34 spoken lines in scenes, 26 dialogue lines and 17 player choice labels.

### elara_heart2 · "Hold Out Your Hand" · funny
event · npc_roster.json · map Clinic · priority 6
plays when: `hearts:elara>=2 && !weekday:sat && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(3, 2)
  · (elara faces down)
  [elara]: "Hold out your hand, [player]."  ✏ `event.elara_heart2.0`
  · (move player to (3,2))
  · (player faces up)
  · (elara emote sparkle)
  [elara]: "A little bandage for that blister. Don't argue."  ✏ `event.elara_heart2.1`
  [elara]: "There. Good as new."  ✏ `event.elara_heart2.2`
  · effects: give:food.salad,2, friend:elara,40, flag:event.elara_heart2

### elara_heart5 · "Something Knitted" · wholesome
event · npc_roster.json · map Clinic · priority 7
plays when: `hearts:elara>=5 && !weekday:sat && hour>=9 && hour<17 && flag:event.elara_heart2`
beats may be ADDED or CUT in this scene
placed: player(3, 2)
  · (elara faces down)
  [elara]: "I knitted something."  ✏ `event.elara_heart5.0`
  · (move player to (3,2))
  · (player faces up)
  · (elara emote sparkle)
  [elara]: "It's a bit crooked. Wear it anyway."  ✏ `event.elara_heart5.1`
  [elara]: "Thank you for being kind."  ✏ `event.elara_heart5.2`
  · effects: give:crop.cranberry,2, friend:elara,60, flag:event.elara_heart5

### past_the_point · "Past the Point" · funny
event · spec_scenes.json · map Beach · priority 7
plays when: `flag:event.elara_heart8 && flag:event.felix_heart8 && hearts:felix>=6 && hearts:elara>=6 && hour>=5 && hour<10 && !weather:storm && !flag:event.past_the_point && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: felix(26, 5), elara(25, 6), wren(32, 7), player(27, 6)
  · (letterbox on)
  · (lighting dawn)
  · (place felix to (26,5))
  · (felix faces up)
  · (place elara to (25,6))
  · (elara faces up)
  · (place wren to (32,7))
  · (wren faces up)
  · (move player to (27,6))
  · (player faces up)
  [felix]: "Mornin'. Boat's ready. She leaks politely. The bailing's the elegant part."
  [elara]: "I knitted a life jacket! It isn't how they work. I brought it anyway, and a sandwich tin."  ✏ `event.past_the_point.1`  (emote sparkle, expr happy)
  ▶ DIALOGUE `past_point.talk` (player-facing graph, plays here):
    n0 [elara]: "Forty-two places on the list, and Felix says we can manage one before breakfast. Which one?"  ✏ `dlg.past_point.talk.0`
       ? "Round the headland." → r0  effects: flag:choice.point.headland, friend:elara,5  ✏ `dlg.past_point.talk.c0`
       ? "The white rocks." → r1  effects: flag:choice.point.rocks, friend:elara,5  ✏ `dlg.past_point.talk.c1`
       ? "Straight out, until the shore is a line." → r2  effects: flag:choice.point.horizon, friend:elara,5  ✏ `dlg.past_point.talk.c2`
    r0 [elara]: "The headland! I've drawn it eleven times. Never from a boat."  ✏ `dlg.past_point.talk.r0`
    r1 [elara]: "The white rocks. I've never been closer than the bench. Row slowly!"  ✏ `dlg.past_point.talk.r1`
    r2 [elara]: "Straight out. I'll hold the sandwich tin. It's very steadying."  ✏ `dlg.past_point.talk.r2`
  · (fadeout)
  · (sfx click)
  · (camera pan)
  · (lighting warm)
  · (fadein)
  [narration]: "The boat leaks. Felix bails with one hand and rows with the other, and never spills his coffee."  ✏ `event.past_the_point.2`
  [elara]: "The headland is bigger than the map made it. Felix, it's enormous. It's the best thing I've ever been wrong about."  ✏ `event.past_the_point.headland`  (expr happy) [only if flag:choice.point.headland]
  [elara]: "The white rocks look like teeth from here. Friendly teeth. I'd like to wave at them. May I wave?"  ✏ `event.past_the_point.rocks`  (expr happy) [only if flag:choice.point.rocks]
  [elara]: "Out here the shore is only a line. I didn't know I could feel this small and this glad together."  ✏ `event.past_the_point.horizon`  (expr happy) [only if flag:choice.point.horizon]
  [wren]: "Don't mind me, friends. I'm only looking. Only looking, I promise, from the point." [only if flag:choice.wren.bag.go && flag:met.wren && hearts:wren>=8]
  [narration]: "Elara takes out her list and crosses one place off with a stub of pencil, and smiles at the pencil."  ✏ `event.past_the_point.3`
  [felix]: "Told you, partner. Dawn's better with two. ...Better with her list in it, too."  (emote heart)
  · (fadeout)
  · (camera reset)
  · (lighting reset)
  · (fadein)
  · (letterbox off)
  · effects: flag:event.past_the_point, friend:felix,40, friend:elara,40

### elara_garden_visit
event · zz_fs_elara_garden.json · map Farm · priority 50
plays when: `year==1 && season:spring && day==2 && hour>=8 && hour<18`
text-only scene: rewrite lines, do not add or cut beats
placed: player(80, 18), elara(80, 16)
  · (sfx Knock)
  · (wait 1.4s)
  · (fadeout)
  · (place player to (80,18))
  · (place elara to (80,16))
  · (player faces down)
  · (elara faces up)
  · (fadein)
  ▶ DIALOGUE `elara.garden.visit` (player-facing graph, plays here):
    n0 [elara]: "Good morning! You must be the new farmer. I'm Elara, from the clinic. Welcome to Wetherell."  ✏ `dlg.elara.garden.visit.0`   (→ n1)
    n1 [elara]: "Edmund used to bring me comfrey from this very farm. It was always the first thing I missed in spring. Oh, listen to me, rambling at your door."  ✏ `dlg.elara.garden.visit.1`   (→ n2)
    n2 [elara]: "May I ask a favor? The clinic garden is bare soil this year. I need three dandelions and three wild garlic. Ordinary things. Great in a poultice!"  ✏ `dlg.elara.garden.visit.2`
       ? "Of course. I'll bring them." → r0  effects: quest.start:elara_garden  ✏ `dlg.elara.garden.visit.c0`
       ? "Maybe later." → r1  ✏ `dlg.elara.garden.visit.c1`
    r0 [elara]: "Thank you! Truly. Bring them to the clinic, or to me wherever you find me. There's no hurry. Well, a little hurry: I'd like them in the ground by the end of the week."  ✏ `dlg.elara.garden.visit.r0`   (expr happy)
    r1 [elara]: "Of course, of course. I'm at the clinic most days, if you change your mind."  ✏ `dlg.elara.garden.visit.r1`
  · effects: met:elara, flag:elara.garden.visited

### elara_heart8 · "The List" · surprise
conversation · wave3_events.json · map Clinic · priority 7
plays when: `hearts:elara>=8 && flag:event.elara_heart5 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(3, 2)
  · (elara faces down)
  [elara]: "Come see! I've never shown this to anyone. It's a list. A long one. With drawings. Don't laugh. Laugh a little."  ✏ `event.elara_heart8.intro`
  · (move player to (3,2))
  · (player faces up)
  · (elara wave (background))
  · (elara emote sparkle)
  ▶ DIALOGUE `elara.list` (player-facing graph, plays here):
    n0 [narration]: "Elara unrolls a very long paper, covered in tiny writing and doodled boats."  ✏ `dlg.elara.list.0`   (→ n1)
    n1 [elara]: "Places I've never been. Forty-one. I've crossed none off. What do you think?"  ✏ `dlg.elara.list.1`
       ? "Pick one together." → r0  effects: friend:elara,15, flag:choice.list.pick  ✏ `dlg.elara.list.c0`
       ? "Add a place." → r1  effects: friend:elara,10, flag:choice.list.add  ✏ `dlg.elara.list.c1`
       ? "Promise to visit." → r2  effects: friend:elara,10, flag:choice.list.promise  ✏ `dlg.elara.list.c2`
    r0 [elara]: "Together! The sea, from a boat. I'm circling it. I'm packing a sandwich. I'm already packing!"  ✏ `dlg.elara.list.r0`   (expr happy)
    r1 [elara]: "A new one! Forty-two! Oh, you've made my whole week. Don't tell Dr. Penn."  ✏ `dlg.elara.list.r1`   (expr happy)
    r2 [elara]: "You promise? ...I'll write it down. In ink. And knit you a sock for the journey."  ✏ `dlg.elara.list.r2`   (expr happy)
  [elara]: "{if choice:list.pick}We picked one together. The sea, from a boat. I've circled it twice, and then once more in pen.{else}{if choice:list.add}You added a place. I've written it in the margin: 'Somewhere [player] suggests.' It's already my favorite.{else}You promised to visit. I'm holding you to it. Gently. With a very long scarf.{/if}{/if}"  ✏ `event.elara_heart8.after`
  · effects: flag:event.elara_heart8
  · effects: friend:elara,70

### elara_birthday · "Elara's Birthday" · funny
conversation · wave4_birthdays.json · map Clinic · priority 8 · repeatable
plays when: `hearts:elara>=2 && birthday.in:elara==0 && unseen:birthday.elara && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(3, 2)
  · (elara faces down)
  [elara]: "It's my birthday! I knitted myself a hat to celebrate! It has a very small hat on it! Come in, come in!"  ✏ `event.elara_birthday.intro`
  · (move player to (3,2))
  · (player faces up)
  · (elara wave (background))
  · (elara emote sparkle)
  ▶ DIALOGUE `elara.birthday` (player-facing graph, plays here):
    n0 [elara]: "You came! What do we do first?"  ✏ `dlg.elara.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:elara,10, flag:choice.bday.elara.wish  ✏ `dlg.elara.birthday.c0`
       ? "Admire the hat." → r1  effects: friend:elara,10, flag:choice.bday.elara.hat  ✏ `dlg.elara.birthday.c1`
       ? "Offer her a day at the beach." → r2  effects: friend:elara,10, flag:choice.bday.elara.beach  ✏ `dlg.elara.birthday.c2`
    r0 [elara]: "Thank you! I'll knit it into a scarf!"  ✏ `dlg.elara.birthday.r0`   (expr happy)
    r1 [elara]: "It's a hat with a hat! You understand! I'm so happy I might cry!"  ✏ `dlg.elara.birthday.r1`   (expr happy)
    r2 [elara]: "The beach! On my birthday! Yes! I'll pack a sandwich and a very long scarf!"  ✏ `dlg.elara.birthday.r2`   (expr happy)
  [elara]: "{if choice:bday.elara.wish}It's in the scarf, row nine hundred, in red.{else}{if choice:bday.elara.hat}The hat on the hat stays. Dr. Penn pretended not to see it. She smiled.{else}The beach was perfect. I'm still sandy. I love it.{/if}{/if}"  ✏ `event.elara_birthday.after`
  · effects: flag:event.elara_birthday
  · effects: mark:birthday.elara, friend:elara,30, give:product.wool,1, mail:elara_bday_thanks

### elara_courtship · "Elara Asks" · wholesome
conversation · wave5_courtship.json · map Clinic · priority 8
plays when: `hearts:elara>=8 && flag:event.elara_heart8 && !flag:partners.elara && !flag:choice.courtship.elara.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.ione.yes && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(3, 2)
  · (elara faces down)
  [elara]: "I knitted a very long scarf and the last row is a question! Would you court me? It's red, so it's very serious!"  ✏ `event.elara_courtship.intro`
  · (move player to (3,2))
  · (player faces up)
  · (elara wave (background))
  · (elara emote sparkle)
  ▶ DIALOGUE `elara.courtship` (player-facing graph, plays here):
    n0 [elara]: "What do you say?"  ✏ `dlg.elara.courtship.0`
       ? "Yes! Show me the whole scarf." → r0  effects: friend:elara,20, flag:choice.courtship.elara.yes  ✏ `dlg.elara.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:elara,15, flag:choice.courtship.elara.friends  ✏ `dlg.elara.courtship.c1`
       ? "Can I think about it?" → r2  effects: friend:elara,5, flag:choice.courtship.elara.later  ✏ `dlg.elara.courtship.c2`
    r0 [elara]: "YES! Oh, I'm so happy my yarn is shaking! The scarf is yours! The whole nine yards! Literally! I measured!"  ✏ `dlg.elara.courtship.r0`   (expr happy)
    r1 [elara]: "Friends! The best! Truly! I'll knit a friend-sized scarf! It's like a regular one but with more feelings! Thank you for saying it kindly!"  ✏ `dlg.elara.courtship.r1`   (expr happy)
    r2 [elara]: "Think! Take a long time! I'll add rows! The scarf will grow! It's fine!"  ✏ `dlg.elara.courtship.r2`   (expr thinking)
  [elara]: "{if flag:choice.courtship.elara.yes}Courting! The scarf is yours! We'll wear it together! It's long enough! I measured!{else}{if flag:choice.courtship.elara.friends}Friends, forever. I'll knit it into the next row. In blue, so it's calm.{else}Take your time. The scarf will keep. I'll add a row a day. It's a ritual.{/if}{/if}"  ✏ `event.elara_courtship.after`
  · effects: flag:event.elara_courtship
  · effects: flag:courtship.asked.elara

### elara_courtship2 · "Elara Asks Again" · wholesome
conversation · wave5_courtship.json · map Clinic · priority 8
plays when: `hearts:elara>=10 && flag:choice.courtship.elara.later && !flag:event.elara_courtship2.done && !flag:partners.elara && !flag:choice.courtship.elara.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.ione.yes && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(3, 2)
  · (elara faces down)
  [elara]: "I knitted a very long scarf and the last row is a question! Would you court me? It's red, so it's very serious!"  ✏ `event.elara_courtship2.intro`
  · (move player to (3,2))
  · (player faces up)
  · (elara wave (background))
  · (elara emote sparkle)
  ▶ DIALOGUE `elara.courtship` (player-facing graph, plays here):
    n0 [elara]: "What do you say?"  ✏ `dlg.elara.courtship.0`
       ? "Yes! Show me the whole scarf." → r0  effects: friend:elara,20, flag:choice.courtship.elara.yes  ✏ `dlg.elara.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:elara,15, flag:choice.courtship.elara.friends  ✏ `dlg.elara.courtship.c1`
       ? "Can I think about it?" → r2  effects: friend:elara,5, flag:choice.courtship.elara.later  ✏ `dlg.elara.courtship.c2`
    r0 [elara]: "YES! Oh, I'm so happy my yarn is shaking! The scarf is yours! The whole nine yards! Literally! I measured!"  ✏ `dlg.elara.courtship.r0`   (expr happy)
    r1 [elara]: "Friends! The best! Truly! I'll knit a friend-sized scarf! It's like a regular one but with more feelings! Thank you for saying it kindly!"  ✏ `dlg.elara.courtship.r1`   (expr happy)
    r2 [elara]: "Think! Take a long time! I'll add rows! The scarf will grow! It's fine!"  ✏ `dlg.elara.courtship.r2`   (expr thinking)
  [elara]: "{if flag:choice.courtship.elara.yes}Courting! The scarf is yours! We'll wear it together! It's long enough! I measured!{else}{if flag:choice.courtship.elara.friends}Friends, forever. I'll knit it into the next row. In blue, so it's calm.{else}Take your time. The scarf will keep. I'll add a row a day. It's a ritual.{/if}{/if}"  ✏ `event.elara_courtship2.after`
  · effects: flag:event.elara_courtship2
  · effects: flag:courtship.asked.elara, flag:event.elara_courtship2.done

### elara_partners · "Elara, Together" · wholesome
conversation · wave5_courtship.json · map Clinic · priority 8
plays when: `hearts:elara>=10 && flag:choice.courtship.elara.yes && !flag:partners.elara && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(3, 2)
  · (elara faces down)
  [elara]: "Hello! I made you something! It's a hat! It has a smaller hat on it! The smaller hat has a tiniest hat! It's hats all the way down!"  ✏ `event.elara_partners.intro`
  · (move player to (3,2))
  · (player faces up)
  · (elara wave (background))
  · (elara emote heart)
  ▶ DIALOGUE `elara.partners` (player-facing graph, plays here):
    n0 [elara]: "Clinic's closed, needles down! How shall we spend the evening?"  ✏ `dlg.elara.partners.0`
       ? "Wear the hat." → r0  effects: friend:elara,10, flag:choice.courtship.elara.hat  ✏ `dlg.elara.partners.c0`
       ? "Knit a row together." → r1  effects: friend:elara,10, flag:choice.courtship.elara.knit  ✏ `dlg.elara.partners.c1`
       ? "Walk the beach at sunset." → r2  effects: friend:elara,10, flag:choice.courtship.elara.beach  ✏ `dlg.elara.partners.c2`
    r0 [elara]: "You look so good! The tiniest hat is slightly crooked! It's perfect! I'm crying a little!"  ✏ `dlg.elara.partners.r0`   (expr happy)
    r1 [elara]: "Together! Hold the needle! No, the other end! There! We made a row! It's the best row!"  ✏ `dlg.elara.partners.r1`   (expr happy)
    r2 [elara]: "The beach at sunset! I'll bring a blanket and three snacks! The snacks are for you. The blanket is for us!"  ✏ `dlg.elara.partners.r2`   (expr happy)
  [elara]: "{if choice:courtship.elara.hat}The hat stays on my shelf. The smaller hat stays on it. We're all very close.{else}{if choice:courtship.elara.knit}Our row is the middle of the scarf. It's slightly crooked. It's the best part.{else}The sunset was pink. So was I. I wore the red scarf. It matched.{/if}{/if}"  ✏ `event.elara_partners.after`
  · effects: flag:event.elara_partners
  · effects: flag:partners.elara, give:prop.hat, friend:elara,50

### overheard_elara_odalys_1 · "The Waiting Room" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.elara && flag:met.odalys && hearts:elara>=2 && hearts:odalys>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: elara(24, 18), odalys(26, 18)
  · (place elara to (24,18))
  · (place odalys to (26,18))
  · (elara faces right)
  · (odalys faces left)
  [elara]: "Dr. Penn! Do not look at the waiting room stool! It is definitely not wearing a cozy!"  ✏ `event.overheard_elara_odalys_1.l1`
  [odalys]: "Elara. A stool does not have pride. ...It looks well on it. Leave it."
  · (odalys shrug (background))
  · (odalys emote sparkle)
  · (elara point (background))
  [elara]: "I knew it! A cozy for the stool, and a smaller cozy for the cozy!"  ✏ `event.overheard_elara_odalys_1.l3`
  [odalys]: "No. Stop there. That is a medical instruction."
  · effects: flag:event.overheard_elara_odalys_1

### overheard_elara_odalys_2 · "Soup Rounds" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.elara && flag:met.odalys && hearts:elara>=4 && hearts:odalys>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_elara_odalys_1`
text-only scene: rewrite lines, do not add or cut beats
placed: elara(24, 18), odalys(26, 18)
  · (place elara to (24,18))
  · (place odalys to (26,18))
  · (elara faces right)
  · (odalys faces left)
  [elara]: "I made soup for the whole clinic! It has seven kinds of vegetable! One of them is a surprise!"  ✏ `event.overheard_elara_odalys_2.l1`
  [odalys]: "Elara. I accept the soup. I do not accept the surprise. Which one is it?"
  · (odalys shrug (background))
  · (odalys emote sparkle)
  · (elara wave (background))
  [elara]: "It's a turnip! It's wearing a carrot costume! It's very brave!"  ✏ `event.overheard_elara_odalys_2.l3`
  [odalys]: "...It's the best soup I've had. Save me a bowl. Quietly."
  · effects: flag:event.overheard_elara_odalys_2

### overheard_elara_odalys_3 · "Late Shift" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.elara && flag:met.odalys && hearts:elara>=6 && hearts:odalys>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_elara_odalys_2`
text-only scene: rewrite lines, do not add or cut beats
placed: elara(24, 18), odalys(26, 18)
  · (place elara to (24,18))
  · (place odalys to (26,18))
  · (elara faces right)
  · (odalys faces left)
  [elara]: "Dr. Penn, you worked eleven hours yesterday. I knitted you a sleeping mask! It's lavender!"  ✏ `event.overheard_elara_odalys_3.l1`
  [odalys]: "Elara. ...It's soft. I'll try it for ten minutes after lunch. That is generous."
  · (odalys point (background))
  · (odalys emote sparkle)
  · (elara shrug (background))
  [elara]: "Ten minutes is a perfect nap! I'll cover the desk! I'll answer nothing, kindly!"  ✏ `event.overheard_elara_odalys_3.l3`
  [odalys]: "Thank you. Keep the clinic quiet. And, Elara. This was good care."
  · effects: flag:event.overheard_elara_odalys_3

### overheard_felix_elara_1 · "The Scarf Net" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.felix && flag:met.elara && hearts:felix>=2 && hearts:elara>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), elara(26, 18)
  · (place felix to (24,18))
  · (place elara to (26,18))
  · (felix faces right)
  · (elara faces left)
  [felix]: "Elara. A fish doesn't want comfort. It wants to be free."
  [elara]: "Then it's a free, comfortable net! Everybody wins!"  ✏ `event.overheard_felix_elara_1.l2`
  · (elara shrug (background))
  · (elara emote sweat)
  · (felix point (background))
  [felix]: "...It does look warm. Hand it over. For science."
  [elara]: "I knew you'd love it! I'll knit a bigger one!"  ✏ `event.overheard_felix_elara_1.l4`
  · effects: flag:event.overheard_felix_elara_1

### overheard_felix_elara_2 · "Big Fish, Big Scarf" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.felix && flag:met.elara && hearts:felix>=4 && hearts:elara>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_felix_elara_1`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), elara(26, 18)
  · (place felix to (24,18))
  · (place elara to (26,18))
  · (felix faces right)
  · (elara faces left)
  [felix]: "It was this big. Bigger. A true giant."
  [elara]: "I made a scarf this big! Same size! They'd be friends!"  ✏ `event.overheard_felix_elara_2.l2`
  · (elara shrug (background))
  · (elara emote sweat)
  · (felix wave (background))
  [felix]: "A scarf-fish friendship. I like it. Tell no one."
  [elara]: "Everyone knows! I told the clinic!"  ✏ `event.overheard_felix_elara_2.l4`
  · effects: flag:event.overheard_felix_elara_2

### overheard_felix_elara_3 · "Sunrise Knitting" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.felix && flag:met.elara && hearts:felix>=6 && hearts:elara>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_felix_elara_2`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), elara(26, 18)
  · (place felix to (24,18))
  · (place elara to (26,18))
  · (felix faces right)
  · (elara faces left)
  [felix]: "Quiet, yes. Until you count stitches aloud."
  [elara]: "Seventeen, eighteen! It's a rhythm!"  ✏ `event.overheard_felix_elara_3.l2`
  · (elara point (background))
  · (elara emote sweat)
  · (felix shrug (background))
  [felix]: "...It's soothing. Don't stop. The fish like it."
  [elara]: "I won't! Nineteen! Twenty! Fish are lovely listeners!"  ✏ `event.overheard_felix_elara_3.l4`
  · effects: flag:event.overheard_felix_elara_3
