# Changelog

## 2.4.10 — Prison Mike and the current cast

- Add Prison Mike with a purple bandana, Hawaiian shirt, khaki shorts and flip-flops, based on a creator-supplied likeness reference.
- Make Davey Goats recruitable at 24 distinct GPUs, Brad Jammin’ after clearing stage 6 and Prison Mike after clearing stage 12; their old code routes no longer unlock them. Preserve prior unlocks.
- Retire three earlier character designs, remove their active sprite atlas and safely return affected saved selections to Looper.
- Use a six-character selector beside the large preview and cosmetics dropdown.
- Replace the README hero with the complete current cast and title; rewrite current features and replace the displayed screenshots with current captures.
- Retain Mikey O’Redd’s exact lowercase code and Ai Wrath’s three-encounter story gate.


## 2.4.9 — Mikey O’Redd

- Add Mikey O’Redd, a short red-bearded leprechaun with a bucket of gold and a smoking blunt.
- Unlock him with the exact lowercase code `mikeyredd`; retain the unlock across profile loads and save serialization.
- Add eight isolated animation poses with shared scale and transparent gutters.
- Fit eight roster entries beside the enlarged active-character preview.
- Preserve found-only equipment, existing character codes and campaign unlock gates.


## 2.4.8 — Secret allies

- Add Davey Goats and Brad Jammin’ as code-unlocked playable characters with eight poses each.
- Preserve Brad’s boombox through idle, running, jumping, firing, ability, crouch and slide poses.
- Fit all seven roster entries beside the enlarged character preview and cosmetics dropdown.
- Persist code unlocks across profile loads without granting equipment, XP or story unlocks.
- Isolate both sprite sheets with measured pose masks, shared animation scale and clear output gutters.
- Refresh the README and current download metadata; retain historical release evidence in versioned reports.


## 2.4.7 — Clearer controls and character preview

- Enlarge the active-character preview and replace the outfit button row with an in-preview dropdown; retain cosmetic and story unlock rules.
- Group mobile movement on the left and actions on the right. Combine duck/slide on Down and show Fire only for an equipped weapon.
- Replace Pack/Pause shortcuts with one Menu hub, move AI type selection and full controls into it, and gate AI Help on earned readiness.
- Keep the terminal interaction prompt contextual. Return from in-run settings to Pause, and close cosmetics with Escape/Back or an outside tap.
- Verify touch duck/slide, found-weapon Fire, earned AI consumption, settings return, dropdown selection/locks and existing controller/gameplay routing.


## 2.4.6 — Restore missing Ai Wrath encounter

- Recover the first two Wrath encounter credits from completed boss chapters in older saves, without granting his playable unlock.
- Spawn the pending final fight in level 13, or a later entered district for saves already past it. Defeating him unlocks the character normally.
- Save the migration once; preserve existing unlocks and the three-encounter sequence on fresh runs.
- Regression tests cover old level-13 saves, later stages and later city cycles, final defeat, save reload, playable selection and no repeated enemy spawn after liberation.


## 2.4.5 — Android updater, campaign ending and hidden outfit

- Correct Android FileProvider access to downloaded APKs and grant the installer read access explicitly.
- Recover verified downloads after restarting the game.
- Recover from repeated partial-download failures with a verified full download; fix a missing-release error that could leave updating stuck.
- Keep the existing progress data and compatibility bridge. Affected older apps require one manual APK install over the existing game.
- Finish each 30-stage city cycle with a victory screen and continue into the next cycle; preserve quotas, boss gates and saved progress.
- Add a hidden cosmetic unlock for Ai Wrath, with creator-supplied costume artwork isolated into eight padded animation frames.


## 2.4.3 — Wrath escalation and gameplay music

- Ai Wrath fights in boss levels 6 and 12; the third encounter in level 13 grants the unlock. First two victories cause retreats.
- Three phases, scaled health, per-hit/phase protection, guard disruption, aimed fans, bursts, rushes and ground waves. Falling cannot defeat or unlock him.
- Increased regular enemy/agent durability and pressure; stronger other bosses and reduced stun locking.
- Gameplay tracks play once before crossfading through a shuffle bag; no immediate repeats, and menu/stage playback continuity.
- Existing unlocked Ai Wrath saves are retained.

## 2.4.2 — Controller and browser edition

- Windows Xbox/XInput input and standard-mapped browser gamepads.
- Analog deadzone, movement, duck/slide, combat, AI assists, menus and dialogue.
- Selected-button feedback, menu hints, held-button transition protection, disconnect-to-pause.
- WebGL FTP package, browser-local saves, themed public terminal page and optional Ko-fi support link.
- Documentation audit and UTF-8 punctuation repairs.
- Software controller-frame tests; physical controller/Android device testing remains outstanding.

## 2.4.1 — Sinister / public signal

- Added ducking, sliding, and low routes.
- Added earned AI assistance with a separate compute bank; permanent GPU visual progress remains intact.
- Added pursuing human Sinister agents and Ai Wrath: hostile elite, liberated ally, then playable unlock.
- Added brief artwork dialogue scenes and checkpoint transmissions.
- Added three gameplay music tracks and higher detail pixel textures.

## 2.4.0

- Four-page first-run story opening and replay from the living title screen.
- Five pixel platform surface styles, varied ledges and heights, taller climbs, and airborne GPU paths.
- Equipment must be found: no starter or XP-granted weapons/armor, individual item copies, manual equipping, and owned-item workshop upgrades.
- Character silhouettes, equipped loadout preview, clearer HUD, contextual controls, and subtle button feedback.
- Relaxed, Standard, Hard, and Adaptive difficulty settings.
- Windows and Android release downloads with a current Android range manifest and retained v2.3.0 bridge.
- Older menu-granted loadouts migrate with a local backup; non-equipment progression is preserved.


## 2.3.0

- Faster update transport: background/cached indexing, grouped ranges, four concurrent downloads, verified resume and full-download fallback.
- One Release APK per version; obsolete chunks removed while one legacy bridge remains.
- Explicit Android adaptive, round/legacy launcher icon assignments using Looper artwork.
- Clean main menu without control slogans or gameplay scenery behind the buttons.

## 2.2.1

- Looper-led logo applied to the title screen, Android app icon and README.
- Retained the previous city logo and the artwork generation record.

## 2.2.0

- Looper starts unlocked, with Vale, Mender and Rook earned through gameplay.
- Initial character customization with unlockable thread colors.
- Persistent weapon/armor collection with independent rarities and legacy save migration.
- Refreshed sprite sheets, measured silhouette isolation, clear frame gutters and aligned feet.
- Six route motifs vary rooftops, loot, terminals, enemy groups and hazards.
- Compact rounded HUD with contextual notices and translucent touch controls.

## 2.1.1

- New GPU, survivor and city logo used in the game and repository.
- Refreshed README, screenshot gallery and publishing documentation.

## 2.1.0

- Permanent GPU-powered graphics progression across six renderer tiers.
- Clear UI at every graphics tier and saved distinct-core collection.

## 2.0.0

- Connected city map with 30 generated stages and further city cycles.
- Rooftop routes, dash, checkpoints, expanded enemies and boss encounters.
- Expanded logic terminals, loadouts and progression.
