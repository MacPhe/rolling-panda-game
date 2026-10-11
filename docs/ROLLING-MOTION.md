# Rolling Panda motion pass

Down/S/downward swipe now presents a compact forward roll instead of the feet-first lunge. The panda curls its head and limbs into a ball and rotates around its body centre at two revolutions per second. Holding the existing low action continues the roll; releasing returns to running when the existing controller allows it. The animation uses a reduced visual scale and centred pivot to keep the silhouette low. The lantern collision capsule, action duration and stand-up clearance checks are unchanged.

A change of target lane triggers a 0.36-second side-kick gesture in that direction. The leading leg extends sideways, the arms guard, and the torso turns slightly. Actual lateral movement adds a small smoothed lean, with head counter-motion and recovery into the existing run. Reversing direction starts the opposite gesture. Roll, jump and menu entry poses take priority over kicks. Jump front flips remain intact; kicks provide no attack or obstacle protection.

The prior animation is retained as LateUpdateLegacy. During Play Mode select Menu and forest run in the Hierarchy, find its Visual Prototype component, and turn off Rolling Motion to compare the previous lunge and running motion. It defaults on for the new version. This keeps the work in the same playable world without duplicating gameplay scenes. The previous published version is also retained in branch history.

## Validation

Unity 6000.6.5f1 runtime/editor, iOS runtime and editor tools compile. Existing input, pickups, shields, magnets, special spawns and endless-runner checks pass. This verifies compilation and existing logic, not the rendered animation: Unity Play Mode is unavailable here.

## Check in Unity

1. StartMenu > Roll/Play: confirm the tree animation and entry flip still work.
2. Press Down/S or swipe down under a lantern. Check the tuck, roll, ground clearance and return to running.
3. Hold Down longer, then release beneath a lantern. The existing low capsule should remain until standing is safe.
4. Move left and right, reverse direction, and try moving at the outer lane boundary. Kicks should follow real changes of target lane, not repeat when a boundary blocks a new lane command.
5. Change lanes during jumping and rolling. Those main actions should remain readable without a competing kick pose.
6. Pause/resume and restart during each action. Check full recovery, limbs, headband and no ground clipping.
7. Try at starting and maximum speed. Side kicks are cosmetic; the runner remains vulnerable to obstacles normally.
8. Compare with Rolling Motion off if the new poses need refinement. Audio levels and music are unchanged.
