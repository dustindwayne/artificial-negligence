# Android launcher icon

The Android-specific icon slots were previously empty. The build now explicitly assigns adaptive foreground/background layers and legacy/round icons at every Unity-supported density. The packaged manifest and icon resources are checked during release validation.

Looper's character artwork is the icon, without small title lettering. The generated foreground is fitted inside the central 60% of a 512px canvas so launcher masks retain the face and GPU. A solid dark background supports both adaptive and older launchers.

Assets: `unity/Assets/Branding/LooperIconSource.png`, `AndroidForeground.png`, `AndroidBackground.png`, and `AndroidLegacy.png`. Creative direction: Dustin (dustindwayne). Artwork adapted with the built-in `image_gen.imagegen` tool; Unity prepares the launcher sizes and layers.

## Final prompt

Use case logo-brand. Android adaptive launcher icon foreground based on Looper, the character in this supplied game logo. Transparent background. One clear centered pixel-art bust of Looper: curly brown hair, black rectangular glasses, recognizable face, black jacket with blue trim, visible blue backpack core, holding one small gold GPU. Preserve supplied character style and identity. No game title, no lettering, no city, no border, no background scenery. Strong readable silhouette at 48 pixels. CRITICAL adaptive icon safe area: all important artwork entirely within the center 60 percent of the square canvas, with at least 20 percent transparent padding on each edge; head, glasses and GPU inside a centered circular safe area. Clean opaque pixel character edges, no diffuse glow outside silhouette. Square 1024 or larger.

![Looper Android icon](android-icon.png)

The linked resource report records an earlier APK inspection after resource shrinking: adaptive XML references both image layers, the manifest sets icon and roundIcon, and all six legacy fallback densities contain Looper rather than Unity’s placeholder. [Packaged resource report](android-icon-validation.json).


The launcher configuration remains active in 2.4.2; final package metadata and signing are checked for each release. The historical JSON report is not a physical-device installation test. The README promotional banner is separate from the Android launcher icon.
