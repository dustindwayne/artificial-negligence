# Android downloads

**[Download the latest APK](https://github.com/dustindwayne/artificial-negligence/releases/latest/download/ArtificialNegligence.apk)**

- `latest-v2.json`: current version, immutable Release download URL, SHA-256 and reusable region map.
- `latest.url`: direct URL for the current version.
- `gameplay.json`: small balance updates.
- `latest.json` and `chunks/`: the single v2.3.0 upgrade bridge for older clients. This bridge stays available as newer releases are published.

New downloads use one APK hosted in GitHub Releases, with parallel byte-range requests for missing regions. The updater validates responses, resumes from verified cached chunks and falls back safely if ranges are unsupported.

Obsolete chunks and the duplicate APK were removed from the current branch. Previous Git commits are retained; this cleanup does not rewrite repository history.
