# Testing and compatibility

## Release 2.4.7

- All three Unity targets built: Windows x64, Android ARM64, WebGL.
- Generated route checks cover 60 routes, 1,960 GPUs and 443 ground gaps; low passages use the crouched collision shape.
- Sprite checks cover 116 isolated native atlas frames and transparent edge gutters.
- Windows runtime checks cover inventory, story, unlocks, graphics tiers, music, puzzles, bosses and controller routing. Wrath checks exercise all three encounters/phases, huge-hit and same-frame burst protection, attacks, pit recovery, retreat scenes, save persistence and final-only unlock. Music checks force actual track endings, verify shuffle variety/no immediate repeat, and verify inventory/stage continuity.
- Controller tests use injected normalized input frames. Web adapter tests exercise ten button mappings, axes, D-pad priority, disconnect handling and standard-map filtering.
- The final web player was loaded in a browser and rendered the game without startup console errors. Earlier browser checks covered keyboard movement/jumping, GPU collection and saved progress.
- Android package version, signature and continuity with the preceding signing certificate were checked.
- Published download links and package SHA-256 values were verified.

**Not yet verified:** physical Xbox/gamepad use over USB/Bluetooth, final-release Android playtesting, and installation through the in-app updater. The 2.4.6 APK was installed over the existing Android app by USB and its installed version was verified. Software routing checks do not establish hardware compatibility for every controller.

[Release verification record](release-validation-v247.json). Current package checksums are supplied as SHA256SUMS.txt in the release.

## Historical evidence

Older JSON reports and generation records in this directory retain the results and requested art specifications from their original builds. They are provenance, not current performance guarantees. In particular, old updater byte savings and old sprite gutter sizes must not be read as measurements of the latest release.

## Campaign completion repair

Physics validation passed for 60 generated levels, 1,960 GPU placements and 443 ground gaps, with an additional check that every exit has walkable ground. A separate scripted progression run completed 60 stages and 11 boss encounters, including the three Wrath encounters. It checked quota and live-boss gates, pickup collection, found equipment, save serialization, both city-cycle victory screens, and continuation to the next cycle.

This is automated regression evidence, not a human playthrough or a difficulty/balance assessment. The script positions the player at pickups, applies damage through boss hit rules, ignores player damage, and uses real movement for each exit approach. [Recorded progression](campaign-completion-validation.json).

The hidden outfit has a separate runtime check for a single-tap input box, wrong-code rejection, successful code entry, retained story unlock, costume selection and all eight rendered frame gutters. [Costume verification](wonder-wrath-validation.json).

## Wrath save recovery

Save fixtures cover progression before/after each boss chapter, level 13, later stages, later cycles, district-only legacy saves and repeated migration. Runtime checks require the final Wrath enemy to spawn, remain locked until defeat, become selectable after defeat/reload, and stop spawning after liberation. Fresh-run campaign checks retain the 6/12/13 sequence.

## Character menu and touch controls

2.4.7 runtime checks cover enlarged portrait dimensions; open/select cosmetics, including the secret outfit and disabled locked choices; combined Down duck/slide; five unarmed versus six armed touch actions; earned AI readiness and consumption; in-run settings returning to Pause; and the existing controller routing regression. Actual screenshots record both closed and open dropdown layouts, the touch HUD and pause hub.

The scripted 60-stage campaign/save-recovery runtime evidence remains from 2.4.6; gameplay progression code is unchanged in this UI update. Fresh physics validation still covers all 60 generated routes in 2.4.7.
