# Panda appearance: first refinement

The character keeps its existing transform animation rig, jump, tucked front flip, side kick slide and menu handoff. This is a procedural character study, not a finished sculpted and skinned production model.

## Visible changes

- Warm ivory and charcoal short-fur surfaces share a generated 128px directional fur texture per panda.
- Short, rounded mesh clumps break up the outline of the head, ears, body, limbs and tail. Clumps use smooth ellipsoid normals and project only about 0.03 unit beyond the head surface (less on limbs), replacing the pointed cone tufts. Fur strokes have low contrast and a small smoothing pass to reduce grain. Each part batches its clumps into one mesh; there are no individual hair objects or simulated strands.
- The running panda has larger eye whites, amber irises, glossy pupils and catchlights, shaped brows, inner ears, fuller cheeks and a subtle smile.
- The sleeping menu panda uses the same surface treatment and fur outline while keeping its closed eyes and existing sleeping pose.

## Cosmetics foundation

The gameplay PandaAppearance component exposes head, back, waist and left/right wrist sockets. Headband and knot/ribbons live under a removable cosmetic root and can be hidden with SetHeadbandVisible(false), without hiding the base body or face. This adds attachment points, not a shop or an equipment persistence system. The sleeping panda retains its separately built headband; sharing an authored character prefab across both poses is a later production step.

Future cosmetics should be independent meshes attached to these sockets. Keep skin/fur colour changes separate from equipment. A later authored model should retain these socket names and the established proportions while replacing the primitive construction with clean topology, UVs, skin weights and facial controls.

## Review in Unity

1. Update the visual-prototype branch and open StartMenu. Check the sleeping panda close up, especially fur edges and headband.
2. Press Roll/Play. Check the furred gameplay panda appears correctly after the fade.
3. Inspect face and fur in Scene view while paused; the normal chase camera mostly shows the back, where the fur outline should be visible too.
4. Test run, jump/front flip, side kick slide, restart and return to menu. Look for detached detail or clipping during the tuck.
5. Compare daylight and night. Fur should read as short clumps, not a shiny plastic or spiky hair shell. Capture a close-up and a normal gameplay view to guide the next sculpting pass.
6. Check device performance when iPhone testing becomes available. There is no mobile frame-rate claim from the desktop compile checks.

C# compilation was checked for editor/runtime and iOS defines. Unity rendering, appearance and animation require the above Play Mode review; no rendered preview was verified in this environment.
