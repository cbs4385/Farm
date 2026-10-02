# Sprite prompts: Tiles and buildings

Read `STYLE_GUIDE.md` first. Every prompt below already contains the shared STYLE BLOCK. Sheet sizes follow `docs/ART_ASSETS.md`; file names are the placeholder names to replace (cell names for animation frames are new: the game does not animate yet).

## Terrain and building base tiles

- **Final cell size:** 16 x 16 px  |  **Grid:** 5 columns x 3 rows  |  **Generate size:** 960 x 576 px (5 columns x 3 rows)
- **Background:** solid, as described
- **Cell order:** `tile_cobble`, `tile_dirt`, `tile_door`, `tile_floor_wood`, `tile_forest`, `tile_grass`, `tile_path`, `tile_roof`, `tile_sand`, `tile_tilled`, `tile_tilled_watered`, `tile_wall`, `tile_water`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Terrain and building base tiles. A sprite sheet laid out as a strict grid of 5 columns by 3 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is ONE seamlessly tileable 16x16 ground or wall tile viewed from above, filling the whole cell edge to edge (no margin, no outline around the tile). Content per cell, in order:
- tile_cobble: rounded cobblestones in warm greys with moss between them
- tile_dirt: packed warm brown earth with a few pebbles
- tile_door: sturdy wooden cottage door with an iron handle and a tiny warm window
- tile_floor_wood: warm honey-brown wooden floorboards with visible planks and nail dots
- tile_forest: dark leafy forest floor, deep pine green with brown leaf litter and tiny ferns
- tile_grass: soft meadow grass in two greens with tiny lighter blades and the odd daisy; spring look
- tile_path: pale sandy footpath, pressed flat, soft worn edge speckle
- tile_roof: terracotta roof shingles in overlapping rows
- tile_sand: pale golden beach sand with tiny shell flecks
- tile_tilled: freshly hoed soil in neat dark furrows
- tile_tilled_watered: the same furrows darker and glossy with water
- tile_wall: rough warm stone and plaster wall, subtle brick pattern, lighter top edge
- tile_water: calm shallow pond water, teal and dusk blue with two small white sparkle ripples
Tiles must repeat without visible seams.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Seasonal ground variants (grass, forest floor, path)

- **Final cell size:** 16 x 16 px  |  **Grid:** 4 columns x 3 rows  |  **Generate size:** 768 x 576 px (4 columns x 3 rows)
- **Background:** solid, as described
- **Cell order:** `tile_grass_spring`, `tile_grass_summer`, `tile_grass_fall`, `tile_grass_winter`, `tile_forest_spring`, `tile_forest_summer`, `tile_forest_fall`, `tile_forest_winter`, `tile_path_spring`, `tile_path_summer`, `tile_path_fall`, `tile_path_winter`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Seasonal ground variants (grass, forest floor, path). A sprite sheet laid out as a strict grid of 4 columns by 3 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Rows: grass, forest floor, path. Columns: spring (fresh bright greens, blossom petals), summer (deep lush greens, tiny yellow flowers), fall (orange and rust leaves scattered, golden grass), winter (soft snow cover with a few green tufts showing). Each 16x16 seamless and the four seasons of one row must share the same underlying pattern so they can be swapped.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Autotile edge set (grass to dirt, path to grass, water to sand)

- **Final cell size:** 16 x 16 px  |  **Grid:** 4 columns x 4 rows  |  **Generate size:** 768 x 768 px (4 columns x 4 rows)
- **Background:** solid, as described
- **Cell order:** `edge_grass_dirt_0`, `edge_grass_dirt_1`, `edge_grass_dirt_2`, `edge_grass_dirt_3`, `edge_grass_dirt_4`, `edge_grass_dirt_5`, `edge_grass_dirt_6`, `edge_grass_dirt_7`, `edge_grass_dirt_8`, `edge_grass_dirt_9`, `edge_grass_dirt_10`, `edge_grass_dirt_11`, `edge_grass_dirt_12`, `edge_grass_dirt_13`, `edge_grass_dirt_14`, `edge_grass_dirt_15`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Autotile edge set (grass to dirt, path to grass, water to sand). A sprite sheet laid out as a strict grid of 4 columns by 4 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
A 16-tile 'blob' autotile set (every combination of the four neighbours): grass meeting packed dirt. Rounded, hand-torn organic transition edge two pixels wide. Draw the set as: row 1 = isolated, end caps; row 2 = straight edges; row 3 = outer and inner corners; row 4 = full interior variants. Repeat this sheet three times with path-to-grass and water-to-sand using the same layout.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Water animation

- **Final cell size:** 16 x 16 px  |  **Grid:** 4 columns x 1 rows  |  **Generate size:** 768 x 192 px (4 columns x 1 rows)
- **Background:** solid, as described
- **Cell order:** `tile_water_0`, `tile_water_1`, `tile_water_2`, `tile_water_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Water animation. A sprite sheet laid out as a strict grid of 4 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Four frames of a loop: gently moving pond water, ripples drifting one pixel per frame, sparkles twinkling in different spots. Seamless tile.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: General store

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `general_store`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: General store. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). a friendly village shop with a striped awning in butter yellow and cream, shuttered windows, hanging basket of flowers, barrels of apples at the door, wooden sign with a basket. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Blacksmith

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `blacksmith`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Blacksmith. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). stone and timber forge with a chimney puffing a little smoke, an anvil sign, coal bucket by the door, warm orange glow in the window. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Carpenter

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `carpenter`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Carpenter. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). timber workshop with stacked planks and a saw sign, sawdust-coloured roof trim, wood-shaving flower box. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Library

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `library`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Library. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). tall narrow tweed-green building with round windows, a stack of books in the window, ivy on one wall, a little owl weather vane. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Saloon

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `saloon`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Saloon. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). two-storey tavern with a swinging sign shaped like a mug, glowing lanterns, chalkboard menu, wooden porch with a bench. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Clinic

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `clinic`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Clinic. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). small white-washed clinic with a green cross on a hanging sign, herb pots on the sill, a bench by the door. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Community Hall

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `community_hall`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Community Hall. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). large timber hall, slightly faded and overgrown (derelict version: boarded windows, missing shingles) and, in a second image, the same hall restored (fresh paint, bunting, lit windows). Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Farmhouse

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `farmhouse`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Farmhouse. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). cosy cottage with a red roof, stone chimney with smoke, small porch with a rocking chair, vegetable patch, wooden front door. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Greenhouse

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `greenhouse`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Greenhouse. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). glass-and-timber greenhouse with leafy plants visible inside, a watering can by the door; a ruined version with cracked panes and a restored one. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Coop

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `coop`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Coop. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). small red-painted chicken coop with a ramp, straw peeking out, a tiny weather vane. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Barn

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `barn`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Barn. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). big red barn with white X-braced doors, hay loft window, a cat on the roof ridge. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Building exterior: Mine entrance

- **Final cell size:** 16 x 16 px  |  **Grid:** 1 columns x 1 rows  |  **Generate size:** 1536 x 1152 px, final about 128 x 96 px
- **Background:** transparent
- **Cell order:** `mine_entrance`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Building exterior: Mine entrance. A sprite sheet laid out as a strict grid of 1 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
One single building, front three-quarter view, sized as a multiple of 16 px tiles (about 7 to 8 tiles wide and 5 to 6 tall). mossy rock arch with timber supports and a lantern, rail tracks leading in, a darkness beyond with faint warm lamp light. Include a one-tile door area aligned to the tile grid. Cozy, lived-in, tidy.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```
