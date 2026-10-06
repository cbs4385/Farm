# Item gap audit (2026-10-06)

Method: every item id used in story data (`Resources/Story/*.json`, `Resources/Mythos`, effects in code) was checked against `Data/Items` (195 items): **no dangling ids** (the `mythos.*` hits are flags/variables, not items). The gaps below are in the *prose*: things dialogue, events and letters say the player is given or can use, that the game does not provide.

## Built in this pass
- Parcels: items that do not fit the backpack wait in the mailbox (`Parcels`, effect `parcel:<item>[,n]`).
- Seats kept by villagers (General Store/Tilda, Saloon/Wren, Library/Ione): `SeatSpot`, restores energy once a day.
- Library desk hands over Ione's almanac (3 hearts) and field guide (10 foraged), not only the slim book.

## Promised but not given (prop art exists in `Art/Placeholders/item_prop_*`, no item definition or event reward)
| Prop | Where the text promises it | Today |
|---|---|---|
| carving | `memory.marcus_heart5` "The Boyhood Carving" | event gives `fertilizer.speed` |
| charm | `event.felix_heart5.0`, memory "The Lucky Charm" | event gives bait |
| feather | `event.dorian_partners.intro` ("a feather, a stick, a bit of twine. Hold it.") | flags only |
| hat | `event.elara_partners.intro` (the hat with a hat) | flags only |
| sock | Elara: `fr.4`, `gift.repeat`, `wool.ask`, `partners.talk0`, missed-birthday letter ("It's always a sock") | nothing is ever sent |
| ribbon | `festival.fall.join.r0_0` (ribbon pinned to your hat), Bram's twelve ribbon pins | no item |

## Promised, no art and no item
- Juno's tiny anvil (`event.juno_partners.intro`; the event shows `prop.trophy` instead) and the clasp "for you" (`dlg.juno.co.2.0`, `dlg.juno.cl.6.0`).
- Bram's "I made you something" (`dlg.bram.fr.5.0`); heart 10 gives a gold bar.
- Dorian's pinecone, and the tiny drawing after the notebook event.
- Odalys's salve (`dlg.odalys.thu.1.0`), Elara's scarf/blanket, Ione's "very long book" (winter) and "very dull book" (tease).

## Places and objects mentioned but absent
Ione's cushioned chair (`dlg.ione.partners.talk0.0`), Hazel's nook chair and kept shelf, Felix's good rock, Elara's couch with blanket, Tilda's cat door, Wren's window lantern, Ione's back shelf of unclaimed books.

## Suggested order
1. Wire the six existing props as event rewards (cheap: `give:prop.x` plus item definition, strings already exist in prose).
2. Elara's sock: a repeatable mailbox parcel after gifts (use `parcel:`).
3. Juno/Bram/Dorian handmade gifts need new art.
4. Places: reuse `SeatSpot` for Ione/Hazel/Felix/Elara once partnered or at friend hearts.
