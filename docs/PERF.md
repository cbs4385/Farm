# Performance (T-064)

Budgets (Tech Design section 10): 60 fps on a mid-range PC, no per-frame allocations in gameplay loops.

## Measured (Windows release build, Mono, 1280x800, vsync on, development machine; `-farmCapture` `[Perf]` line, 102 frames each)
| Scene | avg frame | fps | max frame | gen-0 GCs | managed delta |
|---|---|---|---|---|---|
| Farm | 17.5 ms | 57 | 33 ms | 0 | +660 KB (scene load) |
| Village | 17.5 ms | 57 | 34 ms | 0 | +708 KB |
| Woods | 17.7 ms | 57 | 33 ms | 1 | -312 KB |
| Forest | 17.6 ms | 57 | 33 ms | 0 | +672 KB |

The averages sit at the vsync interval (the capture includes scene start-up, which explains the 33 ms maximum), so the build is vsync-bound, not CPU-bound, in every scene including the busiest (the village with its twelve walkers). No scene allocates per frame.

Automated guards: the mine generator speed test and the two-year / one-year soak tests in `Milestone3Tests` and `Milestone4Tests` (they would time out on a quadratic day cycle).

## Not measured
Steam Deck and other low-end hardware, Linux GPU frame times, IL2CPP builds, memory on long sessions. Do these on the release candidate (see QA.md, Steam Deck checklist).
