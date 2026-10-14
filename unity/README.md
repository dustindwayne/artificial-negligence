# Artificial Negligence — Unity edition

Unity 6000.6.5f1 project using the Universal Render Pipeline 2D renderer. Open `Assets/Scenes/ArtificialNegligence.unity` and press Play.

The current build has 30 generated city stages across five districts, connected city-map progression, rooftop routes, checkpoints, dash, animated enemies, boss uplinks, layered scenery and interactive logic terminals.

Touch movement, jump, pulse, gear, upgrades, and pause controls are built as editable Unity UI. Keyboard controls: A/D or arrows; Space to jump; J/F to fire; E to solve; U/G for upgrades/gear; Escape to pause.

Weapon and armor tiers: Common 1, Rare 2, Epic 3, Legend 4, Ultra 5. Points upgrade through Legend. Ultra is exclusive to one outcome in 200 per box (0.5%). Boots, self running speed, vitality, and gear carry across districts. Completing a district restores two health.

Wrong puzzle answers consume the terminal's attempt and grant no powerup. Closing and reopening preserves its puzzle. Correct answers grant exactly one reward.

`Assets/Resources/Data/levels.csv` is the level layout. Art is in `Assets/Resources/Art`. The supplied logo is the icon and title artwork; `Assets/UI-Reference.png` preserves the supplied visual reference. The VT323 pixel font is distributed under the SIL Open Font License in `Assets/Resources/Fonts/OFL.txt`.

## Build

Install Android Build Support for the matching Unity editor. `BuildGame.Android` configures URP, validates gameplay rules and pickup reachability with Unity Physics2D, and builds an ARM64 APK. `BuildGame.Desktop` produces a Windows test player. Android requires SDK 35+, NDK r27c, and JDK 17; the build helper uses the tools prepared under this task's `work` directory when run here.

Example: `Unity.exe -batchmode -nographics -quit -projectPath <project-directory> -buildTarget Android -executeMethod BuildGame.Android -logFile <build-log>`.

No ads, analytics, or network gameplay. Build outputs are debug-signed for testing.

## Progression update (1.1)

Endless districts remix six reachable normal/mirrored routes across three city environments. Each district changes the enemy lineup and random boxes; reinforcement count, health, movement and shot speeds increase with district number. Every district has an uplink boss. Defeat it and collect all 16 GPUs to advance.

Six classes: scout, gunner, charger, shielded Aegis, fast swarm, long-range sniper. Bosses rotate Auditor (shield and three-shot fans), Harvester (five-shot fans, seven when enraged), Executor (alternating charges and shots). All bosses telegraph attacks and enrage below half health.

XP from GPUs, terminals, enemies, bosses and district completion raises your character level. Each level grants a talent and 25 points. Points buy speed, vitality and gear tiers; talent plus points buys weapon mastery or defense training. Training caps at 20. Levels 3/5/7 unlock Arc/Scatter/Rail and alternate armor; levels 3/5 unlock additional modules. Tier upgrades apply across weapon and armor styles; Ultra is still box-only at exactly 0.5%.

Loadouts can be changed through Gear. Arc doubles damage against shielded units and chains to nearby enemies, but struggles against heavy armor. Scatter fires three short-range pellets. Rail pierces and doubles heavy-armor damage, but shields resist it. Kinetic armor reduces contact damage but increases energy damage; insulated armor does the opposite; light armor increases speed and incoming damage. Capacitor improves Arc and fire rate; stabilizer improves Rail; sprinter adds speed.

Character XP, points, upgrades, equipment and district checkpoint save locally. Retry restores health and starts the current district again; individual pickup/enemy states within an unfinished district reset. No cloud account required.

## Music update (1.2)

Six supplied tracks are encoded as 32 kHz Vorbis OGG (quality 1), preserving their full duration and channel layout. Source MP3s are untouched. Combined compressed source assets: 3,059,384 bytes versus 8,741,315 bytes supplied (65% smaller). Unity imports music as streaming Vorbis with 32 kHz sample rate and Android quality 0.25 to keep memory use low.

Menu and settings use menu; gameplay uses gameplay; gear and upgrade screens use upgrades. Entering an undefeated boss's range switches to bosscombat until it is defeated. District clear/victory and game-over use their respective non-looping cues. Other tracks loop. Transitions crossfade over 0.7 seconds. Pause and puzzles lower the music volume; the Sound setting controls both music and effects.

## City traversal update (2.0)

The city map connects 30 generated stages through Street Settlement, Market Rooftops, Transit Works, Data Gardens and Corporate Spire. Every sixth stage is a boss district. Later city cycles continue generating routes. Local character upgrades and unlocked stages persist; completed stages can be replayed for GPU mastery stars.

Stages have 7-9 ground sections, two-tier roof routes, optional moving lifts, timed power vents and checkpoint beacons. Reach an uplink with at least 60% of its GPUs, and defeat its boss when present. Every GPU remains reachable with base movement; full collection earns three mastery stars and a points bonus.

Movement adds acceleration, air control, variable jump height, dash (Shift/K on keyboard), landing particles and camera damping. Scenery includes parallax building layers, street fixtures, foreground foundations and rain. Nine enemy/boss sprite rows include four animation poses each, readable health bars and attack windups.

Twelve puzzle families include executable-code tracing, route cost, queue arithmetic, deduction and interactive four-switch XOR circuits. Correct submissions give one powerup; wrong submissions give none. A result screen explains the answer. Closing a puzzle preserves its state.

See CREDITS.md: the creator owns the original concept, art-style direction and overall creative vision; AI assistance was used in implementation and generated assets.

## GPU-powered graphics progression (2.1)

Collecting distinct GPUs permanently rebuilds the world's renderer. Graphics upgrades happen automatically and do not spend points or affect collision geometry. Replaying an already credited GPU gives its usual gameplay reward but cannot farm graphics progression.

| Absorbed GPUs | Renderer | World resolution | Visual changes |
| --- | --- | --- | --- |
| 0 | Bootstrap | 240 x 135 | Coarse pixels, four-tone city, clear danger/objective accents |
| 24 | 8-bit | 320 x 180 | Color returns and background city layers appear |
| 75 | 16-bit | 400 x 225 | Finer colors, additional depth and initial bloom |
| 150 | Neon | 480 x 270 | Stronger lighting, weather and richer effects |
| 300 | HD Pixel | 720 x 405 | More detail, weather and particles |
| 600 | Ultra | 960 x 540 | Full color and the richest lighting/effects |

Controls, text and menus remain clear at every tier. The art stays a pixel-art platformer; higher tiers reveal more detail in the existing artwork. GPU identities and renderer progression save with the character and survive restarts. Existing saves retain gameplay progress; distinct graphics-core tracking begins with this version.

The progression concept and creative direction came from Dustin. AI assisted implementation, consistent with the project's credits and artist-led art roadmap.
