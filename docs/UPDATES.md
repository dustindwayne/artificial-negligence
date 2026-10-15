# Updates and publishing

The game checks `apk/latest.json` for APK updates. It reuses matching chunks from the installed APK, downloads missing chunks and verifies the reconstructed APK with SHA-256 before Android verifies its signature and requests installation. Downloads can resume. Savings depend on what changed; unchanged music is not guaranteed to require zero bytes in every build.

Android may ask you to allow the game to install updates. After allowing it, return and tap Install Update again. Version 1.2 and earlier need one full APK download to gain the updater.

## Publish an APK

Build with a higher Android version code and the same signing key. Keep the key private.

```sh
python tools/publish_apk.py /path/to/game.apk --version 2.2.1 --code 221
```

Commit and push `apk/`. Keep all previously published chunk files so downloads using older manifests can finish. The stable latest APK link points to the current build.

## Publish a balance update

Settings checks `apk/gameplay.json` for enemy and boss health, speed, damage, weapon damage and XP settings. Validated settings save locally for offline play. New enemy health applies when a district starts.

```sh
python tools/publish_gameplay.py --version 2 --enemyHealth 1.1 --xp 1.1 --notes "Tougher enemies with more XP."
```

Increase the balance version, then commit and push `apk/gameplay.json`. New executable features require an APK update.
