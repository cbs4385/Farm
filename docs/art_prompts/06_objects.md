# Sprite prompts: World objects

Read `STYLE_GUIDE.md` first. Every prompt below already contains the shared STYLE BLOCK. Sheet sizes follow `docs/ART_ASSETS.md`; file names are the placeholder names to replace (cell names for animation frames are new: the game does not animate yet).

## Farm, village and mine objects

- **Final cell size:** 16 x 16 px  |  **Grid:** 7 columns x 5 rows  |  **Generate size:** 1344 x 960 px (7 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `obj_tree`, `obj_stump`, `obj_rock`, `obj_boulder`, `obj_weed`, `obj_bin`, `obj_bed`, `obj_mailbox`, `obj_trough`, `obj_scarecrow`, `obj_sprinkler1`, `obj_sprinkler2`, `obj_sprinkler3`, `obj_chest`, `obj_keg`, `obj_jar`, `obj_furnace`, `obj_kitchen`, `obj_shop`, `obj_counter`, `obj_shelf`, `obj_table`, `obj_stall`, `obj_board`, `obj_bramble`, `obj_copper_node`, `obj_iron_node`, `obj_gold_node`, `obj_coal_node`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Farm, village and mine objects. A sprite sheet laid out as a strict grid of 7 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one placeable object on a transparent background, sitting on its base line, 16x16 unless noted. Content in order:
- obj_tree: a round leafy oak tree, 16x32, trunk base bottom centre
- obj_stump: a cut tree stump with rings
- obj_rock: a small grey rock
- obj_boulder: a big mossy boulder (2x2 tiles)
- obj_weed: a tuft of weeds with a yellow flower
- obj_bin: a wooden shipping bin with an open lid
- obj_bed: a cozy bed with a patchwork quilt (1x2 tiles)
- obj_mailbox: a blue mailbox on a post with a red flag
- obj_trough: a wooden feed trough with hay
- obj_scarecrow: a straw scarecrow on a pole
- obj_sprinkler1: a small copper sprinkler on the ground
- obj_sprinkler2: a brass sprinkler with a wider head
- obj_sprinkler3: an advanced sprinkler with a violet glass dome
- obj_chest: a closed wooden chest
- obj_keg: an upright oak keg with a tap
- obj_jar: a large preserves jar with a cloth lid
- obj_furnace: a stone furnace (idle)
- obj_kitchen: a cottage stove with a pot
- obj_shop: a market stall counter
- obj_counter: a wooden shop counter segment
- obj_shelf: a shelf of jars and books
- obj_table: a round wooden table with a cloth
- obj_stall: a fishmonger's stall with a striped teal awning and a crate of fish
- obj_board: a notice board with pinned papers
- obj_bramble: a thick wall of thorny bramble, tidy
- obj_copper_node: a rock with copper-orange veins
- obj_iron_node: a rock with pale iron veins
- obj_gold_node: a rock with gold glints
- obj_coal_node: a rock with black coal veins
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Forage on the ground

- **Final cell size:** 16 x 16 px  |  **Grid:** 7 columns x 2 rows  |  **Generate size:** 1344 x 384 px (7 columns x 2 rows)
- **Background:** transparent
- **Cell order:** `obj_blackberry`, `obj_clam`, `obj_dandelion`, `obj_elderflower`, `obj_hazelnut`, `obj_mushroom`, `obj_pearl`, `obj_raspberry`, `obj_seashell`, `obj_snowdrop`, `obj_truffle`, `obj_wildgarlic`, `obj_winterroot`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Forage on the ground. A sprite sheet laid out as a strict grid of 7 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is the plant or find as it lies in the world, small and charming, bigger and clearer than the inventory icon:
- obj_blackberry: a cluster of dark purple berries
- obj_clam: a fan-shaped clam shell
- obj_dandelion: a yellow dandelion flower
- obj_elderflower: a white elderflower umbel
- obj_hazelnut: two brown hazelnuts
- obj_mushroom: a brown-capped forest mushroom
- obj_pearl: a small shimmering pearl
- obj_raspberry: three red raspberries
- obj_seashell: a pink spiral seashell
- obj_snowdrop: a white drooping snowdrop flower
- obj_truffle: a lumpy dark truffle
- obj_wildgarlic: wild garlic leaves with white star flowers
- obj_winterroot: a knobbly pale root with frosty tips
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Seasonal trees and states

- **Final cell size:** 16 x 32 px  |  **Grid:** 5 columns x 4 rows  |  **Generate size:** 960 x 1536 px (5 columns x 4 rows)
- **Background:** transparent
- **Cell order:** `obj_tree_spring_a`, `obj_tree_spring_b`, `obj_tree_spring_chopped`, `obj_tree_spring_stump`, `obj_tree_spring_sapling`, `obj_tree_summer_a`, `obj_tree_summer_b`, `obj_tree_summer_chopped`, `obj_tree_summer_stump`, `obj_tree_summer_sapling`, `obj_tree_fall_a`, `obj_tree_fall_b`, `obj_tree_fall_chopped`, `obj_tree_fall_stump`, `obj_tree_fall_sapling`, `obj_tree_winter_a`, `obj_tree_winter_b`, `obj_tree_winter_chopped`, `obj_tree_winter_stump`, `obj_tree_winter_sapling`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Seasonal trees and states. A sprite sheet laid out as a strict grid of 5 columns by 4 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Rows: spring (fresh green with blossoms), summer (deep green), fall (orange and gold), winter (bare branches with snow). Columns: oak variant A, oak variant B, tree toppling frame, stump, young sapling.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Object states

- **Final cell size:** 16 x 16 px  |  **Grid:** 6 columns x 6 rows  |  **Generate size:** 1152 x 1152 px (6 columns x 6 rows)
- **Background:** transparent
- **Cell order:** `state_keg_0`, `state_keg_1`, `state_keg_2`, `state_keg_3`, `state_keg_4`, `state_keg_5`, `state_jar_0`, `state_jar_1`, `state_jar_2`, `state_jar_3`, `state_jar_4`, `state_jar_5`, `state_furnace_0`, `state_furnace_1`, `state_furnace_2`, `state_furnace_3`, `state_furnace_4`, `state_furnace_5`, `state_bin_0`, `state_bin_1`, `state_bin_2`, `state_bin_3`, `state_bin_4`, `state_bin_5`, `state_door_0`, `state_door_1`, `state_door_2`, `state_door_3`, `state_door_4`, `state_door_5`, `state_node_0`, `state_node_1`, `state_node_2`, `state_node_3`, `state_node_4`, `state_node_5`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Object states. A sprite sheet laid out as a strict grid of 6 columns by 6 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Rows: keg (idle, working with bubbles, ready with a green tick glow), jar (idle, working, ready), furnace (idle, lit, smelting with bright glow, ready), shipping bin (closed, open), wooden door (closed, ajar, open, locked with a padlock), ore node (full, hit, cracked, depleted rubble). Six columns where fewer states exist leave the remaining cells empty.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Interior furniture and decor

- **Final cell size:** 16 x 16 px  |  **Grid:** 6 columns x 3 rows  |  **Generate size:** 1152 x 576 px (6 columns x 3 rows)
- **Background:** transparent
- **Cell order:** `deco_rug`, `deco_lamp`, `deco_window`, `deco_plant`, `deco_clock`, `deco_painting`, `deco_stool`, `deco_fireplace`, `deco_bookshelf`, `deco_curtain`, `deco_barrel`, `deco_crate`, `deco_basket`, `deco_banner_festival`, `deco_flower_pot`, `deco_hay_bale`, `deco_fence_wood`, `deco_fence_corner`, `deco_signpost`, `deco_lantern_post`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Interior furniture and decor. A sprite sheet laid out as a strict grid of 6 columns by 3 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Cozy decor props: patterned rug (2x2), a floor lamp with warm glow, a window with checked curtains, a potted plant, a mantel clock, a framed painting, a stool, a stone fireplace with fire (2x2), a bookshelf, curtains, a barrel, a crate, a woven basket, a festival bunting banner, a flower pot, a hay bale, a wooden fence segment, a fence corner, a village signpost, a lantern post.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```
