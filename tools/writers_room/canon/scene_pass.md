# The scene pass: brief for the villagers (cozy layer)

Every story scene in *Wetherell Farm Saga* is being rewritten line by line by the villager who says it. The result is accepted as written: there is no later editor. You are the writer of your own lines. Make them count.

## What the player should feel

Warm first, funny second, strange third. And curious. After every scene the player should want one more thing: to talk to someone they have not talked to, to go somewhere, to wait for a particular day, to find out what was almost said. The cozy game has to stand alone at horror off, and it must also seed the questions the horror layer answers later, without ever hinting at it (see "Cozy only").

## Craft: what "realistic and compelling" means here

1. **Know what you want in the scene.** Before you touch a line, decide what your villager wants from the people in the room right now and what they are steering around. Lines serve that. A line that only delivers information is not finished.
2. **Subtext over statement.** People do not explain their feelings. Bram says the roof is good; Wren says the porch is full; Hazel corrects a number. Let the line mean one thing and the room understand another.
3. **Specific beats general.** A number, an object, a day of the week, a smell, a half-remembered name. "The returns box is sticky again" beats "the library is old".
4. **Talk like people.** Interrupt yourself. Trail off. Answer the question you wish you had been asked. Repeat a word the other person said. Mishear on purpose. Let one person be funnier and the other warmer. Not everyone is quippy; silence and a gesture are lines.
5. **Leave a thread.** Each scene ends with something unfinished that points forward, in character and cozy-safe: an unanswered question, a person or place half-named, an object that should not be there, a promise with a day attached ("Thursday. Not before."), a thing noticed and not understood, a request to see someone else. The thread must belong to your villager's own want or secret (your bible), so it reads as character, not plot, and it must point at something that exists in the game (see the list of threads). Do not resolve it unless the scene is its payoff.
6. **Callbacks are currency.** Refer to what other villagers do, what happened in earlier scenes of the same arc, the season, the farm. Only refer to things you can see in the cards or in your bible.
7. **Distinct mouths.** No two villagers share a joke shape. Avoid the shapes already overused: the "I'd not write a letter, I'd X instead" confidant joke (only Bram and Juno keep it); a "Thank you, [player]" followed by an in-voice tail; "the warmest room in Wetherell" (that is the forge; Wren may claim the saloon and be corrected); "I leave a lamp on for whoever is out" (Tilda's); "the door argues with the wind". Do not start two lines in a row with the same word.
8. **Specificity of voice.** Your persona file lists your voice rules and the lint limits (longest sentence, exclamation marks, banned phrases). Follow them exactly.

## Mechanics you must keep

- Do not change: ids, text keys, the speaker of a line, the order of beats, who is in the scene, flags, effects, items given, quantities, days, names, places, or the *meaning* of a choice option (its flag name tells you what it means: `choice.wren.bag.go` is "encourage the trip").
- Keep every `{if ...}` / `{else}` / `{/if}` branch, and what each branch does; you may reword inside a branch. Keep `[player]`, `[farm]` and other `[tokens]`, and `{pause=0.4}` / `{speed=slow}` cues (move them if the words move). `{a|b}` picks one at random: keep it only if you want that.
- Lengths: a scene line is at most **40 words** (aim for 25 or fewer); a choice label at most **9 words**; overheard-scene lines at most 20. No line may need more than three lines in the text box. One idea per line; the funniest or truest word goes last.
- American English. No swearing, brands, internet slang, pop-culture quotes. The player has no gender or past: "you"; never "sir", "ma'am", "lad", "lass". Use `[player]` rarely (about one line in five at most).
- Choice labels: 2 to 4 options in a dialogue, each a different attitude (kind, honest, playful, shy, curt), never a wrong answer. Keep each option's meaning. The first is the warmest and the last the funniest.
- Cozy only. This round contains no horror. Cozy text never denies, explains or hints at the cult, the ritual or the woods' secret. Keepers (Tilda, Marcus, Dr. Penn, Wren, Dorian) are completely warm; their only tells are the ones your bible's mythos note allows. Hazel's pattern-spotting stays about weather, birds and the almanac. Unaware villagers (Bram, Juno, Piper, Felix, Elara, Ione) may notice village oddness and not explain it.
- Narration lines (marked `[narration]`, owned by the scene's lead speaker) describe only what the player could see: third person, present tense, no thoughts, and only actions the engine can show (gestures `hop`, `jiggle`, `nod`, `sway`, `dance`, poses `wave`, `sit`, `shrug`, `point`, a turn, an emote, a prop, light, music, sound). No hugs, bows, tears, laughter, or two people dancing together.

## Adding and cutting beats (only in scenes the card marks "may be ADDED or CUT")

Several early heart events (hearts 2 and 5) are three-line stubs. In scenes the card marks as editable you may add up to **four** extra spoken lines for speakers already in the scene, and cut a line that does not earn its place. Rules: the scene still ends on a spoken line that works alone; added lines keep the scene's flag and effects untouched; do not add choices, movement, props or new characters; say where each line goes ("after `event.bram_heart2.1`" or "START"). Scenes marked "text-only" take rewrites only.

## Threads that exist in the game (safe to point at)

People and their wants (from the bibles): Bram's unsent letters to his old teacher, and the word he is waiting for; Juno's own mark and her own forge ("Next door"); Wren's packed bag under the bar, the sea, her bet to get a fourth word out of Bram; Hazel's poems, and the lyrics she writes for Piper in secret; Piper's song the village sings without her, and the four notes she hums when she thinks she is alone; Elara's map behind the kettle, her list of places, a boat; Felix and the boat that leaks politely, the dawn he wants a partner for; Marcus's hall roof, "nearly fixed"; Tilda's slate of kindnesses, and the lamp; Dr. Penn's "a village that rests"; Ione's almanacs, the sea in winter, a reading night; Dorian's notebook and pressed leaves, the quiet he wants to share.
Places and things: the Community Hall and its roof, the library's sticky returns box and the reading nook, the forge's warmth, the saloon's good stool, the lanterns on the first of the month, the festival each season, birthdays, the pie feud, the notes left anonymously, the cat with five names, the whistler at dusk.
The farm's own mysteries (cozy-safe, all from the Edmund topics): Edmund Fenn, the player's late great-uncle, farmed the place for forty years. The cellar door he would not let Marcus oil. The tiny brass key Juno made for "a box he'd lost the lock to". "He'd put his affairs in order" (Dr. Penn). The almanac margins that stop in the fall (Ione). Dorian saying Edmund was "counting the trees". The four notes he hummed (Piper). Tilda wrote the welcome letter. The woods: dogs that will not take the forest path, a gate that stays shut, birds quieter in the fog.

Use these. Point at the one that fits your villager and the moment; do not use more than one per scene; never explain it.

## What to hand back

Write one JSON file (path given in your task), valid JSON, UTF-8, exactly this shape:

```json
{
  "villager": "bram",
  "lines": {
    "event.bram_heart2.0": { "text": "the new line", "why": "one short clause: what it does now" }
  },
  "adds": [
    { "scene": "bram_heart2", "after": "event.bram_heart2.1", "text": "…", "emote": null, "expression": null, "why": "…" }
  ],
  "cuts": [ { "key": "event.some.key", "why": "…" } ],
  "threads": { "bram_heart2": "the thread this scene leaves, one sentence" },
  "notes": [ "anything the Director must know: canon problems, a line of someone else's you cannot work with, a request to another villager" ]
}
```

- `lines` holds only the keys you own (marked ✏) that you changed. Omit lines you keep. Keys must be copied exactly from the cards.
- `adds[].after` is a text key from the same scene, or `"START"`. `emote` / `expression` are optional engine words (`heart note sweat exclaim question ellipsis sparkle zzz`; `neutral happy sad surprised embarrassed thinking`).
- `threads` has an entry for every scene in which you have the last line or the most lines: what the player is left wondering.
- Everything in `text` is exactly what the game will show, with markup.
- Your final reply to the Director is at most 120 words: counts, and the three notes that matter most. The JSON file is the deliverable.
