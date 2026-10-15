# Artificial Negligence — Unity project

Unity 6000.6.5f1 with Universal Render Pipeline 2D. Open Assets/Scenes/ArtificialNegligence.unity and press Play.

## Current game

- Four-page opening on the first run; Story on the title screen replays it.
- 30 generated stages across five city districts, followed by further cycles. CityRoute.cs generates the routes; the old CSV is retained as a historical asset.
- Five pixel platform surface patterns, varied ledges/heights, gap jumps and airborne GPUs. Physics validation covers 60 routes with the base jump and no gear.
- No starting or level-granted weapons/armor. Boxes produce individual finds; inventory stores multiple copies and manual equipment selections. Upgrade only owned equipment.
- Character unlocks, locked silhouettes, selected loadout, outfits, personal upgrades, talents, one-attempt terminals, bosses and music.
- Relaxed / Standard / Hard / Adaptive difficulty. Adaptive changes encounter pressure between stages.

## Controls

A/D or arrows: move. Space/W/Up: jump. J/F: strike or fire equipped weapon. Shift/K: dash. E: terminal. I/G: inventory. C: character. U: upgrades. Escape: pause. Story: Space/Enter advances; Escape skips.

## Build and checks

BuildGame.Desktop builds a Windows player. BuildGame.Android builds an Android 8+ ARM64 APK. Both configure the project and validate gameplay, collection, puzzle rules, rarity rolls, sprite gutters and base-jump route reachability before building.

Example: Unity.exe -batchmode -quit -projectPath <project> -executeMethod BuildGame.Desktop -logFile <log>

For Android, install Android Build Support for the same editor. The current helper uses SDK 35, NDK r27c, and JDK 17, with embedded tools as a fallback when the local task tool paths are absent. Build outputs are signed for playtesting; never publish a private signing key in this repository.

Run the Windows executable with -qa -qaOutput <folder> for isolated runtime checks and screenshots. This mode suppresses save writes. Tests cover the opening, interface, inventory, graphics tiers, movement, upgrades, terminals and bosses.

## Saves and updates

Progress is local. The migration from the older menu-granted loadout backs up progress-before-loot-inventory, clears that loadout, and preserves non-equipment progression. New inventory items have individual IDs and persist through JSON save/load.

Android uses the versioned GitHub Release APK and reusable-region manifest in apk/. One older upgrade bridge remains. Windows downloads are full ZIPs. No ads or analytics are included.

See ../STORY.md and ../CREDITS.md for campaign direction and AI disclosure. Ally signature powers remain planned; current characters share combat stats and equipment.
