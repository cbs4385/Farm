# Sprite generation prompts

Detailed text prompts for Unity's sprite generator, one file per asset family from `docs/ART_ASSETS.md`. They share one style so that every sheet looks like it belongs to the same cozy game.

## Files
| File | Covers |
|---|---|
| `STYLE_GUIDE.md` | The style rules, palette, camera, proportions, do and don't, the consistency checklist |
| `01_tiles.md` | Tiles and buildings (16 sheets) |
| `02_characters.md` | Player and villagers (17 sheets) |
| `03_portraits.md` | Portraits (12 sheets) |
| `04_crops.md` | Crops (6 sheets) |
| `05_items.md` | Item icons (14 sheets) |
| `06_objects.md` | World objects (5 sheets) |
| `07_animals_and_monsters.md` | Animals and monsters (14 sheets) |
| `08_ui.md` | User interface (9 sheets) |
| `09_effects.md` | Effects and lighting (3 sheets) |
| `10_harrow_wood.md` | Harrow Wood (horror layer) (8 sheets) |
| `11_key_art.md` | Logo, title and store art (2 sheets) |

HOW TO USE (every file in this folder)
1. Open the Unity sprite generator, choose a pixel-art style/model if offered, and paste the whole PROMPT block of a sheet (the STYLE BLOCK is already included; do not remove it, consistency depends on it).
2. Generate at a large multiple of the final size (the "Generate size" line, 8x to 16x the final pixel grid), because generators cannot hit an exact 16 px grid.
3. Reduce with nearest-neighbour to the "Final cell size", snap colours to the shared palette, and clean stray pixels by hand.
4. Slice the sheet into the named cells (the CELL ORDER line is left to right, top to bottom) and export one PNG per name into `Assets/_Project/Art/Placeholders/` replacing the file of the same name (see `docs/ART_ASSETS.md`), then run `Farm/Setup/Generate Content` and rebuild the atlases.
5. Review the finished sheet beside the earlier sheets: same palette, same outline colour, same light direction. Re-roll anything that drifts; do not fix drift by changing the style block.

## Order of work
Do `01_tiles`, `02_characters` and `04_crops` first and lock the look; generate the rest by comparing against them. Then `05_items`, `06_objects`, `08_ui`, `03_portraits`, `07_animals_and_monsters`, `09_effects`, then `10_harrow_wood` and `11_key_art`.
