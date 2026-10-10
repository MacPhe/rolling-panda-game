# Endless run checkpoint

Stop Play, allow scripts to import, and open `Assets/_RollingPanda/Scenes/EndlessPrototype.unity`. Press Play and click Game view. Swipe/mouse-drag and keyboard controls, Pause/Resume, Restart, and F8 safe-area preview remain available. Previous fixed-course scenes remain available as checkpoints.

## Behaviour

- Eight 96-metre sections are built once and recycled behind the player. The pool holds forty-eight obstacle slots and 120 gems; unused slots stay inactive. Runtime recycling repositions and resets existing objects rather than spawning additional objects or registering more gems.
- The opening section is clear. Before 150 metres each row has one obstacle. Between 150 and 300 metres, rows alternate between one and two obstacles. From 300 metres, each row has two obstacles in different lanes, leaving exactly one clear lane. The first eligible double row is at 168 m; consistently double rows begin at 312 m. Patterns are deterministic so restarting gives a comparable run.
- After 300 m, roughly two out of three rows offer gems in a gold-hurdle or purple-beam lane. Hurdle gems form an elevated jump trail, and beam gems sit low enough to collect while sliding. No gems are routed into red blocks. The remaining rows keep gems in the clear lane. Risk is optional: a clear lane is still available even when its row has no gems. Actual pickup timing and jump arcs need Play mode testing.
- Rows initially remain 48 metres apart. Sections beginning beyond 2,000 metres have three rows at 32-metre spacing; the transition begins around 2,064 metres. At the maximum speed this gives 1.23 seconds between rows, compared with 0.5 seconds of movement to cross both lanes. This is a rules-based margin, not a guarantee of comfortable human reaction time.
- Speed starts at 14 m/s and rises by 1 m/s per 150 metres, reaching 26 m/s at 1,800 metres. Approximate speeds: 16 at 300 m, 20.7 at 1,000 m, 24 at 1,500 m. There is no course finish at 990 metres in this scene.
- Every 960 local metres the player, sections, and following camera shift back together. Distance is accumulated separately from world position, so score and speed should remain continuous.
- Restart clears score, gems, defeat, movement state, speed progression, section schedule, and world offset. The original clear opening is restored, including after a long run.
- No extra loot, inventory, powers, or progression systems are included.

## Unity acceptance checks

1. Confirm first movement, swipes, jumping, sliding, gems, and obstacle defeat still work.
2. Play past 990 metres: the run continues and distance does not reset or jump backward. Watch for camera jumps near 960 and 1,920 metres.
3. Check the harder progression: mixed double-obstacle rows after 150 m, consistently double rows after 300 m, speed capped at 26 m/s from 1,800 m, and closer rows after roughly 2,000 m. After 300 m, try collecting both elevated hurdle gems and low slide-beam gems, then choose the clear lane to skip the risk.
4. Inspect the runtime Hierarchy: eight Track section objects remain present during the run. Stop/Play must not duplicate them.
5. Survive for ten minutes, watching for ground gaps, abrupt camera motion, missed pickups, or unavoidable layouts. This is a manual stability check, not an automated performance claim.
6. Restart after multiple section recycles, while paused, after defeat, and after a world shift. Speed returns to 14, counters to zero, and the clear starting area returns.
7. Check the Console during the run and after stopping Play. Actual memory/frame-time profiling and iPhone testing remain future checks.

## Verification

The user verified the initial endless version beyond 2,000 metres without issues. After the 14–26 m/s and risky-gem revision, editor and iOS conditional compilation passed without warnings against installed Unity references; this is not a player build. Pure rules tests passed for 30,000 row slots, multi-obstacle safe lanes, minimum spacing and two-lane transition margin, risk-gem introduction timing, exclusion of red/inactive obstacles, gem clearance above hurdles and beneath beams, deterministic selection, contiguous section coordinates, 100,001 speed samples, and a 100 km recycling/origin simulation. All 15 gesture regression checks passed. Revised pacing and risky pickups require Play mode testing; tests do not exercise Unity physics, rendering, trigger callbacks, or actual runtime memory.
