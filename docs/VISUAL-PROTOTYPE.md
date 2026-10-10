# Visual prototype — first art checkpoint

Open Assets/_RollingPanda/Scenes/VisualPrototype.unity and press Play.
For comparison, stop Play and open EndlessPrototype.unity. That scene is unchanged.
Both run the same movement, obstacles, thresholds, points and collectible rules.

This first pass establishes the rounded black/ivory panda, red headband, readable poses,
jade bamboo, stone verges, red temple pavilions and misty atmosphere. It is a procedural
3D shape study, not the final semi-realistic character: no fur textures, skeletal rig,
finished temple meshes or authored animation assets are included yet.

Scenery is non-colliding and attached once to the existing eight pooled track sections.
It recycles and shifts origin with them; no decoration is instantiated during the run.
Obstacle and pickup colours remain the tested gameplay colours for readability.

Compare lane changes, jumps, slides beneath purple beams, restart and pause. Run beyond
960 m to inspect scenery recycling and camera continuity, and compare a narrow portrait
Game view against landscape. The panda should not obscure the next obstacle row.
Check lighting and silhouettes before adding more foliage or visual effects.

Validation: desktop/editor and iOS-symbol C# compilation plus existing gameplay rule
checks. Actual Unity appearance, clipping, draw calls and device performance require
Play-mode/device review. No iPhone performance claim is made.

Next art steps: refine silhouette and camera from play feedback; build a proper rigged
panda with facial expressions; replace pavilion/block shapes with temple environment
assets; then collection feedback, audio and UI polish. Persistent pouch economy remains
a later feature. VisualPrototype is intentionally not added to release build scenes yet.
