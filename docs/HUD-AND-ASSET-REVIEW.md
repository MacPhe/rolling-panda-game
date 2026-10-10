# Imported asset review and HUD checkpoint

Inspected actual files in the current Unity project (10 October 2026). This is a file/
prefab/material review, not a visual or device-performance certification.

## Implemented

- Alagard font extracted from the supplied alagard.zip into the local project's
  Assets/_RollingPanda/Resources/Presentation folder.
- Blue/red/green pickup icons and counters at the top left; score and metres at the
  top right, stacked on narrow screens. Large title, control hints and status strip
  are removed from active gameplay; hints/state appear in a central pause/end panel.
- Existing bottom button positions and mobile touch handling retained. Pointer
  exclusion reduced to the compact top HUD and existing bottom controls.
- Diamondo mesh used in VisualPrototype for all three pickup colours, including the
  temporary green pouch representation. Existing trigger sizes, placement, point
  values, eligibility thresholds and restart behaviour retained. Only the mesh rotates;
  the vendor's float/scale animation is not attached.
- Matching transparent 96px icons are projected from the actual Diamondo mesh in the
  editor. They are baked once, not rendered each frame.

## Local preparation and checking

Stop Play and allow Unity to compile/import. The editor prepares
Assets/_RollingPanda/Resources/Presentation/GameArt.asset and three gem PNGs from
Assets/BTM_Assets/BTM_Items_Gems/Models/Diamondo.fbx. If needed, use
Rolling Panda > Refresh Diamondo HUD art. Then open VisualPrototype and press Play.
The Diamondo importer is made readable to support icon baking. The mesh reference and
icons are serialized for player builds; runtime does not depend on UnityEditor APIs.

The integration code is saved in PR #7. User-imported vendor packs, generated images,
font binary and their dependencies remain local rather than being uploaded as raw asset
files to the public repository. A fresh checkout needs those assets imported and the
font placed in the Presentation resource folder before using the custom presentation.
The supplied font ZIP contained only alagard.ttf, with no accompanying licence text.

Desktop/editor, iOS-symbol and editor-art-preparation compilation passed. Existing
15 gesture checks and endless/scoring/pickup/pool checks passed. Unity-rendered appearance
and actual device performance remain pending. The current editor log was not accessible
for inspection. Check portrait and landscape, F8 safe-area preview, all pickup colours,
restart counters, pause/resume buttons, and recycling past 960 m.

## Plausible next assets

| Asset found | Plausible use | Integration notes |
|---|---|---|
| BTM_Items_Gems/Models/Diamondo.fbx | Main blue/red/green pickups and their matching HUD icons | Implemented mesh-only; our URP materials preserve the requested colours. |
| LowPolyTrees/Prefabs/Tree1_summer through Tree8_summer | Main forest canopy; replace procedural tree masses with a varied pool | Best next checkpoint. Review silhouettes and scale in our camera; convert copied materials for URP. |
| Birch1–3_summer | Secondary tree shapes further from the road | Good for variation without replacing the bamboo identity. |
| Bush1_summer, Bush2_summer | Roadside undergrowth and gaps between trunks | Mix with existing pointed ferns. Review density and lane visibility. |
| Log1–3_summer, Stump1–6_summer | Fallen timber and weathered forest-floor detail | Start outside lanes; do not introduce new obstacle mechanics yet. |
| Autumn tree/bush/birch/log/stump variants | A distinct golden forest stretch later in long runs | Good future environment progression after the summer forest is established. |
| COMICOMI/VFX_FallingCherryBlossom/Art/Prefabs/FX_huaBan.prefab | Drifting petals near shrines or a blossom grove | Preview the material in URP, use a small bounded effect, keep petals off the obstacle sightline. |
| BTM Coin, GoldBar, GoldPile, Money | Future currency/reward presentation | Defer until the pouch economy is defined. |
| BTM Magnet, Shield, Clock/Time, SpeedChev variants | Future power-up presentation | Suitable candidates for later mechanics; none activated now. |
| BTM Trophy, Star/StarGem variants | Results, milestones and achievements | Useful later UI/reward assets. |
| BTM Ruby/Ruby2, other diamond models | Special rewards or alternate collectible designs | Keep current Diamondo colour tiers consistent for now. |
| Palm1/2 and winter variants | A deliberately different future region | Lower priority for the lush Chinese forest direction. |

## Compatibility evidence and limits

- BTM pack includes 52 item prefabs. Diamondo's prefab references its FBX and a vendor
  material plus SimpleGemsAnim, configured to rotate, float and scale. Reusing that
  behaviour directly could move pickups away from our tested obstacle paths.
- LowPolyTrees includes 76 prefabs spanning summer/autumn/winter, trees, birches,
  bushes, logs, stumps, palms and ground. Tree1_summer contains an enabled mesh collider.
  Off-road scenery should disable unnecessary colliders and join the bounded pool.
- LowPolyTrees material references a built-in shader ID (46); the cherry-blossom
  material references built-in shader ID 211, rather than a project URP shader asset.
  Treat both as material-conversion candidates, not confirmed drop-in URP assets.
- The blossom effect loops and plays on awake, with a configured maximum of 1000
  particles. That is a capacity setting, not a measured live count or performance result.
- No full pipeline conversion or blanket replacement of the imported assets was made.
  Next recommendation: test a small summer-tree/bush/log set in the visual scene, then
  add a restrained blossom effect around one shrine section.

## Approved asset integration follow-up

Summer/autumn Tree1–8 and Bush1–2, Log1–3, Stump1–6, and the cherry-blossom effect are
now wired into VisualPrototype via a locally generated EnvironmentArt catalog. Trees
and bushes replace the corresponding procedural shapes; bamboo, ferns and terrain
remain. Autumn sections begin around 2400 m and continue to 4800 m within each 6000 m
route. Logs/stumps remain off-road. Warm shrine lighting is retained with nearby petals.
Runtime URP material copies preserve source atlas textures/tints without modifying vendor
materials. Imported scenery colliders and behaviours are disabled. Both seasonal groups
are prepared once in the existing pool, and petal systems are bounded and distance-culled.

Stop Play and allow imports, or use Rolling Panda > Refresh imported forest art.
Editor/mobile-symbol/editor-preparation compilation and existing gameplay checks passed.
Visual appearance, source pivots/scale, transparency and performance remain pending.
Raw imported assets remain local dependencies. Future power-ups remain deferred.
