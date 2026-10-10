# Endless run checkpoint

Stop Play, allow scripts to import, and open `Assets/_RollingPanda/Scenes/EndlessPrototype.unity`. Press Play and click Game view. Swipe/mouse-drag and keyboard controls, Pause/Resume, Restart, and F8 safe-area preview remain available. Previous fixed-course scenes remain available as checkpoints.

## Behaviour

- Eight 96-metre sections are built once and recycled behind the player. The pool holds sixteen obstacles and eighty gems. Runtime recycling repositions and resets existing objects rather than spawning additional objects or registering more gems.
- The opening section is clear. Later sections have two obstacle rows, 48 metres apart. Each row blocks one lane only and places blue gems in a different lane. All nine combinations of lane and obstacle type can appear. Patterns are deterministic so restarting gives a comparable run.
- Speed starts at 10 m/s, increases with distance, and caps at 18 m/s at 2,000 metres. Row spacing gives at least 2.67 seconds between obstacle rows at the cap. There is no course finish at 990 metres in this scene.
- Every 960 local metres the player, sections, and following camera shift back together. Distance is accumulated separately from world position, so score and speed should remain continuous.
- Restart clears score, gems, defeat, movement state, speed progression, section schedule, and world offset. The original clear opening is restored, including after a long run.
- No extra loot, inventory, powers, or progression systems are included.

## Unity acceptance checks

1. Confirm first movement, swipes, jumping, sliding, gems, and obstacle defeat still work.
2. Play past 990 metres: the run continues and distance does not reset or jump backward. Watch for camera jumps near 960 and 1,920 metres.
3. Reach 2,000 metres: displayed speed reaches 18 m/s and stays there.
4. Inspect the runtime Hierarchy: eight Track section objects remain present during the run. Stop/Play must not duplicate them.
5. Survive for ten minutes, watching for ground gaps, abrupt camera motion, missed pickups, or unavoidable layouts. This is a manual stability check, not an automated performance claim.
6. Restart after multiple section recycles, while paused, after defeat, and after a world shift. Speed returns to 10, counters to zero, and the clear starting area returns.
7. Check the Console during the run and after stopping Play. Actual memory/frame-time profiling and iPhone testing remain future checks.

## Verification

Editor and iOS conditional script compilation passed without warnings against installed Unity references; this is not a player build. Pure rules tests passed for 20,000 rows, safe gem lanes, deterministic selection, all pattern variants, contiguous segment coordinates, 100,001 speed samples, and a 100 km mathematical recycling/origin simulation. All 15 existing gesture regression checks also passed. These tests do not exercise Unity physics, rendering, trigger callbacks, or actual runtime memory, which require the above Play mode checks.
