# Scene cards for felix

18 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 37 spoken lines in scenes, 25 dialogue lines and 18 player choice labels.

### felix_heart2 · "Hold the Line" · funny
event · npc_roster.json · map Beach · priority 6
plays when: `hearts:felix>=2 && !weekday:thu && hour>=6 && hour<14`
beats may be ADDED or CUT in this scene
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Hold the line, [player]."  ✏ `event.felix_heart2.0`
  · (move player to (11,10))
  · (player faces up)
  · (felix emote note)
  [felix]: "Feel that pull? That's a good one."  ✏ `event.felix_heart2.1`
  [felix]: "Keep it. You earned it."  ✏ `event.felix_heart2.2`
  · effects: give:fish.tuna,1, friend:felix,40, flag:event.felix_heart2

### felix_heart5 · "The Lucky Charm" · wholesome
event · npc_roster.json · map Beach · priority 7
plays when: `hearts:felix>=5 && !weekday:thu && hour>=6 && hour<14 && flag:event.felix_heart2`
beats may be ADDED or CUT in this scene
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Every sailor has a lucky charm."  ✏ `event.felix_heart5.0`
  · (move player to (11,10))
  · (player faces up)
  · (felix emote note)
  [felix]: "This was mine. Now it's yours."  ✏ `event.felix_heart5.1`
  [felix]: "Fair winds."  ✏ `event.felix_heart5.2`
  · effects: give:resource.bait,15, give:prop.charm, friend:felix,60, flag:event.felix_heart5

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
  [felix]: "Mornin'. Boat's ready. She leaks politely. The bailing's the elegant part."  ✏ `event.past_the_point.0`
  [elara]: "I knitted a life jacket! It isn't how they work. I brought it anyway, and a sandwich tin."  (emote sparkle, expr happy)
  ▶ DIALOGUE `past_point.talk` (player-facing graph, plays here):
    n0 [elara]: "Forty-two places on the list, and Felix says we can manage one before breakfast. Which one?"
       ? "Round the headland." → r0  effects: flag:choice.point.headland, friend:elara,5
       ? "The white rocks." → r1  effects: flag:choice.point.rocks, friend:elara,5
       ? "Straight out, until the shore is a line." → r2  effects: flag:choice.point.horizon, friend:elara,5
    r0 [elara]: "The headland! I've drawn it eleven times. Never from a boat."
    r1 [elara]: "The white rocks. I've never been closer than the bench. Row slowly!"
    r2 [elara]: "Straight out. I'll hold the sandwich tin. It's very steadying."
  · (fadeout)
  · (sfx click)
  · (camera pan)
  · (lighting warm)
  · (fadein)
  [narration]: "The boat leaks. Felix bails with one hand and rows with the other, and never spills his coffee."
  [elara]: "The headland is bigger than the map made it. Felix, it's enormous. It's the best thing I've ever been wrong about."  (expr happy) [only if flag:choice.point.headland]
  [elara]: "The white rocks look like teeth from here. Friendly teeth. I'd like to wave at them. May I wave?"  (expr happy) [only if flag:choice.point.rocks]
  [elara]: "Out here the shore is only a line. I didn't know I could feel this small and this glad together."  (expr happy) [only if flag:choice.point.horizon]
  [wren]: "Don't mind me, friends. I'm only looking. Only looking, I promise, from the point." [only if flag:choice.wren.bag.go && flag:met.wren && hearts:wren>=8]
  [narration]: "Elara takes out her list and crosses one place off with a stub of pencil, and smiles at the pencil."
  [felix]: "Told you, partner. Dawn's better with two. ...Better with her list in it, too."  ✏ `event.past_the_point.4`  (emote heart)
  · (fadeout)
  · (camera reset)
  · (lighting reset)
  · (fadein)
  · (letterbox off)
  · effects: flag:event.past_the_point, friend:felix,40, friend:elara,40

### story_whistler_felix · "The Whistler" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:mystery_whistler.felix && hour>=18 && hour<21 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: felix(25, 18), player(25, 17)
  · (place felix to (25,18))
  · (felix faces down)
  [felix]: "(The whistling stops. Felix, holding a fishing line, turns bright red.) Don't tell the fish."  ✏ `event.story_whistler_felix.intro`
  · (move player to (25,17))
  · (player faces up)
  · (felix emote sweat)
  ▶ DIALOGUE `story.whistler.felix` (player-facing graph, plays here):
    n0 [felix]: "I only whistle when I'm happy. Which is why the fish are always listening."  ✏ `dlg.story.whistler.felix.0`   (→ n1)
    n1 [felix]: "...Well?"  ✏ `dlg.story.whistler.felix.1`
       ? "Call out a friendly hello." → r0  effects: flag:choice.whistler.felix.hello  ✏ `dlg.story.whistler.felix.c0`
       ? "Whistle the wrong note back." → r1  effects: flag:choice.whistler.felix.back, friend:felix,10  ✏ `dlg.story.whistler.felix.c1`
       ? "Say nothing. Just listen." → r2  effects: flag:choice.whistler.felix.listen, friend:felix,10  ✏ `dlg.story.whistler.felix.c2`
    r0 [felix]: "Hello. Right. Good. Thank you for the normal approach."  ✏ `dlg.story.whistler.felix.r0`
    r1 [felix]: "You... got the wrong note exactly right. I'm moved and mortified."  ✏ `dlg.story.whistler.felix.r1`   (expr embarrassed)
    r2 [felix]: "You listened. To the whole thing. ...I may whistle again. Quietly."  ✏ `dlg.story.whistler.felix.r2`   (expr happy)
  [felix]: "The fish like the wrong note. They bite more."  ✏ `event.story_whistler_felix.after`
  · effects: flag:event.story_whistler_felix, friend:felix,30, flag:storydone.mystery_whistler

### story_pumpkin_felix · "The Missing Pumpkin" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:missing_pumpkin.felix && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: felix(25, 18), player(25, 17)
  · (place felix to (25,18))
  · (felix faces down)
  · (spawn prop prop.pumpkin at (26,18))
  [felix]: "Oh no. Don't tell the fish. I wanted a picture of it next to the biggest carp. For comparison."  ✏ `event.story_pumpkin_felix.intro`
  · (move player to (25,17))
  · (player faces up)
  · (felix emote sweat)
  · (felix shrug (background))
  ▶ DIALOGUE `story.pumpkin.felix` (player-facing graph, plays here):
    n0 [narration]: "You find Felix behind the store, with a very large pumpkin and a very guilty face."  ✏ `dlg.story.pumpkin.felix.0`   (→ n1)
    n1 [tilda]: "Is that my prize pumpkin? The one the whole village has been blaming the goats for?"   (→ n2)
    n2 [felix]: "...It might be."  ✏ `dlg.story.pumpkin.felix.2`
       ? "Let them off kindly." → r0  effects: flag:choice.pumpkin.felix.kind, friend:felix,10  ✏ `dlg.story.pumpkin.felix.c0`
       ? "Insist on a proper apology." → r1  effects: flag:choice.pumpkin.felix.apology  ✏ `dlg.story.pumpkin.felix.c1`
       ? "Offer to help carry it back." → r2  effects: flag:choice.pumpkin.felix.help, friend:tilda,10  ✏ `dlg.story.pumpkin.felix.c2`
    r0 [tilda]: "Kindly? Fine. Kindly. But he's on the slate for a pie."   (expr thinking)
    r1 [felix]: "An apology. Yes. Very sorry. Truly. Pumpkin, I'm also sorry to you."  ✏ `dlg.story.pumpkin.felix.r1`   (emote sweat)
    r2 [tilda]: "You carry the heavy end. I'll supervise. We'll call it civic duty."   (expr happy)
  [felix]: "He's returning it with a photograph. Tilda frames it. The carp has not forgiven anyone."  ✏ `event.story_pumpkin_felix.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_pumpkin_felix, friend:felix,30, friend:tilda,20, flag:storydone.missing_pumpkin

### festival_winter_lanterns · "The Lantern Release" · wholesome
event · wave7_festival_moments.json · map Village · priority 6 · repeatable
plays when: `season:winter && day==25 && flag:festival.winter.joined && unseen:festival.winter.lanterns && hour>=17`
beats may be ADDED or CUT in this scene
placed: hazel(23, 18), ione(24, 18), tilda(26, 18), felix(27, 18)
  · (place hazel to (23,18))
  · (hazel faces up)
  · (place ione to (24,18))
  · (ione faces up)
  · (place tilda to (26,18))
  · (tilda faces up)
  · (place felix to (27,18))
  · (felix faces up)
  · (spawn prop prop.lantern at (23,17))
  · (spawn prop prop.lantern at (24,17))
  · (spawn prop prop.lantern at (26,17))
  · (spawn prop prop.lantern at (27,17))
  [tilda]: "Everyone has a lantern, dear. Hold it up. Don't light it yet. Wait for the bell."
  [hazel]: "(One lantern each.) (A small light, and a small wish.) Please keep yours private."
  · (hazel emote sparkle)
  · (sfx lantern)
  [felix]: "It's this big. The wish, I mean. I'll say no more. The lantern will do the telling."  ✏ `event.festival_winter_lanterns.2`
  [ione]: "Now. Together, and slowly. Let them go. The dark will do the rest."
  · (ione emote heart)
  · (remove prop story_prop_3)
  · (remove prop story_prop_2)
  · (remove prop story_prop_1)
  · (remove prop story_prop_0)
  · effects: mark:festival.winter.lanterns, flag:event.festival_winter_lanterns, friend:hazel,10, friend:ione,10, friend:tilda,10, friend:felix,10

### felix_heart8 · "The Big One" · funny
conversation · wave3_events.json · map Beach · priority 7
plays when: `hearts:felix>=8 && flag:event.felix_heart5 && !weekday:thu && hour>=6 && hour<14`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Dawn, the pond, and a line that's tugging. I've a feeling about this one. Hold the net. Don't breathe."  ✏ `event.felix_heart8.intro`
  · (move player to (11,10))
  · (player faces up)
  · (felix wave (background))
  · (felix emote sweat)
  ▶ DIALOGUE `felix.big` (player-facing graph, plays here):
    n0 [narration]: "Something very large tugs on Felix's line. It might be a fish. It might be the bottom of the pond."  ✏ `dlg.felix.big.0`   (→ n1)
    n1 [felix]: "It's this big. Maybe bigger. What do we do?"  ✏ `dlg.felix.big.1`
       ? "Net it." → r0  effects: friend:felix,10, flag:choice.big.net  ✏ `dlg.felix.big.c0`
       ? "Let it go." → r1  effects: friend:felix,15, flag:choice.big.release  ✏ `dlg.felix.big.c1`
       ? "Measure it honestly." → r2  effects: friend:felix,10, flag:choice.big.measure  ✏ `dlg.felix.big.c2`
    r0 [felix]: "The net! Hold steady. ...There she is. A beauty. I'll never doubt your wrist again."  ✏ `dlg.felix.big.r0`   (expr happy)
    r1 [felix]: "Let it go? ...Fine. A good fish deserves a long life. And a good story. Both stay."  ✏ `dlg.felix.big.r1`   (expr happy)
    r2 [felix]: "Honestly. Well. Eleven inches. ...It was this big a moment ago. I'm not saying it was your fault."  ✏ `dlg.felix.big.r2`   (emote sweat)
  [felix]: "{if choice:big.net}You netted it! It was this big. I'll say it's bigger by Friday.{else}{if choice:big.release}You let it go. It waved, I'm sure. I'll catch it again. We've an understanding.{else}You measured it honestly. Eleven inches. I'm devastated. And moved. Mostly devastated.{/if}{/if}"  ✏ `event.felix_heart8.after`
  · effects: flag:event.felix_heart8
  · effects: friend:felix,70

### felix_birthday · "Felix's Birthday" · funny
conversation · wave4_birthdays.json · map Beach · priority 8 · repeatable
plays when: `hearts:felix>=2 && birthday.in:felix==0 && unseen:birthday.felix && !weekday:thu && hour>=6 && hour<14`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 10)
  · (felix faces down)
  [felix]: "It's my birthday. I'd be fishing, but Tilda says I mustn't. She brought a pie. I brought a story. It's about a fish."  ✏ `event.felix_birthday.intro`
  · (move player to (11,10))
  · (player faces up)
  · (felix wave (background))
  · (felix emote note)
  ▶ DIALOGUE `felix.birthday` (player-facing graph, plays here):
    n0 [felix]: "The story can wait. What's the word?"  ✏ `dlg.felix.birthday.0`
       ? "Wish him a happy birthday." → r0  effects: friend:felix,10, flag:choice.bday.felix.wish  ✏ `dlg.felix.birthday.c0`
       ? "Listen to the fish story." → r1  effects: friend:felix,10, flag:choice.bday.felix.story  ✏ `dlg.felix.birthday.c1`
       ? "Offer to fish with him." → r2  effects: friend:felix,10, flag:choice.bday.felix.fish  ✏ `dlg.felix.birthday.c2`
    r0 [felix]: "Thank you. I'll tell the fish. They'll be unmoved, but I'll feel better."  ✏ `dlg.felix.birthday.r0`   (expr happy)
    r1 [felix]: "It was this big. Bigger, since I began. Thank you for listening."  ✏ `dlg.felix.birthday.r1`   (expr happy)
    r2 [felix]: "Dawn tomorrow. Bring a line. I'll bring the good rock."  ✏ `dlg.felix.birthday.r2`   (expr happy)
  [felix]: "{if choice:bday.felix.wish}The fish know. They're pretending they don't.{else}{if choice:bday.felix.story}The story's longer now. I added a hand. You were in it.{else}Dawn, the pond, and two lines. A fine birthday present.{/if}{/if}"  ✏ `event.felix_birthday.after`
  · effects: flag:event.felix_birthday
  · effects: mark:birthday.felix, friend:felix,30, give:fish.carp,1, mail:felix_bday_thanks

### felix_courtship · "Felix Asks" · wholesome
conversation · wave5_courtship.json · map Beach · priority 8
plays when: `hearts:felix>=8 && flag:event.felix_heart8 && !flag:partners.felix && !flag:choice.courtship.felix.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:thu && hour>=6 && hour<14`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Well. Right. I've caught bigger things than my nerve. Here goes. Would you court me? Slowly. Like a good lure."  ✏ `event.felix_courtship.intro`
  · (move player to (11,10))
  · (player faces up)
  · (felix wave (background))
  · (felix emote note)
  ▶ DIALOGUE `felix.courtship` (player-facing graph, plays here):
    n0 [felix]: "What do you say?"  ✏ `dlg.felix.courtship.0`
       ? "Yes. I'd like that." → r0  effects: friend:felix,20, flag:choice.courtship.felix.yes  ✏ `dlg.felix.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:felix,15, flag:choice.courtship.felix.friends  ✏ `dlg.felix.courtship.c1`
       ? "Give me a little time." → r2  effects: friend:felix,5, flag:choice.courtship.felix.later  ✏ `dlg.felix.courtship.c2`
    r0 [felix]: "Yes! Oh, the best catch of my life, and I wasn't even trying. Dawn tomorrow, two lines, and the good rock."  ✏ `dlg.felix.courtship.r0`   (expr happy)
    r1 [felix]: "Friends. A fine thing. The best fishing partners are friends. I'm happy. I'm also a little quiet. Thank you."  ✏ `dlg.felix.courtship.r1`   (expr happy)
    r2 [felix]: "A little time. Good. Fish take time. So can the rest of it."  ✏ `dlg.felix.courtship.r2`   (expr thinking)
  [felix]: "{if flag:choice.courtship.felix.yes}Dawn, the pond, and two lines. I'll bring the thermos. You bring the stories.{else}{if flag:choice.courtship.felix.friends}Friends, then. The finest bait, and no hooks. I'll keep the invitation open.{else}Time, then. The pond's patient. So am I. Mostly.{/if}{/if}"  ✏ `event.felix_courtship.after`
  · effects: flag:event.felix_courtship
  · effects: flag:courtship.asked.felix

### felix_courtship2 · "Felix Asks Again" · wholesome
conversation · wave5_courtship.json · map Beach · priority 8
plays when: `hearts:felix>=10 && flag:choice.courtship.felix.later && !flag:event.felix_courtship2.done && !flag:partners.felix && !flag:choice.courtship.felix.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:thu && hour>=6 && hour<14`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Well. Right. I've caught bigger things than my nerve. Here goes. Would you court me? Slowly. Like a good lure."  ✏ `event.felix_courtship2.intro`
  · (move player to (11,10))
  · (player faces up)
  · (felix wave (background))
  · (felix emote note)
  ▶ DIALOGUE `felix.courtship` (player-facing graph, plays here):
    n0 [felix]: "What do you say?"  ✏ `dlg.felix.courtship.0`
       ? "Yes. I'd like that." → r0  effects: friend:felix,20, flag:choice.courtship.felix.yes  ✏ `dlg.felix.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:felix,15, flag:choice.courtship.felix.friends  ✏ `dlg.felix.courtship.c1`
       ? "Give me a little time." → r2  effects: friend:felix,5, flag:choice.courtship.felix.later  ✏ `dlg.felix.courtship.c2`
    r0 [felix]: "Yes! Oh, the best catch of my life, and I wasn't even trying. Dawn tomorrow, two lines, and the good rock."  ✏ `dlg.felix.courtship.r0`   (expr happy)
    r1 [felix]: "Friends. A fine thing. The best fishing partners are friends. I'm happy. I'm also a little quiet. Thank you."  ✏ `dlg.felix.courtship.r1`   (expr happy)
    r2 [felix]: "A little time. Good. Fish take time. So can the rest of it."  ✏ `dlg.felix.courtship.r2`   (expr thinking)
  [felix]: "{if flag:choice.courtship.felix.yes}Dawn, the pond, and two lines. I'll bring the thermos. You bring the stories.{else}{if flag:choice.courtship.felix.friends}Friends, then. The finest bait, and no hooks. I'll keep the invitation open.{else}Time, then. The pond's patient. So am I. Mostly.{/if}{/if}"  ✏ `event.felix_courtship2.after`
  · effects: flag:event.felix_courtship2
  · effects: flag:courtship.asked.felix, flag:event.felix_courtship2.done

### felix_partners · "Felix, Together" · wholesome
conversation_prop · wave5_courtship.json · map Beach · priority 8
plays when: `hearts:felix>=10 && flag:choice.courtship.felix.yes && !flag:partners.felix && !weekday:thu && hour>=6 && hour<14`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Morning. I caught something. It's for you. It isn't a fish. It's a shell. A very good one. Sit. Hold it up to your ear."  ✏ `event.felix_partners.intro`
  · (move player to (11,10))
  · (player faces up)
  · (felix wave (background))
  · (spawn prop prop.shell at (12,10))
  · (felix emote heart)
  ▶ DIALOGUE `felix.partners` (player-facing graph, plays here):
    n0 [felix]: "Tide's out. Evening's ours. What'll it be?"  ✏ `dlg.felix.partners.0`
       ? "Listen to the shell." → r0  effects: friend:felix,10, flag:choice.courtship.felix.shell  ✏ `dlg.felix.partners.c0`
       ? "Fish together at dawn." → r1  effects: friend:felix,10, flag:choice.courtship.felix.fish  ✏ `dlg.felix.partners.c1`
       ? "Watch the sunrise with him." → r2  effects: friend:felix,10, flag:choice.courtship.felix.sun  ✏ `dlg.felix.partners.c2`
    r0 [felix]: "Hear it? That's the sea. It's saying something. I think it's saying your name. It could be 'hello'."  ✏ `dlg.felix.partners.r0`   (expr happy)
    r1 [felix]: "Dawn, two lines, and the good rock. You hold one. I'll hold the thermos. It's the finest morning."  ✏ `dlg.felix.partners.r1`   (expr happy)
    r2 [felix]: "The sun comes up. We don't say much. The fish don't mind. It's perfect."  ✏ `dlg.felix.partners.r2`   (expr happy)
  [felix]: "{if choice:courtship.felix.shell}The shell sits on my shelf, next to the best hook. It still says your name. Or 'hello'.{else}{if choice:courtship.felix.fish}Dawn, two lines, one very small fish. It was the best catch in years. We threw it back.{else}The sunrise was quiet. So were we. It was the longest sentence I've ever said.{/if}{/if}"  ✏ `event.felix_partners.after`
  · effects: flag:event.felix_partners
  · (remove prop scene_prop)
  · effects: flag:partners.felix, friend:felix,50

### overheard_tilda_felix_1 · "Fish Prices" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.felix && hearts:tilda>=2 && hearts:felix>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), felix(26, 18)
  · (place tilda to (24,18))
  · (place felix to (26,18))
  · (tilda faces right)
  · (felix faces left)
  [tilda]: "Felix. Your carp is eight coins. Last week it was six."
  [felix]: "It's a bigger carp. It's grown. It's grown in the stall, overnight, out of pride."  ✏ `event.overheard_tilda_felix_1.l2`
  · (felix shrug (background))
  · (felix emote ellipsis)
  · (tilda point (background))
  [tilda]: "It's been in your bucket since dawn. It hasn't grown. You've inflated it."
  [felix]: "Fine. Six. But I'm adding a story to the price. It's a good story."  ✏ `event.overheard_tilda_felix_1.l4`
  · effects: flag:event.overheard_tilda_felix_1

### overheard_tilda_felix_2 · "The Oldest Friend" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.felix && hearts:tilda>=4 && hearts:felix>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_felix_1`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), tilda(26, 18)
  · (place felix to (24,18))
  · (place tilda to (26,18))
  · (felix faces right)
  · (tilda faces left)
  [felix]: "Tilda. You remember the winter the pond froze, and I fell in?"  ✏ `event.overheard_tilda_felix_2.l1`
  [tilda]: "I remember the winter you fell in, and also the winter you fell in again."
  · (tilda shrug (background))
  · (tilda emote ellipsis)
  · (felix wave (background))
  [felix]: "It was the same winter. It was a long one. You brought me soup."  ✏ `event.overheard_tilda_felix_2.l3`
  [tilda]: "I brought you soup, and a blanket, and I chalked 'Thin Ice' on the slate. You ignored the slate."
  · effects: flag:event.overheard_tilda_felix_2

### overheard_tilda_felix_3 · "Slate and Line" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.felix && hearts:tilda>=6 && hearts:felix>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_felix_2`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), tilda(26, 18)
  · (place felix to (24,18))
  · (place tilda to (26,18))
  · (felix faces right)
  · (tilda faces left)
  [felix]: "Tilda. I wrote the first fish of the year on your slate. In capital letters."  ✏ `event.overheard_tilda_felix_3.l1`
  [tilda]: "Oh, Felix. It's a very good fish. I'll leave it up until the spring thaw."
  · (tilda point (background))
  · (tilda emote ellipsis)
  · (felix shrug (background))
  [felix]: "It was this big. Bigger, since I began. Don't tell the pond."  ✏ `event.overheard_tilda_felix_3.l3`
  [tilda]: "The pond knows, dear. The pond always knows. Sit. I'll put the kettle on."
  · effects: flag:event.overheard_tilda_felix_3

### overheard_tilda_felix_4 · "The Last Pie" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.felix && hearts:tilda>=8 && hearts:felix>=8 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_felix_3`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), tilda(26, 18)
  · (place felix to (24,18))
  · (place tilda to (26,18))
  · (felix faces right)
  · (tilda faces left)
  [felix]: "Tilda, I brought a pie. For once. A fish pie. Don't judge."  ✏ `event.overheard_tilda_felix_4.l1`
  [tilda]: "Felix, I'll judge it kindly. And salt it."
  · (tilda wave (background))
  · (tilda emote ellipsis)
  · (felix wave (background))
  [felix]: "It's a fair sentence. Pass the fork."  ✏ `event.overheard_tilda_felix_4.l3`
  [tilda]: "To an old friend, dear. The crust is mine. The fish is the cat's."
  · effects: flag:event.overheard_tilda_felix_4

### overheard_felix_elara_1 · "The Scarf Net" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.felix && flag:met.elara && hearts:felix>=2 && hearts:elara>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: felix(24, 18), elara(26, 18)
  · (place felix to (24,18))
  · (place elara to (26,18))
  · (felix faces right)
  · (elara faces left)
  [felix]: "Elara. A fish doesn't want comfort. It wants to be free."  ✏ `event.overheard_felix_elara_1.l1`
  [elara]: "Then it's a free, comfortable net! Everybody wins!"
  · (elara shrug (background))
  · (elara emote sweat)
  · (felix point (background))
  [felix]: "...It does look warm. Hand it over. For science."  ✏ `event.overheard_felix_elara_1.l3`
  [elara]: "I knew you'd love it! I'll knit a bigger one!"
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
  [felix]: "It was this big. Bigger. A true giant."  ✏ `event.overheard_felix_elara_2.l1`
  [elara]: "I made a scarf this big! Same size! They'd be friends!"
  · (elara shrug (background))
  · (elara emote sweat)
  · (felix wave (background))
  [felix]: "A scarf-fish friendship. I like it. Tell no one."  ✏ `event.overheard_felix_elara_2.l3`
  [elara]: "Everyone knows! I told the clinic!"
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
  [felix]: "Quiet, yes. Until you count stitches aloud."  ✏ `event.overheard_felix_elara_3.l1`
  [elara]: "Seventeen, eighteen! It's a rhythm!"
  · (elara point (background))
  · (elara emote sweat)
  · (felix shrug (background))
  [felix]: "...It's soothing. Don't stop. The fish like it."  ✏ `event.overheard_felix_elara_3.l3`
  [elara]: "I won't! Nineteen! Twenty! Fish are lovely listeners!"
  · effects: flag:event.overheard_felix_elara_3
