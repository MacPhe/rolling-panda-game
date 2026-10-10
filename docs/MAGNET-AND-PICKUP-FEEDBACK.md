# Pickup feedback and first magnet powerup

The repeated `CreateImpl is not allowed ... constructor (or instance field initializer)` errors were caused by allocating MaterialPropertyBlock in PickupArt's field initializer. It is now allocated in Awake, together with the shader property ID. Compilation alone did not expose the original Unity lifecycle error; confirm the Console stays clear during Play after updating.

## Pickup feedback
Each collected blue/red/green pickup sends a small, matching-colour +1 from its screen position upward and into its top-left HUD icon over 0.7 seconds. The +1 means one collectible, not its score value: blue still gives 10 points, red 25, green one pouch with no score. Counters update immediately. A fixed 32-entry feedback buffer avoids spawning objects and keeps dense magnet collections bounded. Animations freeze on pause/end, clear on restart and follow safe-area HUD targets.

## Magnet
- First spawn is randomly 324–492 m; subsequent gaps are randomly 312–480 m, within the requested 300–500 m interval. Half-row offsets keep the pickup between obstacle rows, in a random lane.
- One reusable pickup uses the locally imported BTM Magnet prefab, converted to URP. A red/white U-shaped fallback is used if the asset reference is missing.
- Collect it to activate 15 seconds of attraction, with a countdown under the HUD counters. Timer pauses with gameplay; another magnet refreshes it to 15 seconds rather than adding time.
- All three gem types in every lane are attracted from up to 24 m ahead and 3 m behind, including the high green pouch. This deliberately makes rare pouches easier to obtain during the powerup.
- Attraction moves pickup objects toward the panda and collects once. Uncollected attracted pickups return to their previous section-relative positions when the effect ends. Pool reuse resets attraction state. Restart removes the effect and schedules a new first spawn; origin rebasing preserves the magnet's absolute distance.
- Magnet collection uses a swept distance check to avoid skipping the pickup between frames. The magnet itself has no physical obstacle collider. No change to obstacle collisions, speed, points or ordinary spawn rules.

Stop Play to allow the imported-art resource to refresh. Use Rolling Panda > Refresh imported forest art if the Magnet prefab is not picked up.

## Validation
Desktop, iOS conditional and editor-tool compilation pass. New MagnetChecks verify 100,000 spawn gaps and obstacle-row exclusions, pause/expiry timing, all-lane/high-pouch attraction range and swept pickup collection. Existing gesture/endless checks pass. These are rules/compilation checks, not a Unity physics or visual test.

Unity checklist: clear old Console entries, run and collect gems; check no new CreateImpl errors; check +1 reaches the correct counter; encounter a magnet after 300 m, miss one then collect another; check countdown, refresh, pause/resume and expiry; restart during attraction; run beyond the 960 m origin reset and collect red/green pickups at their thresholds. Confirm no duplicate counts or stray attracted gems after section recycling. Test portrait/landscape HUD and physical iPhone performance when available.
