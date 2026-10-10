# Extended BTM pickups

Five reusable slots use the local BTM SpeedChevBlue, Time, Bomb, GoldPile and Battery prefabs, with runtime URP material copies. The resource builder updates older EnvironmentArt resources with their references. Stop Play and allow import; Rolling Panda > Refresh imported forest art refreshes the references manually. Missing assets fall back to simple shapes. Vendor binaries remain local rather than publicly redistributed.

| Item | Effect | Spawn schedule |
| --- | --- | --- |
| Bright blue SpeedChev | 50 m/s for 7.5 s, blue emission/aura and glints | First 1000–3000 m; later 2500–3000 m apart, reconciling the requested range with the minimum cooldown |
| Time | 10 m/s for 7.5 s; source atlas preserved, cyan glass overlay and dark wood caps | First after 2000 m (up to 6000 m); later 4000–8000 m apart |
| Glossy Bomb | Run-ending contact and a bounded 64-particle explosion; shield does not protect against bombs | First 120–500 m; later 3000–10000 m apart |
| Shiny GoldPile | Adds 50 green pouch units, no score points; +50 flies to green counter | First 15000–16000 m; later 10000–20000 m apart |
| Glossy Battery | Extra-high 6.2 m jumps for 15 s, as selected by the user | Exactly two per 10000 m block, one randomly in 500–4500 m and one in 5500–9500 m |

All positions snap between obstacle rows, with random lanes. Schedules advance on pickup or miss, measured by spawn locations rather than collection time. The system prefers a different lane when two special items share a row. Magnet attraction affects ordinary gems/pouches, not these special pickups. Existing shield/magnet settings remain unchanged.

Speed and Time use a single speed override: the latest pickup replaces the earlier one. Expiry returns to the normal distance-based speed. Battery and speed effects can run together. Timers freeze on pause/end, clear on restart/menu reload and refresh rather than stack on repeat pickup. A jump already launched retains its trajectory if Battery expires in mid-air. Extra height changes launch velocity, not gravity or collision size.

Special pickup detection checks both forward crossing and the player's interpolated height/lane so jumps can clear the low bomb. Bomb calls normal defeat directly, bypassing shield obstacle protection. The explosion plays briefly after defeat and clears on restart. Pickup slots and their glint meshes/materials are created once; no per-spawn objects or lights are allocated. The particle effect has at most 80 particles and no collision.

Validation: desktop, iOS conditional and editor-tool C# compilation pass. New rules checks cover 500000 schedules, all thresholds/cooldowns, obstacle-row exclusion, two batteries per 10km, speed replacement/expiry, bomb vertical range and +50 currency without score changes. Existing shield/magnet/gesture/endless rules checks pass. These do not simulate Unity collisions, particle rendering or physical-device performance.

Unity checklist:
1. Clear the Console and allow art preparation. Check blue SpeedChev glow, Time glass/wood, glossy Bomb/Battery and shiny GoldPile for missing/pink materials.
2. Collect SpeedChev/Time; confirm 50/10 m/s HUD, 7.5-second duration, latest-wins behavior, baseline recovery and pause/restart reset.
3. Collect Battery; check 15 seconds of extra-high jumps and clearance over tall rocks when timed correctly. Check expiry during a jump, landing, front-flip visuals and no enlarged collider.
4. Jump/dodge a Bomb, then intentionally hit one; check explosion and defeat both with and without a shield. Restart and confirm no lingering effect.
5. On longer runs, verify Time only after 2000m and GoldPile only after 15000m, +50 on the green counter and unchanged score; check repeat cooldowns and origin rebasing.
6. Compare simultaneous timer layout in portrait/landscape. Device profiling remains deferred until iPhone build access is available.
