# Update delivery

## Current clients (2.3+)

`apk/latest-v2.json` points to one immutable APK in GitHub Releases. Its SHA-256 region map uses `gear32-v1` content boundaries. The client indexes the installed APK on a background worker and caches the index by installed version, size and modification time.

Verified local and cached regions are reused. Adjacent missing regions are grouped into requests of at most 1 MiB, with four downloads in flight. The client checks HTTP 206, the exact Content-Range, length and each region hash. Interrupted/corrupt requests retry up to three times; cached verified regions survive restarts. Obsolete cached chunks are pruned. An ignored Range header triggers one bounded full-file fallback. The complete APK is verified on a background worker before Android checks its signing key and asks to install it.

Actual savings depend on the build: Unity repacking or native-code changes can affect many regions. The updater does not promise zero music bytes in every release.

## Older clients

`apk/latest.json` and its referenced `apk/chunks/` files form one frozen v2.3.0 upgrade bridge. Older versions use their existing updater to reach 2.3.0, then use the new transport. Keep this bridge when publishing future releases. All other old chunk files are removed from the current branch. A download already using a removed old manifest may require restarting the app and checking for updates again.

## Publish a release

1. Build a signed APK with a higher version code and the same private signing key.
2. Upload it as `ArtificialNegligence.apk` to the corresponding immutable GitHub Release. Mark the new release latest only after validation.
3. Publish metadata:

```sh
python tools/publish_apk.py /path/to/ArtificialNegligence.apk --version 2.3.1 --code 231 --download-url https://github.com/dustindwayne/artificial-negligence/releases/download/v2.3.1/ArtificialNegligence.apk
```

4. Commit the small manifests/link files. Do not commit another full APK or generate another chunk store for current clients. Keep the current release and the single bridge; obsolete Release assets may be removed after updating the manifest.

The `--bridge` option deliberately replaces the one bridge and prunes chunks not referenced by it. Use it only for a coordinated bridge refresh, not every routine release. The publisher never rewrites Git history or deletes releases by itself.

## Balance-only updates

```sh
python tools/publish_gameplay.py --version 2 --enemyHealth 1.1 --xp 1.1 --notes "Tougher enemies with more XP."
```

Increase the balance version and commit `apk/gameplay.json`. Validated values save for offline play; new enemy health applies when the next district starts. New executable features still require an APK update.
