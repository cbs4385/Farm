# Style guide

The look of the game: **cozy, warm, hand-crafted pixel art**, like a storybook diorama in late-afternoon light. The optional horror layer is expressed through colour temperature and quiet mystery, never through gore: the cozy game must stay complete and inviting, and the dark story should feel like a hush under it.

## The style block
Pasted unchanged into every prompt:

```text
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.
```

## Negative block
```text
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Palette (shared by every sheet)
| Name | Hex | Typical use |
|---|---|---|
| cream | `#F4E6C3` | highlights, parchment, walls |
| butter yellow | `#F2C14E` | gold, sun, accents, UI highlight |
| honey | `#D9983B` | wood light, wheat, lamps |
| terracotta | `#B8553A` | roofs, brick, pumpkins |
| rosewood | `#7A3B3B` | dark reds, berries, barns |
| moss green | `#6E9E4F` | grass light, leaves |
| leaf green | `#4F8A3F` | grass, crops |
| deep pine | `#2F5D3A` | forest, shadow greens |
| sky teal | `#5FA8A0` | water, glass, fog |
| dusk blue | `#4A6FA5` | water shade, night, rain |
| lavender | `#8E7CC3` | magic, the horror layer, dusk |
| bark brown | `#6B4A2F` | wood, trunks |
| soil brown | `#4A3224` | soil, shadow brown |
| warm stone grey | `#8C8577` | stone, rock |
| outline brown | `#3A2618` | all outlines (never pure black) |

## Rules
- **Perspective:** top-down three-quarter; objects show their front face and top, as if seen from above at about 30 degrees. Characters face the camera when facing down.
- **Grid:** 16 px per tile; characters 16x32; portraits 32x32; large objects are multiples of 16.
- **Proportions:** chibi: head about 40 percent of a character's height, big simple eyes, small mouths. Animals and monsters are rounded and cute.
- **Light and shading:** light from the upper left; two tones per colour (base and shade) plus one highlight pixel; no gradients; no dithering except in sky overlays.
- **Outline:** one pixel, warm dark brown; interior lines may be lighter. Sprites sit on a one-row drop shadow in outline brown at 30 percent.
- **Mood:** calm, nostalgic, friendly. Warm colours dominate; cool colours signal night, rain, water and the woods.
- **Seasons:** spring = fresh greens and pink blossom, summer = deep saturated greens and yellow, fall = rust, orange and gold, winter = soft snow, pale blue shadows.
- **Horror layer (Harrow Wood, Keepers, strange crops):** same line, same palette, same shading. Use cooler teal, dusk blue and lavender, longer shadows, pale fireflies, and carved knot symbols. Allowed: hushed, mysterious, uncanny. Not allowed: blood, gore, corpses, skulls (the cartoonish Bone Walker excepted), glowing red eyes on friendly characters, harm to animals or people. Offerings always dissolve into light. Provide a gentler variant wherever noted ('mild').
- **Accessibility:** do not rely on red versus green alone: crops, items and status icons also differ in shape and brightness.
- **Text:** never put text in sprites (the game localizes all text); the logo is the single exception.

## Consistency checklist (run after each sheet)
1. Palette: every colour snaps to the shared palette.
2. Outline: one pixel, `#3A2618`, no black.
3. Light from the upper left on every sprite.
4. Same eye style and head size across characters and animals.
5. Same level of detail: nothing busier than a tile-sized crop; silhouettes readable at 1x.
6. Placed next to the previous sheets, nothing looks pasted from another game.
7. Names and cell order match the sheet's CELL ORDER line.

## Prompt tips for the generator
- Keep the STYLE BLOCK first and unchanged; edit only the content paragraph.
- If a model ignores the grid, generate one row at a time with the same style block and assemble by hand.
- Fix a recurring character by reusing the first approved sprite as the reference image for later sheets (image-to-image) with the same prompt.
- Generate animation frames from the approved idle frame as a reference to keep them on-model.
