# Endless run checkpoint

Stop Play, allow scripts to import, and open `Assets/_RollingPanda/Scenes/EndlessPrototype.unity`. Press Play and click Game view. Swipe/mouse-drag and keyboard controls, Pause/Resume, Restart, and F8 safe-area preview remain available. Previous fixed-course scenes remain available as checkpoints.

## Behaviour

- Eight 96-metre sections are built once and recycled behind the player. The pool holds forty-eight obstacle slots, eighty regular gem slots, and eight pouch slots. Unused slots stay inactive; no new objects or registrations are added during recycling.
- The opening section is clear. Before 150 metres each row has one obstacle. Between 150 and 300 metres, rows alternate between one and two obstacles. From 300 metres, each row has two obstacles in different lanes, leaving exactly one clear lane. The first eligible double row is at 168 m; consistently double rows begin at 312 m. Patterns are deterministic so restarting gives a comparable run.
- After 300 m, roughly two out of three rows offer gems in a gold-hurdle or purple-beam lane. Hurdle gems form an elevated jump trail, and beam gems sit low enough to collect while sliding. No gems are routed into red blocks. The remaining rows keep gems in the clear lane. Risk is optional: a clear lane is still available even when its row has no gems. Actual pickup timing and jump arcs need Play mode testing.
- Primary rows remain 48 metres apart. Beyond 600 m, single-obstacle rows appear halfway between them, including across section boundaries; the first appears at 624 m. This replaces the previous denser-primary-row stage after 2,000 m. At 32 m/s the alternating rows are 0.75 seconds apart. Consecutive primary clear lanes are adjacent, and the intermediate obstacle is outside both lanes, preserving their connecting route. Raw one-lane movement takes 0.25 seconds; human reaction time and optional risky routes still need playtesting.
- Speed starts at 15 m/s and rises by 1 m/s per 150 metres, reaching 32 m/s at 2,550 metres. Speeds are approximately 19 at 600 m, 21.7 at 1,000 m, and 30 at 2,250 m. There is no course finish.
- Blue spheres give 10 points. Red spheres mix into regular trails only at pickup distances strictly beyond 1,000 m and give 25 points. Red spheres are collectibles; red blocks remain obstacles.
- Green spheres represent future grab bags/pouches. Single drops first appear at 2,280 m, strictly beyond 2,250 m, then every six sections (576 m). They are placed at the obstacle itself: high over a hurdle at 3.7 m or low beneath a beam at 0.28 m. They have a separate Pouches counter and award no score points. Actual collection difficulty needs gameplay testing.
- Every 960 local metres the player, sections, and following camera shift back together. Distance is accumulated separately from world position, so score and speed should remain continuous.
- Restart clears score, gems, defeat, movement state, speed progression, section schedule, and world offset. The original clear opening is restored, including after a long run.
- Pouches are counted for the current run only. Persistent currency, spending, bag contents, final artwork, inventory and powers remain later work.

## Unity acceptance checks

1. Confirm first movement, swipes, jumping, sliding, gems, and obstacle defeat still work.
2. Play past 990 metres: the run continues and distance does not reset or jump backward. Watch for camera jumps near 960 and 1,920 metres.
3. Check mixed double rows after 150 m, consistent double rows after 300 m, intervening single rows after 600 m, and speed capped at 32 m/s at 2,550 m. Try the clear connecting route and risky jump/slide gem lanes.
4. Inspect the runtime Hierarchy: eight Track section objects remain present during the run. Stop/Play must not duplicate them.
5. Survive for ten minutes, watching for ground gaps, abrupt camera motion, missed pickups, or unavoidable layouts. This is a manual stability check, not an automated performance claim.
6. Restart after multiple section recycles, while paused, after defeat, and after a world shift. Speed returns to 15, all pickup counters and score return to zero, and the clear starting area returns.
7. Check the Console during the run and after stopping Play. Actual memory/frame-time profiling and iPhone testing remain future checks.
8. Collect red gems after 1,000 m: the red counter increments once and pickup score gains 25; blue still gains 10. Look for a green pouch at 2,280 m, then at 576 m intervals. Collect it with the relevant jump/slide: Pouches increments once with no score bonus. Restart and confirm all introductions are delayed again.

## Verification

The user verified earlier endless pacing and stability. For this revision, editor and iOS conditional compilation passed without warnings; this is not a player build. Pure checks passed for 30,000 primary-row slots, intermediate-route clearance, adjacent clear-lane transitions, collectible thresholds and rarity, 10/25-point values and reset, risky-gem geometry, 100,001 speed samples and a 100 km recycling/origin simulation. All 15 gesture checks passed. The new speed, obstacle density and pouch placements need Play mode testing; these checks do not exercise Unity physics, rendered visibility, pickup callbacks or actual memory/performance.
