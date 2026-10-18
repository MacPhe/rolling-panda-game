# Panda placeholder vocals

Three original synthetic cartoon grunts are generated once in RunnerMovementAudio.Awake: a 0.24s hop effort, a 0.24s lower roll effort and a 0.38s falling-pitch surprised oof. These are temporary sound-design placeholders, not recordings of real pandas or a finished voice performance.

One 2D voice source at 0.25 volume (raised lightly from 0.22) with a 2000 Hz low-pass keeps them quiet. Jump and roll vocals follow accepted motor actions with a shared 1.2s cooldown and no overlap. Holding roll or passing through a bamboo chute does not repeat the grunt. Lane changes do not add vocal spam. A genuine run-ending reaction interrupts an action grunt and can finish while the run is ended; shield/chute-protected contacts do not trigger it. Pause, restart, menu handoff and disabling the runner stop/reset vocals. Settings master volume applies. Generated clips are destroyed with the runner. Existing footsteps, swoosh, slide, gem sounds and music retain their current levels.

Unity 6000.6.5f1 Windows/editor, iOS runtime and editor tools compile. Existing chute, gesture, pickup, shield, magnet, special and endless checks pass. Unity audio playback is unavailable here, so listen in-game for tone and balance.

Try accepted jumps and rolls, rapid repeated actions, a held roll in the chute, a genuine defeat and protected obstacle contacts. Pause or restart during a vocal and confirm it stops. Use Settings mute to confirm all audio follows master volume.

## Chase story proposal (not implemented)

Stylised poachers raid the forest shrines for their spirit gems. The panda escapes with a rescued gem pouch and races to keep it out of their hands. The existing sleeping-tree opening could show a distant approach or snapping twig before the fall-and-roll departure. Bamboo chutes become escape shortcuts through routes the pursuers cannot easily follow. Nets and bamboo cages fit the world and supply readable pursuer silhouettes.

Start with a short opening cue and occasional distant pursuit presence; agree the story and pursuer design before adding chase AI or changing failure rules. The panda's distinctive movement and escape routes remain the central player experience. The first pursuit implementation is now described in POACHER-PURSUIT.md, including the opening, two placeholder pursuers and a net/dart capture ending.

