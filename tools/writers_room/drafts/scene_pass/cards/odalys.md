# Scene cards for odalys

14 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 31 spoken lines in scenes, 12 dialogue lines and 6 player choice labels.

### odalys_heart2 · "Pulse Check" · funny
event · npc_roster.json · map Clinic · priority 6
plays when: `(hearts:odalys>=2 && !weekday:sat && hour>=9 && hour<17) && !flag:event.odalys_heart2`
beats may be ADDED or CUT in this scene
placed: player(6, 2)
  · (odalys faces down)
  [odalys]: "Sit. No, stand. Wrist, [player]. You walked in without a single complaint. In this clinic, that is an anomaly."  ✏ `event.odalys_heart2.0`
  · (move player to (6,2))
  · (player faces up)
  · (odalys emote ellipsis)
  [odalys]: "Sixty-two, steady. The fields are doing half my job. I'll allow it."  ✏ `event.odalys_heart2.1`
  [odalys]: "When did you last eat sitting down? Bread over a seed bag does not count."  ✏ `event.odalys_heart2.3`  (expr thinking)
  [odalys]: "Two salads, then. At a table, on a chair, in daylight. Doctor's orders."  ✏ `event.odalys_heart2.2`
  [odalys]: "Half of Wetherell eats standing up. I keep a list. You have just been crossed off it."  ✏ `event.odalys_heart2.4`
  · effects: give:food.salad,2, friend:odalys,40, flag:event.odalys_heart2

### odalys_heart5 · "The Remedy Notebook" · wholesome
event · npc_roster.json · map Clinic · priority 7
plays when: `(hearts:odalys>=5 && !weekday:sat && hour>=9 && hour<17 && flag:event.odalys_heart2) && !flag:event.odalys_heart5`
beats may be ADDED or CUT in this scene
placed: player(6, 2)
  · (odalys faces down)
  [odalys]: "This is my notebook of remedies. Every cure that ever worked in Wetherell, and several that only worked on Marcus."  ✏ `event.odalys_heart5.0`
  · (move player to (6,2))
  · (player faces up)
  · (odalys emote ellipsis)
  [odalys]: "I copied one out for you. The forager's plate: hedge greens, mushrooms, a little salt. Eaten sitting down. We've discussed this."  ✏ `event.odalys_heart5.1`
  [odalys]: "Edmund has a page in here. That last fall he followed every instruction exactly. He never had before. ...Tidy, at the end."  ✏ `event.odalys_heart5.3`  (expr thinking)
  [odalys]: "Use it well. If it works, write it in the margin. I like to know what helps."  ✏ `event.odalys_heart5.2`
  · effects: learn:cook_forager_plate, friend:odalys,60, flag:event.odalys_heart5

### story_pie_payoff · "A Slice for Odalys" · funny
event · storyline_events.json · map Village · priority 7
plays when: `(storyline:pie_feud && (hearts:marcus>=3 || hearts:odalys>=3) && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.story_pie_payoff`
beats may be ADDED or CUT in this scene
placed: marcus(25, 18), player(25, 17)
  · (place marcus to (25,18))
  · (marcus faces down)
  [marcus]: "[player]. A word, quietly. You walk past the clinic most days, don't you?"
  · (move player to (25,17))
  · (player faces up)
  · (marcus emote exclaim)
  ▶ DIALOGUE `story.pie.talk` (player-facing graph, plays here):
    n0 [marcus]: "One slice of my pie. Her door. No name. If she asks, a stranger with excellent taste."   (→ n1)
    n1 [marcus]: "You can say no. The pie and I will just stand here. We're patient."
       ? "Deliver it, no tricks." → r0  effects: flag:choice.pie.honest
       ? "I'll improve it on the way." → r1  effects: flag:choice.pie.improve
       ? "It might not survive the walk." → r2  effects: flag:choice.pie.eat
    r0 [narration]: "You leave the slice on Odalys's step. She reads the note, sighs, and takes a bite."   (→ r0b)
    r0b [odalys]: "A stranger with excellent taste and sawdust on the note. ...It's rather good. Tell him nothing."  ✏ `dlg.story.pie.talk.r0b`   (expr thinking)
    r1 [narration]: "You add a pinch of cinnamon on the way. Odalys tastes it, then frowns at it for a long time."   (→ r1b)
    r1b [odalys]: "Cinnamon, added late and not by the baker. It's better now. I resent the improvement, professionally."  ✏ `dlg.story.pie.talk.r1b`   (expr embarrassed)
    r2 [narration]: "You eat the slice on the road. It is excellent. Two bakers will hear about this."   (→ r2b)
    r2b [odalys]: "An empty plate and a crumb on your sleeve. I'll bill Marcus for one pie. You, for the crumb."  ✏ `dlg.story.pie.talk.r2b`
  [marcus]: "{if choice:pie.honest}'Rather good.' She said 'rather good.' Don't repeat it. I'll repeat it enough.{else}{if choice:pie.improve}She sent a note back: 'Cinnamon? Cheek.' Best review I've ever had.{else}You ate it. Fair. Now she's baking a second, and so am I. Ambitiously.{/if}{/if}"
  · effects: flag:event.story_pie_payoff, friend:marcus,20, friend:odalys,20, flag:storydone.pie_feud

### odalys_heart8 · "Office Hours" · wholesome
conversation · wave3_events.json · map Clinic · priority 7
plays when: `(hearts:odalys>=8 && flag:event.odalys_heart5 && !weekday:sat && hour>=9 && hour<17) && !flag:event.odalys_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 2)
  · (odalys faces down)
  [odalys]: "Sit. The clinic is closed; I closed it, deliberately, while no one was ill. I'm taking my own prescription tonight: soup, and company. My supplies of both are low."  ✏ `event.odalys_heart8.intro`
  · (move player to (6,2))
  · (player faces up)
  · (odalys wave (background))
  · (odalys emote ellipsis)
  ▶ DIALOGUE `odalys.office` (player-facing graph, plays here):
    n0 [odalys]: "I prescribe rest to everyone. I have never once swallowed the dose myself. This is unfamiliar. Be gentle."  ✏ `dlg.odalys.office.0`   (→ n1; expr thinking)
    n1 [odalys]: "So, [player]. The patient is stubborn, overdue, and out of excuses. Your recommendation?"  ✏ `dlg.odalys.office.1`
       ? "Insist she rests tonight, no appeals." → r0  effects: friend:odalys,10, flag:choice.office.insist  ✏ `dlg.odalys.office.c0`
       ? "Pull up a chair and share the soup." → r1  effects: friend:odalys,15, flag:choice.office.soup  ✏ `dlg.odalys.office.c1`
       ? "Write her a prescription for once." → r2  effects: friend:odalys,10, flag:choice.office.write  ✏ `dlg.odalys.office.c2`
    r0 [odalys]: "You insist. ...Accepted. Under protest, with soup, and in front of a witness."  ✏ `dlg.odalys.office.r0`
    r1 [odalys]: "Two spoons, one pot. The entire course of treatment. You diagnosed me in one visit. It took me years."  ✏ `dlg.odalys.office.r1`   (expr happy)
    r2 [odalys]: "A prescription. For me. 'Come back anyway.' ...I've never been given orders I wanted to follow."  ✏ `dlg.odalys.office.r2`   (expr happy)
  [odalys]: "{if choice:office.insist}You insisted I rest. I objected for a full minute. Then I slept nine hours. Elara is unbearable about it.{else}{if choice:office.soup}We finished the pot. I've written it up: 'Treatment successful. Repeat weekly.' That last part was not a question.{else}Your prescription is pinned above my desk. 'Come back anyway.' I intend to comply. Doctor's orders.{/if}{/if}"  ✏ `event.odalys_heart8.after`
  · effects: flag:event.odalys_heart8
  · effects: friend:odalys,70

### odalys_birthday · "Dr. Penn's Birthday" · wholesome
conversation · wave4_birthdays.json · map Clinic · priority 8 · repeatable
plays when: `hearts:odalys>=2 && birthday.in:odalys==0 && unseen:birthday.odalys && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(6, 2)
  · (odalys faces down)
  [odalys]: "It's my birthday. I observe it annually, with soup. Consider this an invitation, not an order. For once."  ✏ `event.odalys_birthday.intro`
  · (move player to (6,2))
  · (player faces up)
  · (odalys wave (background))
  · (odalys emote ellipsis)
  ▶ DIALOGUE `odalys.birthday` (player-facing graph, plays here):
    n0 [odalys]: "You stayed. I have no protocol for this. What now?"  ✏ `dlg.odalys.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:odalys,10, flag:choice.bday.odalys.wish  ✏ `dlg.odalys.birthday.c0`
       ? "Pull up a chair for the soup." → r1  effects: friend:odalys,10, flag:choice.bday.odalys.soup  ✏ `dlg.odalys.birthday.c1`
       ? "Prescribe her an afternoon nap." → r2  effects: friend:odalys,10, flag:choice.bday.odalys.nap  ✏ `dlg.odalys.birthday.c2`
    r0 [odalys]: "...Thank you. Most people wish me well by not needing me today. This is better."  ✏ `dlg.odalys.birthday.r0`   (expr happy)
    r1 [odalys]: "Then pass your bowl. Leek and potato, and an irresponsible amount of pepper. It's my birthday."  ✏ `dlg.odalys.birthday.r1`   (expr happy)
    r2 [odalys]: "A nap, prescribed by a farmer. ...Very well. My patients will be told it was an emergency."  ✏ `dlg.odalys.birthday.r2`   (expr happy)
  [odalys]: "{if choice:bday.odalys.wish}It went in the file labeled 'Unexpected'. That file was empty last winter. It isn't now.{else}{if choice:bday.odalys.soup}Shared soup. Elara says I smiled twice. She's counting, and she's wrong. It was three.{else}I napped twenty minutes. The clinic survived. Elara says nothing happened. She said it very quickly.{/if}{/if}"  ✏ `event.odalys_birthday.after`
  · effects: flag:event.odalys_birthday
  · effects: mark:birthday.odalys, friend:odalys,30, give:artisan.juice,1, mail:odalys_bday_thanks

### overheard_elara_odalys_1 · "The Waiting Room" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.elara && flag:met.odalys && hearts:elara>=2 && hearts:odalys>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_elara_odalys_1`
text-only scene: rewrite lines, do not add or cut beats
placed: elara(24, 18), odalys(26, 18)
  · (place elara to (24,18))
  · (place odalys to (26,18))
  · (elara faces right)
  · (odalys faces left)
  [elara]: "Dr. Penn, the stool is normal today! Normal stool! No reason to look at it at all!"
  [odalys]: "Elara. The stool is wearing a cozy. I noticed on Tuesday. It stays; wool aids recovery."  ✏ `event.overheard_elara_odalys_1.l2`
  · (odalys shrug (background))
  · (odalys emote sparkle)
  · (elara point (background))
  [elara]: "You like it! There's a spare for laundry day. Stools get cold feet. All four!"
  [odalys]: "No. One cozy per stool. That is clinic policy, as of now."  ✏ `event.overheard_elara_odalys_1.l4`
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
  [elara]: "Soup rounds! Seven vegetables, and one of them is a surprise!"
  [odalys]: "Seven vegetables. I'll need the surprise identified before anyone swallows it. Professionally."  ✏ `event.overheard_elara_odalys_2.l2`
  · (odalys shrug (background))
  · (odalys emote sparkle)
  · (elara wave (background))
  [elara]: "The turnip! It's dressed as a carrot! Very brave of it!"
  [odalys]: "...It's the best soup I've had all year. Save me a bowl. Don't tell the turnip."  ✏ `event.overheard_elara_odalys_2.l4`
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
  [elara]: "Eleven hours yesterday, Dr. Penn. I counted. So I knitted you a sleeping mask. Lavender, the nurse's orders!"
  [odalys]: "You counted wrong. It was twelve. ...The mask is very soft. Ten minutes, after lunch. Once."  ✏ `event.overheard_elara_odalys_3.l2`
  · (odalys point (background))
  · (odalys emote sparkle)
  · (elara shrug (background))
  [elara]: "Ten minutes is a perfect nap! I'll mind the desk. I'll answer nothing, kindly!"
  [odalys]: "Good care. Wake me for emergencies only. Splinters are not emergencies."  ✏ `event.overheard_elara_odalys_3.l4`
  · effects: flag:event.overheard_elara_odalys_3

### overheard_odalys_ione_1 · "Late Lamps" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.odalys && flag:met.ione && hearts:odalys>=2 && hearts:ione>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_odalys_ione_1`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "Ione. The library lamps were lit an hour past closing on Thursday. I keep records."  ✏ `event.overheard_odalys_ione_1.l1`
  [ione]: "Dr. Penn. Your clinic stayed lit two hours past. I could see your lamp from my desk."
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (odalys point (background))
  [odalys]: "Patients keep odd hours. Books do not. ...Neither point survives examination. A draw."  ✏ `event.overheard_odalys_ione_1.l3`
  [ione]: "Then we both go home on time. Starting tomorrow. Possibly Thursday."
  · effects: flag:event.overheard_odalys_ione_1

### overheard_odalys_ione_2 · "Reading Prescription" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.odalys && flag:met.ione && hearts:odalys>=4 && hearts:ione>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_odalys_ione_1) && !flag:event.overheard_odalys_ione_2`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "Ione. I prescribe you a nap. Fifteen minutes. I've already written it down."  ✏ `event.overheard_odalys_ione_2.l1`
  [ione]: "And I prescribe you a novel, Dr. Penn. One chapter. Equally non-negotiable."
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (odalys wave (background))
  [odalys]: "...Chapter one, then the nap. I'll return the book by hand. Your returns box is sticky again."  ✏ `event.overheard_odalys_ione_2.l3`
  [ione]: "Done. I picked a book whose first chapter is two pages. Sleep well."
  · effects: flag:event.overheard_odalys_ione_2

### overheard_odalys_ione_3 · "Tea Rounds" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.odalys && flag:met.ione && hearts:odalys>=6 && hearts:ione>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_odalys_ione_2) && !flag:event.overheard_odalys_ione_3`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "The tea, Ione. I've been told to drink it slowly, so I shall. Under observation."  ✏ `event.overheard_odalys_ione_3.l1`
  [ione]: "It's the calm kind. Even the library stove stopped ticking once the pot was on it."
  · (ione point (background))
  · (ione emote ellipsis)
  · (odalys shrug (background))
  [odalys]: "A tea that calms a stove. I'll want the leaf's name for my notebook. Pass the cup."  ✏ `event.overheard_odalys_ione_3.l3`
  [ione]: "Always. Here. It's gone a little cold. That's the slow part working."
  · effects: flag:event.overheard_odalys_ione_3

### overheard_marcus_odalys_1 · "Bend the Knees" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.marcus && flag:met.odalys && hearts:marcus>=2 && hearts:odalys>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_marcus_odalys_1`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), marcus(26, 18)
  · (place odalys to (24,18))
  · (place marcus to (26,18))
  · (odalys faces right)
  · (marcus faces left)
  [odalys]: "Marcus. You lifted that beam alone. I saw it from the clinic window. It's in your file."  ✏ `event.overheard_marcus_odalys_1.l1`
  [marcus]: "I measured it twice first."
  · (marcus shrug (background))
  · (marcus emote exclaim)
  · (odalys point (background))
  [odalys]: "Measure twice, bend the knees once. That one isn't carpentry. It's mine."  ✏ `event.overheard_marcus_odalys_1.l3`
  [marcus]: "Knees. Noted. Twice."
  · effects: flag:event.overheard_marcus_odalys_1

### overheard_marcus_odalys_2 · "The Clinic Bench" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.marcus && flag:met.odalys && hearts:marcus>=4 && hearts:odalys>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_odalys_1) && !flag:event.overheard_marcus_odalys_2`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), odalys(26, 18)
  · (place marcus to (24,18))
  · (place odalys to (26,18))
  · (marcus faces right)
  · (odalys faces left)
  [marcus]: "Dr. Penn. The bench outside your clinic had a loose leg. It's fixed."
  [odalys]: "I don't sit on that bench. I've never had the time."  ✏ `event.overheard_marcus_odalys_2.l2`
  · (odalys shrug (background))
  · (odalys emote ellipsis)
  · (marcus point (background))
  [marcus]: "That's why it worked loose. Nobody sits on it. Five minutes would do it good."
  [odalys]: "...Five minutes, then. Strictly for the bench's health. Elara is not to hear of this."  ✏ `event.overheard_marcus_odalys_2.l4`
  · effects: flag:event.overheard_marcus_odalys_2

### overheard_marcus_odalys_3 · "Doctor's Own Orders" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.marcus && flag:met.odalys && hearts:marcus>=6 && hearts:odalys>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_odalys_2) && !flag:event.overheard_marcus_odalys_3`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), marcus(26, 18)
  · (place odalys to (24,18))
  · (place marcus to (26,18))
  · (odalys faces right)
  · (marcus faces left)
  [odalys]: "Marcus. Your back. Honestly, this time."  ✏ `event.overheard_marcus_odalys_3.l1`
  [marcus]: "Better. I rest it Wednesdays. The doctor told me to."
  · (marcus point (background))
  · (marcus emote heart)
  · (odalys shrug (background))
  [odalys]: "I did say that. You listened. You're the only patient who does, and it ruins my averages."  ✏ `event.overheard_marcus_odalys_3.l3`
  [marcus]: "Then take your own advice. Saturdays. I'll build the chair."
  · effects: flag:event.overheard_marcus_odalys_3
