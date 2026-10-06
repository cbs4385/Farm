# Documentation index

| Document | Purpose | Audience |
|---|---|---|
| [`../CLAUDE.md`](../CLAUDE.md) | Entry point and non-negotiable rules for AI agents | agents |
| [`01-GameDesign.md`](01-GameDesign.md) | What we are building: pillars, systems, content guidelines, Steam requirements, the horror layer, open design questions | everyone |
| [`02-TechnicalDesign.md`](02-TechnicalDesign.md) | Architecture as built, extension points, conventions, testing, build, risks | developers, agents |
| [`03-ImplementationPlan.md`](03-ImplementationPlan.md) | Ordered task backlog, Definition of Done, release checklist | agents |
| [`ItemGapAudit.md`](ItemGapAudit.md) | Items, gifts and places that dialogue/events promise but the game lacks | agents |
| [`mythos/M3b-COMPLETION-PLAN.md`](mythos/M3b-COMPLETION-PLAN.md) | What remains of Milestone 3b and how to finish and verify it | agents |
| [`STATUS.md`](STATUS.md) | Task status, what a human verified, open decisions | everyone |
| [`BUILD.md`](BUILD.md) | Setup, test/build commands, QA flags, running players | developers, agents |
| [`QA.md`](QA.md) | Automated checks and the manual smoke checklist | testers, agents |
| [`ASSET_LICENSES.md`](ASSET_LICENSES.md) | Licenses of art, audio, fonts and third-party code | everyone |
| [`ART_ASSETS.md`](ART_ASSETS.md) | Every visual asset still to be created or replaced (placeholders and missing art) | artists, T-060 |
| [`art_prompts/`](art_prompts/README.md) | Text prompts for generating consistent cozy sprite sheets, plus the style guide | artists, T-060 |
| [`adr/0001-m1-design-deviations.md`](adr/0001-m1-design-deviations.md) | Where the build differs from the original design, and lessons learned | developers |
| [`adr/0002-mythos-extension-points.md`](adr/0002-mythos-extension-points.md) | The hooks that let the horror layer plug in, and the rules for them | developers, agents |
| [`NPC_DIALOGUE_ANALYSIS.md`](NPC_DIALOGUE_ANALYSIS.md) | Inventory and critique of NPC dialogue versus highly rated cozy games, with prioritised recommendations | design, writers |
| [`NPC_DIALOGUE_PLAN.md`](NPC_DIALOGUE_PLAN.md) | Accepted plan (T-080 to T-150): NPC dialogue, interaction variety, events and relationships at a professional standard, aimed at being worth streaming | design, agents |
| [`adr/0005-narrative-depth-and-stream-appeal.md`](adr/0005-narrative-depth-and-stream-appeal.md) | Decisions D1-D8 for the narrative plan and the streaming goal | everyone |
| [`narrative/BIBLE.md`](narrative/BIBLE.md), [`STYLE.md`](narrative/STYLE.md), [`FSCRIPT.md`](narrative/FSCRIPT.md), [`EVENTS.md`](narrative/EVENTS.md), [`STREAMING.md`](narrative/STREAMING.md), [`PACING.md`](narrative/PACING.md), [`SLICE_RECALIBRATION.md`](balance/SLICE_RECALIBRATION.md), [`FIRST_HOUR.md`](narrative/FIRST_HOUR.md) (draft), [`BIBLE_WAVE2.md`](narrative/BIBLE_WAVE2.md), [`BIBLE_ENHANCED.md`](narrative/BIBLE_ENHANCED.md), [`LOCALIZATION.md`](narrative/LOCALIZATION.md), [`QA_HEART_EVENTS.md`](narrative/QA_HEART_EVENTS.md), [`storylines/`](narrative/storylines/00_INDEX.txt) | Narrative bible and writing style guide (approved), the FScript authoring format, how to write scenes, and the streaming features | writers, agents |
| [`mythos/LORE.md`](mythos/LORE.md) | Draft lore bible for the horror layer (needs owner decisions) | design |

Keep these in sync: when behaviour or architecture changes, update the relevant document in the same change (Definition of Done in the plan). Decisions that change the design go in a new ADR (`adr/NNNN-title.md`).
