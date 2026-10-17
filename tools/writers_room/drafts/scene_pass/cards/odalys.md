# Scene cards for odalys

14 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 28 spoken lines in scenes, 12 dialogue lines and 6 player choice labels.

### odalys_heart2 · "Pulse Check" · funny
event · npc_roster.json · map Clinic · priority 6
plays when: `hearts:odalys>=2 && !weekday:sat && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(6, 2)
  · (odalys faces down)
  [odalys]: "[player], hold still. Pulse check."  ✏ `event.odalys_heart2.0`
  · (move player to (6,2))
  · (player faces up)
  · (odalys emote ellipsis)
  [odalys]: "Strong. All that digging pays off."  ✏ `event.odalys_heart2.1`
  [odalys]: "Here, a tonic. On the house."  ✏ `event.odalys_heart2.2`
  · effects: give:food.salad,2, friend:odalys,40, flag:event.odalys_heart2

### odalys_heart5 · "The Remedy Notebook" · wholesome
event · npc_roster.json · map Clinic · priority 7
plays when: `hearts:odalys>=5 && !weekday:sat && hour>=9 && hour<17 && flag:event.odalys_heart2`
beats may be ADDED or CUT in this scene
placed: player(6, 2)
  · (odalys faces down)
  [odalys]: "I keep a notebook of remedies."  ✏ `event.odalys_heart5.0`
  · (move player to (6,2))
  · (player faces up)
  · (odalys emote ellipsis)
  [odalys]: "I copied this one out for you."  ✏ `event.odalys_heart5.1`
  [odalys]: "Use it well."  ✏ `event.odalys_heart5.2`
  · effects: learn:cook_forager_plate, friend:odalys,60, flag:event.odalys_heart5

### story_pie_payoff · "A Slice for Odalys" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:pie_feud && (hearts:marcus>=3 || hearts:odalys>=3) && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(25, 18), player(25, 17)
  · (place marcus to (25,18))
  · (marcus faces down)
  [marcus]: "[player]. A word. You're good with a delivery, aren't you?"
  · (move player to (25,17))
  · (player faces up)
  · (marcus emote exclaim)
  ▶ DIALOGUE `story.pie.talk` (player-facing graph, plays here):
    n0 [marcus]: "One slice of my pie. Her door. Don't say who sent it. If she asks, say a stranger with excellent taste."   (→ n1)
    n1 [marcus]: "What do you say?"
       ? "Deliver it honestly." → r0  effects: flag:choice.pie.honest
       ? "Improve it first." → r1  effects: flag:choice.pie.improve
       ? "Eat it." → r2  effects: flag:choice.pie.eat
    r0 [narration]: "You carry the slice to Odalys's door. She reads the note, sighs, and takes a bite."   (→ r0b)
    r0b [odalys]: "Ambitious. ...It's rather good. Tell him nothing."  ✏ `dlg.story.pie.talk.r0b`   (expr thinking)
    r1 [narration]: "You add a pinch of cinnamon on the way. Odalys takes a bite and frowns for a long time."   (→ r1b)
    r1b [odalys]: "Who touched this? It's better. I hate that."  ✏ `dlg.story.pie.talk.r1b`   (expr embarrassed)
    r2 [narration]: "You eat the slice on the road. It is excellent. You now have two angry customers."   (→ r2b)
    r2b [odalys]: "Marcus owes me an apology. And a pie. And, apparently, you."  ✏ `dlg.story.pie.talk.r2b`
  [marcus]: "{if choice:pie.honest}She said 'good'. Out loud. Do not repeat it.{else}{if choice:pie.improve}She sent back a note: 'Cinnamon? Cheek.' I'm delighted.{else}You ate it. Fair. Odalys is baking a second. So am I.{/if}{/if}"
  · effects: flag:event.story_pie_payoff, friend:marcus,20, friend:odalys,20, flag:storydone.pie_feud

### odalys_heart8 · "Office Hours" · wholesome
conversation · wave3_events.json · map Clinic · priority 7
plays when: `hearts:odalys>=8 && flag:event.odalys_heart5 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 2)
  · (odalys faces down)
  [odalys]: "Sit. The clinic is closed. I'm taking my own prescription tonight: soup and company. I haven't had either in some time."  ✏ `event.odalys_heart8.intro`
  · (move player to (6,2))
  · (player faces up)
  · (odalys wave (background))
  · (odalys emote ellipsis)
  ▶ DIALOGUE `odalys.office` (player-facing graph, plays here):
    n0 [odalys]: "I advise everyone to rest. I take none of it. Tonight I'm trying. It's unfamiliar. Be gentle."  ✏ `dlg.odalys.office.0`   (→ n1; expr thinking)
    n1 [odalys]: "What's your professional opinion, [player]?"  ✏ `dlg.odalys.office.1`
       ? "Insist she rests." → r0  effects: friend:odalys,10, flag:choice.office.insist  ✏ `dlg.odalys.office.c0`
       ? "Share the soup with her." → r1  effects: friend:odalys,15, flag:choice.office.soup  ✏ `dlg.odalys.office.c1`
       ? "Write her a prescription." → r2  effects: friend:odalys,10, flag:choice.office.write  ✏ `dlg.odalys.office.c2`
    r0 [odalys]: "Insist? ...Fine. I'll rest. Under protest, with soup, and with a witness."  ✏ `dlg.odalys.office.r0`
    r1 [odalys]: "Soup, shared. That's the whole treatment. You've diagnosed me. Accurately."  ✏ `dlg.odalys.office.r1`   (expr happy)
    r2 [odalys]: "A prescription? For me? Well. "Come back anyway." I'll follow it to the letter."  ✏ `dlg.odalys.office.r2`   (expr happy)
  [odalys]: "{if choice:office.insist}You insisted I rest. I filed it under 'Unreasonable'. Then I did as I was told. It was remarkably effective.{else}{if choice:office.soup}We shared the soup. I'll write it up as 'Treatment, successful'.{else}Your prescription says 'Come back anyway.' I've pinned it above my desk. Doctor's orders.{/if}{/if}"  ✏ `event.odalys_heart8.after`
  · effects: flag:event.odalys_heart8
  · effects: friend:odalys,70

### odalys_birthday · "Dr. Penn's Birthday" · wholesome
conversation · wave4_birthdays.json · map Clinic · priority 8 · repeatable
plays when: `hearts:odalys>=2 && birthday.in:odalys==0 && unseen:birthday.odalys && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 2)
  · (odalys faces down)
  [odalys]: "It's my birthday. I observe it with soup. If you'd like to join, sit. That is not an order. It's a gentle one."  ✏ `event.odalys_birthday.intro`
  · (move player to (6,2))
  · (player faces up)
  · (odalys wave (background))
  · (odalys emote ellipsis)
  ▶ DIALOGUE `odalys.birthday` (player-facing graph, plays here):
    n0 [odalys]: "You stayed. I have no protocol for this. What now?"  ✏ `dlg.odalys.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:odalys,10, flag:choice.bday.odalys.wish  ✏ `dlg.odalys.birthday.c0`
       ? "Share the soup." → r1  effects: friend:odalys,10, flag:choice.bday.odalys.soup  ✏ `dlg.odalys.birthday.c1`
       ? "Prescribe her a nap." → r2  effects: friend:odalys,10, flag:choice.bday.odalys.nap  ✏ `dlg.odalys.birthday.c2`
    r0 [odalys]: "Thank you. I'll file it under 'Unexpected, pleasant'."  ✏ `dlg.odalys.birthday.r0`   (expr happy)
    r1 [odalys]: "Soup, shared. A treatment with no side effects. Thank you."  ✏ `dlg.odalys.birthday.r1`   (expr happy)
    r2 [odalys]: "A nap. As prescribed. ...I'll follow your orders. Rarely done, never regretted."  ✏ `dlg.odalys.birthday.r2`   (expr happy)
  [odalys]: "{if choice:bday.odalys.wish}It's in the file labeled 'Unexpected'. The file is growing.{else}{if choice:bday.odalys.soup}Shared soup. I noted it as 'Successful'. Elara agrees.{else}I napped. For twenty minutes. The clinic survived.{/if}{/if}"  ✏ `event.odalys_birthday.after`
  · effects: flag:event.odalys_birthday
  · effects: mark:birthday.odalys, friend:odalys,30, give:artisan.juice,1, mail:odalys_bday_thanks

### overheard_elara_odalys_1 · "The Waiting Room" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.elara && flag:met.odalys && hearts:elara>=2 && hearts:odalys>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: elara(24, 18), odalys(26, 18)
  · (place elara to (24,18))
  · (place odalys to (26,18))
  · (elara faces right)
  · (odalys faces left)
  [elara]: "Dr. Penn! Do not look at the waiting room stool! It is definitely not wearing a cozy!"
  [odalys]: "Elara. A stool does not have pride. ...It looks well on it. Leave it."  ✏ `event.overheard_elara_odalys_1.l2`
  · (odalys shrug (background))
  · (odalys emote sparkle)
  · (elara point (background))
  [elara]: "I knew it! A cozy for the stool, and a smaller cozy for the cozy!"
  [odalys]: "No. Stop there. That is a medical instruction."  ✏ `event.overheard_elara_odalys_1.l4`
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
  [elara]: "I made soup for the whole clinic! It has seven kinds of vegetable! One of them is a surprise!"
  [odalys]: "Elara. I accept the soup. I do not accept the surprise. Which one is it?"  ✏ `event.overheard_elara_odalys_2.l2`
  · (odalys shrug (background))
  · (odalys emote sparkle)
  · (elara wave (background))
  [elara]: "It's a turnip! It's wearing a carrot costume! It's very brave!"
  [odalys]: "...It's the best soup I've had. Save me a bowl. Quietly."  ✏ `event.overheard_elara_odalys_2.l4`
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
  [elara]: "Dr. Penn, you worked eleven hours yesterday. I knitted you a sleeping mask! It's lavender!"
  [odalys]: "Elara. ...It's soft. I'll try it for ten minutes after lunch. That is generous."  ✏ `event.overheard_elara_odalys_3.l2`
  · (odalys point (background))
  · (odalys emote sparkle)
  · (elara shrug (background))
  [elara]: "Ten minutes is a perfect nap! I'll cover the desk! I'll answer nothing, kindly!"
  [odalys]: "Thank you. Keep the clinic quiet. And, Elara. This was good care."  ✏ `event.overheard_elara_odalys_3.l4`
  · effects: flag:event.overheard_elara_odalys_3

### overheard_odalys_ione_1 · "Late Lamps" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.odalys && flag:met.ione && hearts:odalys>=2 && hearts:ione>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "Ione. The library stayed open an hour past closing."  ✏ `event.overheard_odalys_ione_1.l1`
  [ione]: "Dr. Penn. The clinic stayed open two."
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (odalys point (background))
  [odalys]: "Professional courtesy. I'd call it a draw."  ✏ `event.overheard_odalys_ione_1.l3`
  [ione]: "Then let's both go home. Tomorrow."
  · effects: flag:event.overheard_odalys_ione_1

### overheard_odalys_ione_2 · "Reading Prescription" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.odalys && flag:met.ione && hearts:odalys>=4 && hearts:ione>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_odalys_ione_1`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "I prescribe you a nap, Ione. Ten minutes. Non-negotiable."  ✏ `event.overheard_odalys_ione_2.l1`
  [ione]: "I prescribe you a novel, Dr. Penn. One chapter. Equally firm."
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (odalys wave (background))
  [odalys]: "...Chapter one. Then the nap."  ✏ `event.overheard_odalys_ione_2.l3`
  [ione]: "Agreed. Chapter one is short. The nap can be long."
  · effects: flag:event.overheard_odalys_ione_2

### overheard_odalys_ione_3 · "Tea Rounds" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.odalys && flag:met.ione && hearts:odalys>=6 && hearts:ione>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_odalys_ione_2`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "Thank you for the tea, Ione. I will drink it slowly."  ✏ `event.overheard_odalys_ione_3.l1`
  [ione]: "It's the calm kind. The stove likes it too."
  · (ione point (background))
  · (ione emote ellipsis)
  · (odalys shrug (background))
  [odalys]: "Then it is a verified remedy. Pass the cup."  ✏ `event.overheard_odalys_ione_3.l3`
  [ione]: "Always. Here. It's nearly warm."
  · effects: flag:event.overheard_odalys_ione_3

### overheard_marcus_odalys_1 · "Bend the Knees" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.odalys && hearts:marcus>=2 && hearts:odalys>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), marcus(26, 18)
  · (place odalys to (24,18))
  · (place marcus to (26,18))
  · (odalys faces right)
  · (marcus faces left)
  [odalys]: "Marcus. You lifted that beam alone. I saw it from the clinic window."  ✏ `event.overheard_marcus_odalys_1.l1`
  [marcus]: "Measured it twice first."
  · (marcus shrug (background))
  · (marcus emote exclaim)
  · (odalys point (background))
  [odalys]: "Measuring is not lifting. Bend the knees. Doctor's orders."  ✏ `event.overheard_marcus_odalys_1.l3`
  [marcus]: "Knees. Noted. Twice."
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
  [marcus]: "Dr. Penn. The bench outside your clinic had a loose leg. Fixed it."
  [odalys]: "I do not sit on that bench."  ✏ `event.overheard_marcus_odalys_2.l2`
  · (odalys shrug (background))
  · (odalys emote ellipsis)
  · (marcus point (background))
  [marcus]: "That's why the leg was loose. Nobody tests it. Try it. Whenever you like."
  [odalys]: "...Five minutes, then. It is a good bench. Do not tell Elara."  ✏ `event.overheard_marcus_odalys_2.l4`
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
  [odalys]: "Your back, Marcus. How is it?"  ✏ `event.overheard_marcus_odalys_3.l1`
  [marcus]: "Better. I've been resting it. The doctor said to."
  · (marcus point (background))
  · (marcus emote heart)
  · (odalys shrug (background))
  [odalys]: "I did. You listened. You are the only patient who does."  ✏ `event.overheard_marcus_odalys_3.l3`
  [marcus]: "Then take your own advice. Sundays. I'll build the chair."
  · effects: flag:event.overheard_marcus_odalys_3
