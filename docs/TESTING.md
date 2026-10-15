# Testing and compatibility

## Release 2.4.2

- All three Unity targets built: Windows x64, Android ARM64, WebGL.
- Generated route checks cover 60 routes, 1,960 GPUs and 443 ground gaps; low passages use the crouched collision shape.
- Sprite checks cover 108 isolated native atlas frames and transparent edge gutters.
- Windows runtime checks cover inventory, story, unlocks, graphics tiers, music, puzzles, bosses and controller routing.
- Controller tests use injected normalized input frames. Web adapter tests exercise ten button mappings, axes, D-pad priority, disconnect handling and standard-map filtering.
- The final web player was loaded in a browser and rendered the game without startup console errors. Earlier browser checks covered keyboard movement/jumping, GPU collection and saved progress.
- Android package version, signature and continuity with the preceding signing certificate were checked.
- Published download links and package SHA-256 values were verified.

**Not yet verified:** physical Xbox/gamepad use over USB/Bluetooth across browsers, and installation/playtesting on a physical Android device. Software routing checks do not establish hardware compatibility for every controller.

[Release verification record](release-validation-v242.json). Current package checksums are supplied as SHA256SUMS.txt in the release.

## Historical evidence

Older JSON reports and generation records in this directory retain the results and requested art specifications from their original builds. They are provenance, not current performance guarantees. In particular, old updater byte savings and old sprite gutter sizes must not be read as measurements of the latest release.
