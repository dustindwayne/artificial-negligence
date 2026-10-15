# Sprite refresh — 2.2

> Status for 2.4.2: Historical 2.2 prompt record with current fitting notes below. Source prompts describe requests, not a claim that the raw generated cells were perfectly aligned.

Creative direction and references: Dustin (dustindwayne). Looper is based on the character sheet supplied by the creator. The refreshed assets were generated or edited using the built-in `image_gen.imagegen` tool. Generated images are retained as sources; measured layouts and silhouette masks isolate each runtime animation frame. Current runtime frames use shared animation scales and grounded baselines: characters and equipment 64px with four-pixel gutters; bot/boss frames 192×120 with three-pixel gutters; ships 128×64 with two-pixel gutters. Measured bounds and masks isolate effects that cross nominal source cells.

## Assets

- `unity/Assets/Resources/Art/looper.png`: starting survivor, eight poses.
- `unity/Assets/Resources/Art/survivors-v3.png`: Rook, Vale and Mender, eight poses each.
- `unity/Assets/Resources/Art/enemies-v3.png`: six enemy classes and three bosses, four frames each.
- `unity/Assets/Resources/Art/items-v3.png`: sixteen collectible and equipment icons.
- Existing background ship sheet: remeasured and isolated with the same runtime padding.

## Prompt record

### Survivor atlas

Use case: stylized-concept. Production pixel-art animation sprite atlas for Artificial Negligence. Transparent background, exact regular grid 8 columns by 3 rows, wide landscape image. Three distinct playable street survivors, one character per row, all facing right. Row 1 dark-skinned hooded unhoused man with patched olive coat, backpack and gold accents. Row 2 dark-skinned woman mechanic with short curls, cyan patched jacket, utility belt. Row 3 brown-skinned older street medic with cream hood/scarf, purple coat and satchel. Consistent side-on platformer pixel sprites, simple readable silhouettes, hand-drawn retro pixel style, restricted palette, not glossy illustrations. Each row has 8 poses in order: idle, run left stride, run right stride, airborne, firing handheld pulse weapon, recoil/hurt, crouch, landing. All sprites same visual height and grounded feet baseline per cell, no text or grid lines. STRICT complete body, backpack, muzzle and limb containment in each cell, at least 15% transparent margin on every cell edge, no overlap between cells, no cropped heads/feet/weapons, generous gutters. No scenery, no shadows outside cell, genuine transparency.

### Survivor cleanup

Edit target: the attached survivor sprite atlas. Preserve all three characters and all 24 poses. Remove ALL brown/gray haze and background atmosphere, retaining only the opaque crisp pixel character pixels and compact muzzle effects on genuine transparent background. This is a game sprite sheet, not a painting. Reposition each complete character into exactly 8 equally spaced columns and 3 equally spaced rows, with 12 percent transparent gutters inside EVERY cell. Shrink firing muzzle effects so they never cross into the next cell. No pose may touch any cell boundary. Consistent size per character row, feet baseline for idle and running, airborne pose raised slightly. Pixel art, no scenery, no labels, no gridlines, no ambient clouds, no backdrop.

### Enemy atlas

Use case: stylized-concept. Edit this enemy sprite atlas for Artificial Negligence, preserving nine distinct enemy designs and their four poses per row. Make an EXACT uniformly spaced 4 columns by 9 rows grid, transparent background. Rows: blue scout biped, magenta gunner, gold armored charger, cyan shield biped, green mechanical quadruped swarm creature, purple long rifle sniper, large cyan shield Auditor boss, magenta claw Harvester boss, gold heavy cannon Executor boss. Columns idle, walking stride A, walking stride B, attack windup/fire. Pixel-art platformer style, preserve designs and colors. Every complete sprite fits INSIDE its equal-size cell with 12% transparent margins around head, feet, rifle, shield, claws and muzzle. No artwork crosses cell boundaries, no heads or rifles cropped, no connecting effects, no text, no grid lines. All frames within a row share one feet baseline and consistent body size. Genuine transparency, clean readable pixels.

### Equipment atlas

Use case: stylized-concept. Edit and expand this pixel-art item sheet for Artificial Negligence. Exact evenly spaced grid 4 columns by 4 rows, genuine transparent background, complete separate centered icons each with at least 15 percent transparent margins, no touching other cells, no clipping or text or grid. Uniform readable retro hand-drawn pixel-art style, cyan gold cream purple accents, dark outlines. Row1: collectible gold GPU chip, purple loot crate, cyan intellect terminal, gold uplink tower. Row2: patched running boots, handheld Pulse pistol, balanced patched protective vest, blue powerup diamond. Row3: cyan Arc emitter gun, magenta Scatter shotgun, gold Rail long rifle, gold Kinetic armored coat. Row4: cyan Insulated jacket, purple lightweight armor vest, cyan capacitor module, gold stabilizer module. Each icon completely inside equal cell; consistent visual scale. Keep recognizable designs from reference row1 and row2. No letters or numbers, no background scenery.

### Looper adaptation

Use case stylized-concept. Create a corrected production sprite atlas for Looper in EXACT SAME PIXEL ART STYLE AND BODY PROPORTIONS as the other survivor atlas reference (second image). First image is Looper identity reference: curly brown hair, rectangular black glasses, warm skin, patched black jacket with blue trim over gray shirt, fingerless gloves, backpack glowing blue core, white/blue shoes. Preserve Looper identity while matching the other survivors' grounded street-survivor proportions, limited pixel palette, crisp blocky pixel shading, dark outline and detail density. A game atlas only, genuine transparent background, exact 4 columns by 2 rows. Eight distinct full-body right-facing poses: row1 idle, run stride A, run stride B, jumping; row2 firing small handheld pistol, special ability hands raised, crouching, landing knees bent. NO MUZZLE FLASH, NO ENERGY BALLS, NO GLOW CLOUDS, NO DUST, NO PROJECTILES, NO AMBIENT HAZE OR BACKGROUND. Effects are rendered separately by the game. Every single pose fits fully inside its cell with at least 18 percent clear gutter on all four sides. Shoes, hands, backpack, hair and small weapon fully contained. The 8 silhouettes must be separated by broad empty transparent space. Consistent character height, not a caricature or enlarged head, no text or grid lines.


## Review evidence

[Frame audit](sprite-audit.json). Runtime captures: [Looper](atlas-looper.png), [survivors](atlas-survivors.png), [enemies and bosses](atlas-enemies.png), [equipment](atlas-equipment.png), [background sprites](atlas-ships.png). All source silhouettes remain inside the image bounds; normalized frames retain clear two-pixel borders. Wide sprites preserve their aspect ratio, and animation groups share a consistent scale.


## Current 2.4.2 additions

Sinister agents add twelve poses and Ai Wrath adds eight poses. The raw PNG masters are preserved; runtime masks and measured layouts prevent cross-frame bleed. The validator checks 108 native atlas frames, with minimum clear edge gutters. Full-resolution masked portraits are used in short narrative scenes. This is pixel-style generated art, not a claim of creator-drawn final assets.
