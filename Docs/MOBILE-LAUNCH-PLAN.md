# Mobile launch costs and readiness � 18 October 2026

These are planning estimates, not a promised release date. Store fees are quoted in USD because that is how the official pages state them. UK checkout pricing, currency conversion and applicable tax can differ. No subscriptions, cloud builds, developer enrolments or purchases were started.

## Verified platform costs

| Item | Published cost | Practical effect |
| --- | --- | --- |
| Apple Developer Program | US$99 per membership year; local currency where available | Required for the proposed TestFlight/App Store route. The Enterprise programme is not needed. |
| TestFlight | Included with developer membership | Internal testing for App Store Connect team members; first external beta build requires review. |
| Unity Personal | Free while eligible under Unity's revenue/funding rules | No engine upgrade is currently justified for this prototype. |
| Unity Build Automation standard Mac | 100 included Mac minutes monthly; US$0.07 per additional standard Mac minute | A cloud Mac can build/sign iOS without buying a Mac. Verify the account allowance, supported Unity/Xcode version and storage/egress usage before enabling billing. |
| Google Play registration, if Android is included | US$25 one-time | Separate from Apple. Android APK testing outside Play is possible before store registration. |
| Existing assets/original prototype cosmetics | US$0 additional planned | Private build inputs must include imported dependencies; do not publish vendor packs in the public source repository. |
| Test devices | US$0 if existing/borrowed devices suffice | iPhone 16 Pro Max plus an older supported iPhone and representative Android devices should be tested. Buying devices or paying testers is not included. |

Example lean first-year budget, excluding labour, marketing, new devices, tax/FX and any paid services added later: Apple-only US$99�129, or both stores US$124�154. These ranges reserve US$0�30 for initial cloud build overage; they are a budget allowance, not a service quote or spending cap. At the published standard rate, 500 total Mac minutes in one month would mean 400 billable minutes, approximately US$28 for compute. Storage/transfer overages can be additional. Keep build usage monitored.

The shop currently spends earned green gems only. It has no real-money purchases, payment SDK, advertising or online inventory service. Adding these later changes both implementation and release work.

## Current access and build barriers

The connected Windows editor currently has Windows and WebGL build support only. Android and iOS build support are not installed locally. C# compilation with iOS defines is useful validation, but it is not a native iPhone build and does not test Metal shaders, IL2CPP, touch, thermal behaviour or memory on the phone.

The iOS route needs an Apple developer membership, app identity, signing certificate/profile, App Store Connect record and an approved macOS build environment. A Unity cloud Mac is viable in principle, but the account entitlement, Unity 6000.6.5f1 availability and compatible Xcode must be confirmed. The public code repository omits vendor asset binaries; build automation needs a licensed private build checkout or private dependency delivery to reproduce the current local game. Credentials belong in the provider's signing/secret controls, never in chat or Git.

Android needs the Unity Android module/SDK/NDK, package identity, a release signing key and at least one physical Android device. The current iPhone is not an Android testing substitute.

## Conditional timeline

| Milestone | Working estimate | Exit evidence |
| --- | --- | --- |
| Reproducible private mobile build and signing setup | 3�7 working days once accounts, signing and build inputs are available | Signed iPhone TestFlight build and installable Android build; first-launch smoke test |
| Small device beta | About 1�2 weeks after a working build pipeline | iPhone 16 Pro Max plus older iPhone/Android test sessions; touch/safe areas, interruption/resume, settings, persistent wallet and clothing validated |
| Performance, stability and game-flow tuning | Another 2�4 weeks, partly overlapping beta | Stable chosen frame target on minimum supported devices; 20�30-minute thermal sessions, repeated restarts, no critical crashes, measured earn/unlock balance |
| Store preparation and release candidate | About 1�2 weeks | Final icons/screenshots, asset licence audit, accurate privacy/data declarations/manifests, age ratings, support/privacy URLs and complete user-facing screens; no unfinished release-blocking placeholder features |
| Review and controlled rollout | Allow additional days to 1�2 weeks for review/rework | Store acceptance and monitored limited release, not only successful editor playback |

With scope held steady and prerequisites resolved promptly, a controlled soft launch in roughly 6�10 weeks is a reasonable planning target: 29 November�27 December 2026. A wider release should follow device/retention feedback rather than a fixed calendar promise. Additional game systems, real-money purchases, online leaderboards, substantial final art work or signing/account delays can extend this.

For Google Play personal accounts created after 13 November 2023, production access requires a closed test with at least 12 continuously opted-in testers for 14 days, followed by an application for production access. That is not automatic approval after day 14. Start recruiting testers early; internal Android testing can begin sooner. Apple also reviews external TestFlight builds; review times and acceptance are not guaranteed.

## Unbiased priorities

1. A real device build is now more valuable than more decorative systems. The previous editor soak cannot establish mobile GPU or thermal performance, particularly with fur and dense scenery.
2. Persistent green currency, safe purchases, preview-before-buy and fast direct restarts are higher-value flow improvements than adding more powerups right now. These are implemented in the current shop/opening pass.
3. Shop prices are provisional. Green drops begin late in a run and are deliberately scarce, so new players may take too long to unlock their first cosmetic. Track runs-to-first-unlock before raising prices or widening the catalogue. No drop threshold or free currency grant was silently changed.
4. Keep the approved percussion and soft shrine music for now. The opening fades shrine music out for twig/voice/net foley, then gameplay percussion resumes. There is no listening evidence justifying replacement of an approved track. Judge jungle layering on actual phone speakers/headphones before changing it.
5. Local PlayerPrefs saves include a previous-save backup but are not cloud sync, uninstall recovery, encryption or a secure paid-currency ledger. Keep this shop earned-currency-only until its persistence requirements are revisited.

## Official sources checked

- [Apple membership enrolment and fees](https://developer.apple.com/help/account/membership/program-enrollment/)
- [Apple TestFlight](https://developer.apple.com/testflight/)
- [Apple internal testers](https://developer.apple.com/help/app-store-connect/test-a-beta-version/add-internal-testers)
- [Apple review status](https://developer.apple.com/help/app-review/after-submitting-for-review/review-status)
- [Unity plans and current build compute pricing](https://unity.com/products)
- [Unity iOS build configuration](https://docs.unity.com/en-us/build-automation/basic-build-configuration/set-up-an-ios-build-configuration)
- [Google Play registration fee](https://support.google.com/googleplay/android-developer/answer/6112435)
- [Google Play new personal-account testing requirements](https://support.google.com/googleplay/android-developer/answer/14151465)
