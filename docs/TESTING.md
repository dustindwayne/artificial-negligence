# Testing and compatibility

## Release 2.4.11

Build and validation results are recorded in [the release report](release-validation-v251.json). Editor validation covers 90 generated routes using Unity Physics2D, supply-box and GPU pickup reachability, ground gaps, a 180-pixel GPU exclusion before uplinks, 1,800 puzzle solutions and 124 isolated atlas frames.

Windows runtime checks exercise all 90 campaign stages, quota/live-boss gates, found equipment, three city endings and persistence. The scripted campaign positions the player at pickups, applies damage through boss hit rules, ignores player damage and uses real movement for each exit approach. This is automated regression evidence, not a human playthrough or difficulty assessment.

New checks cover virtual-host defeat unlocks, huge-hit protection, saved recruits, independent item levels, fixed rarity, salvage protection, excellent-clear probabilities and one-time rewards, the visible prologue fracture and unlocked title roster. Existing runtime checks cover Wrath’s three encounters, music shuffle, graphics tiers, puzzles, HUD/cosmetics and normalized controller routing.

**Not yet verified:** physical Linux distribution/device compatibility, physical gamepads over USB/Bluetooth, final-release Android playtesting or installation through the in-app updater. Controller routing checks do not establish hardware compatibility for every device. Android signing continuity and public package checksums are checked during release packaging.

## Historical evidence

Older versioned JSON reports, screenshots and generation records retain the conditions of their original build. They are historical provenance, not current measurements. Old recruit codes, GPU thresholds, two-cycle campaign counts and updater byte savings in those records do not describe this release.

## Linux packaging

The native x64 Mono target is built with Unity Linux Build Support. The tar.gz preserves executable permission on the ELF player and carries its Data/Mono/shared-library dependencies. Linux smoke tests run in the existing Ubuntu environment under WSL with rendering. This does not establish compatibility across Linux distributions or physical gamepads.

The Linux controller adapter uses Unity Input System 1.20.1. A virtual gamepad exercises all ten actions, sticks, D-pad priority and disconnect before running the shared menu/gameplay/dialogue routing checks. These are software tests, not a physical controller test.
