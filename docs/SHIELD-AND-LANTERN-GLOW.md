# Shield and brighter lanterns

## Shield
A reusable pickup uses the local BTM Shield prefab, with runtime URP material conversion and a simple blue plate fallback. Stop Play to allow EnvironmentArt to refresh; use Rolling Panda > Refresh imported forest art if needed.

One shield appears in each 1,000 m block, randomly between 250–750 m into the block on a half-row position. This shared powerup scheduler guarantees at least 500 m between consecutive shields and avoids obstacle rows. Shield and magnet positions are chosen independently.

Collecting gives 10 seconds of protection, a translucent blue panda bubble and HUD countdown. Existing CharacterController/obstacle collisions stay enabled: a rock can block forward movement, but contact does not end the run while protected. Lane changes, jump and slide remain available to get around it. Distance only increases with actual forward movement. The timer keeps counting down while blocked; it freezes on pause, resets on restart and refreshes to 10 seconds on another shield. Once expired, the next obstacle contact can end the run normally.

Both collision callbacks and capsule-overlap obstacle checks route through the shield guard. No obstacle hitbox, collider radius or invulnerability-through-walls behavior was added. The bubble has no collision or shadows and follows the low slide pose.

## Lantern glow
Paper emission is now roughly five times stronger and warmer. A shared soft amber halo makes the glow visible without depending on bloom. Two nearby shadowless lantern lights illuminate the surrounding forest; lights select the closest lanterns within 35 m. Halo updates are limited to active lanterns within 90 m in front of the camera. Existing bob/sway remains cosmetic and hitboxes remain fixed. This adds two lights beyond the existing four shrine lights; iPhone profiling remains outstanding.

## Validation and Unity checks
Desktop, iOS conditional and editor-tool compilation pass. Shield rule checks cover duration, pause and expiry while stationary. The shared scheduler passes 100,000 kilometre-block/minimum-spacing/obstacle-row checks; existing magnet/gesture/endless checks pass. These do not simulate Unity CharacterController behavior or verify rendered glow.

In Unity, collect a shield then run into a large rock: confirm the run stays active, forward distance stops and lane changes let you escape. Repeat with small rocks and lanterns; let the shield expire while blocked and check normal defeat returns. Test pause/resume, restart and menu return; check both powerups active together and the countdown layout. Run across origin rebasing and confirm one shield per kilometre with at least 500 m separation. Compare lantern glow in day/dusk/night, confirm rocks remain static, and watch for console errors or excessive glare.
