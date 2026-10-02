# Sprite prompts: Logo, title and store art

Read `STYLE_GUIDE.md` first. Every prompt below already contains the shared STYLE BLOCK. Sheet sizes follow `docs/ART_ASSETS.md`; file names are the placeholder names to replace (cell names for animation frames are new: the game does not animate yet).

## Game logo and title

- **Final cell size:** 160 x 90 px  |  **Grid:** 1 columns x 2 rows  |  **Generate size:** 3840 x 2160 px
- **Background:** solid, as described
- **Cell order:** `logo_farm`, `title_background`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Game logo and title. A sprite sheet laid out as a strict grid of 1 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Top: the game logo as the word 'Farm' (working title; replace with the final name) in chunky hand-lettered wooden letters with leaves and a tiny sprouting seedling growing from the F, warm butter-yellow with dark brown outline, a small wheat sprig, centred, transparent background. Bottom: the title screen background, a golden-hour farm at the edge of a quiet village with the farmhouse chimney smoking, rolling fields, and at the far edge a dark mysterious tree line with a pale moon rising (a subtle hint of the story), peaceful and inviting.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Application and store art

- **Final cell size:** 64 x 64 px  |  **Grid:** 3 columns x 2 rows  |  **Generate size:** as each Steam size x2
- **Background:** solid, as described
- **Cell order:** `app_icon`, `capsule_header`, `capsule_small`, `capsule_main`, `capsule_vertical`, `library_hero`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Application and store art. A sprite sheet laid out as a strict grid of 3 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Store art in the same style: an app icon (a farmhouse and sprouting seedling in a round badge), a header capsule (460x215) showing the farm, a small capsule, a main capsule, a vertical capsule (600x900) and a library hero. Include the cozy farm foreground, the village, and a thin hint of moonlit woods; leave room for the logo.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```
