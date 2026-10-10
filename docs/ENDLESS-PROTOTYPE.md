# Endless run checkpoint

Stop Play, allow scripts to import, and open `Assets/_RollingPanda/Scenes/EndlessPrototype.unity`. Press Play and click Game view. Swipe/mouse-drag and keyboard controls, Pause/Resume, Restart, and F8 safe-area preview remain available. Previous fixed-course scenes remain available as checkpoints.

## Behaviour

- Eight 96-metre sections are built once and recycled behind the player. The pool holds forty-eight obstacle slots and 120 gems; unused slots stay inactive. Runtime recycling repositions and resets existing objects rather than spawning additional objects or registering more gems.
- The opening section is clear. Before 500 metres each row has one obstacle. Between 500 and 1,000 metres, rows alternate between one and two obstacles. From 1,000 metres, each row has two obstacles in different lanes, leaving exactly one clear lane. Gems follow that clear lane. The other lanes can also be traversed by correctly jumping or sliding, depending on obstacle type. Patterns are deterministic so restarting gives a comparable run.
- Rows initially remain 48 metres apart. Sections beginning beyond 2,000 metres have three rows at 32-metre spacing; the transition begins around 2,064 metres. At the maximum speed this gives 1.45 seconds between rows, compared with 0.5 seconds of movement to cross both lanes. This is a rules-based margin, not a guarantee of comfortable human reaction time.
- Speed starts at 10 m/s and rises by 1 m/s per 150 metres, reaching 22 m/s at 1,800 metres. Approximate speeds: 13.3 at 500 m, 16.7 at 1,000 m, 20 at 1,500 m. There is no course finish at 990 metres in this scene.
- Every 960 local metres the player, sections, and following camera shift back together. Distance is accumulated separately from world position, so score and speed should remain continuous.
- Restart clears score, gems, defeat, movement state, speed progression, section schedule, and world offset. The original clear opening is restored, including after a long run.
- No extra loot, inventory, powers, or progression systems are included.

## Unity acceptance checks

1. Confirm first movement, swipes, jumping, sliding, gems, and obstacle defeat still work.
2. Play past 990 metres: the run continues and distance does not reset or jump backward. Watch for camera jumps near 960 and 1,920 metres.
3. Check the harder progression: mixed double-obstacle rows after 500 m, consistently double rows after 1,000 m, speed capped at 22 m/s from 1,800 m, and closer rows after roughly 2,000 m.
4. Inspect the runtime Hierarchy: eight Track section objects remain present during the run. Stop/Play must not duplicate them.
5. Survive for ten minutes, watching for ground gaps, abrupt camera motion, missed pickups, or unavoidable layouts. This is a manual stability check, not an automated performance claim.
6. Restart after multiple section recycles, while paused, after defeat, and after a world shift. Speed returns to 10, counters to zero, and the clear starting area returns.
7. Check the Console during the run and after stopping Play. Actual memory/frame-time profiling and iPhone testing remain future checks.

## Verification

The user verified the previous easy version beyond 2,000 metres without issues. After this difficulty revision, editor and iOS conditional script compilation passed without warnings against installed Unity references; this is not a player build. Pure rules tests passed for 30,000 row slots, multi-obstacle safe lanes, minimum row spacing and two-lane transition margin, deterministic selection, all pattern variants, contiguous segment coordinates, 100,001 speed samples, and a 100 km mathematical recycling/origin simulation. All 15 gesture regression checks also passed. The revised difficulty and larger bounded pool still need Play mode testing; tests do not exercise Unity physics, rendering, trigger callbacks, or actual runtime memory.
