# Web build and FTP hosting — 2.4.2

The release includes **ArtificialNegligence-Web-FTP.zip**, a static Unity WebGL build. It contains a `play/` folder and upload instructions. No database or server application is required.

## Upload to dustinwann.com

1. Extract the ZIP.
2. Upload the `play` folder into the domain's document root, commonly `public_html`, using your FTP client.
3. Visit `https://dustinwann.com/play/` after upload, click Enter the City, then Play now.
4. Link `/play/` from the website homepage.

The website deployment is separate from the GitHub release. Publishing this package does not upload it to dustinwann.com.

The supplied full HTML embeds the page CSS. Existing `hero.png` and the complete `Build/` directory must remain beside `index.html`. A separate CSS version is also kept in the source under `web/`. Do not open the game via `file://`; serve it through HTTP/HTTPS.

## Hosting settings

Gzip compression with Unity's decompression fallback supports ordinary static hosting. Serve `.unityweb` as binary files; do not force a `Content-Encoding: gzip` header onto them. Optional Apache `.htaccess` files provide MIME/cache hints. If hosting rejects these files, remove them and retry. Hashed Build files can be cached; the index should be revalidated. Upload new Build files first and index.html last. Keep older hashed files during the transition for already-open sessions.

## Input and saves

Desktop browsers with WebGL 2 and a keyboard are the primary target. Standard-mapped gamepads use the [controller mapping](CONTROLS.md); click the game, then press a controller button. Browser saves are local to that browser and site, do not sync with Windows or Android, and are removed when site data is cleared. Keep the hosting URL stable. Reload for updates; the web edition never installs an Android APK.

## Page and attribution

The page uses the game's public-resistance terminal theme, Looper/Ai Wrath banner, mission briefing, field controls and direct native downloads. An optional [Ko-fi support link](https://ko-fi.com/stonerautist) opens only when selected. No advertising or analytics scripts are added. Normal web-host access logs and the destination payment provider are separate from the game page; this is not a promise that those services collect no data.

The promotional banner is AI-generated; gameplay captures are actual game output. Creator and AI-assistance credits appear on the page.

## Build

Open the Unity project with Web Build Support for 6000.6.5f1. `BuildWeb.Build` builds the WebGL player with Web audio import settings, a 512 MiB initial heap, gzip fallback and hashed filenames. `tools/package_web.py` restores the themed page around the generated filenames and creates the FTP archive. The initial download is roughly 40 MB; size changes with each build. Web audio uses compressed-in-memory imports because browser builds do not use the native streaming path.
