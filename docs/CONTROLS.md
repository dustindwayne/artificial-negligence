# Controls — 2.4.9

## Supported targets

- **Windows:** Xbox controllers and other controllers exposing XInput. Up to four device slots are scanned; one connected controller controls this single-player game. DirectInput-only devices require an XInput-compatible mode or mapper.
- **Web:** browser-recognized gamepads with the standard Gamepad API mapping, including Xbox-style controllers. Click the page/game, then press a controller button so the browser exposes the device. Use HTTPS on the public website. Nonstandard browser mappings are not guessed.
- **Android:** touch controls remain the supported input for this release; native Android controller support is not implemented.

## Mapping

| Action | Xbox-style input |
| --- | --- |
| Move / duck | Left stick or D-pad; down ducks |
| Jump | A |
| Attack / equipped weapon | X or RT, hold to repeat |
| Slide while moving | B |
| Dash | RB |
| Nearby intellect terminal | Y |
| Earned AI assist | LB |
| Cycle assist type | LT |
| Inventory | View / Back |
| Pause / resume pause screen | Menu / Start |
| Navigate menus | Stick or D-pad |
| Confirm menu / advance dialogue | A |
| Back / skip dialogue | B |

Character selection and upgrades are reachable from the pause menu. The character screen allows gameplay-unlocked or code-unlocked characters and found equipment; controller support grants no items or progress. Full momentum and sufficient compute are still required for AI assistance.

## Behavior

A 22% axis deadzone filters stick drift, and movement retains analog strength. D-pad input overrides the stick. Menus use directional selection with a 0.22-second repeat interval, skip disabled buttons, and highlight the selected button. Disconnecting during play pauses the game. Keyboard and mouse remain available. Held action buttons must be released after a screen transition, preventing an A press used to confirm a menu from becoming a jump.

## Verification and limits

Windows runtime checks inject normalized controller frames to verify deadzone behavior, menu confirmation, held-button gating, movement/duck/jump/attack routing, disconnect-to-pause, and dialogue confirmation/skip. The web adapter is built as a JavaScript plugin using the standard browser mapping. This is software-level validation, not a completed physical Xbox-controller playtest. USB/Bluetooth hardware testing across browsers remains outstanding. Browser audio/fullscreen activation still requires page interaction.

## Android touch layout

Left/right move. Hold Down to duck; press Down while moving to slide. Jump is the large right-hand button; Dash and Fire sit beside it. Fire appears only after a found weapon is equipped. AI Help remains disabled until full momentum, enough compute and cooldown readiness. The Interact prompt appears only near an intellect terminal.

Menu groups inventory, character, upgrades, AI type, settings and the full control guide. The active character has a larger preview with a cosmetics dropdown. Disabled outfits cannot be equipped; Escape/controller B closes the list first, and tapping outside also closes it. In-run settings returns to Pause.
