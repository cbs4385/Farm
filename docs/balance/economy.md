# Economy snapshot (T-066)

Generated from `CropDefaults.cs`. Profit per day = (sell - seed) / (days to mature, plus regrowth for the first harvest only). Gold is per crop at normal quality; the day count excludes the planting day.

| Crop | Season | Days | Regrow | Seed | Sell | Profit | Profit/day |
|---|---|---|---|---|---|---|---|
| parsnip | Sp | 4 | 0 | 20 | 35 | 15 | 3.8 |
| potato | Sp | 6 | 0 | 50 | 80 | 30 | 5.0 |
| cauliflower | Sp | 12 | 0 | 80 | 175 | 95 | 7.9 |
| greenbean | Sp | 10 | 3 | 60 | 40 | -20 | -2.0 |
| strawberry | Sp | 8 | 4 | 100 | 120 | 20 | 2.5 |
| kale | Sp | 6 | 0 | 70 | 110 | 40 | 6.7 |
| tomato | Su | 11 | 4 | 50 | 60 | 10 | 0.9 |
| melon | Su | 15 | 0 | 80 | 250 | 170 | 11.3 |
| blueberry | Su | 13 | 4 | 80 | 50 | -30 | -2.3 |
| pepper | Su | 6 | 3 | 40 | 40 | 0 | 0.0 |
| radish | Su | 6 | 0 | 40 | 90 | 50 | 8.3 |
| hops | Su | 11 | 1 | 60 | 35 | -25 | -2.3 |
| cucumber | Su | 10 | 3 | 60 | 50 | -10 | -1.0 |
| pumpkin | Fa | 13 | 0 | 100 | 320 | 220 | 16.9 |
| eggplant | Fa | 9 | 5 | 20 | 60 | 40 | 4.4 |
| cranberry | Fa | 7 | 5 | 240 | 130 | -110 | -15.7 |
| yam | Fa | 10 | 0 | 60 | 160 | 100 | 10.0 |
| beet | Fa | 6 | 0 | 20 | 100 | 80 | 13.3 |
| artichoke | Fa | 8 | 0 | 30 | 160 | 130 | 16.2 |
| tree_cherry | Sp | 28 | 0 | 350 | 80 | -270 | -9.6 |
| tree_peach | Su | 28 | 0 | 400 | 90 | -310 | -11.1 |
| tree_apple | Fa | 28 | 0 | 400 | 90 | -310 | -11.1 |

## Reading the table
- Crops that regrow (Regrow > 0) show a negative or low first-harvest profit by design: they pay back over repeated harvests (one every Regrow days for the rest of the season), which the single-harvest column does not capture. The best early crop earns roughly 2-6 gold/day/tile, so a starting 500 gold plus the first season's harvest funds the first tool upgrades by early summer, matching the intended pacing.
- Tool, backpack and energy upgrades (`UpgradeDefaults`) cost 2,000-25,000 gold: reachable in year 1 for the first tier, year 2-3 for the last.
- Horror crops (`MythosData`) sell for 400-1,200 but only grow at high dread; they are an optional income, not required.

## Quality-of-life options (T-066)
- **Relaxed energy** halves every energy cost (minimum 1). **Day length** long/normal/short changes seconds per ten game minutes to 10/7/5.
- Defaults are unchanged; both live in Options > Gameplay and are read live.

## Not yet verified
No person has played a full year to judge pacing; these numbers are from the data tables and the automated one-year soak (which checks stability, not fun). Treat as a starting point for the human balance pass.
