# Testing and compatibility

## Release 2.4.5

- All three Unity targets built: Windows x64, Android ARM64, WebGL.
- Generated route checks cover 60 routes, 1,960 GPUs and 443 ground gaps; low passages use the crouched collision shape.
- Sprite checks cover 116 isolated native atlas frames and transparent edge gutters.
- Windows runtime checks cover inventory, story, unlocks, graphics tiers, music, puzzles, bosses and controller routing. Wrath checks exercise all three encounters/phases, huge-hit and same-frame burst protection, attacks, pit recovery, retreat scenes, save persistence and final-only unlock. Music checks force actual track endings, verify shuffle variety/no immediate repeat, and verify inventory/stage continuity.
- Controller tests use injected normalized input frames. Web adapter tests exercise ten button mappings, axes, D-pad priority, disconnect handling and standard-map filtering.
- The final web player was loaded in a browser and rendered the game without startup console errors. Earlier browser checks covered keyboard movement/jumping, GPU collection and saved progress.
- Android package version, signature and continuity with the preceding signing certificate were checked.
- Published download links and package SHA-256 values were verified.

**Not yet verified:** physical Xbox/gamepad use over USB/Bluetooth across browsers, and final-release playtesting or in-app installation on a physical Android device. The earlier 2.4.4 updater test APK was installed over the existing app by USB and its installed version was verified. Software routing checks do not establish hardware compatibility for every controller.

[Release verification record](release-validation-v245.json). Current package checksums are supplied as SHA256SUMS.txt in the release.

## Historical evidence

Older JSON reports and generation records in this directory retain the results and requested art specifications from their original builds. They are provenance, not current performance guarantees. In particular, old updater byte savings and old sprite gutter sizes must not be read as measurements of the latest release.

## Campaign completion repair

Physics validation passed for 60 generated levels, 1,960 GPU placements and 443 ground gaps, with an additional check that every exit has walkable ground. A separate scripted progression run completed 60 stages and 11 boss encounters, including the three Wrath encounters. It checked quota and live-boss gates, pickup collection, found equipment, save serialization, both city-cycle victory screens, and continuation to the next cycle.

This is automated regression evidence, not a human playthrough or a difficulty/balance assessment. The script positions the player at pickups, applies damage through boss hit rules, ignores player damage, and uses real movement for each exit approach. [Recorded progression](campaign-completion-validation.json).

The hidden outfit has a separate runtime check for a single-tap input box, wrong-code rejection, successful code entry, retained story unlock, costume selection and all eight rendered frame gutters. [Costume verification](wonder-wrath-validation.json).
