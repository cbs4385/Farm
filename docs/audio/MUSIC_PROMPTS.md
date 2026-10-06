# Music prompts for Suno (T-061, T-133)

**Status (2026-10-06): tracks 1 to 6 (`title`, `farm_spring`, `farm_summer`, `farm_fall`, `farm_winter`, `farm_night`) were generated from these prompts by the owner and are in the game (`Assets/_Project/Resources/Music/`, played by `MusicDirector`; the source files are in `audio/`). Tracks 7 and later do not exist yet.** Prompts for the music the game still lacks. Licence and ownership depend on the Suno plan used: record the plan and each track's generation date in `docs/ASSET_LICENSES.md`, and keep the prompts with the files (as `tools/art` does for art). Do not name artists, games or composers in any prompt (originality rule; Suno also rejects artist names).

## How to use
- Create every track as **Instrumental** (lyrics box: `[Instrumental]`). Put the *Style* line in the **Style of music** box. Where a *Structure* line is given, put those tags in the lyrics box.
- Ask for a **seamless loop**. Suno cannot guarantee one, so generate 2 to 4 versions, keep the one whose last bar leads back into its first, and trim on a bar line in an editor (loops 90 to 150 seconds, stingers 3 to 60 seconds).
- Export WAV if the plan allows, else the highest-rate MP3; the game imports Ogg Vorbis (convert once, keep the master).
- File names are the music cue names that scenes and the audio director will use (the `music` event step, `MusicCue`).

## Shared base (start every Style line with this)
`instrumental, cozy countryside farming game soundtrack, warm acoustic, hand-played, unhurried, gentle dynamics, no vocals, no drum kit unless stated, loopable, clean mix with space for sound effects`

Palette to reuse so the score sounds like one world: nylon-string and steel-string acoustic guitar, upright piano, harmonium or pump organ, fiddle, tin whistle, mandolin, glockenspiel, soft upright bass, brushed snare, hand percussion, warm cello. Keep the keys in the D major, G major and B minor family. Use one short recurring four-note motif (a rising fourth, then a step down) in the title, farm and village themes.

## Core (the cozy game, always available)

### 1. `title`
- Style: base + `main menu theme, hopeful, a sunrise over fields, fingerstyle acoustic guitar leading, upright piano, soft strings swell, 84 BPM, D major, memorable simple melody`
- Structure: `[Intro] [Verse] [Chorus] [Verse] [Chorus] [Outro]` (45 to 75 s, may end instead of looping)

### 2 to 5. `farm_spring`, `farm_summer`, `farm_fall`, `farm_winter` (farm by day; four versions of one theme)
- Shared: base + `farm daytime theme, working in the fields, steady relaxed pulse, 92 BPM, same melody family as the title theme`
- Spring: `bright, fresh, pizzicato strings, glockenspiel, tin whistle, G major`
- Summer: `lazy and sunny, mandolin and acoustic guitar, light brushed percussion, warm, slightly swung, D major`
- Fall: `golden and reflective, cello and fiddle, pump organ, slower (84 BPM), B minor turning to D major`
- Winter: `quiet and snowy, felt piano, celesta, soft sleigh-bell texture, very sparse, 72 BPM, no percussion`

### 6. `farm_night`
- Style: base + `evening and night on the farm, soft pizzicato like crickets, solo nylon guitar and slow piano, calm, 66 BPM, D major, very quiet, nothing sudden`

### 7. `village`
- Style: base + `village square in the daytime, friendly bustle, accordion or harmonium, acoustic guitar, upright bass, light hand percussion, 100 BPM, a small-town waltz feel in 3/4`

### 8. `saloon`
- Style: base + `cozy tavern evening, upright piano and fiddle, a warm slightly tipsy folk tune, brushed snare, stomping feet, 112 BPM, G major, live-room feel, a deliberately cheeky bridge`

### 9 and 10. `library`, `indoors` (shops, clinic, houses)
- `library`: base + `quiet library, solo piano with a soft cello pad, page-turn calm, 60 BPM, very sparse, F major`
- `indoors`: base + `small shop and home interiors, music box and nylon guitar, a tiny gentle melody, 76 BPM, C major`

### 11. `forest`
- Style: base + `a walk in a mossy forest, airy flute, harp-like guitar arpeggios, soft frame drum, curious and light, 88 BPM, A minor to C major`

### 12. `beach`
- Style: base + `seaside afternoon, tenor ukulele-like guitar, light marimba, soft brush on a box drum, relaxed, 96 BPM, A major`

### 13. `mine`
- Style: base + `underground caves, calm but a little tense, low cello and bowed psaltery, dripping pizzicato, soft timpani heartbeat, 76 BPM, D minor, spacious reverb, still gentle enough for a family game`

### 14. `rain` (plays instead of the day theme in rain)
- Style: base + `a rainy day feeling, felt piano and a warm pad, soft guitar harmonics, slow, 62 BPM, F major`

## Festivals and moments

### 15. `festival_spring` (flower festival)
- Style: base + `village spring flower dance, lively folk dance, fiddle, tin whistle, accordion, tambourine, joyful, 118 BPM, D major, ends on a clean final chord`

### 16. `festival_summer` (fishing contest)
- Style: base + `summer lakeside contest, cheerful small brass-band feel, tuba-like bass, banjo, snare march, bouncy, 124 BPM, F major`

### 17. `festival_fall` (harvest fair)
- Style: base + `autumn harvest fair, barn-dance fiddle, stomping and clapping, mandolin, big friendly sing-along energy, 128 BPM, G major`

### 18. `festival_winter` (lantern evening)
- Style: base + `winter lantern festival at dusk, glowing and tender, celesta, harp, warm wordless choir pads, slow waltz, 60 BPM, A major, magical but not sad`

### 19. `heart_event`
- Style: base + `a quiet emotional moment between two friends, solo piano with warm strings, tender, rubato, 58 BPM, B-flat major` (45 to 60 s, no strong ending beat so dialogue can run over it)

### 20. `performance` (a villager sings or plays for the player)
- Style: base + `a short charming performance piece, solo acoustic guitar or fiddle, light and funny, builds to an applause-ready ending, 108 BPM, G major` (20 to 40 s, must end)

### 21 to 23. `stinger_day_end`, `stinger_level_up`, `stinger_quest_done` (3 to 8 seconds each)
- Style: base + `very short musical sting`, then: day end `a gentle falling resolution on piano and strings`; level up `a bright rising glockenspiel and strings flourish`; quest done `a warm two-bar fanfare, tin whistle and guitar`.

## Mythos layer (played only at horror intensity mild or full; at off the game uses the cozy tracks)

Keep these restrained and uncanny, never gory and with no jump scares. The Keepers protect the world, so their music is solemn and ceremonial, not villainous. Use the same acoustic palette as the cozy score, detuned and thinned out, so it sounds like the same world at night. These tracks belong with the horror layer (`Farm.Mythos` and data), not the core assemblies.

### 24 to 26. `wake_01_unease`, `wake_02_watching`, `wake_03_wrong` (mixed over the day themes as wakefulness rises)
- Shared: `instrumental ambient layer, drone, sparse, no beat, no melody that resolves, free time or 50 BPM, loopable, long evolving texture, quiet`
- 01: `a slightly detuned music box and soft pump organ drone, distant bowed glass, a feeling that something is just off`
- 02: `a low cello drone, whispering brushed cymbal, a single slow muted piano note every few seconds, the sense of being watched`
- 03: `a warped, slowed-down gentle folk tune, tape wobble, detuned fiddle, low sub rumble, distant reversed chime, unsettling but quiet`

### 27. `harrow_wood`
- Style: `instrumental dark ambient folk, a deep old wood at night, bowed double bass harmonics, hurdy-gurdy drone, tin whistle far away, sparse frame drum like a heartbeat, 54 BPM, D minor, mist, awe more than fear, loopable`

### 28. `keepers_theme` (Keeper gatherings, meetings in the Community Hall)
- Style: `instrumental, solemn ceremonial chamber music, pipe organ and harmonium, low cello choir without words, slow processional, 50 BPM, A minor, candlelit, dignified and a little menacing, not horror-movie`

### 29. `ritual` (the new-moon ritual in Harrow Wood; 3 to 4 minutes, builds slowly, ends on a held chord)
- Style: `instrumental, a slow-building ritual, frame drum heartbeat entering gradually, harmonium and bowed strings rising, wordless choir pads, bells like a distant church, tension rising to one grand held chord then silence, 48 BPM, D minor, awe and dread, cinematic but organic`
- Structure: `[Intro: silence and wind] [Build: drums enter] [Build: strings rise] [Climax: held chord] [Outro: silence]`

### 30. `nharoth_stirs` (a wakefulness-threshold sting, 10 to 20 seconds)
- Style: `instrumental sting, one enormous low note swelling from silence, sub drone, distant rolling timpani, strings tremolo, resolves into quiet, no screaming, awe`

### 31 to 34. `ending_sealed`, `ending_joined`, `ending_ignored`, `ending_awakened` (one per ending; each plays under its illustration and caption, so 60 to 90 seconds, must end; the cue name is `ending_<ending id>`, published by `MythosEnding.Finish`)
Each ending has its own picture (`Resources/Endings/ending_<id>.png`); the music should match what the picture shows. All four reuse the title theme's four-note motif (a rising fourth, then a step down) so the player hears the same world one last time.
- `ending_sealed` (dawn in the clearing, the relics glowing, Nharoth asleep for good): base + `a hopeful resolution, the title melody returning on solo piano then full strings, a sunrise in a quiet forest, birdsong-like flute, relief and gratitude, gentle and full, 72 BPM, D major, ends on a long warm chord`
- `ending_joined` (the moonlit circle of hooded Keepers, the player chosen): `instrumental, solemn and tender chamber music, harmonium and pipe organ, low cello choir without words, a slow processional in 3/4, the title motif played slowly and in a minor key, bells like a distant church, candlelight and moon, belonging and duty rather than menace, 54 BPM, A minor resolving to A major at the very end, ends on a long held chord`
- `ending_ignored` (a cozy sunset farm, a faint strange light in the wood that nobody notices): base + `a warm golden-hour farm theme, fingerstyle guitar and upright piano, the title melody played sweetly and contentedly, and then, in the last twenty seconds, one faint detuned music-box note and a low drone slip in underneath and are not resolved, sweet with a very faint uneasy hint, 80 BPM, G major, ends on an open unresolved chord`
- `ending_awakened` (the sky burns, Nharoth rises; the world ends): `instrumental, the title melody broken apart and slowed down, low pipe organ, rolling timpani and frame drums, embers and wind, strings tremolo swelling to a vast chord, then thinning to a single fading piano note, tragic grandeur and awe, 46 BPM, D minor, no screaming, no choir shrieks, ends on a long fading note` (sad and awe-struck, not gory)

At mild the same pieces play; none contains explicit ritual imagery or harsh sounds.

## After generating
1. Pick, trim and loop-check each track; keep the master with its Suno prompt and date.
2. Name files after the cue (`farm_spring.ogg` ...) and put them under `Assets/_Project/Audio/Music/` once the audio pass (T-061) defines the layout.
3. Update `docs/ASSET_LICENSES.md` (source, plan, date) before any track is committed.
4. Listen for melodies that resemble an existing piece and regenerate those rather than ship them.
