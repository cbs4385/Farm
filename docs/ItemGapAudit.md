# Item gap audit (2026-10-06): all items resolved

Method: every item id used in story data (`Resources/Story/*.json`, `Resources/Mythos`, effects in code) was checked against `Data/Items`: **no dangling ids** (the `mythos.*` hits are flags and variables, not items). The gaps were in the *prose*: things dialogue, events and letters say the player is given or can use, that the game did not provide. Every one of them is now built (automated checks only; a person has not played any of it).

## What was built, and where the text promised it
| Promise | Built as |
|---|---|
| Items that do not fit the backpack are lost | `Parcels`: they wait in the mailbox (effect `parcel:<item>[,n]`) |
| Tilda's window seat, Wren's good stool, Ione's chair by the window, Hazel's nook chair, Elara's couch with a blanket, Felix's good rock | `SeatSpot` objects (General Store, Saloon, Library, Clinic, Beach): sit for a little energy, once a day; the words name who kept it once you are friends |
| Tilda's very small cat door | `Curio` in the General Store |
| Ione's back shelf of books nobody came for, Hazel's shelf "kept for you", Wren's window lantern | `Curio` objects in the Library (two shelves) and the Saloon (the lantern), with their lines |
| Ione sets books aside | Library desk (`LibraryDesk`): the slim book, the almanac (3 hearts), the field guide (10 foraged), the very long book (winter, 2 hearts) and the **very dull book** (after a great tease; the social effect now leaves a flag `social.<npc>.<action>.<outcome>`) |
| Marcus's boyhood carving, Felix's lucky charm | rewards of their heart-5 events (`prop.carving`, `prop.charm`) |
| Dorian's feather and pinecone, tiny drawing | partners event reward; a letter at 4 hearts; a letter after the notebook event |
| Elara's hat with a hat, socks, scarf | partners event reward (`prop.hat`); the missed-birthday letter, a friend letter at 4 hearts and the wool quest reward all give a sock (`prop.sock`; "a sock, always"); a scarf letter at 6 hearts |
| The harvest fair ribbon (and Bram's ribbon pins) | joining the fall contest gives `prop.ribbon`; Bram's pins are the ribbons Tilda pins on |
| Juno's tiny anvil and clasp | the partners event shows and gives `prop.anvil`; a letter at 6 hearts gives `prop.clasp` |
| Bram's "I made you something" | a letter at 5 hearts gives `prop.horseshoe`; the heart-10 heirloom is the gold bar he poured ("Take it") |
| Odalys's salve | a letter at 3 hearts gives `prop.salve` |

Art: the 15 new prop and object sprites were made with `tools/art/generate_sheet.py` (sets `keep`, `gifts`, `places`, `dull`, `wide`), except the wide shop counter, which is drawn by `tools/art/build_counter.py` so that neighbouring counters join up.

## Decisions taken while resolving
- Handmade gifts that were promised in dialogue but have no event of their own arrive by letter at a friendship level, so they never depend on the player standing in the right place.
- Items that a letter or event gives that do not fit the backpack wait in the mailbox (`GiveItem` sends the rest to `Parcels`).
- The shipping bin and the mailbox are still not movable with the builder's mallet (not an audit item).

## Not an audit item but worth a look by a person
Whether the new Curio lines read well in context, and whether Elara's three socks are too many (the quest, the friend letter and the missed birthday).
