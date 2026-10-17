# Scene cards for felix

18 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 40 spoken lines in scenes, 25 dialogue lines and 18 player choice labels.

### felix_heart2 · "Hold the Line" · funny
event · npc_roster.json · map Beach · priority 6
plays when: `hearts:felix>=2 && !weekday:thu && hour>=6 && hour<14`
beats may be ADDED or CUT in this scene
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Mornin'. Take the rod. Both hands. Hold the line, [player]."  ✏ `event.felix_heart2.0`
  · (move player to (11,10))
  · (player faces up)
  · (felix emote note)
  [felix]: "Feel that? Don't reel yet. Let it run. It's deciding about you."  ✏ `event.felix_heart2.1`
  [felix]: "Now! Up, easy. ...A tuna. First try. Took me nine summers."  ✏ `event.felix_heart2.3`  (emote exclaim, expr surprised)
  [felix]: "Keep it. You earned it. I'm on this rock at six most days. ...Just saying where the rock is."  ✏ `event.felix_heart2.2`
  · effects: give:fish.tuna,1, friend:felix,40, flag:event.felix_heart2

### felix_heart5 · "The Lucky Charm" · wholesome
event · npc_roster.json · map Beach · priority 7
plays when: `hearts:felix>=5 && !weekday:thu && hour>=6 && hour<14 && flag:event.felix_heart2`
beats may be ADDED or CUT in this scene
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Every fisher has a lucky charm. Mine's sat through forty years of dawns."  ✏ `event.felix_heart5.0`
  · (move player to (11,10))
  · (player faces up)
  · (felix emote note)
  [felix]: "It was mine. It's yours now. I don't need luck on the mornings you're here."  ✏ `event.felix_heart5.1`
  [felix]: "And fifteen bait, hand-dug. Don't tell the stall I gave it away."  ✏ `event.felix_heart5.3`  (expr happy)
  [felix]: "Fair winds. ...Someday I'll show you the boat. She leaks, but politely."  ✏ `event.felix_heart5.2`
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
  [felix]: "There you both are. I promised you the boat. She's ready. The bailing tin is the elegant part."  ✏ `event.past_the_point.0`
  [elara]: "I knitted a life jacket! Felix says that's not how they work. It goes over the real one. Sandwiches are in the tin!"  (emote sparkle, expr happy)
  [felix]: "Wear both, then. The knitted one can look brave. The real one can do the floating."  ✏ `event.past_the_point.5`  (expr happy)
  ▶ DIALOGUE `past_point.talk` (player-facing graph, plays here):
    n0 [elara]: "My list is longer than the boat, and Felix says one place before breakfast. Which one?"
       ? "Round the headland. You've drawn it enough." → r0  effects: flag:choice.point.headland, friend:elara,5
       ? "The white rocks. Slowly." → r1  effects: flag:choice.point.rocks, friend:elara,5
       ? "Straight out, till the shore's a line." → r2  effects: flag:choice.point.horizon, friend:elara,5
    r0 [elara]: "The headland! I've drawn it nine times from the bench. Never from the water side!"
    r1 [elara]: "The white rocks! I've never been closer than the bench. Row slowly, Felix. I want it to last."
    r2 [elara]: "Straight out! I'll hold the sandwich tin. It's very steadying. Medically."
  · (fadeout)
  · (sfx click)
  · (camera pan)
  · (lighting warm)
  · (fadein)
  [narration]: "The boat leaks, politely. Felix pulls the oars. Elara holds her hat down with both hands. The headland slides closer."
  [elara]: "The headland is twice the size the map said! Felix, it's enormous! Best thing I've ever been wrong about!"  (expr happy) [only if flag:choice.point.headland]
  [elara]: "The white rocks look like teeth from here. Friendly teeth! Very good gums. May I wave at them?"  (expr happy) [only if flag:choice.point.rocks]
  [elara]: "The shore's just a line now. I didn't know I could feel this small and this glad at once."  (expr happy) [only if flag:choice.point.horizon]
  [wren]: "Just looking, that's all. From the point, the sea seems to start somewhere else." [only if flag:choice.wren.bag.go && flag:met.wren && hearts:wren>=8]
  [narration]: "Elara takes out her list, looks at it, and smiles at the page."
  [felix]: "Dawn's better with two. With three, it's an expedition. ...One place crossed off. I'll want a bigger boat for the rest."  ✏ `event.past_the_point.4`  (emote heart)
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
  [felix]: "Oh. You heard that. ...Don't tell the fish."  ✏ `event.story_whistler_felix.intro`
  · (move player to (25,17))
  · (player faces up)
  · (felix emote sweat)
  ▶ DIALOGUE `story.whistler.felix` (player-facing graph, plays here):
    n0 [felix]: "I only whistle when I'm happy. Lately that's most evenings. Couldn't tell you why. ...Well. I could."  ✏ `dlg.story.whistler.felix.0`   (→ n1)
    n1 [felix]: "...Well? Say something. The quiet's worse."  ✏ `dlg.story.whistler.felix.1`
       ? "Call out a friendly hello." → r0  effects: flag:choice.whistler.felix.hello  ✏ `dlg.story.whistler.felix.c0`
       ? "Whistle the wrong note back." → r1  effects: flag:choice.whistler.felix.back, friend:felix,10  ✏ `dlg.story.whistler.felix.c1`
       ? "Say nothing. Just listen." → r2  effects: flag:choice.whistler.felix.listen, friend:felix,10  ✏ `dlg.story.whistler.felix.c2`
    r0 [felix]: "Hello. Yes. Hello back. Very sensible, given the circumstances."  ✏ `dlg.story.whistler.felix.r0`
    r1 [felix]: "You got the wrong note exactly right. Nobody's ever whistled back. ...I've gone red. It's the sunset."  ✏ `dlg.story.whistler.felix.r1`   (expr embarrassed)
    r2 [felix]: "You listened. The whole tune. Nobody's done that but the pond. ...I may whistle again. Quietly."  ✏ `dlg.story.whistler.felix.r2`   (expr happy)
  [felix]: "Same hour tomorrow, same lane, same tune. You can pretend you were passing."  ✏ `event.story_whistler_felix.after`
  · effects: flag:event.story_whistler_felix, friend:felix,30, flag:storydone.mystery_whistler

### story_pumpkin_felix · "The Missing Pumpkin" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:missing_pumpkin.felix && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: felix(25, 18), player(25, 17)
  · (place felix to (25,18))
  · (felix faces down)
  · (spawn prop prop.pumpkin at (26,18))
  [felix]: "Ah. Caught. I only borrowed it. I wanted it beside my biggest carp. For scale."  ✏ `event.story_pumpkin_felix.intro`
  · (move player to (25,17))
  · (player faces up)
  · (felix emote sweat)
  · (felix shrug (background))
  ▶ DIALOGUE `story.pumpkin.felix` (player-facing graph, plays here):
    n0 [narration]: "Behind the store, Felix stands beside a very large pumpkin, wearing a very guilty face."  ✏ `dlg.story.pumpkin.felix.0`   (→ n1)
    n1 [tilda]: "Felix. Is that my prize pumpkin? I blamed the goats! You don't even like vegetables!"   (→ n2)
    n2 [felix]: "...It might be. In my defense, I didn't eat it. I only admired it."  ✏ `dlg.story.pumpkin.felix.2`
       ? "Let Felix off kindly." → r0  effects: flag:choice.pumpkin.felix.kind, friend:felix,10  ✏ `dlg.story.pumpkin.felix.c0`
       ? "Ask for a proper apology." → r1  effects: flag:choice.pumpkin.felix.apology  ✏ `dlg.story.pumpkin.felix.c1`
       ? "Offer to carry the heavy end." → r2  effects: flag:choice.pumpkin.felix.help, friend:tilda,10  ✏ `dlg.story.pumpkin.felix.c2`
    r0 [tilda]: "Kindly. Fine. He owes me a pie, and if there's a fish in it, I'll know."   (expr thinking)
    r1 [felix]: "An apology. Yes. I'm sorry, Tilda. Truly. ...And sorry to you, pumpkin."  ✏ `dlg.story.pumpkin.felix.r1`   (emote sweat)
    r2 [tilda]: "You take the heavy end. Felix can carry the excuses. They weigh more."   (expr happy)
  [felix]: "Back it goes. Beside it, my biggest carp looked like bait. I'll not be telling that story."  ✏ `event.story_pumpkin_felix.after`
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
  [tilda]: "Lanterns up, everyone. Not lit yet. Wait for the bell. Felix, I mean you."
  [hazel]: "One lantern, one wish. Please keep yours folded up. (Last winter four people said theirs aloud.)"
  · (hazel emote sparkle)
  · (sfx lantern)
  [felix]: "My wish is this big. Last winter it was this big. Wishes grow, like fish."  ✏ `event.festival_winter_lanterns.2`
  [ione]: "That's the bell. Together now, and slowly. Let them go. ...Mine is drifting toward the sea. I didn't steer it."
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
  [felix]: "The pond, and a line that's tugging. I've had a feeling all week. Hold the net. Don't breathe."  ✏ `event.felix_heart8.intro`
  · (move player to (11,10))
  · (player faces up)
  · (felix wave (background))
  · (felix emote sweat)
  ▶ DIALOGUE `felix.big` (player-facing graph, plays here):
    n0 [narration]: "Something very large tugs on Felix's line. It might be a fish. It might be the bottom of the pond."  ✏ `dlg.felix.big.0`   (→ n1)
    n1 [felix]: "It's this big. No, this big. ...Partner, what do we do?"  ✏ `dlg.felix.big.1`
       ? "Net it." → r0  effects: friend:felix,10, flag:choice.big.net  ✏ `dlg.felix.big.c0`
       ? "Let it go." → r1  effects: friend:felix,15, flag:choice.big.release  ✏ `dlg.felix.big.c1`
       ? "Measure it honestly." → r2  effects: friend:felix,10, flag:choice.big.measure  ✏ `dlg.felix.big.c2`
    r0 [felix]: "The net! Hold steady. ...There she is! A beauty. I'll never doubt your wrist again."  ✏ `dlg.felix.big.r0`   (expr happy)
    r1 [felix]: "Let it go? ...Right. A good fish deserves a long life. We keep the story. It's bigger anyway."  ✏ `dlg.felix.big.r1`   (expr happy)
    r2 [felix]: "Honestly. Fine. Eleven inches. ...It was this big a moment ago. I blame the light."  ✏ `dlg.felix.big.r2`   (emote sweat)
  [felix]: "{if choice:big.net}Netted. It was this big. By Friday it'll be this big, and Tilda will groan.{else}{if choice:big.release}Gone home. It waved, I'm sure. Same dawn next year, we said. We've an understanding.{else}Eleven inches. Honest inches. I'm devastated. And proud. Mostly devastated.{/if}{/if}"  ✏ `event.felix_heart8.after`
  · effects: flag:event.felix_heart8
  · effects: friend:felix,70

### felix_birthday · "Felix's Birthday" · funny
conversation · wave4_birthdays.json · map Beach · priority 8 · repeatable
plays when: `hearts:felix>=2 && birthday.in:felix==0 && unseen:birthday.felix && !weekday:thu && hour>=6 && hour<14`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 10)
  · (felix faces down)
  [felix]: "It's my birthday. Tilda says no fishing today. She brought a pie. I brought a story. It's about a fish."  ✏ `event.felix_birthday.intro`
  · (move player to (11,10))
  · (player faces up)
  · (felix wave (background))
  · (felix emote note)
  ▶ DIALOGUE `felix.birthday` (player-facing graph, plays here):
    n0 [felix]: "The story can wait. What's the word?"  ✏ `dlg.felix.birthday.0`
       ? "Wish him a happy birthday." → r0  effects: friend:felix,10, flag:choice.bday.felix.wish  ✏ `dlg.felix.birthday.c0`
       ? "Ask for the fish story." → r1  effects: friend:felix,10, flag:choice.bday.felix.story  ✏ `dlg.felix.birthday.c1`
       ? "Offer to fish with him." → r2  effects: friend:felix,10, flag:choice.bday.felix.fish  ✏ `dlg.felix.birthday.c2`
    r0 [felix]: "Kind of you. I'll tell the fish. They won't care, but I'll feel taller."  ✏ `dlg.felix.birthday.r0`   (expr happy)
    r1 [felix]: "Sit, then. It was dawn, it was this big, and it was smirking at me."  ✏ `dlg.felix.birthday.r1`   (expr happy)
    r2 [felix]: "Dawn tomorrow. Bring a line. I'll bring the good rock."  ✏ `dlg.felix.birthday.r2`   (expr happy)
  [felix]: "{if choice:bday.felix.wish}Two well-wishers before noon. Tilda, then you. Record year. I'll chalk it on her slate.{else}{if choice:bday.felix.story}The story's longer now. I added a hand. And you, at the end, holding the net.{else}Tomorrow, then, off the good rock. Best present I've had. Don't tell Tilda. She brought pie.{/if}{/if}"  ✏ `event.felix_birthday.after`
  · effects: flag:event.felix_birthday
  · effects: mark:birthday.felix, friend:felix,30, give:fish.carp,1, mail:felix_bday_thanks

### felix_courtship · "Felix Asks" · wholesome
conversation · wave5_courtship.json · map Beach · priority 8
plays when: `hearts:felix>=8 && flag:event.felix_heart8 && !flag:partners.felix && !flag:choice.courtship.felix.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:thu && hour>=6 && hour<14`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Well. Right. I've held a line through a gale. This is harder. Would you court me? Slowly, like the tide."  ✏ `event.felix_courtship.intro`
  · (move player to (11,10))
  · (player faces up)
  · (felix wave (background))
  · (felix emote note)
  ▶ DIALOGUE `felix.courtship` (player-facing graph, plays here):
    n0 [felix]: "What do you say?"  ✏ `dlg.felix.courtship.0`
       ? "Yes. I'd like that." → r0  effects: friend:felix,20, flag:choice.courtship.felix.yes  ✏ `dlg.felix.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:felix,15, flag:choice.courtship.felix.friends  ✏ `dlg.felix.courtship.c1`
       ? "Give me a little time." → r2  effects: friend:felix,5, flag:choice.courtship.felix.later  ✏ `dlg.felix.courtship.c2`
    r0 [felix]: "Yes! Oh. Best catch of my life, and I wasn't even holding a rod."  ✏ `dlg.felix.courtship.r0`   (expr happy)
    r1 [felix]: "Friends. That's a fine thing. The best dawn partners are friends. I'm glad. I'll be a bit quiet today."  ✏ `dlg.felix.courtship.r1`   (expr happy)
    r2 [felix]: "A little time. Good. Fish take time. So does the rest of it."  ✏ `dlg.felix.courtship.r2`   (expr thinking)
  [felix]: "{if flag:choice.courtship.felix.yes}Tomorrow, then. The good rock, two lines. I'll bring the thermos. You bring yourself. That's plenty.{else}{if flag:choice.courtship.felix.friends}I'll still save you half the rock. That doesn't change. Rocks are loyal like that.{else}I'll ask again some dawn. You'll know it's coming. I'll go very quiet beforehand.{/if}{/if}"  ✏ `event.felix_courtship.after`
  · effects: flag:event.felix_courtship
  · effects: flag:courtship.asked.felix

### felix_courtship2 · "Felix Asks Again" · wholesome
conversation · wave5_courtship.json · map Beach · priority 8
plays when: `hearts:felix>=10 && flag:choice.courtship.felix.later && !flag:event.felix_courtship2.done && !flag:partners.felix && !flag:choice.courtship.felix.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:thu && hour>=6 && hour<14`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Second cast, then. You asked for time, and I gave it. Would you court me? Still slowly."  ✏ `event.felix_courtship2.intro`
  · (move player to (11,10))
  · (player faces up)
  · (felix wave (background))
  · (felix emote note)
  ▶ DIALOGUE `felix.courtship` (player-facing graph, plays here):
    n0 [felix]: "What do you say?"  ✏ `dlg.felix.courtship.0`
       ? "Yes. I'd like that." → r0  effects: friend:felix,20, flag:choice.courtship.felix.yes  ✏ `dlg.felix.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:felix,15, flag:choice.courtship.felix.friends  ✏ `dlg.felix.courtship.c1`
       ? "Give me a little time." → r2  effects: friend:felix,5, flag:choice.courtship.felix.later  ✏ `dlg.felix.courtship.c2`
    r0 [felix]: "Yes! Oh. Best catch of my life, and I wasn't even holding a rod."  ✏ `dlg.felix.courtship.r0`   (expr happy)
    r1 [felix]: "Friends. That's a fine thing. The best dawn partners are friends. I'm glad. I'll be a bit quiet today."  ✏ `dlg.felix.courtship.r1`   (expr happy)
    r2 [felix]: "A little time. Good. Fish take time. So does the rest of it."  ✏ `dlg.felix.courtship.r2`   (expr thinking)
  [felix]: "{if flag:choice.courtship.felix.yes}Worth the wait. Every tide of it. Tomorrow I bring the thermos and, for once, two cups.{else}{if flag:choice.courtship.felix.friends}The rock still has room for two. Rocks don't hold grudges. Neither do I.{else}More time, then. I've waited forty years for a partner. I can wait a season.{/if}{/if}"  ✏ `event.felix_courtship2.after`
  · effects: flag:event.felix_courtship2
  · effects: flag:courtship.asked.felix, flag:event.felix_courtship2.done

### felix_partners · "Felix, Together" · wholesome
conversation_prop · wave5_courtship.json · map Beach · priority 8
plays when: `hearts:felix>=10 && flag:choice.courtship.felix.yes && !flag:partners.felix && !weekday:thu && hour>=6 && hour<14`
text-only scene: rewrite lines, do not add or cut beats
placed: player(11, 10)
  · (felix faces down)
  [felix]: "Mornin'. I caught something for you. Not a fish. A shell. A very good one. Hold it to your ear."  ✏ `event.felix_partners.intro`
  · (move player to (11,10))
  · (player faces up)
  · (felix wave (background))
  · (spawn prop prop.shell at (12,10))
  · (felix emote heart)
  ▶ DIALOGUE `felix.partners` (player-facing graph, plays here):
    n0 [felix]: "Tide's out. Morning's ours. What'll it be?"  ✏ `dlg.felix.partners.0`
       ? "Listen to the shell." → r0  effects: friend:felix,10, flag:choice.courtship.felix.shell  ✏ `dlg.felix.partners.c0`
       ? "Fish together at dawn." → r1  effects: friend:felix,10, flag:choice.courtship.felix.fish  ✏ `dlg.felix.partners.c1`
       ? "Watch the sunrise with him." → r2  effects: friend:felix,10, flag:choice.courtship.felix.sun  ✏ `dlg.felix.partners.c2`
    r0 [felix]: "Hear it? That's the sea. It's saying something. I think it's saying your name. It could be 'hello'."  ✏ `dlg.felix.partners.r0`   (expr happy)
    r1 [felix]: "You take the rod. I'll take the thermos. Finest arrangement on the coast."  ✏ `dlg.felix.partners.r1`   (expr happy)
    r2 [felix]: "The sun comes up. We don't say much. That's the whole plan."  ✏ `dlg.felix.partners.r2`   (expr happy)
  [felix]: "{if choice:courtship.felix.shell}The shell sits on my shelf, by the best hook. It still says your name. Or 'hello'.{else}{if choice:courtship.felix.fish}One very small fish, all morning. We threw it back. I've already made it bigger.{else}The sunrise was quiet. So were we. Longest conversation I've ever had.{/if}{/if}"  ✏ `event.felix_partners.after`
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
  [tilda]: "Felix. Your carp is eight coins today. On Monday it was six."
  [felix]: "It's a bigger carp. It grew. In the stall, overnight, out of pride."  ✏ `event.overheard_tilda_felix_1.l2`
  · (felix shrug (background))
  · (felix emote ellipsis)
  · (tilda point (background))
  [tilda]: "It's been in a bucket since dawn. It hasn't grown. Your prices have."
  [felix]: "Fine. Six. The story's two more. It's a very good story."  ✏ `event.overheard_tilda_felix_1.l4`
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
  [felix]: "Tilda. Remember the winter the pond froze, and I fell in?"  ✏ `event.overheard_tilda_felix_2.l1`
  [tilda]: "I remember the winter you fell in. And the Tuesday you fell in again."
  · (tilda shrug (background))
  · (tilda emote ellipsis)
  · (felix wave (background))
  [felix]: "Same winter. A long one. You brought me soup. ...Both times."  ✏ `event.overheard_tilda_felix_2.l3`
  [tilda]: "Soup, a blanket, and 'Thin Ice' on the slate in capitals. You walked straight past it."
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
  [felix]: "Tilda. I chalked the year's first fish on your slate. Capital letters."  ✏ `event.overheard_tilda_felix_3.l1`
  [tilda]: "Oh, Felix. A very good fish. It stays up until the thaw."
  · (tilda point (background))
  · (tilda emote ellipsis)
  · (felix shrug (background))
  [felix]: "I chalked it life-size. Then I added a hand. Don't tell the pond."  ✏ `event.overheard_tilda_felix_3.l3`
  [tilda]: "The pond always knows. Sit down, Felix. I've made tea for two again."
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
  [felix]: "Tilda. I brought a pie, for once. A fish pie. Don't judge it."  ✏ `event.overheard_tilda_felix_4.l1`
  [tilda]: "I'll judge it kindly, Felix. Then I'll salt it."
  · (tilda wave (background))
  · (tilda emote ellipsis)
  · (felix wave (background))
  [felix]: "A fair sentence. Pass the fork. ...And the salt."  ✏ `event.overheard_tilda_felix_4.l3`
  [tilda]: "To old friends, dear. The crust is mine. The fish goes to the cat."
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
  [felix]: "Elara. A fish doesn't want a cozy net. It wants to be free."  ✏ `event.overheard_felix_elara_1.l1`
  [elara]: "Then it's a very loose net! Free and comfortable! Everybody wins!"
  · (elara shrug (background))
  · (elara emote sweat)
  · (felix point (background))
  [felix]: "...It does look warm. Give it here. For science."  ✏ `event.overheard_felix_elara_1.l3`
  [elara]: "I knew you'd love it! Next one's bigger. Big enough for the boat!"
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
  [felix]: "Three feet, nose to tail. Last spring it was two. It grows in the telling."  ✏ `event.overheard_felix_elara_2.l1`
  [elara]: "I made a scarf that big! Same size exactly! They'd be friends!"
  · (elara shrug (background))
  · (elara emote sweat)
  · (felix wave (background))
  [felix]: "A scarf and a fish, friends. I like it. Tell no one."  ✏ `event.overheard_felix_elara_2.l3`
  [elara]: "Too late! I told the whole waiting room!"
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
  [felix]: "Dawn's quiet, yes. Until you count stitches out loud."  ✏ `event.overheard_felix_elara_3.l1`
  [elara]: "Seventeen, eighteen! It isn't counting, it's a rhythm!"
  · (elara point (background))
  · (elara emote sweat)
  · (felix shrug (background))
  [felix]: "...It's soothing. Don't stop. The fish like it."  ✏ `event.overheard_felix_elara_3.l3`
  [elara]: "I won't! Nineteen! Twenty! Fish are the best listeners. They never interrupt."
  · effects: flag:event.overheard_felix_elara_3
