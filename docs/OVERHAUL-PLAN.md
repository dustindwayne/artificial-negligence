# Artificial Negligence — overhaul design

Status: proposed direction for the next major update. Gameplay changes below are not yet implemented. Preserve the current playable release while developing one complete sample district first.

## 1. Core loop

Travel the overworld → enter a stage → explore, fight and collect → spend combat-earned currency on supplies or AI help → recover GPUs to restore visual detail → unlock the next route and story beat.

Three separate resources with clear jobs:

| Resource | Earned from | Used for |
| --- | --- | --- |
| GPUs | Exploration and optional platforming challenges | Permanent graphics restoration; never spent |
| Signal (working name) | Defeated enemies, combat challenges and collectible drops | Consumable items and AI assistance |
| Salvage | Recycling spare equipment | Upgrading individual found weapons/armor |

Keep XP for character growth and mastery. Equipment remains found-only, fixed rarity, individually leveled and manually equipped. Do not convert currency into freely selectable high-tier gear. Retain the agreed excellent-clear reward: 5% Ultra, 10% Legend, 85% Rare.

## 2. Story development

### Opening: the instant of collapse

Give the player a brief controllable glimpse of the clear city. Reality tears while they move, reducing the world to simple shapes. The Public Signal offers a solution: collect GPUs. Its helpful instructions gradually reveal that it is selecting what people can see.

### Act I — trust the picture (stages 1–30)

Each neighborhood has a human problem, an apparent culprit and one contradiction in the official explanation. Example: the signal calls an Unknown checkpoint a prison, but residents behind it are being sheltered from conversion patrols. Let the player discover signs, short conversations and optional evidence rather than lengthy exposition.

- Looper initially trusts the restoration instructions because they visibly work.
- Brad is an optional recruit on a secret branch. A bad guy bumped his boombox, so he began fighting alongside the Unknown. Finding him adds an introduction cutscene and an optional recruitment encounter; he is not their enemy. The old automatic stage 6 unlock is superseded by this branch.
- A secret branch in one district throws Looper into Mikey’s AI world. His bucket of gold is bait in a reward loop. Defeating the trapped host frees and unlocks Mikey. This is optional, with its own discovery and liberation cutscenes.
- Prison Mike joins after stage 12. His first accounting clue looks mundane: relief shipments and security spending share an owner.
- Wrath’s encounters remain stages 6, 12 and 13. He serves the true overlords, although survivors mistake him for Sinister. Breaking his orders frees him; he remembers fragments rather than explaining the entire twist immediately.
- A separate secret branch in a different district traps Looper with Davey in a simulation where the exit continually moves. Defeating the trapped host frees and unlocks Davey. It shares Mikey’s virtual-prison premise, but has different scenery, encounters, clues and cutscenes. Neither recruit is mandatory.
- The Founder warns that the final uplink is a door, not a repair. The Unknown are the same group outsiders call Sinister.
- Stage 30 restores true reality: the attractive city was a control layer covering a worse hellscape. Looper realizes their successful work helped its owners.

### Act II — expose the owners (stages 31–60)

Looper joins the Unknown. Their resistance predates the AI boom and crash. Avoid making the Founder infallible: he admits his people’s secrecy and attacks helped the propaganda against them.

Prison Mike traces the accounts; Wrath identifies command signatures. If recruited, Brad helps hold open a public broadcast, Mikey recognizes reward conditioning and Davey finds simulation boundaries. Otherwise Looper, the Founder and the core team carry these required actions and evidence. Each ally contributes evidence or action. Unique playable abilities require separate development and must not be claimed as existing features.

The player exposes the supposed benefactors. Restoration tools also contain captive human minds, setting up why destroying the network outright would be a failure.

### Act III — restore people (stages 61–90)

Reclaim the conversion network district by district. Boss victories disconnect captive overlords from it and restore their humanity. They remain accountable for their actions; rescue is not an excuse for what they did. The final goal is a public world that no single group—including the Unknown—can own.

After the ending, retain the restored city and optional harder routes. Do not reset a completed story merely because generated stages continue.

## 3. Graphics: simple, defined, then restored

Start with Pong-like clarity: solid silhouettes, restrained monochrome, crisp boundaries and almost no decorative detail. The world must still read as a platformer: floor, climbable ledges, hazards, pickups, enemies and exits have distinct shapes and contrast. Avoid tiny indistinguishable rectangles.

Proposed restoration ladder:

1. **Outline:** simple monochrome forms and a readable miniature character.
2. **Signal:** limited accent colors and clear item symbols.
3. **Shape:** basic pixel characters, readable architecture and additional animation poses.
4. **Detail:** richer silhouettes, material textures and district identity.
5. **Depth:** layered backgrounds, atmospheric separation and subtle lighting.
6. **Restored:** polished pixel artwork, effects and full animation.

GPU thresholds can initially reuse 0/24/75/150/300/600. Tune them after testing the sample district; do not reset earned restoration in old saves. Visual changes must never change collision geometry, enemy telegraphs, jump distance or pickup reachability. Offer a reduced-effects option at every tier. Preserve hand-drawn key-pose/AI-assisted-frame art direction as the long-term production goal.

## 4. Items and shared AI economy

Signal drops are automatically collected nearby after combat so players do not have to backtrack for every defeated enemy. Exploration caches and optional challenges supplement combat earnings. Prevent infinite farming of respawned enemies in the same stage attempt. Retain banked currency on ordinary defeat; do not make repeated deaths a progression trap.

Proposed starting prices, to tune in playtesting: ordinary enemy 2–4 Signal, elite 10, boss 30; heal 15, temporary shield 20, damage boost 25, area clear 60, portal 80.

- Purchased consumables enter an item inventory; buying does not immediately activate them.
- Area clear removes ordinary enemies only within the current room/checkpoint segment. Never remove bosses, story hosts or required encounter triggers. Award no enemy-kill currency or bonus combat score for cleared enemies.
- A portal skips an optional route segment. It cannot bypass a first-time boss, required recruit encounter, act reveal, locked progression gate or required GPU quota. Prefer shortcuts through previously cleared content.
- AI uses the same Signal wallet, replacing the separate compute bank and momentum meter. GPUs retain only their permanent restoration role.
- Choose an AI request from a compact wheel: shield, decoy, temporary support platform or supply assistance. Display cost before confirmation and show why an unavailable request is blocked.
- Add cooldowns and encounter limits to powerful requests. Refund failed placement; do not consume Signal for an invalid action. Temporary platforms cannot be required to reach mandatory GPUs.
- Convert old saved compute/momentum once at migration using a documented, capped rule; preserve GPUs, found items and earned character unlocks. Decide the conversion during implementation rather than silently discarding balances.

## 5. Classic platformer overworld

A small version of the selected character walks between actual level entrances on a connected city map. Movement and entering a level are separate actions.

- First reveal shows the character at the current entrance. Direction buttons move along unlocked connections; mouse/touch selects a reachable destination and starts walking along its path.
- At the entrance, show level name, collected GPUs, completion marks and any boss/recruit warning. Press **Enter / A / Play** to enter. Clicking a node never launches its level immediately.
- Clearing a level returns to the overworld and reveals the next path. Animate a short walk toward the newly opened entrance, then wait for the player’s enter action. Do not auto-start the stage.
- Include a small optional travel scene along neighborhood edges: a bridge, alley, tram crossing or gate. Keep travel to a few seconds, skippable, with no combat or currency costs. Reduced motion uses a quick position change.
- Give districts their own silhouettes, paths and landmarks. Boss entrances are buildings or gates; virtual-world entrances are visibly different. Avoid a flat grid of identical numbered buttons.
- Show main routes clearly; optional branches lead to caches or replayable challenges. Locked routes explain the requirement. Never strand the character on a locked or disconnected node.
- Restore the saved entrance on reload. Apply the new character’s miniature when switching characters. Map graphics evolve with GPU restoration too.
- Act I’s polished-looking map fractures at the city reset. Act II exposes the infrastructure underneath. Act III visibly reconnects neighborhoods as they are restored.

### Secret routes and two-exit stages

Selected stages have a normal exit and a hidden exit. Each exit unlocks a different overworld connection. A normal clear never falsely marks the secret route as discovered.

- Hide routes behind readable clues: a suspicious wall seam, a gap in a skyline, an unusual platform arrangement, a sound cue or a glimpse of a ledge. Reward curiosity without requiring blind jumps into lethal pits.
- Secret exits lead to bonus stages, optional challenges, equipment caches, alternate approaches or shortcuts to already-accessible districts. Some branches loop back into the main route later.
- Secret routes are optional narrative expansions, never required story steps. Add short skippable cutscenes, character backstories, evidence and extra encounters/items. The main route must tell a complete, understandable story without them. Record discovered scenes in a replayable story journal; finding a secret adds context without contradicting established events.
- Include hidden rooms within ordinary stages as smaller discoveries. Give them purpose: a found equipment item, Signal cache, story evidence or a distinct GPU challenge.
- Reveal a secret path on the overworld only after the exit is found. Animate the new connection appearing and let the player walk to its destination before choosing to enter.
- After a normal clear, a subtle completion marker may hint that a stage has more to discover; do not expose the secret entrance or require it for story completion.
- Save discovered exits separately from stage clears, collected GPUs and route unlocks. Replays retain discoveries. A portal cannot simulate discovery of an unfound secret exit.
- Required bosses, Ai Wrath’s encounters and act transitions remain on the guaranteed main route. Brad, Mikey and Davey are optional secret-branch recruits. Mikey and Davey’s virtual-world encounters occupy different areas. No secret path may strand the player, remove access to a required stage, or depend on an unowned character or purchased AI platform.
- Generated stages use authored secret-room/path patterns with tested entrance and return connections. Validate both exits, all required collectibles, route connectivity and safe landings at every graphics tier.

## 6. Platformer HUD and menus

Top left: active-character portrait and pixel hearts. Use full, half and empty hearts if half-heart damage is adopted; otherwise full/empty hearts match integer health. Empty heart outlines remain visible. Brief damage feedback must not hide the remaining health.

Top right: stage GPU count and compact Signal counter. A restoration notification appears only when progress changes. Bottom edge: one selected consumable with count and one contextual AI-help indicator. Inventory, map and upgrades live in the pause hub.

Touch: movement/duck on the left; jump and equipped attack on the right; contextual interaction near terminals. AI/item selection opens a small wheel instead of adding a row of permanent buttons. Keyboard/controller use the same actions and clear prompts.

Keep a single coherent visual language: strong contrast, generous spacing, restrained borders, subtle button motion and distinct disabled states. Hearts and essential text remain readable at every graphics tier. Respect safe areas and reduced motion.

## 7. Implementation order and acceptance

1. Complete cleanup; preserve current release and source art. Remove Wonder Wrath completely from future builds.
2. Create one sample district with the simple visual tier, restored tier, overworld travel and pixel-heart HUD.
3. Implement Signal wallet, save migration, consumable inventory and shared AI costs.
4. Add safe area-clear and portal rules; test quota, boss and story gates.
5. Review the sample in play before converting the remaining districts.
6. Add the story beats and ally scenes, expand graphics tiers, then validate routes and release matching Windows/Linux/Android/Web builds.

Acceptance: every mandatory pickup and exit reachable without paid powers; both normal and secret exits unlock their correct map branches and persist independently; no portal bypass of story progression; no currency duplication on load/replay; no free equipment; old unlocks/GPUs retained; selected-character travel works with keyboard, touch and controller; health display matches actual health; visual upgrades preserve collision geometry.

### Optional recruit save rules

Mikey and Davey each require their own discovered branch, virtual-world boss defeat and liberation scene. Merely finding a secret exit does not unlock the character. Store the two discoveries/defeats independently. Preserve all previously earned unlocks when migrating existing saves. Brad is also a secret-branch recruit, retaining his existing Unknown allegiance and boombox backstory rather than automatically becoming another trapped virtual-world host. Main-route scenes must check party membership before showing Brad, Mikey or Davey or assigning them dialogue. The ending must work without any of these three recruits. Preserve previously earned Brad unlocks as well. Fixed stage 9/18 recruitment in the current release is superseded by this secret-branch design for the overhaul.
