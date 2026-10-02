# Sprite prompts: Item icons

Read `STYLE_GUIDE.md` first. Every prompt below already contains the shared STYLE BLOCK. Sheet sizes follow `docs/ART_ASSETS.md`; file names are the placeholder names to replace (cell names for animation frames are new: the game does not animate yet).

## Tools

- **Final cell size:** 16 x 16 px  |  **Grid:** 8 columns x 2 rows  |  **Generate size:** 1536 x 384 px (8 columns x 2 rows)
- **Background:** transparent
- **Cell order:** `item_tool_axe`, `item_tool_hoe`, `item_tool_pickaxe`, `item_tool_rod`, `item_tool_scythe`, `item_tool_sword`, `item_tool_sword_gold`, `item_tool_sword_steel`, `item_tool_wateringcan`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Tools. A sprite sheet laid out as a strict grid of 8 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_tool_axe: a hand axe with a wooden handle and a steel head
- item_tool_hoe: a long-handled garden hoe
- item_tool_pickaxe: a double-headed pickaxe
- item_tool_rod: a bamboo fishing rod with a red bobber
- item_tool_scythe: a curved scythe
- item_tool_sword: a short bronze sword (tier 1)
- item_tool_sword_gold: a short golden sword with a leaf guard
- item_tool_sword_steel: a silver-steel sword
- item_tool_wateringcan: a blue tin watering can with a long spout
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Tool upgrade tiers

- **Final cell size:** 16 x 16 px  |  **Grid:** 5 columns x 5 rows  |  **Generate size:** 960 x 960 px (5 columns x 5 rows)
- **Background:** transparent
- **Cell order:** `item_tool_axe_basic`, `item_tool_axe_copper`, `item_tool_axe_steel`, `item_tool_axe_gold`, `item_tool_axe_iridium`, `item_tool_hoe_basic`, `item_tool_hoe_copper`, `item_tool_hoe_steel`, `item_tool_hoe_gold`, `item_tool_hoe_iridium`, `item_tool_pickaxe_basic`, `item_tool_pickaxe_copper`, `item_tool_pickaxe_steel`, `item_tool_pickaxe_gold`, `item_tool_pickaxe_iridium`, `item_tool_wateringcan_basic`, `item_tool_wateringcan_copper`, `item_tool_wateringcan_steel`, `item_tool_wateringcan_gold`, `item_tool_wateringcan_iridium`, `item_tool_scythe_basic`, `item_tool_scythe_copper`, `item_tool_scythe_steel`, `item_tool_scythe_gold`, `item_tool_scythe_iridium`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Tool upgrade tiers. A sprite sheet laid out as a strict grid of 5 columns by 5 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Rows: axe, hoe, pickaxe, watering can, scythe. Columns: basic (grey-brown), copper (warm orange head), steel (cool silver-blue), gold (bright butter yellow with sparkle pixel), iridium (soft violet with a pearly sheen). The same tool shape in each row, only material colour changes.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Seed packets

- **Final cell size:** 16 x 16 px  |  **Grid:** 9 columns x 4 rows  |  **Generate size:** 1728 x 768 px (9 columns x 4 rows)
- **Background:** transparent
- **Cell order:** `item_seed_artichoke`, `item_seed_ashfruit`, `item_seed_beet`, `item_seed_blueberry`, `item_seed_cauliflower`, `item_seed_corn`, `item_seed_cranberry`, `item_seed_cucumber`, `item_seed_eggplant`, `item_seed_generic`, `item_seed_greenbean`, `item_seed_hollowroot`, `item_seed_hops`, `item_seed_kale`, `item_seed_melon`, `item_seed_mutant_gourd`, `item_seed_mutant_leaf`, `item_seed_mutant_root`, `item_seed_nightbloom`, `item_seed_onion`, `item_seed_parsnip`, `item_seed_pepper`, `item_seed_potato`, `item_seed_pumpkin`, `item_seed_radish`, `item_seed_spinach`, `item_seed_strawberry`, `item_seed_sunflower`, `item_seed_tomato`, `item_seed_tree_apple`, `item_seed_tree_cherry`, `item_seed_tree_peach`, `item_seed_tree_persimmon`, `item_seed_wheat`, `item_seed_yam`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Seed packets. A sprite sheet laid out as a strict grid of 9 columns by 4 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_seed_artichoke: a small paper seed packet with a cream label showing a tiny picture of the artichoke plant; trees show a small potted sapling
- item_seed_ashfruit: a small paper seed packet with a cream label showing a tiny picture of the ashfruit plant; trees show a small potted sapling
- item_seed_beet: a small paper seed packet with a cream label showing a tiny picture of the beet plant; trees show a small potted sapling
- item_seed_blueberry: a small paper seed packet with a cream label showing a tiny picture of the blueberry plant; trees show a small potted sapling
- item_seed_cauliflower: a small paper seed packet with a cream label showing a tiny picture of the cauliflower plant; trees show a small potted sapling
- item_seed_corn: a small paper seed packet with a cream label showing a tiny picture of the corn plant; trees show a small potted sapling
- item_seed_cranberry: a small paper seed packet with a cream label showing a tiny picture of the cranberry plant; trees show a small potted sapling
- item_seed_cucumber: a small paper seed packet with a cream label showing a tiny picture of the cucumber plant; trees show a small potted sapling
- item_seed_eggplant: a small paper seed packet with a cream label showing a tiny picture of the eggplant plant; trees show a small potted sapling
- item_seed_generic: a plain paper seed packet
- item_seed_greenbean: a small paper seed packet with a cream label showing a tiny picture of the greenbean plant; trees show a small potted sapling
- item_seed_hollowroot: a small paper seed packet with a cream label showing a tiny picture of the hollowroot plant; trees show a small potted sapling
- item_seed_hops: a small paper seed packet with a cream label showing a tiny picture of the hops plant; trees show a small potted sapling
- item_seed_kale: a small paper seed packet with a cream label showing a tiny picture of the kale plant; trees show a small potted sapling
- item_seed_melon: a small paper seed packet with a cream label showing a tiny picture of the melon plant; trees show a small potted sapling
- item_seed_mutant_gourd: a small paper seed packet with a cream label showing a tiny picture of the mutant gourd plant; trees show a small potted sapling
- item_seed_mutant_leaf: a small paper seed packet with a cream label showing a tiny picture of the mutant leaf plant; trees show a small potted sapling
- item_seed_mutant_root: a small paper seed packet with a cream label showing a tiny picture of the mutant root plant; trees show a small potted sapling
- item_seed_nightbloom: a small paper seed packet with a cream label showing a tiny picture of the nightbloom plant; trees show a small potted sapling
- item_seed_onion: a small paper seed packet with a cream label showing a tiny picture of the onion plant; trees show a small potted sapling
- item_seed_parsnip: a small paper seed packet with a cream label showing a tiny picture of the parsnip plant; trees show a small potted sapling
- item_seed_pepper: a small paper seed packet with a cream label showing a tiny picture of the pepper plant; trees show a small potted sapling
- item_seed_potato: a small paper seed packet with a cream label showing a tiny picture of the potato plant; trees show a small potted sapling
- item_seed_pumpkin: a small paper seed packet with a cream label showing a tiny picture of the pumpkin plant; trees show a small potted sapling
- item_seed_radish: a small paper seed packet with a cream label showing a tiny picture of the radish plant; trees show a small potted sapling
- item_seed_spinach: a small paper seed packet with a cream label showing a tiny picture of the spinach plant; trees show a small potted sapling
- item_seed_strawberry: a small paper seed packet with a cream label showing a tiny picture of the strawberry plant; trees show a small potted sapling
- item_seed_sunflower: a small paper seed packet with a cream label showing a tiny picture of the sunflower plant; trees show a small potted sapling
- item_seed_tomato: a small paper seed packet with a cream label showing a tiny picture of the tomato plant; trees show a small potted sapling
- item_seed_tree_apple: a small paper seed packet with a cream label showing a tiny picture of the apple plant; trees show a small potted sapling
- item_seed_tree_cherry: a small paper seed packet with a cream label showing a tiny picture of the cherry plant; trees show a small potted sapling
- item_seed_tree_peach: a small paper seed packet with a cream label showing a tiny picture of the peach plant; trees show a small potted sapling
- item_seed_tree_persimmon: a small paper seed packet with a cream label showing a tiny picture of the persimmon plant; trees show a small potted sapling
- item_seed_wheat: a small paper seed packet with a cream label showing a tiny picture of the wheat plant; trees show a small potted sapling
- item_seed_yam: a small paper seed packet with a cream label showing a tiny picture of the yam plant; trees show a small potted sapling
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Harvested crops

- **Final cell size:** 16 x 16 px  |  **Grid:** 9 columns x 4 rows  |  **Generate size:** 1728 x 768 px (9 columns x 4 rows)
- **Background:** transparent
- **Cell order:** `item_crop_artichoke`, `item_crop_ashfruit`, `item_crop_beet`, `item_crop_blueberry`, `item_crop_cauliflower`, `item_crop_corn`, `item_crop_cranberry`, `item_crop_cucumber`, `item_crop_eggplant`, `item_crop_generic`, `item_crop_greenbean`, `item_crop_hollowroot`, `item_crop_hops`, `item_crop_kale`, `item_crop_melon`, `item_crop_mutant_gourd`, `item_crop_mutant_leaf`, `item_crop_mutant_root`, `item_crop_nightbloom`, `item_crop_onion`, `item_crop_parsnip`, `item_crop_pepper`, `item_crop_potato`, `item_crop_pumpkin`, `item_crop_radish`, `item_crop_spinach`, `item_crop_strawberry`, `item_crop_sunflower`, `item_crop_tomato`, `item_crop_tree_apple`, `item_crop_tree_cherry`, `item_crop_tree_peach`, `item_crop_tree_persimmon`, `item_crop_wheat`, `item_crop_yam`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Harvested crops. A sprite sheet laid out as a strict grid of 9 columns by 4 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_crop_artichoke: the freshly harvested artichoke, plump and bright
- item_crop_ashfruit: the freshly harvested ashfruit, plump and bright
- item_crop_beet: the freshly harvested beet, plump and bright
- item_crop_blueberry: the freshly harvested blueberry, plump and bright
- item_crop_cauliflower: the freshly harvested cauliflower, plump and bright
- item_crop_corn: the freshly harvested corn, plump and bright
- item_crop_cranberry: the freshly harvested cranberry, plump and bright
- item_crop_cucumber: the freshly harvested cucumber, plump and bright
- item_crop_eggplant: the freshly harvested eggplant, plump and bright
- item_crop_generic: a generic vegetable
- item_crop_greenbean: the freshly harvested greenbean, plump and bright
- item_crop_hollowroot: the freshly harvested hollowroot, plump and bright
- item_crop_hops: the freshly harvested hops, plump and bright
- item_crop_kale: the freshly harvested kale, plump and bright
- item_crop_melon: the freshly harvested melon, plump and bright
- item_crop_mutant_gourd: the freshly harvested mutant gourd, plump and bright
- item_crop_mutant_leaf: the freshly harvested mutant leaf, plump and bright
- item_crop_mutant_root: the freshly harvested mutant root, plump and bright
- item_crop_nightbloom: the freshly harvested nightbloom, plump and bright
- item_crop_onion: the freshly harvested onion, plump and bright
- item_crop_parsnip: the freshly harvested parsnip, plump and bright
- item_crop_pepper: the freshly harvested pepper, plump and bright
- item_crop_potato: the freshly harvested potato, plump and bright
- item_crop_pumpkin: the freshly harvested pumpkin, plump and bright
- item_crop_radish: the freshly harvested radish, plump and bright
- item_crop_spinach: the freshly harvested spinach, plump and bright
- item_crop_strawberry: the freshly harvested strawberry, plump and bright
- item_crop_sunflower: the freshly harvested sunflower, plump and bright
- item_crop_tomato: the freshly harvested tomato, plump and bright
- item_crop_tree_apple: the freshly harvested apple, plump and bright
- item_crop_tree_cherry: the freshly harvested cherry, plump and bright
- item_crop_tree_peach: the freshly harvested peach, plump and bright
- item_crop_tree_persimmon: the freshly harvested persimmon, plump and bright
- item_crop_wheat: the freshly harvested wheat, plump and bright
- item_crop_yam: the freshly harvested yam, plump and bright
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Forage

- **Final cell size:** 16 x 16 px  |  **Grid:** 8 columns x 2 rows  |  **Generate size:** 1536 x 384 px (8 columns x 2 rows)
- **Background:** transparent
- **Cell order:** `item_forage_blackberry`, `item_forage_clam`, `item_forage_dandelion`, `item_forage_elderflower`, `item_forage_hazelnut`, `item_forage_mushroom`, `item_forage_pearl`, `item_forage_raspberry`, `item_forage_seashell`, `item_forage_snowdrop`, `item_forage_truffle`, `item_forage_wildgarlic`, `item_forage_winterroot`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Forage. A sprite sheet laid out as a strict grid of 8 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_forage_blackberry: a cluster of dark purple berries
- item_forage_clam: a fan-shaped clam shell
- item_forage_dandelion: a yellow dandelion flower
- item_forage_elderflower: a white elderflower umbel
- item_forage_hazelnut: two brown hazelnuts
- item_forage_mushroom: a brown-capped forest mushroom
- item_forage_pearl: a small shimmering pearl
- item_forage_raspberry: three red raspberries
- item_forage_seashell: a pink spiral seashell
- item_forage_snowdrop: a white drooping snowdrop flower
- item_forage_truffle: a lumpy dark truffle
- item_forage_wildgarlic: wild garlic leaves with white star flowers
- item_forage_winterroot: a knobbly pale root with frosty tips
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Fish

- **Final cell size:** 16 x 16 px  |  **Grid:** 5 columns x 4 rows  |  **Generate size:** 960 x 768 px (5 columns x 4 rows)
- **Background:** transparent
- **Cell order:** `item_fish_anchovy`, `item_fish_bream`, `item_fish_carp`, `item_fish_catfish`, `item_fish_eel`, `item_fish_flounder`, `item_fish_glass_minnow`, `item_fish_herring`, `item_fish_mackerel`, `item_fish_moonfish`, `item_fish_octopus`, `item_fish_perch`, `item_fish_pike`, `item_fish_pufferfish`, `item_fish_sardine`, `item_fish_sea_bass`, `item_fish_squid`, `item_fish_sturgeon`, `item_fish_trout`, `item_fish_tuna`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Fish. A sprite sheet laid out as a strict grid of 5 columns by 4 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_fish_anchovy: a tiny silver-blue fish
- item_fish_bream: a deep-bodied silver bream
- item_fish_carp: a golden-bronze carp
- item_fish_catfish: a whiskered grey-brown catfish
- item_fish_eel: a long olive eel
- item_fish_flounder: a flat sandy flounder with both eyes on top
- item_fish_glass_minnow: a nearly transparent tiny fish with pink gills
- item_fish_herring: a slim silver herring
- item_fish_mackerel: a striped blue-green mackerel
- item_fish_moonfish: a round pale-silver fish with a faint glow (night only)
- item_fish_octopus: a small purple octopus
- item_fish_perch: a green striped perch with orange fins
- item_fish_pike: a long toothy green pike
- item_fish_pufferfish: a round spiky puffer, cheerful
- item_fish_sardine: a small shiny sardine
- item_fish_sea_bass: a grey-silver sea bass
- item_fish_squid: a pink-cream squid
- item_fish_sturgeon: a long armoured grey sturgeon
- item_fish_trout: a speckled rainbow trout
- item_fish_tuna: a big blue torpedo-shaped tuna
All fish drawn flat in side view facing left, each with a distinct silhouette and colour.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Cooked food

- **Final cell size:** 16 x 16 px  |  **Grid:** 5 columns x 2 rows  |  **Generate size:** 960 x 384 px (5 columns x 2 rows)
- **Background:** transparent
- **Cell order:** `item_food_bean_stew`, `item_food_berry_tart`, `item_food_corn_chowder`, `item_food_forager_plate`, `item_food_gratin`, `item_food_mashed_potato`, `item_food_pumpkin_pie`, `item_food_roasted_roots`, `item_food_salad`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Cooked food. A sprite sheet laid out as a strict grid of 5 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_food_bean_stew: a bowl of bean stew with steam
- item_food_berry_tart: a golden tart topped with glazed berries
- item_food_corn_chowder: a creamy bowl with corn kernels
- item_food_forager_plate: a plate of mushrooms, nuts and berries
- item_food_gratin: a bubbling golden cheese gratin dish
- item_food_mashed_potato: a mound of mashed potato with butter
- item_food_pumpkin_pie: a slice of orange pumpkin pie with cream
- item_food_roasted_roots: a tray of roasted root vegetables
- item_food_salad: a bowl of fresh green salad
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Artisan goods

- **Final cell size:** 16 x 16 px  |  **Grid:** 4 columns x 1 rows  |  **Generate size:** 768 x 192 px (4 columns x 1 rows)
- **Background:** transparent
- **Cell order:** `item_artisan_jam`, `item_artisan_juice`, `item_artisan_pickles`, `item_artisan_wine`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Artisan goods. A sprite sheet laid out as a strict grid of 4 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_artisan_jam: a glass jar of red jam with a gingham lid
- item_artisan_juice: a glass bottle of orange juice with a cork
- item_artisan_pickles: a jar of green pickles in brine
- item_artisan_wine: a dark green bottle of purple wine with a label
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Animal products

- **Final cell size:** 16 x 16 px  |  **Grid:** 6 columns x 1 rows  |  **Generate size:** 1152 x 192 px (6 columns x 1 rows)
- **Background:** transparent
- **Cell order:** `item_product_duck_egg`, `item_product_egg`, `item_product_goat_milk`, `item_product_milk`, `item_product_rabbit_wool`, `item_product_wool`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Animal products. A sprite sheet laid out as a strict grid of 6 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_product_duck_egg: a pale blue-green duck egg
- item_product_egg: a cream-white hen's egg
- item_product_goat_milk: a bottle of goat's milk
- item_product_milk: a glass bottle of cow's milk
- item_product_rabbit_wool: a soft fluffy tuft of angora wool
- item_product_wool: a skein of cream sheep wool
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Animals (purchase icons)

- **Final cell size:** 16 x 16 px  |  **Grid:** 6 columns x 1 rows  |  **Generate size:** 1152 x 192 px (6 columns x 1 rows)
- **Background:** transparent
- **Cell order:** `item_animal_chicken`, `item_animal_cow`, `item_animal_duck`, `item_animal_goat`, `item_animal_rabbit`, `item_animal_sheep`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Animals (purchase icons). A sprite sheet laid out as a strict grid of 6 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_animal_chicken: a small cute chicken portrait icon
- item_animal_cow: a small cute cow portrait icon
- item_animal_duck: a small cute duck portrait icon
- item_animal_goat: a small cute goat portrait icon
- item_animal_rabbit: a small cute rabbit portrait icon
- item_animal_sheep: a small cute sheep portrait icon
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Resources and drops

- **Final cell size:** 16 x 16 px  |  **Grid:** 8 columns x 2 rows  |  **Generate size:** 1536 x 384 px (8 columns x 2 rows)
- **Background:** transparent
- **Cell order:** `item_resource_bait`, `item_resource_bat_wing`, `item_resource_bone`, `item_resource_coal`, `item_resource_copperbar`, `item_resource_copperore`, `item_resource_ember`, `item_resource_fiber`, `item_resource_goldbar`, `item_resource_goldore`, `item_resource_ironbar`, `item_resource_ironore`, `item_resource_slime`, `item_resource_stone`, `item_resource_warden_core`, `item_resource_wood`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Resources and drops. A sprite sheet laid out as a strict grid of 8 columns by 2 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_resource_bait: a wriggling worm on a tiny hook
- item_resource_bat_wing: a leathery bat wing
- item_resource_bone: a small clean bone
- item_resource_coal: a lump of black coal with a shine
- item_resource_copperbar: an orange-copper ingot
- item_resource_copperore: a grey rock with copper-orange veins
- item_resource_ember: a glowing ember in a pinch of ash
- item_resource_fiber: a bundle of green plant fibre
- item_resource_goldbar: a bright gold ingot
- item_resource_goldore: a grey rock with gold flecks
- item_resource_ironbar: a cool grey iron ingot
- item_resource_ironore: a grey rock with rusty red veins
- item_resource_slime: a friendly green blob of slime
- item_resource_stone: a plain grey stone
- item_resource_warden_core: a purple crystal core with a faint glow
- item_resource_wood: a short bundle of brown logs
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Machines and placeables (inventory icons)

- **Final cell size:** 16 x 16 px  |  **Grid:** 8 columns x 1 rows  |  **Generate size:** 1536 x 192 px (8 columns x 1 rows)
- **Background:** transparent
- **Cell order:** `item_machine_chest`, `item_machine_furnace`, `item_machine_jar`, `item_machine_keg`, `item_machine_scarecrow`, `item_machine_sprinkler1`, `item_machine_sprinkler2`, `item_machine_sprinkler3`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Machines and placeables (inventory icons). A sprite sheet laid out as a strict grid of 8 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_machine_chest: a small wooden chest with iron bands
- item_machine_furnace: a stone furnace with a glowing mouth
- item_machine_jar: a preserves jar machine with a lid
- item_machine_keg: a wooden keg with brass bands
- item_machine_scarecrow: a friendly straw scarecrow in a hat
- item_machine_sprinkler1: a small copper sprinkler head
- item_machine_sprinkler2: a medium brass sprinkler head
- item_machine_sprinkler3: an iridescent advanced sprinkler head
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Fertilizer

- **Final cell size:** 16 x 16 px  |  **Grid:** 2 columns x 1 rows  |  **Generate size:** 384 x 192 px (2 columns x 1 rows)
- **Background:** transparent
- **Cell order:** `item_fertilizer_quality`, `item_fertilizer_speed`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Fertilizer. A sprite sheet laid out as a strict grid of 2 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Each cell is one inventory icon 16x16, a single object viewed slightly from above, centred, readable at small size, with a one-pixel dark brown outline and a soft drop-shadow pixel row below. Items in order:
- item_fertilizer_quality: a small cloth sack of quality-boosting fertilizer, tied with string
- item_fertilizer_speed: a small cloth sack of growth-speeding fertilizer, tied with string
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```

## Quality stars and badges

- **Final cell size:** 16 x 16 px  |  **Grid:** 6 columns x 1 rows  |  **Generate size:** 1152 x 192 px (6 columns x 1 rows)
- **Background:** transparent
- **Cell order:** `quality_silver`, `quality_gold`, `quality_iridium`, `badge_tool_tier`, `badge_new`, `badge_marked`

```text
PROMPT
STYLE BLOCK (identical in every prompt of this project):
Cozy hand-crafted pixel art for a warm countryside farming game, top-down three-quarter view (camera looking slightly down, like a storybook diorama). Soft, gentle, friendly shapes with slightly rounded corners and chunky readable silhouettes. Limited warm palette of about 24 colours shared across all art: cream #F4E6C3, butter yellow #F2C14E, honey #D9983B, terracotta #B8553A, rosewood #7A3B3B, moss green #6E9E4F, leaf green #4F8A3F, deep pine #2F5D3A, sky teal #5FA8A0, dusk blue #4A6FA5, lavender #8E7CC3, bark brown #6B4A2F, soil brown #4A3224, warm stone grey #8C8577. Outlines are one pixel of warm dark brown #3A2618 (never pure black), with a lighter inner highlight on the upper-left. Light comes from the upper left; shading uses two soft tones per colour with no gradients and no dithering noise. Cheerful, calm, nostalgic mood in late-afternoon golden light. Crisp hard pixel edges, no anti-aliasing, no blur, transparent background unless stated. Everything is drawn on a strict square pixel grid at 16 pixels per tile.

SHEET: Quality stars and badges. A sprite sheet laid out as a strict grid of 6 columns by 1 rows of equal square cells, each cell separated by an empty margin, every sprite centred in its cell, none overlapping, all in the same scale and style.
Small 8x8 symbols centred in 16x16 cells: a silver star, a gold star, a violet-pearl star, a tool tier chevron, a small 'new' sparkle, and a tiny red thread knot mark meaning 'marked for the ritual'.
NEGATIVE: photorealism, 3D render, smooth gradients, anti-aliased edges, blur, black outlines, neon colours, harsh contrast, gore, blood, violence, scary faces, text, watermark, signature, extra limbs, inconsistent perspective, different art style, background scenery (unless requested).
```
