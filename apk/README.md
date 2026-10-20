# Android — 2.4.7

[Download latest APK](https://github.com/dustindwayne/artificial-negligence/releases/latest/download/ArtificialNegligence.apk). Android 8+, ARM64. Install over the existing app to keep progress. Touch input is supported; controller implementation in this release targets Windows/web.

- latest-v2.json: current version, immutable release APK URL, SHA-256 and reusable-region map.
- latest.url: current direct APK URL.
- gameplay.json: small balance adjustments.
- latest.json and chunks/: frozen v2.3.0 bridge for older clients; retained across routine releases.

Use Settings → Check for Updates. Missing regions download in parallel; matching installed/cached regions can be reused. The complete APK is verified before Android asks to install it. Savings depend on build packing and are not guaranteed for every unchanged song. The current APK is hosted once in Releases. [Update protocol and historical tests](../docs/UPDATES.md).
