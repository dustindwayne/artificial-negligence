<p align="center">
  <img src="docs/logo.png" width="440" alt="Artificial Negligence — GPU core, street survivor and a city rebuilding from pixels">
</p>

<p align="center"><strong>Absorb the AI. Keep it public.</strong></p>
<p align="center">An Android pixel platformer where collecting GPUs upgrades the world itself.</p>
<p align="center">
  <a href="https://raw.githubusercontent.com/dustindwayne/artificial-negligence/main/apk/ArtificialNegligence-latest.apk"><strong>Download APK</strong></a> ·
  <a href="#screenshots">Screenshots</a> ·
  <a href="ROADMAP.md">Art roadmap</a> ·
  <a href="CREDITS.md">Credits</a>
</p>
<p align="center">v2.1.1 · Android 8+ · ARM64 · Unity 6000.6.5f1 · Playtest build</p>

## The game

Run across a city as a street survivor collecting GPUs before corporations seize them. Fight through rooftop routes, solve intellect terminals and reach district bosses. Every core helps reclaim public AI—and brings more color, detail and depth to the world.

### Features

- **A connected city:** 30 generated stages across five districts, followed by further city cycles.
- **Platforming routes:** rooftops, dash, checkpoints, moving lifts and timed hazards.
- **Enemies with presence:** six enemy classes, animated attacks and three rotating bosses.
- **Build your survivor:** permanent XP, talents and upgrades; Pulse, Arc, Scatter and Rail weapons with armor and module tradeoffs.
- **Rare equipment:** Common through Ultra tiers. Ultra equipment has a 0.5% chance in boxes.
- **Changing logic puzzles:** twelve puzzle families, explanations after answers and no powerup for a wrong answer.

## Graphics are progression

Distinct GPUs permanently improve the renderer. Upgrades are automatic; points remain available for character upgrades. Replaying an already credited GPU still awards gameplay rewards, but does not advance graphics again.

| GPUs collected | Graphics tier | World resolution |
| ---: | --- | --- |
| 0 | Bootstrap | 240 × 135 |
| 24 | 8-bit | 320 × 180 |
| 75 | 16-bit | 400 × 225 |
| 150 | Neon | 480 × 270 |
| 300 | HD Pixel | 720 × 405 |
| 600 | Ultra | 960 × 540 |

The pixel-art style stays consistent as color, scenery, lighting and weather become richer. Menus and controls stay sharp throughout; collision geometry stays consistent.

## Screenshots

<table>
<tr><th>Bootstrap graphics</th><th>Ultra graphics</th></tr>
<tr><td><img src="docs/graphics-tier-0.png" alt="Coarse Bootstrap graphics"></td><td><img src="docs/graphics-tier-5.png" alt="Detailed Ultra graphics"></td></tr>
<tr><th>City route</th><th>Boss encounter</th></tr>
<tr><td><img src="docs/city-map.png" alt="Connected city map"></td><td><img src="docs/boss-encounter.png" alt="Boss combat"></td></tr>
</table>

Graphics comparison captures use staged GPU totals in the same test scene.

## Play on Android

1. [Download the latest APK](https://raw.githubusercontent.com/dustindwayne/artificial-negligence/main/apk/ArtificialNegligence-latest.apk).
2. Open it on an Android 8.0+ ARM64 device and approve installation.
3. Install over an existing compatible copy to keep local progress.

This is a testing build. Generated route reachability and gameplay rules have automated checks; desktop gameplay has been checked. Physical Android installation and update testing remains outstanding.

### Updates

Open **Settings → Check for Updates**. APK updates reuse matching installed content chunks, reducing downloads when music and artwork are unchanged. Android asks you to approve installation. Small balance updates can download separately without reinstalling the APK.

[Update and publishing guide](docs/UPDATES.md) · [APK folder](apk/) · [Current version manifest](apk/latest.json) · [Changelog](CHANGELOG.md)

## Creative direction and AI use

**Dustin (dustindwayne)** created the concept, art-style direction, world, gameplay vision and overall creative direction, and supplied music and visual references. This build uses AI assistance for programming, generated artwork, integration and testing. [Full credits and disclosure](CREDITS.md).

The art roadmap moves toward creator-drawn characters and key poses, with AI assisting animation frames under the creator’s review. [Read the roadmap](ROADMAP.md). The new emblem combines the survivor, GPU collection and the city’s transition from coarse pixels to richer detail. [Logo generation record](docs/logo-generation.md).

## Development

Open `unity/` in **Unity 6000.6.5f1** with Android Build Support. [Project setup and controls](unity/README.md).

| Folder | Contents |
| --- | --- |
| `unity/` | Editable Unity project and game source |
| `apk/` | Latest public APK, update manifests and reusable chunks |
| `docs/` | Screenshots, branding and release documentation |
| `tools/` | APK and balance-update publishing scripts |

Report problems through [GitHub Issues](https://github.com/dustindwayne/artificial-negligence/issues), including your device, Android version, game version and steps to reproduce.
