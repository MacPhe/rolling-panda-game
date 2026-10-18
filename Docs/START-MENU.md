# Forest-clearing start menu

Open `Assets/_RollingPanda/Scenes/StartMenu.unity` and press Play. The real endless track, imported forest and gameplay systems are created once behind the menu and held at zero distance. Roll / Play animates the sleeping panda down from the tree into the course. A 0.25-second fade covers the visual-panda handoff, followed by a 0.75-second reveal/forward roll, then controls unlock. There is no gameplay scene load on Play and no screen-capture dependency. Returning to Main menu reloads StartMenu to prepare a fresh world.

## Art used
- Menu tree: imported `LowPolyTrees/Prefabs/Tree5_autumn.prefab`, normalized to eight metres tall and converted to runtime URP materials that retain its atlas. This is autumn foliage, not a literal pink cherry tree. A small supporting perch branch supplements the vendor mesh for the sleeping pose.
- Surrounding clearing: imported autumn bushes and summer logs; actual run forest continues behind it.
- Gem pouch: no dedicated bag model was found in the imported packs. A new open, pleated sack mesh with gathered mouth, golden drawcord, knot and seam replaces the sphere. It contains blue/red/green Diamondo gems when the local GameArt resource is prepared.
- Bamboo: no dedicated imported bamboo prefab/model was found. Existing stalks now have segmented nodes and individual angular foliage sprays. A purpose-built bamboo clump and authored cloth pouch would be sensible later art upgrades; no purchase or external asset import was made.

Stop Play and let the editor prepare the EnvironmentArt resource. If Tree5 does not appear, use **Rolling Panda > Refresh imported forest art** and check the tree pack is installed at the original path. The builder refreshes older resources missing MenuTree. Raw vendor packs and font binaries remain local dependencies rather than being redistributed publicly.

## Buttons
Roll / Play enters the already-present forest. Settings saves master volume (audible once sounds are added). Leaderboards shows a saved personal best and marks online rankings as future work. Shop is a preview without purchases. Paused/ended runs offer Main menu; Restart/Resume retain their behavior.

## Unity checks
1. Open StartMenu; confirm Tree5, the sleeping perch, sack and actual course behind the menu. Check for pink/missing materials.
2. Wait on the menu: distance/score and pickups must remain unchanged. Test Settings and Back before Play.
3. Repeatedly tap Play: one entrance only. Check the landing, camera move, brief fade and handoff; no second level load should occur.
4. Confirm controls unlock after the reveal at zero distance; play, pause, restart, end and return to Main menu. Repeat the entrance.
5. Compare portrait and landscape framing, especially the tree canopy, branch and off-road sack. Scenery colliders are disabled.

Validation: desktop runtime, iOS conditional code and editor tools compile against Unity 6000.6.5f1. Existing gesture/endless rules regression checks pass. Play Mode visuals, vendor mesh pivot/perch fit, touch input and physical iPhone performance still require testing. The full game forest now resides in memory during the menu; no mobile performance claim is made.


The current shared-character stalk/getaway opening and persistent green-gem cosmetic shop supersede the earlier preview-only flow described above. See [SHOP-AND-GETAWAY.md](SHOP-AND-GETAWAY.md) for current behaviour, catalogue, saves and tests, and [MOBILE-LAUNCH-PLAN.md](MOBILE-LAUNCH-PLAN.md) for device and release readiness.
