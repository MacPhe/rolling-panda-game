# Bamboo rolling chute

A pooled 28m bamboo cage appears in one lane from approximately 952m, then every 1152m (section 10 and every twelfth section thereafter). Its tall entrance frame is labelled ROLL. The surrounding ribs are open so the character can remain visible. Other lanes remain optional bypass routes.

Enter the front while rolling and reasonably centred in its lane. Upright or side entry ends the run with a roll instruction. On accepted entry the runner centres in the chute lane, ignores lane changes/jump commands, and maintains the low rolling capsule for the full tunnel. Releasing the roll input cannot make the panda stand inside. A small protected indicator appears above the bottom buttons. Protection suppresses obstacle and bomb defeat until the whole capsule clears the exit; it does not consume or extend a shield timer. Bomb explosions are suppressed on protected contact. At exit lane control returns and normal vulnerability resumes; the ordinary roll timer/stand-up clearance can still keep the panda low briefly.

Entry is tested with a swept X/Z interval rather than relying on trigger callbacks, preventing missed entrances at high speed. The active chute is retained by reference through world rebasing, so protection follows its moving exit correctly. Restart clears chute state before resetting the track pool. Pause freezes movement and retains the tunnel state. Menu return destroys the existing world normally.

Each track section owns one reusable cage mesh/material. No tunnels are created during a run. A chute section clears the intermediate obstacle at 48m and both main obstacles at 72m, reserving the tunnel and exit space. Five existing pooled gems are moved into the chute at rolling height; scores and pickup types retain their existing rules. Obstacles before the entrance and after the exit continue normally. This is a first procedural bamboo visual that can be refined after gameplay testing.

## Validation

Unity 6000.6.5f1 editor/runtime, iOS runtime and editor tools compile. New standalone ChuteChecks cover front/side/off-centre entries, whole-tunnel high-speed sweeps, trailing-capsule exit boundaries, 100,000 scheduling slots and 300,000 moving/rebase sweeps. Existing pickup, shield, magnet, specials, input and endless-runner checks pass. These verify the rules and compilation; Unity Play Mode rendering/collision integration remains a manual check here.

## Unity test

1. Run to about 952m. Look for the ROLL entrance; roll into it and release the button immediately.
2. The panda should continue rolling all the way through and collect the gem trail. Try jump and lane changes inside: neither should break out of the chute.
3. Check normal lane control and vulnerability after the exit. Repeat at higher speed and with a speed/time powerup.
4. Enter upright on another run, and try cutting into a chute through its side: these should end the run. Other lanes should safely bypass the cage.
5. Pause/resume, restart inside and return to menu. No forced roll or protection should leak into the new run.
6. Check the first chute across the 960m world rebase, as this intentionally exercises rebasing while protected.
7. Check a shield expiring inside and a bomb contact if one happens to overlap the tunnel. Tunnel protection should persist until exit; bombs elsewhere retain normal behaviour.

Standalone rules test: dotnet run --project Tests/ChuteChecks/ChuteChecks.csproj.
