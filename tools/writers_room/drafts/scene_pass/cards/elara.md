# Scene cards for elara

15 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 37 spoken lines in scenes, 26 dialogue lines and 17 player choice labels.

### elara_heart2 · "Hold Out Your Hand" · funny
event · npc_roster.json · map Clinic · priority 6
plays when: `(hearts:elara>=2 && !weekday:sat && hour>=9 && hour<17) && !flag:event.elara_heart2`
beats may be ADDED or CUT in this scene
placed: player(3, 2)
  · (elara faces down)
  [elara]: "Hold out your hand, [player]. No, the other one. The one that hid when I reached for the bandages."  ✏ `event.elara_heart2.0`
  · (move player to (3,2))
  · (player faces up)
  · (elara emote sparkle)
  [elara]: "A hoe blister, the spring crop. Ten a week come through that door. Hold still. There. You were still deciding whether to argue!"  ✏ `event.elara_heart2.1`
  [elara]: "Good as new. Now, your salad. The bandage was the treatment. Lunch is the follow-up appointment!"  ✏ `event.elara_heart2.2`
  [elara]: "Dr. Penn says I over-bandage. I say she under-salads. We agreed to disagree, medically."  ✏ `event.elara_heart2.4`  (expr happy)
  [elara]: "Oh! Don't look behind the kettle. ...Fine. It's a map. Of everywhere I haven't been. Yet!"  ✏ `event.elara_heart2.3`  (expr embarrassed)
  · effects: give:food.salad,2, friend:elara,40, flag:event.elara_heart2

### elara_heart5 · "Something Knitted" · wholesome
event · npc_roster.json · map Clinic · priority 7
plays when: `(hearts:elara>=5 && !weekday:sat && hour>=9 && hour<17 && flag:event.elara_heart2) && !flag:event.elara_heart5`
beats may be ADDED or CUT in this scene
placed: player(3, 2)
  · (elara faces down)
  [elara]: "I knitted something! For you! On purpose, this time."  ✏ `event.elara_heart5.0`
  · (move player to (3,2))
  · (player faces up)
  · (elara emote sparkle)
  [elara]: "A mitten with two thumbs, so it fits either hand. Or one hand, twice. The crooked thumb is the lucky one."  ✏ `event.elara_heart5.1`
  [elara]: "You visit when nothing hurts. Nobody visits when nothing hurts. Here, cranberries. I tried knitting some once. These are better."  ✏ `event.elara_heart5.2`
  [elara]: "The map behind the kettle has a friend now. A list. Someday I'll show you. When the doodles are braver."  ✏ `event.elara_heart5.3`  (expr embarrassed)
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
  [felix]: "There you are. The boat I promised. She floats. I checked twice. The bailing tin is the elegant part."
  [elara]: "Felix explained floating to me very kindly, so I knitted a life jacket! It goes over the real one. For morale!"  ✏ `event.past_the_point.1`  (emote sparkle, expr happy)
  [felix]: "Wear both, then. The knitted one can look brave. The real one can do the floating."  (expr happy)
  ▶ DIALOGUE `past_point.talk` (player-facing graph, plays here):
    n0 [elara]: "A whole list, and Felix says one place before breakfast. You choose. My hands are full of sandwiches."  ✏ `dlg.past_point.talk.0`
       ? "Round the headland. You've drawn it nine times." → r0  effects: flag:choice.point.headland, friend:elara,5  ✏ `dlg.past_point.talk.c0`
       ? "The white rocks. Slowly." → r1  effects: flag:choice.point.rocks, friend:elara,5  ✏ `dlg.past_point.talk.c1`
       ? "Straight out, till the shore's a line." → r2  effects: flag:choice.point.horizon, friend:elara,5  ✏ `dlg.past_point.talk.c2`
    r0 [elara]: "Round the headland! Nine drawings from the bench, and today it finally has to look at me!"  ✏ `dlg.past_point.talk.r0`
    r1 [elara]: "Slowly, yes! I've only ever waved at the white rocks from the bench. Felix, row like we're early."  ✏ `dlg.past_point.talk.r1`
    r2 [elara]: "Straight out! I'll hold the sandwich tin. It's very steadying. Medically."  ✏ `dlg.past_point.talk.r2`
  · (fadeout)
  · (sfx click)
  · (camera pan)
  · (lighting warm)
  · (fadein)
  [narration]: "The boat leaks, politely. Felix pulls the oars. Elara holds her hat down with both hands. The headland slides closer."  ✏ `event.past_the_point.2`
  [elara]: "My map made it thumb-sized! Felix, it's enormous! Best thing I've ever been wrong about!"  ✏ `event.past_the_point.headland`  (expr happy) [only if flag:choice.point.headland]
  [elara]: "Look, the white rocks are teeth from here! Friendly teeth. Excellent gums. May I wave at them?"  ✏ `event.past_the_point.rocks`  (expr happy) [only if flag:choice.point.rocks]
  [elara]: "Shore's just a line now. I didn't know a person could feel this small and this glad at once."  ✏ `event.past_the_point.horizon`  (expr happy) [only if flag:choice.point.horizon]
  [wren]: "I open at noon. That leaves me mornings, and I've started spending them here. The sea starts somewhere else. I'm checking where." [only if flag:choice.wren.bag.go && flag:met.wren && hearts:wren>=8]
  [narration]: "Elara unrolls her list against her knees. One pencil stroke. The first line through anything, ever."  ✏ `event.past_the_point.3`
  [felix]: "Dawn's better with two. Three makes it an expedition. ...One place off the list. For the rest, I'll want a bigger boat."  (emote heart)
  · (fadeout)
  · (camera reset)
  · (lighting reset)
  · (fadein)
  · (letterbox off)
  · effects: flag:event.past_the_point, friend:felix,40, friend:elara,40

### elara_garden_visit
event · zz_fs_elara_garden.json · map Farm · priority 50
plays when: `(year==1 && season:spring && day==2 && hour>=8 && hour<18) && !flag:event.elara_garden_visit`
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
    n0 [elara]: "Good morning! You're the new farmer! I'm Elara, from the clinic. Am I the first to knock? Don't tell anyone I ran."  ✏ `dlg.elara.garden.visit.0`   (→ n1)
    n1 [elara]: "Edmund brought me comfrey from this farm every spring, tied with the same green twine. I'd save the twine. ...Oh, listen to me, rambling on your step."  ✏ `dlg.elara.garden.visit.1`   (→ n2)
    n2 [elara]: "Could I ask a favor? The clinic garden came up bare this spring. Three dandelions and three wild garlic would fix it. Weeds to you, a pharmacy to me!"  ✏ `dlg.elara.garden.visit.2`
       ? "Happy to. Three and three." → r0  effects: quest.start:elara_garden  ✏ `dlg.elara.garden.visit.c0`
       ? "Maybe later. I've barely unpacked." → r1  ✏ `dlg.elara.garden.visit.c1`
    r0 [elara]: "Wonderful! The clinic, or wherever you find me. No hurry. ...A small hurry. Seeds are patient, but I'm not!"  ✏ `dlg.elara.garden.visit.r0`   (expr happy)
    r1 [elara]: "Of course! You've a whole farm to meet first. I'm at the clinic most days. Keep your socks on — the soil will wait."  ✏ `dlg.elara.garden.visit.r1`
  · effects: met:elara, flag:elara.garden.visited

### elara_heart8 · "The List" · surprise
conversation · wave3_events.json · map Clinic · priority 7
plays when: `(hearts:elara>=8 && flag:event.elara_heart5 && !weekday:sat && hour>=9 && hour<17) && !flag:event.elara_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: player(3, 2)
  · (elara faces down)
  [elara]: "Shut the door a minute! Remember the shy list behind the kettle? The doodles are brave now. Don't laugh. ...Laugh a little."  ✏ `event.elara_heart8.intro`
  · (move player to (3,2))
  · (player faces up)
  · (elara wave (background))
  · (elara emote sparkle)
  ▶ DIALOGUE `elara.list` (player-facing graph, plays here):
    n0 [narration]: "Elara unrolls a paper longer than the desk. Tiny writing, little doodled boats, and one corner worn soft from folding."  ✏ `dlg.elara.list.0`   (→ n1)
    n1 [elara]: "Forty-one places I've never been. Crossed off: none. I know every one by heart. ...Say something before I roll it back up."  ✏ `dlg.elara.list.1`
       ? "Let's pick one. Together." → r0  effects: friend:elara,15, flag:choice.list.pick  ✏ `dlg.elara.list.c0`
       ? "Make it forty-two." → r1  effects: friend:elara,10, flag:choice.list.add  ✏ `dlg.elara.list.c1`
       ? "Promise to visit. I'll even pack." → r2  effects: friend:elara,10, flag:choice.list.promise  ✏ `dlg.elara.list.c2`
    r0 [elara]: "Together! Then it's the sea, from a boat. Circling it now. In pen! Pen means it's happening!"  ✏ `dlg.elara.list.r0`   (expr happy)
    r1 [elara]: "Forty-two! I'll draw yours a tiny boat. The best doodle I own. ...It'll still be bad. It gets the spot of honor."  ✏ `dlg.elara.list.r1`   (expr happy)
    r2 [elara]: "You promise? ...I'm writing that down. In ink. And I'll knit you a sock for the journey. Just the one, for luck."  ✏ `dlg.elara.list.r2`   (expr happy)
  [elara]: "{if choice:list.pick}We picked one. The sea, from a boat. Circled twice, then once more in pen. ...Felix owns a boat. I'm just saying it near you.{else}{if choice:list.add}It lives in the margin now: 'Somewhere [player] suggests.' No doodle yet. I'm saving my best boat.{else}You promised. I'm holding you to it. Gently, like a stitch you don't pull too tight.{/if}{/if}"  ✏ `event.elara_heart8.after`
  · effects: flag:event.elara_heart8
  · effects: friend:elara,70

### elara_birthday · "Elara's Birthday" · funny
conversation · wave4_birthdays.json · map Clinic · priority 8 · repeatable
plays when: `hearts:elara>=2 && birthday.in:elara==0 && unseen:birthday.elara && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(3, 2)
  · (elara faces down)
  [elara]: "It's my birthday! The stitches I dropped all spring spelled today. I knitted a hat, and the hat earned a smaller hat! Come in, come in!"  ✏ `event.elara_birthday.intro`
  · (move player to (3,2))
  · (player faces up)
  · (elara wave (background))
  · (elara emote sparkle)
  ▶ DIALOGUE `elara.birthday` (player-facing graph, plays here):
    n0 [elara]: "You came! What do we do first?"  ✏ `dlg.elara.birthday.0`
       ? "Happy birthday, Elara!" → r0  effects: friend:elara,10, flag:choice.bday.elara.wish  ✏ `dlg.elara.birthday.c0`
       ? "That hat is magnificent. Both of them." → r1  effects: friend:elara,10, flag:choice.bday.elara.hat  ✏ `dlg.elara.birthday.c1`
       ? "Beach day. Bring the hat's hat." → r2  effects: friend:elara,10, flag:choice.bday.elara.beach  ✏ `dlg.elara.birthday.c2`
    r0 [elara]: "Thank you! That goes straight into the scarf. Every word, in red. Red rows are for keeps!"  ✏ `dlg.elara.birthday.r0`   (expr happy)
    r1 [elara]: "You noticed the small hat! Nobody notices the small hat! I'm so happy I might need a lie-down!"  ✏ `dlg.elara.birthday.r1`   (expr happy)
    r2 [elara]: "Beach, yes! Dr. Penn can mind the plant, and the plant can mind Dr. Penn. Hat's hat, towel, done!"  ✏ `dlg.elara.birthday.r2`   (expr happy)
  [elara]: "{if choice:bday.elara.wish}It's in the scarf now. Row nine hundred, in red, right under 'Felix waved'.{else}{if choice:bday.elara.hat}The hat's hat stays on. Dr. Penn pretended not to see it. She smiled at the wall.{else}The beach was perfect. I'm still sandy. I found a shell shaped like a place I've never been.{/if}{/if}"  ✏ `event.elara_birthday.after`
  · effects: flag:event.elara_birthday
  · effects: mark:birthday.elara, friend:elara,30, give:product.wool,1, mail:elara_bday_thanks

### elara_courtship · "Elara Asks" · wholesome
conversation · wave5_courtship.json · map Clinic · priority 8
plays when: `(hearts:elara>=8 && flag:event.elara_heart8 && !flag:partners.elara && !flag:choice.courtship.elara.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.ione.yes && !weekday:sat && hour>=9 && hour<17) && !flag:event.elara_courtship`
text-only scene: rewrite lines, do not add or cut beats
placed: player(3, 2)
  · (elara faces down)
  [elara]: "Made you a scarf. Mind the end, it trails. The last row took all my nerve. It's a question: would you court me? In red. Red means very serious!"  ✏ `event.elara_courtship.intro`
  · (move player to (3,2))
  · (player faces up)
  · (elara wave (background))
  · (elara emote sparkle)
  ▶ DIALOGUE `elara.courtship` (player-facing graph, plays here):
    n0 [elara]: "Read the last row. Out loud, if you like. I'll just... count stitches. ...What do you say?"  ✏ `dlg.elara.courtship.0`
       ? "Yes! Show me the whole scarf." → r0  effects: friend:elara,20, flag:choice.courtship.elara.yes  ✏ `dlg.elara.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:elara,15, flag:choice.courtship.elara.friends  ✏ `dlg.elara.courtship.c1`
       ? "Let me think. Knit slowly." → r2  effects: friend:elara,5, flag:choice.courtship.elara.later  ✏ `dlg.elara.courtship.c2`
    r0 [elara]: "Yes! Oh! My needles are shaking — look, actual shaking! The scarf is yours. All nine feet of it!"  ✏ `dlg.elara.courtship.r0`   (expr happy)
    r1 [elara]: "Oh. ...Friends! The best kind! I'll knit you a friend-sized scarf. Same length, more pockets. Thank you for saying it kindly."  ✏ `dlg.elara.courtship.r1`   (expr happy)
    r2 [elara]: "Think away! I'll add a row a day meanwhile. The scarf gets longer either way. That's just how scarves are!"  ✏ `dlg.elara.courtship.r2`   (expr thinking)
  [elara]: "{if flag:choice.courtship.elara.yes}Courting! We'll wear the scarf together. It's long enough for two. I checked. Twice.{else}{if flag:choice.courtship.elara.friends}Friends, then. I'll knit it into the next row. In blue, so it's calm. Blue suits us.{else}Take your time. The scarf will keep. A row a day, behind the kettle, next to the list.{/if}{/if}"  ✏ `event.elara_courtship.after`
  · effects: flag:event.elara_courtship
  · effects: flag:courtship.asked.elara

### elara_courtship2 · "Elara Asks Again" · wholesome
conversation · wave5_courtship.json · map Clinic · priority 8
plays when: `hearts:elara>=10 && flag:choice.courtship.elara.later && !flag:event.elara_courtship2.done && !flag:partners.elara && !flag:choice.courtship.elara.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.ione.yes && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(3, 2)
  · (elara faces down)
  [elara]: "The scarf grew. A row a day, like I said. It reaches the door now. The last row still asks: would you court me?"  ✏ `event.elara_courtship2.intro`
  · (move player to (3,2))
  · (player faces up)
  · (elara wave (background))
  · (elara emote sparkle)
  ▶ DIALOGUE `elara.courtship` (player-facing graph, plays here):
    n0 [elara]: "Read the last row. Out loud, if you like. I'll just... count stitches. ...What do you say?"  ✏ `dlg.elara.courtship.0`
       ? "Yes! Show me the whole scarf." → r0  effects: friend:elara,20, flag:choice.courtship.elara.yes  ✏ `dlg.elara.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:elara,15, flag:choice.courtship.elara.friends  ✏ `dlg.elara.courtship.c1`
       ? "Let me think. Knit slowly." → r2  effects: friend:elara,5, flag:choice.courtship.elara.later  ✏ `dlg.elara.courtship.c2`
    r0 [elara]: "Yes! Oh! My needles are shaking — look, actual shaking! The scarf is yours. All nine feet of it!"  ✏ `dlg.elara.courtship.r0`   (expr happy)
    r1 [elara]: "Oh. ...Friends! The best kind! I'll knit you a friend-sized scarf. Same length, more pockets. Thank you for saying it kindly."  ✏ `dlg.elara.courtship.r1`   (expr happy)
    r2 [elara]: "Think away! I'll add a row a day meanwhile. The scarf gets longer either way. That's just how scarves are!"  ✏ `dlg.elara.courtship.r2`   (expr thinking)
  [elara]: "{if flag:choice.courtship.elara.yes}Courting! We'll wear the scarf together. It's long enough for two now. All that waiting was knitting.{else}{if flag:choice.courtship.elara.friends}Friends, then. I'll knit it into the next row. In blue, so it's calm. Blue suits us.{else}Still thinking? That's all right. I'll keep the scarf. It's warm, and it's patient. So am I.{/if}{/if}"  ✏ `event.elara_courtship2.after`
  · effects: flag:event.elara_courtship2
  · effects: flag:courtship.asked.elara, flag:event.elara_courtship2.done

### elara_partners · "Elara, Together" · wholesome
conversation · wave5_courtship.json · map Clinic · priority 8
plays when: `(hearts:elara>=10 && flag:choice.courtship.elara.yes && !flag:partners.elara && !weekday:sat && hour>=9 && hour<17) && !flag:event.elara_partners`
text-only scene: rewrite lines, do not add or cut beats
placed: player(3, 2)
  · (elara faces down)
  [elara]: "Hat delivery! It looked lonely, so it got a smaller hat. So did that one. Somebody had to stop me. Nobody did. There are three!"  ✏ `event.elara_partners.intro`
  · (move player to (3,2))
  · (player faces up)
  · (elara wave (background))
  · (elara emote heart)
  ▶ DIALOGUE `elara.partners` (player-facing graph, plays here):
    n0 [elara]: "Dr. Penn gave me the afternoon off. She said 'go,' then 'now.' Two whole words! How do we spend them?"  ✏ `dlg.elara.partners.0`
       ? "Put the hat on. All three." → r0  effects: friend:elara,10, flag:choice.courtship.elara.hat  ✏ `dlg.elara.partners.c0`
       ? "Teach me to knit a row." → r1  effects: friend:elara,10, flag:choice.courtship.elara.knit  ✏ `dlg.elara.partners.c1`
       ? "Beach walk, till the sun goes down." → r2  effects: friend:elara,10, flag:choice.courtship.elara.beach  ✏ `dlg.elara.partners.c2`
    r0 [elara]: "You look wonderful! The tiniest hat sits crooked. Crooked is my best stitch. I'm a little teary. The happy kind!"  ✏ `dlg.elara.partners.r0`   (expr happy)
    r1 [elara]: "Hold this needle! No, the other end! There! We made a row! It's the best row in Wetherell!"  ✏ `dlg.elara.partners.r1`   (expr happy)
    r2 [elara]: "The beach at sunset! I'll bring a blanket and three snacks. The snacks are yours. The blanket is ours."  ✏ `dlg.elara.partners.r2`   (expr happy)
  [elara]: "{if choice:courtship.elara.hat}The hat lives on my shelf now, with its smaller hats. We're all very close.{else}{if choice:courtship.elara.knit}Our row is the middle of the scarf. It's crooked. It's my favorite part.{else}The sunset was pink. So was I. We counted boats. Someday one of them is ours.{/if}{/if}"  ✏ `event.elara_partners.after`
  · effects: flag:event.elara_partners
  · effects: flag:partners.elara, give:prop.hat, friend:elara,50

### overheard_elara_odalys_1 · "The Waiting Room" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.elara && flag:met.odalys && hearts:elara>=2 && hearts:odalys>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_elara_odalys_1`
text-only scene: rewrite lines, do not add or cut beats
placed: elara(24, 18), odalys(26, 18)
  · (place elara to (24,18))
  · (place odalys to (26,18))
  · (elara faces right)
  · (odalys faces left)
  [elara]: "Dr. Penn, the stool is normal today! Normal stool! No reason to look at it at all!"  ✏ `event.overheard_elara_odalys_1.l1`
  [odalys]: "Elara. The stool is wearing a cozy. I noticed on Tuesday. It stays; wool aids recovery."
  · (odalys shrug (background))
  · (odalys emote sparkle)
  · (elara point (background))
  [elara]: "You like it! There's a spare for laundry day. Stools get cold feet. All four!"  ✏ `event.overheard_elara_odalys_1.l3`
  [odalys]: "No. One cozy per stool. That is clinic policy, as of now."
  · effects: flag:event.overheard_elara_odalys_1

### overheard_elara_odalys_2 · "Soup Rounds" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.elara && flag:met.odalys && hearts:elara>=4 && hearts:odalys>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_elara_odalys_1) && !flag:event.overheard_elara_odalys_2`
text-only scene: rewrite lines, do not add or cut beats
placed: elara(24, 18), odalys(26, 18)
  · (place elara to (24,18))
  · (place odalys to (26,18))
  · (elara faces right)
  · (odalys faces left)
  [elara]: "Soup rounds! Seven vegetables, and one of them is a surprise!"  ✏ `event.overheard_elara_odalys_2.l1`
  [odalys]: "Seven vegetables. I'll need the surprise identified before anyone swallows it. Professionally."
  · (odalys shrug (background))
  · (odalys emote sparkle)
  · (elara wave (background))
  [elara]: "The turnip! It's dressed as a carrot! Very brave of it!"  ✏ `event.overheard_elara_odalys_2.l3`
  [odalys]: "...It's the best soup I've had all year. Save me a bowl. Don't tell the turnip."
  · effects: flag:event.overheard_elara_odalys_2

### overheard_elara_odalys_3 · "Late Shift" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.elara && flag:met.odalys && hearts:elara>=6 && hearts:odalys>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_elara_odalys_2) && !flag:event.overheard_elara_odalys_3`
text-only scene: rewrite lines, do not add or cut beats
placed: elara(24, 18), odalys(26, 18)
  · (place elara to (24,18))
  · (place odalys to (26,18))
  · (elara faces right)
  · (odalys faces left)
  [elara]: "Eleven hours yesterday, Dr. Penn. I counted. So I knitted you a sleeping mask. Lavender, the nurse's orders!"  ✏ `event.overheard_elara_odalys_3.l1`
  [odalys]: "You counted wrong. It was twelve. ...The mask is very soft. Ten minutes, after lunch. Once."
  · (odalys point (background))
  · (odalys emote sparkle)
  · (elara shrug (background))
  [elara]: "Ten minutes is a perfect nap! I'll mind the desk. I'll answer nothing, kindly!"  ✏ `event.overheard_elara_odalys_3.l3`
  [odalys]: "Good care. Wake me for emergencies only. Splinters are not emergencies."
  · effects: flag:event.overheard_elara_odalys_3

### overheard_felix_elara_1 · "The Scarf Net" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.felix && flag:met.elara && hearts:felix>=2 && hearts:elara>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_felix_elara_1`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), elara(26, 18)
  · (place felix to (24,18))
  · (place elara to (26,18))
  · (felix faces right)
  · (elara faces left)
  [felix]: "Elara. A fish doesn't want a cozy net. It wants to be free."
  [elara]: "Free and warm aren't enemies, Felix! It's a very loose knit! The fish can leave whenever!"  ✏ `event.overheard_felix_elara_1.l2`
  · (elara shrug (background))
  · (elara emote sweat)
  · (felix point (background))
  [felix]: "...It does look warm. Hand it over. Strictly for the fish."
  [elara]: "For science! Of course! The next one's bigger. Boat-sized. Your boat looked cold, Felix!"  ✏ `event.overheard_felix_elara_1.l4`
  · effects: flag:event.overheard_felix_elara_1

### overheard_felix_elara_2 · "Big Fish, Big Scarf" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.felix && flag:met.elara && hearts:felix>=4 && hearts:elara>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_felix_elara_1) && !flag:event.overheard_felix_elara_2`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), elara(26, 18)
  · (place felix to (24,18))
  · (place elara to (26,18))
  · (felix faces right)
  · (elara faces left)
  [felix]: "Three feet, nose to tail. Last spring it was two. It grows in the telling."
  [elara]: "Three feet! My longest scarf is three feet, nose to tail! They could be friends!"  ✏ `event.overheard_felix_elara_2.l2`
  · (elara shrug (background))
  · (elara emote sweat)
  · (felix wave (background))
  [felix]: "A scarf and a fish, friends. I like it. Tell no one."
  [elara]: "Too late! I told the whole waiting room!"  ✏ `event.overheard_felix_elara_2.l4`
  · effects: flag:event.overheard_felix_elara_2

### overheard_felix_elara_3 · "Sunrise Knitting" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.felix && flag:met.elara && hearts:felix>=6 && hearts:elara>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_felix_elara_2) && !flag:event.overheard_felix_elara_3`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), elara(26, 18)
  · (place felix to (24,18))
  · (place elara to (26,18))
  · (felix faces right)
  · (elara faces left)
  [felix]: "Dawn's quiet, yes. Until you count stitches out loud."
  [elara]: "Seventeen, eighteen! That's not counting, that's a sea shanty for wool!"  ✏ `event.overheard_felix_elara_3.l2`
  · (elara point (background))
  · (elara emote sweat)
  · (felix shrug (background))
  [felix]: "...It's soothing. Don't stop. The fish like it."
  [elara]: "I won't! Nineteen! Twenty! Fish are the best listeners. They never interrupt."  ✏ `event.overheard_felix_elara_3.l4`
  · effects: flag:event.overheard_felix_elara_3
