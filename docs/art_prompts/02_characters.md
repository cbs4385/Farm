# Sprite prompts: Player and villagers

Read `STYLE_GUIDE.md` first. Every prompt below already contains the shared STYLE BLOCK. Sheet sizes follow `docs/ART_ASSETS.md`; file names are the placeholder names to replace (cell names for animation frames are new: the game does not animate yet).

## Player: walk cycle

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 4 rows  |  **Generate size:** 768 x 1536 px (4 columns x 4 rows)
- **Background:** transparent
- **Cell order:** `player_walk_down_0`, `player_walk_down_1`, `player_walk_down_2`, `player_walk_down_3`, `player_walk_up_0`, `player_walk_up_1`, `player_walk_up_2`, `player_walk_up_3`, `player_walk_left_0`, `player_walk_left_1`, `player_walk_left_2`, `player_walk_left_3`, `player_walk_right_0`, `player_walk_right_1`, `player_walk_right_2`, `player_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Player: walk cycle. A sprite sheet laid out as a strict grid of 4 columns by 4 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
The player farmer, a friendly everyman with a warm face: round straw hat with a blue band, rolled-sleeve blue work shirt, brown trousers, sturdy boots, a small satchel strap. Large head (about 40 percent of the height), chibi proportions, no visible mouth detail other than a small smile. Four rows: facing down (toward camera), up (away), left, right. Four columns: a walk cycle (contact, passing, contact, passing). Feet always on the same baseline; character centred.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Player: idle poses

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 1 rows  |  **Generate size:** 768 x 384 px (4 columns x 1 rows)
- **Background:** transparent
- **Cell order:** `player_idle_down`, `player_idle_up`, `player_idle_left`, `player_idle_right`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Player: idle poses. A sprite sheet laid out as a strict grid of 4 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Same farmer, standing relaxed, one pose per facing, slightly bobbing breath implied by pose.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Player: tool and action animations

- **Final cell size:** 16 x 32 px  |  **Grid:** 6 columns x 8 rows  |  **Generate size:** 1152 x 3072 px (6 columns x 8 rows)
- **Background:** transparent
- **Cell order:** `player_hoe_down_0`, `player_hoe_down_1`, `player_hoe_down_2`, `player_hoe_down_3`, `player_hoe_down_4`, `player_hoe_down_5`, `player_hoe_side_0`, `player_hoe_side_1`, `player_hoe_side_2`, `player_hoe_side_3`, `player_hoe_side_4`, `player_hoe_side_5`, `player_water_down_0`, `player_water_down_1`, `player_water_down_2`, `player_water_down_3`, `player_water_down_4`, `player_water_down_5`, `player_water_side_0`, `player_water_side_1`, `player_water_side_2`, `player_water_side_3`, `player_water_side_4`, `player_water_side_5`, `player_axe_down_0`, `player_axe_down_1`, `player_axe_down_2`, `player_axe_down_3`, `player_axe_down_4`, `player_axe_down_5`, `player_axe_side_0`, `player_axe_side_1`, `player_axe_side_2`, `player_axe_side_3`, `player_axe_side_4`, `player_axe_side_5`, `player_pick_down_0`, `player_pick_down_1`, `player_pick_down_2`, `player_pick_down_3`, `player_pick_down_4`, `player_pick_down_5`, `player_pick_side_0`, `player_pick_side_1`, `player_pick_side_2`, `player_pick_side_3`, `player_pick_side_4`, `player_pick_side_5`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Player: tool and action animations. A sprite sheet laid out as a strict grid of 6 columns by 8 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Same farmer performing actions, 6 frames each, rows in pairs (facing down, then facing right; left is mirrored): hoe swing, watering can pour, axe chop, pickaxe strike. Wind-up, swing, impact, recover. The tool is large and readable. Small motion puffs are NOT included here (they are separate effects).
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Player: more actions

- **Final cell size:** 16 x 32 px  |  **Grid:** 6 columns x 6 rows  |  **Generate size:** 1152 x 2304 px (6 columns x 6 rows)
- **Background:** transparent
- **Cell order:** `player_scythe_0`, `player_scythe_1`, `player_scythe_2`, `player_scythe_3`, `player_sword_0`, `player_sword_1`, `player_sword_2`, `player_sword_3`, `player_fish_cast_0`, `player_fish_cast_1`, `player_fish_cast_2`, `player_fish_cast_3`, `player_fish_wait_0`, `player_fish_wait_1`, `player_fish_wait_2`, `player_fish_wait_3`, `player_eat_0`, `player_eat_1`, `player_eat_2`, `player_eat_3`, `player_carry_0`, `player_carry_1`, `player_carry_2`, `player_carry_3`, `player_sleep_0`, `player_sleep_1`, `player_sleep_2`, `player_sleep_3`, `player_collapse_0`, `player_collapse_1`, `player_collapse_2`, `player_collapse_3`, `player_hurt_0`, `player_hurt_1`, `player_hurt_2`, `player_hurt_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Player: more actions. A sprite sheet laid out as a strict grid of 6 columns by 6 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Same farmer: scythe sweep (facing right), sword swing, fishing cast, waiting with rod, eating with a happy face, carrying a crate overhead, sleeping in a curled pose, collapsing from tiredness (sitting down with swirly dizziness stars), hurt flinch. Four frames each.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Tilda (general store owner)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_tilda_idle_down`, `npc_tilda_idle_up`, `npc_tilda_idle_left`, `npc_tilda_idle_right`, `npc_tilda_walk_down_0`, `npc_tilda_walk_down_1`, `npc_tilda_walk_down_2`, `npc_tilda_walk_down_3`, `npc_tilda_walk_up_0`, `npc_tilda_walk_up_1`, `npc_tilda_walk_up_2`, `npc_tilda_walk_up_3`, `npc_tilda_walk_left_0`, `npc_tilda_walk_left_1`, `npc_tilda_walk_left_2`, `npc_tilda_walk_left_3`, `npc_tilda_walk_right_0`, `npc_tilda_walk_right_1`, `npc_tilda_walk_right_2`, `npc_tilda_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Tilda (general store owner). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Tilda, the general store owner: warm motherly woman, green apron over a cream blouse, chestnut hair in a bun, round spectacles. (a Keeper) with a quiet watchful look Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Bram (blacksmith)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_bram_idle_down`, `npc_bram_idle_up`, `npc_bram_idle_left`, `npc_bram_idle_right`, `npc_bram_walk_down_0`, `npc_bram_walk_down_1`, `npc_bram_walk_down_2`, `npc_bram_walk_down_3`, `npc_bram_walk_up_0`, `npc_bram_walk_up_1`, `npc_bram_walk_up_2`, `npc_bram_walk_up_3`, `npc_bram_walk_left_0`, `npc_bram_walk_left_1`, `npc_bram_walk_left_2`, `npc_bram_walk_left_3`, `npc_bram_walk_right_0`, `npc_bram_walk_right_1`, `npc_bram_walk_right_2`, `npc_bram_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Bram (blacksmith). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Bram, the blacksmith: broad, bearded man, grey work apron, dark hair, rolled sleeves, soot smudge on one cheek, leather gloves.  Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Ione (librarian)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_ione_idle_down`, `npc_ione_idle_up`, `npc_ione_idle_left`, `npc_ione_idle_right`, `npc_ione_walk_down_0`, `npc_ione_walk_down_1`, `npc_ione_walk_down_2`, `npc_ione_walk_down_3`, `npc_ione_walk_up_0`, `npc_ione_walk_up_1`, `npc_ione_walk_up_2`, `npc_ione_walk_up_3`, `npc_ione_walk_left_0`, `npc_ione_walk_left_1`, `npc_ione_walk_left_2`, `npc_ione_walk_left_3`, `npc_ione_walk_right_0`, `npc_ione_walk_right_1`, `npc_ione_walk_right_2`, `npc_ione_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Ione (librarian). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Ione, the librarian: slender woman, lavender cardigan, golden hair in a braid, a pencil behind her ear, holding a book.  Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Marcus (carpenter)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_marcus_idle_down`, `npc_marcus_idle_up`, `npc_marcus_idle_left`, `npc_marcus_idle_right`, `npc_marcus_walk_down_0`, `npc_marcus_walk_down_1`, `npc_marcus_walk_down_2`, `npc_marcus_walk_down_3`, `npc_marcus_walk_up_0`, `npc_marcus_walk_up_1`, `npc_marcus_walk_up_2`, `npc_marcus_walk_up_3`, `npc_marcus_walk_left_0`, `npc_marcus_walk_left_1`, `npc_marcus_walk_left_2`, `npc_marcus_walk_left_3`, `npc_marcus_walk_right_0`, `npc_marcus_walk_right_1`, `npc_marcus_walk_right_2`, `npc_marcus_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Marcus (carpenter). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Marcus, the carpenter: sturdy man, brown work vest, flat cap, a pencil on his ear, tool belt. (a Keeper) calm, slightly too still Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Odalys (Dr. Penn, the village doctor)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_odalys_idle_down`, `npc_odalys_idle_up`, `npc_odalys_idle_left`, `npc_odalys_idle_right`, `npc_odalys_walk_down_0`, `npc_odalys_walk_down_1`, `npc_odalys_walk_down_2`, `npc_odalys_walk_down_3`, `npc_odalys_walk_up_0`, `npc_odalys_walk_up_1`, `npc_odalys_walk_up_2`, `npc_odalys_walk_up_3`, `npc_odalys_walk_left_0`, `npc_odalys_walk_left_1`, `npc_odalys_walk_left_2`, `npc_odalys_walk_left_3`, `npc_odalys_walk_right_0`, `npc_odalys_walk_right_1`, `npc_odalys_walk_right_2`, `npc_odalys_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Odalys (Dr. Penn, the village doctor). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Odalys, the Dr. Penn, the village doctor: tall woman, long white coat, dark hair pinned up, small round pendant; kind but unreadable. (the Keepers' leader) Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Wren (saloon keeper)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_wren_idle_down`, `npc_wren_idle_up`, `npc_wren_idle_left`, `npc_wren_idle_right`, `npc_wren_walk_down_0`, `npc_wren_walk_down_1`, `npc_wren_walk_down_2`, `npc_wren_walk_down_3`, `npc_wren_walk_up_0`, `npc_wren_walk_up_1`, `npc_wren_walk_up_2`, `npc_wren_walk_up_3`, `npc_wren_walk_left_0`, `npc_wren_walk_left_1`, `npc_wren_walk_left_2`, `npc_wren_walk_left_3`, `npc_wren_walk_right_0`, `npc_wren_walk_right_1`, `npc_wren_walk_right_2`, `npc_wren_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Wren (saloon keeper). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Wren, the saloon keeper: cheery woman, red waistcoat and rolled sleeves, a dish towel on her shoulder, copper-red hair. (a Keeper) Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Felix (fishmonger)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_felix_idle_down`, `npc_felix_idle_up`, `npc_felix_idle_left`, `npc_felix_idle_right`, `npc_felix_walk_down_0`, `npc_felix_walk_down_1`, `npc_felix_walk_down_2`, `npc_felix_walk_down_3`, `npc_felix_walk_up_0`, `npc_felix_walk_up_1`, `npc_felix_walk_up_2`, `npc_felix_walk_up_3`, `npc_felix_walk_left_0`, `npc_felix_walk_left_1`, `npc_felix_walk_left_2`, `npc_felix_walk_left_3`, `npc_felix_walk_right_0`, `npc_felix_walk_right_1`, `npc_felix_walk_right_2`, `npc_felix_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Felix (fishmonger). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Felix, the fishmonger: easygoing man, blue oilskin jacket, yellow sou'wester hat, fishing net on his back.  Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Juno (blacksmith apprentice)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_juno_idle_down`, `npc_juno_idle_up`, `npc_juno_idle_left`, `npc_juno_idle_right`, `npc_juno_walk_down_0`, `npc_juno_walk_down_1`, `npc_juno_walk_down_2`, `npc_juno_walk_down_3`, `npc_juno_walk_up_0`, `npc_juno_walk_up_1`, `npc_juno_walk_up_2`, `npc_juno_walk_up_3`, `npc_juno_walk_left_0`, `npc_juno_walk_left_1`, `npc_juno_walk_left_2`, `npc_juno_walk_left_3`, `npc_juno_walk_right_0`, `npc_juno_walk_right_1`, `npc_juno_walk_right_2`, `npc_juno_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Juno (blacksmith apprentice). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Juno, the blacksmith apprentice: teenage girl, oversized apron, auburn hair in short ponytail, grimy goggles on her forehead.  Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Hazel (forager who senses something is wrong)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_hazel_idle_down`, `npc_hazel_idle_up`, `npc_hazel_idle_left`, `npc_hazel_idle_right`, `npc_hazel_walk_down_0`, `npc_hazel_walk_down_1`, `npc_hazel_walk_down_2`, `npc_hazel_walk_down_3`, `npc_hazel_walk_up_0`, `npc_hazel_walk_up_1`, `npc_hazel_walk_up_2`, `npc_hazel_walk_up_3`, `npc_hazel_walk_left_0`, `npc_hazel_walk_left_1`, `npc_hazel_walk_left_2`, `npc_hazel_walk_left_3`, `npc_hazel_walk_right_0`, `npc_hazel_walk_right_1`, `npc_hazel_walk_right_2`, `npc_hazel_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Hazel (forager who senses something is wrong). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Hazel, the forager who senses something is wrong: wiry person, moss-green hooded cloak with leaf trim, basket of mushrooms, brown braided hair.  Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Piper (saloon musician)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_piper_idle_down`, `npc_piper_idle_up`, `npc_piper_idle_left`, `npc_piper_idle_right`, `npc_piper_walk_down_0`, `npc_piper_walk_down_1`, `npc_piper_walk_down_2`, `npc_piper_walk_down_3`, `npc_piper_walk_up_0`, `npc_piper_walk_up_1`, `npc_piper_walk_up_2`, `npc_piper_walk_up_3`, `npc_piper_walk_left_0`, `npc_piper_walk_left_1`, `npc_piper_walk_left_2`, `npc_piper_walk_left_3`, `npc_piper_walk_right_0`, `npc_piper_walk_right_1`, `npc_piper_walk_right_2`, `npc_piper_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Piper (saloon musician). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Piper, the saloon musician: lanky young man, mustard coat, black curly hair, a fiddle on his back.  Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Dorian (library assistant)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_dorian_idle_down`, `npc_dorian_idle_up`, `npc_dorian_idle_left`, `npc_dorian_idle_right`, `npc_dorian_walk_down_0`, `npc_dorian_walk_down_1`, `npc_dorian_walk_down_2`, `npc_dorian_walk_down_3`, `npc_dorian_walk_up_0`, `npc_dorian_walk_up_1`, `npc_dorian_walk_up_2`, `npc_dorian_walk_up_3`, `npc_dorian_walk_left_0`, `npc_dorian_walk_left_1`, `npc_dorian_walk_left_2`, `npc_dorian_walk_left_3`, `npc_dorian_walk_right_0`, `npc_dorian_walk_right_1`, `npc_dorian_walk_right_2`, `npc_dorian_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Dorian (library assistant). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Dorian, the library assistant: quiet young man, olive sweater, grey scarf, round glasses, stack of books. (a Keeper) Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Villager: Elara (clinic nurse)

- **Final cell size:** 16 x 32 px  |  **Grid:** 4 columns x 5 rows  |  **Generate size:** 768 x 1920 px (4 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `npc_elara_idle_down`, `npc_elara_idle_up`, `npc_elara_idle_left`, `npc_elara_idle_right`, `npc_elara_walk_down_0`, `npc_elara_walk_down_1`, `npc_elara_walk_down_2`, `npc_elara_walk_down_3`, `npc_elara_walk_up_0`, `npc_elara_walk_up_1`, `npc_elara_walk_up_2`, `npc_elara_walk_up_3`, `npc_elara_walk_left_0`, `npc_elara_walk_left_1`, `npc_elara_walk_left_2`, `npc_elara_walk_left_3`, `npc_elara_walk_right_0`, `npc_elara_walk_right_1`, `npc_elara_walk_right_2`, `npc_elara_walk_right_3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Villager: Elara (clinic nurse). A sprite sheet laid out as a strict grid of 4 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Elara, the clinic nurse: gentle woman, pale blue uniform and cap, dark purple hair in twin tails, a small satchel of herbs.  Same chibi proportions, line weight and palette as the player. Row 1: standing idle facing down, up, left, right. Rows 2 to 5: four-frame walk cycles facing down, up, left, right. Friendly expression; distinct silhouette and colour scheme from the other villagers.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Keeper ritual outfits (the five Keepers)

- **Final cell size:** 16 x 32 px  |  **Grid:** 5 columns x 2 rows  |  **Generate size:** 960 x 768 px (5 columns x 2 rows)
- **Background:** transparent
- **Cell order:** `npc_tilda_robe_down`, `npc_tilda_robe_side`, `npc_marcus_robe_down`, `npc_marcus_robe_side`, `npc_odalys_robe_down`, `npc_odalys_robe_side`, `npc_dorian_robe_down`, `npc_dorian_robe_side`, `npc_wren_robe_down`, `npc_wren_robe_side`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Keeper ritual outfits (the five Keepers). A sprite sheet laid out as a strict grid of 5 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
The five Keepers of the Covenant in ritual robes: long hooded robes in deep moss green with an embroidered pale-gold knot symbol on the chest, hoods casting shadow over the eyes, calm still posture. Each keeps a hint of their own identity (Tilda's spectacles glint, Marcus's cap-shaped hood, Odalys taller with a pendant, Dorian's scarf, Wren's red trim). The look is solemn and mysterious but still storybook-gentle: dignified, never frightening, no weapons, no blood.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```
