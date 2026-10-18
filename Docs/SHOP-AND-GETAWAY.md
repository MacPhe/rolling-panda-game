# Shop and stalk-to-getaway opening

The menu now uses the actual gameplay panda on the tree branch. Its equipment and approved fur/face stay with it through the wake, fall, tucked forward rolls, moving camera and running handoff. The camera follows the panda's centre through the drop and near-miss, then eases into the usual road view. Shrine music fades out as Play starts; twig, quiet murmurs and a missed net swish provide the opening foley. Poachers stalk for approximately 2.6 seconds, reach/throw during the getaway, and continue as the existing cosmetic pursuit. The missed net settles briefly behind the runner. It never changes collision or defeat rules.

The shared-world opening removes the previous full black transition and hidden character swap. A short 0.35-second moving roll handoff retains the existing input hold while the motor advances; control is released after the presentation handoff. The established 17.5�40m/s run pacing remains unchanged. Retry goes straight back to gameplay, without replaying the opening. Animation delta is bounded during the opening to avoid a large resume hitch skipping the whole fall.

## First shop catalogue

All prices are earned green gems, with no payment integration or stat boosts. Starter items and remove/none options are free and permanently owned.

| Slot | Item | Rarity | Green gems |
| --- | --- | --- | --- |
| Coat | Moonlit blue | Uncommon | 15 |
| Coat | Plum blossom / Copper | Rare | 35 each |
| Headband | Blue | Common | 5 |
| Headband | Jade | Uncommon | 15 |
| Headband | Golden | Rare | 35 |
| Belt | White karate belt | Common | 10 |
| Belt | Black karate belt | Rare | 40 |
| Outfit | Blossom kimono | Rare | 100 |
| Outfit | Forest samurai | Epic | 250 |
| Hat | Bamboo straw hat | Uncommon | 25 |

The shop shows the wallet, category, rarity, ownership and price, with Previous/Next browsing, preview, Buy/Equip, turn controls and bamboo/idle pose. Browsing previews one slot while retaining other equipped slots, and does not charge or equip. Closing returns to actual equipped items. One item is equipped per slot; ownership is permanent and equipping an already owned item never charges again. The samurai crest yields to the straw hat when both would occupy the head. Coat variants tint the dark fur/patches while preserving the pale panda markings. These are original simple placeholder clothing meshes and a functional UI, not finished skinned garments or cloth simulation.

## Persistence and safety

New green pickups credit the bank once when collected; gold piles add 50. The existing HUD continues to show the current run's pickups, while the shop shows the persistent balance. Defeat, retry, return to menu, suspension and shutdown flush pending bank changes. Purchases/equipment save immediately. Resetting a run does not credit the run's pickups again. Earlier unrecorded prototype pickups cannot be reconstructed; no currency or ownership is granted to the real player by the tests.

The versioned local profile retains a previous-save backup, validates item IDs/ownership/slots on reload, clamps invalid balances and avoids integer overflow. It is local PlayerPrefs data, not secure real-money currency, cloud sync or uninstall recovery. Stable item IDs should be preserved during later catalogue revisions.

Character tint uses per-renderer property overrides so preview copies do not recolour shared materials on the running panda. Small prebuilt accessories follow the existing head, arm and body pivots and have no active colliders. Cached renderer references tolerate delayed removal of legacy setup geometry; that lifecycle bug was found in the first live test and fixed. No imported vendor models or audio are redistributed.

## Verification

Windows/editor, iOS-defined runtime and editor-tool C# compilation pass against Unity 6000.6.5f1. The new pure ShopChecks cover insufficient funds, duplicate charges, unequip, ownership/slot restore validation, overflow and 100,000 earning/buy/equip actions with repeated save-model round trips. Gesture, endless scheduling, chute, shield, magnet, special pickups and capture/gait rule suites also pass.

Connected Unity tests exercise a temporary isolated profile, purchase totals, repeated equip, persisted wallet/unlocks/equipment, corrupt-primary backup recovery, preview-without-purchase, menu collection rejection, green/gold banking, the complete opening, real small-rock defeat, equipped capture and direct restart. Test saves are restored afterwards. Opening screenshots cover stalking, falling, missed throw, rolling and handoff; shop screenshots cover coat/outfit previews. See the task workspace outputs/shop-opening-test/report.txt for the latest run, errors and camera-framing checks. The temporary harness is removed before publication.

The final connected run completed with zero new runtime errors. It also verified that the equipped headband tint reaches the live menu/gameplay panda and that the body centre stays inside the opening viewport (x approximately 0.448�0.511, y 0.329�0.518 in the tested landscape view). These bounds do not prove complete silhouette visibility or absence of every prop occlusion.

Native mobile builds, touch interaction, final costume clipping/fit and audio balance on phone hardware remain unverified. The iPhone/App Store prerequisites, costs, timing and release criteria are documented in MOBILE-LAUNCH-PLAN.md.

Standalone economy checks: dotnet run --project Tests/ShopChecks/ShopChecks.csproj.
