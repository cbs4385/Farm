# Brief: Opening Night (the Hall Dance), table read and expansion

Scene id (new, for the expanded version): `hall_dance` is shipped and stays; the room drafts `hall_dance_v2`, which the owner may merge over it before 1.0 since nobody has played it.
Map: `CommunityHall` (interior 14x10, floor x1-12 y1-8, door at the bottom (7,0)-(8,0), player spawns at (7,2)). Solid: notice board (7,7), two tables (3,4) and (10,4), shelves (2,8) and (11,8), and the restored rooms' fittings: pantry (1,5)-(1,6), fish tank (12,5)-(12,6), hearth (4-5, 7-8), bulletin (9,8) and (12,2), vault (10,8) and (1,2), roof ladder (7,8). The crafts rugs (6,3) and (8,3) are walkable.
Time: an evening, 18:00 to 23:00, any season, clear. Horror level: **0** (this is the cozy climax; nothing may hint).
Lighting warm, letterbox on. Music: the game has no "piper_theme"; `saloon` is the nearest cue, or silence while Piper plays and the hall sings.

What is true (flags): `hall.restored`, `hall.roof` (Marcus finished the roof with the player's planks), `quest:wren_full_house=done` (the player brought Wren the wine she asked for, for "a full house, a real one, music, food, nobody in a corner"), `event.piper_heart10` (the saloon sang Piper's song without her; the player chose the chorus word: `choice:chorus.home`, `choice:chorus.road` or neither = "room"), `event.juno_heart10` (Juno's first piece with her own mark; she also made the hall door's hinges), `event.bram_heart10` (Bram said the player's name for the first time), `event.wren_heart8` (the player has seen Wren's packed bag and chose `choice:wren.bag.go`, `.stay` or `.listen`; treat all three as possible with `{if}`), `event.marcus_heart8` (the roof plan). Hearts: everyone present at 8 or more. Everyone has met everyone.

Present, with stage positions: Marcus (8,6) facing up; Tilda (5,5) facing right; Wren (7,5) facing down; Piper (6,4) facing down; Bram (9,5) facing left; Juno (8,4) facing down; the player walks in to (7,2) facing up. Others (Hazel, Ione, Dorian, Felix, Elara, Odalys) are "on the porch": off screen, may be referred to, do not speak.

What the scene must close (docs/NPC_DESIGN_REVIEW_2026-10-16.md 2.1, 2.2, 5.3): Wren's want (a full room and a story to tell about it: this is the full house she asked the player for), Piper's want (a song the village sings without her), Marcus's want (the roof, finished, under a building that outlives him), Juno's bell/hinges on the door, Bram humming, Tilda's slate. Memory title "Opening Night". Tag: wholesome.

Shape wanted (docs/narrative/STYLE.md 8): setup, turn, payoff; 25 to 45 spoken lines (the shipped draft has 13); one player choice with 2 to 4 options, kind/honest/playful/shy/curt, never wrong, each setting `choice.hall_dance.<name>`; end on a spoken line that works as a clip. Under 150 seconds.

Known problem: line 11 is given to Marcus but opens with "Hm.", which is Bram's tic.
