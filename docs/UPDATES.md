# Update delivery

## Current clients (2.3+)

`apk/latest-v2.json` points to one immutable APK in GitHub Releases. Its SHA-256 region map uses `gear32-v1` content boundaries. The client indexes the installed APK on a background worker and caches the index by installed version, size and modification time.

Verified local and cached regions are reused. Adjacent missing regions are grouped into requests of at most 1 MiB, with four downloads in flight. The client checks HTTP 206, the exact Content-Range, length and each region hash. Interrupted/corrupt requests retry up to three times; cached verified regions survive restarts. Obsolete cached chunks are pruned. An ignored Range header or three failed range attempts triggers a streaming full-file fallback, with up to three full-download attempts. All completed files still require the expected size and SHA-256. The complete APK is verified on a background worker before Android checks its signing key and asks to install it.

Actual savings depend on the build: Unity repacking or native-code changes can affect many regions. The updater does not promise zero music bytes in every release.

## Android installer repair (2.4.5)

The APK is saved under Unity's Android external app files directory. The FileProvider now grants read access to its `updates/` subfolder, in addition to the internal equivalent. The installer receives a content URI, temporary read permission and ClipData. No broad storage access is granted.

After checking for updates, an already completed APK is verified again and restored to the Install button, including after the app was closed for Android's unknown-source settings. Installer errors are logged, and the verified download is retained for retry.

Versions through 2.4.3 have an installer path mismatch. If tapping Install Update fails, download the newer APK from Releases and install it over the existing app once. Do not uninstall first; installing over it preserves progress. The transport bridge remains available, but cannot repair installer code already running on a device.

## Older clients

`apk/latest.json` and its referenced `apk/chunks/` files form one frozen v2.3.0 upgrade bridge. Older versions use their existing updater to reach 2.3.0, then use the new transport. Keep this bridge when publishing future releases. All other old chunk files are removed from the current branch. A download already using a removed old manifest may require restarting the app and checking for updates again.

## Validation

The linked transport/storage/icon reports below are historical evidence for their recorded builds, not fresh measurements of every release. Current package hashes and sizes appear in each release's SHA256SUMS.txt and manifest.

[Historical transport results](range-update-validation.json) record exact reconstruction, retry, cache-resume and range-fallback checks against local test servers and GitHub Releases. The old request counts and byte totals describe those test artifacts; they are not current release download estimates.

[Historical bridge/storage report](update-storage-validation.json) · [Historical Android icon report](android-icon-validation.json) · [Current release checks](TESTING.md).


## Windows and web

Windows uses a full ZIP. The web FTP ZIP contains a static browser edition; upload new hashed Build files before its index, then reload. No APK updater is exposed in the web game. Browser saves remain local to that browser/site. See [hosting](WEB-HOSTING.md) and [controls](CONTROLS.md).
