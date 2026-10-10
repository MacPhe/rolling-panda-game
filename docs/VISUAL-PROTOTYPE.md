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

## Forest and martial-arts pass

The visual scene now adds grassy banks, rounded hills and distant mountain silhouettes,
layered bamboo fronds, open-sided pavilions with columns, sloped roofs and raised eaves,
small altars, hanging lantern shapes and stone forest shrines. These are still procedural
shape studies for the anime-inspired Chinese forest direction, not finished environment
assets. All scenery stays outside the running lanes and has no active colliders.

Scenery is combined into shared-material meshes once per pooled section. Generated meshes
are destroyed when the scene closes. There is no scenery construction during recycling.
This reduces draw calls; actual mobile performance and startup cost remain unmeasured.

The panda run now blends opposing limb swings, body sway and headband motion. Jump uses
one front flip around the body centre over 0.72 seconds. Slide uses a low, feet-first
karate kick lunge. Collision capsules, jump arc, slide duration, controls and points are
unchanged. This is visual choreography, not an attack/damage mechanic. Restart clears
animation progress and pause freezes the pose.

Play-mode review: repeat jump/landing and jump immediately after sliding; hold slide
beneath purple beams; restart while flipping or lunging; pause/resume in both poses.
Run past 960 m to check scenery recycling. Check obstacle visibility in portrait and
landscape and look for visual clipping on slide transitions. Existing EndlessPrototype
remains the gameplay comparison scene. Desktop/editor and iOS-symbol compilation passed;
existing gesture/endless checks passed. Visual review and device profiling are pending.

## Tucked flip and side-kick refinement

The front flip now draws both arms and legs toward the body, curls the head forward,
and slightly compresses the visual silhouette into a panda ball. It tucks during the
middle of the flip and unfolds before landing. Rotation remains around the body centre.
The slide now rolls the torso onto its side, extends one leg forward along the track,
and folds the other leg back. These remain visual-only changes with unchanged jump
arc, slide timing and collision capsules.

Editor and iOS-symbol compilation passed. Play-mode checks pending: inspect the ball
silhouette through a full jump, landing recovery, side-kick direction, purple-beam
clearance and repeated slide-to-jump transitions.
