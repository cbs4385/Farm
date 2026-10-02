# Sprite prompts: Harrow Wood (horror layer)

Read `STYLE_GUIDE.md` first. Every prompt below already contains the shared STYLE BLOCK. Sheet sizes follow `docs/ART_ASSETS.md`; file names are the placeholder names to replace (cell names for animation frames are new: the game does not animate yet).

## Harrow Wood tiles

- **Final cell size:** 16 x 16 px  |  **Grid:** 4 columns x 2 rows  |  **Generate size:** 768 x 384 px (4 columns x 2 rows)
- **Background:** solid, as described
- **Cell order:** `tile_wood_floor`, `tile_wood_track`, `tile_wood_clearing`, `tile_wood_roots`, `tile_wood_moss`, `tile_wood_stones`, `tile_wood_ferns`, `tile_wood_edge`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Harrow Wood tiles. A sprite sheet laid out as a strict grid of 4 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Harrow Wood is still drawn in the same cozy storybook style and palette: it is an old, quiet, mossy wood that feels uncanny only through colour (cooler teal, dusk blue and lavender tones, long soft shadows, a few pale fireflies). Nothing is gory or graphic; no skulls, blood or monsters. Mood: hushed, mysterious, gently eerie.
Tileable ground tiles: a dark mossy floor, a pale worn dirt track, a smooth trodden clearing around the altar with faint pale ring marks, twisting roots, thick moss, flat stones, ferns, and a dark treeline edge.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Harrow Wood trees and plants

- **Final cell size:** 16 x 32 px  |  **Grid:** 5 columns x 2 rows  |  **Generate size:** 960 x 768 px (5 columns x 2 rows)
- **Background:** transparent
- **Cell order:** `wood_tree_1`, `wood_tree_2`, `wood_tree_3`, `wood_tree_4`, `wood_tree_5`, `wood_bush_1`, `wood_bush_2`, `wood_bush_3`, `wood_bush_4`, `wood_bush_5`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Harrow Wood trees and plants. A sprite sheet laid out as a strict grid of 5 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Harrow Wood is still drawn in the same cozy storybook style and palette: it is an old, quiet, mossy wood that feels uncanny only through colour (cooler teal, dusk blue and lavender tones, long soft shadows, a few pale fireflies). Nothing is gory or graphic; no skulls, blood or monsters. Mood: hushed, mysterious, gently eerie.
Tall old gnarled trees with thick mossy trunks and lavender-tinted deep green canopies (five variants), and five low bushes, fern clumps and glowing-mushroom clusters (small).
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## The altar and clearing objects

- **Final cell size:** 32 x 32 px  |  **Grid:** 4 columns x 3 rows  |  **Generate size:** 1536 x 1152 px
- **Background:** transparent
- **Cell order:** `obj_altar_idle`, `obj_altar_lit`, `obj_altar_ritual_0`, `obj_altar_ritual_1`, `obj_stone_1`, `obj_stone_2`, `obj_stone_3`, `obj_stone_mild`, `obj_relic_seal`, `obj_relic_bell`, `obj_relic_thread`, `obj_relic_gone`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: The altar and clearing objects. A sprite sheet laid out as a strict grid of 4 columns by 3 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Harrow Wood is still drawn in the same cozy storybook style and palette: it is an old, quiet, mossy wood that feels uncanny only through colour (cooler teal, dusk blue and lavender tones, long soft shadows, a few pale fireflies). Nothing is gory or graphic; no skulls, blood or monsters. Mood: hushed, mysterious, gently eerie.
A low flat altar of old grey stone carved with a knot symbol, 2x2 tiles: idle (dull), lit (soft gold glow ring), and two ritual frames (a ring of pale gold light rising). Three standing carved stones with moss (different shapes, faint carved lines), a simpler 'mild' stone with no carvings, and the three relics lying on the ground: an ash-grey seal disc, a dull bronze bell, a green thread coil; then an empty patch of disturbed moss where a relic was taken.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Offering dissolve effect

- **Final cell size:** 16 x 16 px  |  **Grid:** 6 columns x 2 rows  |  **Generate size:** 1152 x 384 px (6 columns x 2 rows)
- **Background:** transparent
- **Cell order:** `fx_offering_animal_0`, `fx_offering_animal_1`, `fx_offering_animal_2`, `fx_offering_animal_3`, `fx_offering_plant_0`, `fx_offering_plant_1`, `fx_offering_plant_2`, `fx_offering_plant_3`, `fx_offering_craft_0`, `fx_offering_craft_1`, `fx_offering_craft_2`, `fx_offering_craft_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Offering dissolve effect. A sprite sheet laid out as a strict grid of 6 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Harrow Wood is still drawn in the same cozy storybook style and palette: it is an old, quiet, mossy wood that feels uncanny only through colour (cooler teal, dusk blue and lavender tones, long soft shadows, a few pale fireflies). Nothing is gory or graphic; no skulls, blood or monsters. Mood: hushed, mysterious, gently eerie.
The ritual offerings dissolve into soft light, never harming anything: row 1 = (a) a gentle glowing animal-shaped wisp of light, (b) a plant item turning into drifting golden motes, (c) a crafted item turning into light; four frames each fading to pure light and sparkles. For 'mild' intensity a plain small floating light orb only (one frame is enough). No bodies, no distress.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Horror and mutant crops

- **Final cell size:** 16 x 16 px  |  **Grid:** 6 columns x 6 rows  |  **Generate size:** 1152 x 1152 px (6 columns x 6 rows)
- **Background:** transparent
- **Cell order:** `crop_nightbloom_0`, `crop_nightbloom_1`, `crop_nightbloom_2`, `crop_nightbloom_3`, `crop_nightbloom_4`, `crop_nightbloom_5`, `crop_hollowroot_0`, `crop_hollowroot_1`, `crop_hollowroot_2`, `crop_hollowroot_3`, `crop_hollowroot_4`, `crop_hollowroot_5`, `crop_ashfruit_0`, `crop_ashfruit_1`, `crop_ashfruit_2`, `crop_ashfruit_3`, `crop_ashfruit_4`, `crop_ashfruit_5`, `crop_mutant_root_0`, `crop_mutant_root_1`, `crop_mutant_root_2`, `crop_mutant_root_3`, `crop_mutant_root_4`, `crop_mutant_root_5`, `crop_mutant_gourd_0`, `crop_mutant_gourd_1`, `crop_mutant_gourd_2`, `crop_mutant_gourd_3`, `crop_mutant_gourd_4`, `crop_mutant_gourd_5`, `crop_mutant_leaf_0`, `crop_mutant_leaf_1`, `crop_mutant_leaf_2`, `crop_mutant_leaf_3`, `crop_mutant_leaf_4`, `crop_mutant_leaf_5`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Horror and mutant crops. A sprite sheet laid out as a strict grid of 6 columns by 6 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Harrow Wood is still drawn in the same cozy storybook style and palette: it is an old, quiet, mossy wood that feels uncanny only through colour (cooler teal, dusk blue and lavender tones, long soft shadows, a few pale fireflies). Nothing is gory or graphic; no skulls, blood or monsters. Mood: hushed, mysterious, gently eerie.
Six rows, six growth stages (they must read as the same cozy plants but strange): nightbloom (pale violet flowers that close at night, glowing faintly), hollowroot (a pale knobbly root with a dark hollow, grey-green leaves), ashfruit (a fruit of glowing orange cinders with a soft ash-grey bush), mutant root (an ordinary root vegetable that has grown a second tiny curly root and oddly pink skin), mutant gourd (a gourd with swirling stripes and an extra bump), mutant leaf (a leafy plant with leaves in two different colours). Stage 0 sprout through stage 5 mature.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Horror crop and relic item icons

- **Final cell size:** 16 x 16 px  |  **Grid:** 6 columns x 3 rows  |  **Generate size:** 1152 x 576 px (6 columns x 3 rows)
- **Background:** transparent
- **Cell order:** `item_seed_nightbloom`, `item_seed_hollowroot`, `item_seed_ashfruit`, `item_seed_mutant_root`, `item_seed_mutant_gourd`, `item_seed_mutant_leaf`, `item_crop_nightbloom`, `item_crop_hollowroot`, `item_crop_ashfruit`, `item_crop_mutant_root`, `item_crop_mutant_gourd`, `item_crop_mutant_leaf`, `item_mythos_relic_seal`, `item_mythos_relic_bell`, `item_mythos_relic_thread`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Horror crop and relic item icons. A sprite sheet laid out as a strict grid of 6 columns by 3 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Harrow Wood is still drawn in the same cozy storybook style and palette: it is an old, quiet, mossy wood that feels uncanny only through colour (cooler teal, dusk blue and lavender tones, long soft shadows, a few pale fireflies). Nothing is gory or graphic; no skulls, blood or monsters. Mood: hushed, mysterious, gently eerie.
Inventory icons: seed packets with a dusky-violet label for the six strange crops; the harvested strange crops; and three relic icons (ash seal, hollow bell, green thread).
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Atmosphere overlays

- **Final cell size:** 64 x 64 px  |  **Grid:** 4 columns x 2 rows  |  **Generate size:** 3072 x 1536 px
- **Background:** transparent
- **Cell order:** `ov_fog_wisp`, `ov_vignette_dread`, `ov_vine_creep`, `ov_ground_crack`, `ov_red_sky`, `ov_firefly_swarm`, `ov_ripple_hum`, `ov_ash_fall`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Atmosphere overlays. A sprite sheet laid out as a strict grid of 4 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Harrow Wood is still drawn in the same cozy storybook style and palette: it is an old, quiet, mossy wood that feels uncanny only through colour (cooler teal, dusk blue and lavender tones, long soft shadows, a few pale fireflies). Nothing is gory or graphic; no skulls, blood or monsters. Mood: hushed, mysterious, gently eerie.
Full-screen-ish overlays on transparent: soft fog wisps, a gentle dark-blue vignette, creeping vine tendrils along an edge, hairline ground cracks with a faint warm glow, a thin red dusk sky wash, a swarm of pale fireflies, concentric 'hum' ripples, and drifting ash flakes. Subtle, low contrast, painterly. These grow in strength with the god's wakefulness steps, so each must stay quiet at low opacity.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Ending illustrations

- **Final cell size:** 160 x 90 px  |  **Grid:** 2 columns x 2 rows  |  **Generate size:** 3840 x 2160 px, 2x2 grid
- **Background:** solid, as described
- **Cell order:** `ending_awakened`, `ending_sealed`, `ending_joined`, `ending_ignored`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Ending illustrations. A sprite sheet laid out as a strict grid of 2 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Four storybook scenes in the same palette, framed like picture-book pages. Sealed: dawn light returning over a calm Harrow Wood, birds, the Keepers standing quietly with their hoods down. Joined: the player kneeling in a clearing among hooded, gentle Keepers under a full moon, calm and solemn. Ignored: the farm in warm golden evening, a lamp in the window, the wood dark but still on the horizon. Awakened: the sky turning into a vast, abstract field of fire and ember colours over the village silhouettes, dramatic but stylised, no creatures, no gore, no people shown harmed.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```
