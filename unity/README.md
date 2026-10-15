# Artificial Negligence — Unity 2D project / 2.4.2

Unity 6000.6.5f1, URP 2D. Open Assets/Scenes/ArtificialNegligence.unity and press Play. The active game is 2D; discarded perspective experiments are not the production game.

## Implemented

30 generated stages across five districts and further cycles; 60 route validation cases. Varied platforms, gaps, airborne GPUs, duck/slide passages, permanent GPU graphics restoration and separate compute/momentum-gated AI support. No starter or XP-granted weapons/armor: find individual items, retain copies, manually equip and upgrade owned equipment. Five gameplay-unlocked characters including Ai Wrath, short artwork story scenes, twelve puzzle families with one attempt, bots/human Sinister agents/bosses, four difficulty modes and contextual music.

Individual ally signature powers, the full campaign outline and creator-drawn sprite replacements remain planned.

## Controls

Keyboard: A/D or arrows move; Space/W/Up jump; S/Down duck; Ctrl moving slide; J/F attack; Shift/K dash; E terminal; I/G inventory; C character; U upgrades; Escape pause/skip; Enter/Space story advance.

Windows XInput and standard-mapped web gamepads: stick/D-pad movement and duck, A jump/confirm, B slide/back/skip, X/RT attack, Y terminal, RB dash, LB earned AI assist, LT assist type, View inventory, Menu pause. Android uses touch controls. See ../docs/CONTROLS.md for target limits and verification.

## Build

- BuildGame.Desktop: Windows x64.
- BuildGame.Android: Android 8+ ARM64, SDK 35, NDK r27c, JDK 17.
- BuildWeb.Build: WebGL with gzip fallback, hashed names and Web audio import override.
- BuildRelease.All: all three targets sequentially.
- After a Web build, run tools/package_web.py from the repository root to restore the themed index and package it.

Install matching Unity platform modules. Build methods configure settings and validate gameplay, puzzle rewards, rarity, inventory, sprite gutters and generated reachability. Native audio uses streaming; Web uses compressed-in-memory AAC imports. Game surfaces and collisions retain the same world footprint at all visual tiers.

Example: Unity.exe -batchmode -quit -projectPath <project> -executeMethod BuildRelease.All -logFile <log>

Android helper paths use the original workspace tool locations when present and embedded tools otherwise. Configure your own SDK/JDK/NDK and signing key when building elsewhere. Never commit a signing key or password. Published APKs currently use the same playtest signing certificate across updates.

## Checks and saves

Run the Windows executable with -qa -qaOutput <folder> for isolated checks and captures; QA suppresses save writes. Checks include story flow, found-only inventory, locked silhouettes, GPU visual tiers, movement, upgrades, puzzles, bosses, music, Wrath liberation and injected controller input. Physical controller and Android-device validation remain outstanding.

Native progress is local; browser progress belongs to that browser/site. The legacy menu-granted loadout migration backs up progress-before-loot-inventory and clears old granted gear while preserving other progression. New equipment has individual IDs and retains found copies.

Android updates use the current release manifest plus a frozen v2.3.0 bridge. Windows updates are full ZIPs. Web hosting uses static files and page reloads, with no Android updater. See ../docs/UPDATES.md and ../docs/WEB-HOSTING.md.

Concept, world, gameplay vision and art direction: Dustin (dustindwayne). AI assisted programming, visuals, implementation and testing. See CREDITS.md, STORY.md and ROADMAP.md; STORY-PRESENTATION.md describes limited dialogue.
