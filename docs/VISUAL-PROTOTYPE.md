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

## Evolving forest light

VisualPrototype now changes atmosphere with total run distance: warm daylight at the
start, mist building from 800 to 1500 m, dusk from about 1800 m, and a blue moonlit
shrine forest by 3400 m. Dawn returns between 4800 and 6000 m, then the six-kilometre
lighting journey repeats smoothly. These are first-pass adjustable art timings, not
new difficulty thresholds. Existing movement and collision rules are unchanged.

Fog colour/distance, directional light, ambient fill and forest palette transition
together. Night keeps blue ambient fill and directional light so obstacles stay readable.
Shrine windows and pavilion lanterns become warmly emissive as darkness builds. Up to
four nearby shadowless point lights illuminate the shrine surroundings; they fade with
distance and follow pooled shrine sockets through section recycling and origin shifts.
Current desktop/mobile URP assets both allow four additional lights per object. No bloom
is required for the luminous material, and there is no mobile performance claim yet.

Restart returns daylight; pause freezes distance and therefore the atmosphere. The
unchanged EndlessPrototype remains available for comparison. The scenery geometry is
currently shared between moods: this checkpoint is lighting/palette progression, not
separate biome layouts. Future passes can introduce distinct forest/temple environments.

Validation: editor and iOS-symbol compilation and existing gesture/endless checks passed.
Play-mode review pending: run to 1500, 2400 and 3400 m; inspect warm shrine lighting,
purple-beam/red-block visibility, reset to daylight, and scenery continuity after rebases.
If running beyond 6000 m, check the dawn-to-daylight loop. Device profiling remains pending.

## Mountain detail and obstacle art

Added layered rock faces, moss shelves and horizontal strata to the hillsides, with
ferns, exposed roots and weathered stepping stones outside the running lanes. All are
non-colliding scenery combined into the existing section meshes at startup.

VisualPrototype now dresses the existing pooled obstacle boxes as weathered stone
ruins (red dodge marking), timber barriers (gold jump bands), and temple timber lintels
(purple slide bands). The forms remain close to their collision bounds, without adding
unmarked posts or changing the slide gap. All three variants are built once per obstacle;
only the matching variant is active when the pooled obstacle changes type. No new
colliders, gameplay rules, pickup positions or timing changes are introduced. The plain
EndlessPrototype retains its original blocks for comparison.

These are still procedural art forms, not finished sculpted/textured assets. Editor and
iOS-symbol compilation and existing gameplay checks passed. Play-mode review pending:
identify all three actions at speed in daylight and night, check purple-beam clearance,
run past section recycling, and compare collision edges. Added geometry/renderers need
profiling before making any mobile performance claims.

## Rocks, floating lanterns and enclosed forest

Dodge obstacles are now large asymmetrical faceted rock formations with red mineral
markings. Jump obstacles are smaller uneven rock formations with gold markings. Slide
obstacles are luminous purple floating lanterns with timber caps and gold ribs. Lanterns
stay at the tested beam height; no bobbing or tassels change their clearance. Original
box collisions remain conservative at the rocks' chamfered upper corners; check the
visual/collision agreement before choosing final authored rock assets.

The visual scene road now uses staggered cobbles with small rotational variation, built
directly as one surface mesh per pooled section before scenery batching. Original bright
lane lines and distance stripes are hidden only in VisualPrototype. Collision stays flat
so cobbles do not alter jumps, ground detection or slide movement.

Grassy knolls, layered shrubs, grass sprays and high foliage crowns fill the roadside
and draw the forest closer around the path. Running lanes stay open; all extra scenery
is non-colliding. Existing day/night palette transitions and shrine lighting are retained.
Generated rock meshes and lantern materials are cleaned up with the scene. No spawning
of art occurs during the run. These remain procedural geometry rather than finished
sculpted/textured assets; increased scenery density needs device profiling.

Validation: editor and iOS-symbol compilation and existing gameplay checks passed.
Play-mode appearance review pending: dodge/jump rock readability and edge collisions,
lantern slide clearance in daylight/night, cobble continuity at section joins, forest
occlusion in portrait, and recycling after 960 m. EndlessPrototype remains unchanged.
