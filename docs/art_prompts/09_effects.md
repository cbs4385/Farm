# Sprite prompts: Effects and lighting

Read `STYLE_GUIDE.md` first. Every prompt below already contains the shared STYLE BLOCK. Sheet sizes follow `docs/ART_ASSETS.md`; file names are the placeholder names to replace (cell names for animation frames are new: the game does not animate yet).

## Weather particles and overlays

- **Final cell size:** 16 x 16 px  |  **Grid:** 6 columns x 3 rows  |  **Generate size:** 1152 x 576 px (6 columns x 3 rows)
- **Background:** transparent
- **Cell order:** `fx_rain_drop`, `fx_rain_splash_0`, `fx_rain_splash_1`, `fx_snowflake_a`, `fx_snowflake_b`, `fx_wind_leaf`, `fx_wind_streak`, `fx_fog_a`, `fx_fog_b`, `fx_lightning_flash`, `fx_cloud_shadow`, `fx_petal`, `fx_autumn_leaf`, `fx_dust`, `fx_sparkle`, `fx_puff_0`, `fx_puff_1`, `fx_puff_2`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Weather particles and overlays. A sprite sheet laid out as a strict grid of 6 columns by 3 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Transparent effect sprites: a thin blue-grey rain streak, two splash frames, two snowflake shapes, a blowing leaf, a wind streak, two soft fog wisps (large, translucent), a lightning flash flare, a cloud shadow blob, a pink blossom petal, an orange autumn leaf, a dust speck, a small sparkle, and three puff-of-smoke frames. Soft and gentle.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Action effects

- **Final cell size:** 16 x 16 px  |  **Grid:** 6 columns x 3 rows  |  **Generate size:** 1152 x 576 px (6 columns x 3 rows)
- **Background:** transparent
- **Cell order:** `fx_dig_dirt_0`, `fx_dig_dirt_1`, `fx_water_drop_0`, `fx_water_drop_1`, `fx_chop_chip_0`, `fx_chop_chip_1`, `fx_ore_spark_0`, `fx_ore_spark_1`, `fx_harvest_pop_0`, `fx_harvest_pop_1`, `fx_level_up_0`, `fx_level_up_1`, `fx_heart_pop`, `fx_coin_fly`, `fx_hit_star`, `fx_sword_arc_0`, `fx_sword_arc_1`, `fx_splash_big`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Action effects. A sprite sheet laid out as a strict grid of 6 columns by 3 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Short two-frame action effects in the same warm palette: dirt crumbs, water drops, wood chips, ore sparks, a harvest pop with leaves, a level-up burst of golden stars, a floating heart, a flying coin, a hit star, two sword-swing arcs, a big water splash.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Lighting sprites

- **Final cell size:** 32 x 32 px  |  **Grid:** 4 columns x 1 rows  |  **Generate size:** 1536 x 384 px
- **Background:** transparent
- **Cell order:** `light_window_glow`, `light_lamp_glow`, `light_campfire_glow`, `light_lantern_glow`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Lighting sprites. A sprite sheet laid out as a strict grid of 4 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Soft round radial light cookies in warm yellow to transparent (alpha falloff only, no hard edge) for night: a window light spill, a street lamp glow, a campfire glow, a lantern glow. Pure white-yellow with alpha; no outlines.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```
