# First game loop

Open `Assets/_RollingPanda/Scenes/GameLoopPrototype.unity`, press Play, then click Game view. Use Assets > Refresh if new files are not visible. Stop Play before opening the new scene.

Controls remain A/D or Left/Right for lanes, Space/W/Up to jump, S/Down to slide, P/Escape to pause, and R to restart. There is also a Restart button.

The course creates 24 fixed obstacle rows, starting at 40 metres and spaced 36 metres apart. Only one lane is blocked per row, so an adjacent lane always offers a safe alternative. Red blocks require dodging, gold hurdles can be jumped, and purple overhead beams can be slid under. Blue gem trails run through a safe neighbouring lane. Every gem counts once and adds 10 points. Score is whole metres travelled plus gem points.

A collision ends the run and freezes movement, score, and collection. Restart resets position, lane, camera, jump/slide state, pause, defeat, score, and every gem. The course ends at 990 metres; this checkpoint still uses a fixed course rather than endless generation. The previous MovementPrototype scene remains available without obstacles or scoring.

## Acceptance checks

1. Collect a blue gem: gem count increases once, score gains 10, and the gem disappears.
2. Pause beside gems: player and score remain still. Resume and continue collecting.
3. Run into a red block: the run ends with guidance to dodge, and movement/score stop.
4. Restart using both R and the button: score and gem count return to zero and all gems reappear.
5. Jump over a gold hurdle, slide under a purple beam, and dodge to a free lane. Test both success and deliberate collision for each type.
6. Hold S/Down through a purple beam, then release: the player stays low until there is room to stand. A tapped slide lasts one second; if it expires beneath a beam, standing is delayed until clear. Jumping beneath a beam must not force the player upright.
7. Restart while paused, jumping, sliding, and after defeat. Each time the player returns upright to the centre lane at the start.
8. Complete the course, restart, stop Play, and re-enter Play. No duplicate tracks or players, and no Console errors.

## Validation status

Purple-beam correction: beam undersides are now 1.35 metres above the track, compared with a 0.9-metre sliding capsule and a 2-metre upright capsule. Tapped slides last one second, holding S/Down sustains a slide, and a standing-clearance query delays standing or jumping into an overhead obstacle. Scripts compile after the change; the corrected interaction needs an in-editor retest. The user confirmed gem collection, hurdle jumping, red-block defeat, and R restart before this correction.

The user verified the preceding movement checkpoint in Unity: controls behave correctly and the Console has no red errors. This checkpoint's seven scripts were compiled against installed Unity 6000.6.5f1 and Input System assemblies. The new scene enables the course while the movement scene retains its default movement-only behaviour. Interactive collisions, pickup callbacks, visual layout, and restart behaviour require the checks above; no phone or player build has been tested.

The coloured shapes and keyboard overlay are temporary. The next checkpoints are phone controls, then endless sections and the panda/bamboo/temple art pass.
