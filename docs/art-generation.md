# Generated art record

> Status for 2.4.2: Historical generation record. This v2 atlas is retained as a source; the active enemy atlas is enemies-v3.png, plus sinister-agents.png and ai-wrath.png. See sprite-refresh.md for current frame fitting.

Tool: built-in image_gen (transparent background). Final asset: `unity/Assets/Resources/Art/enemy-atlas-v2.png`.

Prompt: Production pixel-art animation spritesheet for Artificial Negligence, transparent background. Four equal columns, nine equal rows, ideally 1024x1152. No text or labels or grid lines. Each cell contains one side-view character facing left, feet aligned, generous clear gutters. Four frames per row: idle, walk stride A, walk stride B, attack anticipation. Rows: triangular two-legged cyan scout bot; magenta gun-arm sentry; heavy gold armored wheeled charger; cyan shield robot; lime-eyed four-legged mechanical scavenger creature; violet long-barrel sniper; imposing circular-shield Auditor boss; mechanical insect-claw Harvester boss; massive armored Executor boss with gold shoulder plates and magenta core. Bold distinct silhouettes, crisp visible square pixel clusters, navy shadows, cream highlights, cyan/gold/magenta palette, no smooth painterly gradients. Comparable scale across normal rows, wide imposing bosses. Entirely transparent true alpha between sprites; all characters centered within the central 75% of evenly spaced cells, no overlap.

The returned atlas uses slightly varied row heights. Unity imports each row using measured bounds and displays it with point filtering. The legacy source PNG is retained in the project for provenance; current runtime assets are identified in [sprite-refresh.md](sprite-refresh.md).
