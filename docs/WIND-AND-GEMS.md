# Wind and gem presentation

Imported summer/autumn trees and bushes now sway gently around their grounded presentation pivots. Tree lean is under one degree, with different phases and a slow gust envelope. Menu Tree5, bushes and segmented bamboo also move. Logs, stumps, obstacles, track roots and colliders remain fixed. This is a lightweight whole-prop breeze, not individual leaf deformation. Wind continues in the menu; run pause/end holds it. Inactive seasonal groups do not update, and props outside -25..125 metres of the runner skip transform updates.

VisualPrototype and the in-world menu run now use polished blue/red/green pickup materials, 110-degree/second spin, subtle bob and occasional four-point glints. The original pickup transform/collider is untouched; only a visual child moves. Source materials are checked on pool reuse so pickup colours remain correct. Diamondo is used when installed, with the original mesh as fallback. Menu pouch display gems remain static props.

One shared glint mesh/material and one polished copy per source pickup material serve the pool. No sparkle particles, per-gem lights, new shadow lights, bloom dependency or per-frame material creation. Glints use per-renderer colour blocks and only run within 85 metres in front of the camera. Animation freezes with gameplay pause/end and is held during the entrance; menu wind remains active.

Unity checks:
1. Watch trees and bushes in both seasons; roots should stay planted, with no lane obstruction. Compare menu Tree5 and bamboo.
2. Check blue, red and green pickups: correct colour, steady spin, small bob, short glints visible in daylight and night.
3. Pause/resume, restart, collect gems and run through section recycling. Pickup counts and collision timing must remain unchanged; distant glints should not linger.
4. Check the entrance and return-to-menu flow, then profile on the iPhone when device builds become available.

Desktop runtime, iOS conditional and editor-tool assembly compilation pass. Existing 15 gesture checks, 30,000 row slots, 100,001 speed samples and 100 km pool/rebase rules simulation pass. Actual rendered wind/glint appearance, transparency and mobile performance remain unverified here.
