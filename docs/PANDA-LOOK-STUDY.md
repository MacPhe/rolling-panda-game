# Original panda look study

## Open it

Update phase-7-visual-prototype and wait for shader/script import to finish. Outside Play Mode choose **Rolling Panda > Open panda look study**. Unity offers to save the current scene, then opens or creates Assets/_RollingPanda/Scenes/PandaLookStudy.unity. Press Play.

Use Turn left/right to inspect the front, cheeks, side and back. Headband toggles the independent cloth accessory. Coat colour cycles charcoal, muted plum and rust while retaining the ivory regions. These are study controls, not shop purchases or saved equipment.

The study is separate from StartMenu and the running character, and is not added to build settings. Open StartMenu again to return to the game. Reopening the study reuses its scene without overwriting it. Its geometry is constructed when Play begins, so the bust does not appear in edit mode.

## Direction

Target: the approved original concept's rounded face, full muzzle, warm expressive eyes and short softly brushed coat. The main head is a single shaped parametric mesh rather than a sphere covered in fur lumps. This is a first construction study, not a final sculpt or finished animation rig.

Fur uses an original 256px generated fibre-coverage map and a URP shader. Six fine shells sit close to an opaque base, with light downward grooming and a short 0.014-unit coat. There are no cone spikes or rounded geometric bumps. Small facial pieces use only the base layer to keep eyes/muzzle readable and reduce layer cost. The shader has simple studio lighting, no fur physics or shadow-casting; it is not XFur and does not claim equivalent quality/features. No paid assets or copied vendor shaders/textures are used.

The study's separate headband and independently tinted dark coat establish the cosmetic direction. Existing gameplay art/animations are retained until this appearance is visually reviewed. Full body, skinning, animation, fur distance detail and gameplay lighting integration follow after the face/coat direction is sound.

## Review

Take a front and a three-quarter screenshot at a useful Game view size. Compare head shape, eye placement, muzzle and fur silhouette with the approved artwork. Check the ivory/charcoal contrast, remove the headband, try the other palettes, and inspect whether the layer edges look like soft short fibres. Check the Console for shader errors or pink rendering.

C# compilation passed for runtime/editor and iOS defines. HLSL compilation, actual appearance and physical-iPhone performance have not been verified here. Shell rendering adds draw calls and overdraw; this close-up study is not yet a mobile performance-approved replacement for the runner.

## Screenshot review refinement

The first screenshots exposed a key light facing away from the front, near-black coat losing detail, raised eye-patch discs, a smile buried in the muzzle and shell fibres intersecting the band. The study now corrects the existing key at runtime (so its saved scene need not be recreated), raises charcoal to a readable dark grey, paints softly edged eye patches directly on the head shader, reduces eye protrusion, moves the smile in front of the muzzle, and masks outer fur layers beneath the headband. Patch colours follow the coat palette. These changes require another Unity visual review; shader compilation is not verified by the C# checks.

## Integrated face pass

Removes the two separate muzzle ellipsoids and builds a broad smooth muzzle bulge directly into the head surface. The head has denser topology and recalculated smooth normals with a shared seam. The nose is narrower at its base, and a continuous tube mesh follows the actual curved muzzle for the smile/philtrum. Larger irises and reduced eye-white height aim to soften the startled expression; upper eyelid rims and flatter brows add definition. The band now has six vertical subdivisions with shallow folds. Fibre strokes are longer and tiled less densely to reduce the grainy appearance. This remains a procedural study awaiting front/three-quarter/side visual review, not a finished character or validated match to the concept.
