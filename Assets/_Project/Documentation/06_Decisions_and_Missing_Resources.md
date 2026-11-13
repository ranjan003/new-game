# Elemental Clash — Essential Decisions and Missing Resources

Version 0.1 · 13 Nov 2026

## 1. What I actually checked (13 Nov 2026)

**My cloud environment (where I write files):** Linux container. **No Unity, no Android SDK, no Blender.** I can write C# scripts, docs, and config files here, but I **cannot compile, run, or build** the Unity project. Any "it compiles / it works" claim has to come from your Unity editor or a phone.

**Your Windows PC ("revenge"), names-only folder check, no file contents read:**

| Found | Notes |
|---|---|
| Unity Hub | `C:\Program Files\Unity Hub` |
| Unity **6000.0.67f1** (Unity 6 LTS) | Its `PlaybackEngines` folder only shows Windows support. An `AndroidPlayer` folder sits in a separate folder named `6000.0.67f1-x86_64`, which looks like the Android module was installed to the wrong place. **Needs checking (Q1).** |
| Unity 2022.3.62f2 and 2022.3.62f3 (LTS) | Both have `AndroidPlayer` (Android Build Support). Also a standalone `Unity 2022.3.62f1` in Program Files. |
| Blender 4.3 | Usable for placeholder edits and retargeting |
| Android Studio projects, `Android` SDK folder, `.android`, `.gradle` | Android SDK likely available; Unity normally uses its own bundled SDK/NDK/JDK |
| Microsoft Visual Studio | C# IDE for Unity |
| Git, GitHub CLI | Version control is possible |

I could not verify whether the Android **SDK & NDK Tools** and **OpenJDK** sub-modules are installed for any Unity version.

## 2. Decisions — answered 13 Nov 2026

| # | Answer |
|---|---|
| Q1 | Unity **6000.0.67f1**, Android Build Support confirmed installed |
| Q2 | Test phone: **iQOO Z7s** |
| Q3 | Placeholders now, final art decided later (before M5) |
| Q4 | Project folder: `C:\data\game` (repo cloned to `C:\data\game\new-game`) |
| Q5 | GitHub repo: `ranjan003/new-game` |
| Q6 | Not decided; using placeholder `com.arenalegends.elementalclash` (changeable until first Play Store upload) |
| Q7 | Mixamo OK for placeholder animations |

### Original questions

| # | Question | My recommendation |
|---|---|---|
| **Q1** | Which Unity version? Please open Unity Hub → Installs → 6000.0.67f1 → "Add modules" and check that **Android Build Support** (with SDK & NDK + OpenJDK) is ticked. | Use **6000.0.67f1** if Android is listed as installed. If not, add it there (free). Use 2022.3.62f3 only if Unity 6 gives trouble. |
| **Q2** | Which Android phone will be the test device? (model, or RAM + chipset) | Any phone from roughly 2020+ with 4 GB+ RAM. This sets the 30 FPS target. |
| **Q3** | How will final character art be made? (a) placeholders now, decide later; (b) you or an artist model in Blender; (c) commission an artist (cost, needs your approval); (d) AI-assisted 3D generation tools with clear commercial licenses | **(a)** now. Decide before M5. The prototype doesn't need final art. |
| **Q4** | Where should the Unity project live on your PC? | e.g. `C:\Users\<you>\Documents\UnityProjects\ElementalClash`. If you grant me that folder, I can write scripts straight into it. |
| **Q5** | Version control: do you want a GitHub repo for the project? | Yes, private repo + Git LFS. You create it on GitHub (I can't create repos), then I can work in it. |
| **Q6** | Android package name | `com.<yourstudio>.elementalclash` — you choose the studio part |
| **Q7** | Mixamo for placeholder animations? Needs a free Adobe account (yours). | Yes, it's the fastest source of humanoid animations. Quaternius CC0 as backup. |

## 3. Decisions that can wait (not blocking M1/M2)

| # | Question | When needed | Default if you don't decide |
|---|---|---|---|
| Q8 | Monetization (none / premium / ads / IAP) | Before M8 | None |
| Q9 | Store publishing (Google Play account, $25 one-time fee, your account) | M8 | Not published; APK sideload only |
| Q10 | Music and SFX source (original commission, CC0, self-made) | M4 | CC0 placeholders |
| Q11 | Languages beyond English | M6 | English only, strings in a table |
| Q12 | Accept the elemental affinity ring in GDD §3.6? | M3 | Yes, as drafted |
| Q13 | Accept villain unlock rules (Story **or** win milestones)? | M6 | Yes, as drafted |
| Q14 | Ultimate cinematics: short camera cuts vs fully choreographed paired animations | M3 | Short camera cuts (cheaper) |
| Q15 | Target age rating (affects violence/VFX tone) | M7 | Everyone 10+ style, no blood |

## 4. Missing resources

| Resource | Needed by | Who | Status |
|---|---|---|---|
| Unity 6 with Android Build Support verified | M1 | You (Unity Hub) | Unverified |
| Test Android phone with USB debugging enabled | M1 Checkpoint A | You | Unknown |
| Project folder on your PC (and optional folder access for me) | M1 | You | Not created |
| Placeholder humanoid models and animations | M2 Batch C | You download (Mixamo/Quaternius), I give exact list and import steps | Not started |
| Free Adobe account for Mixamo (optional) | M2 Batch C | You | Unknown |
| Private GitHub repo (optional, recommended) | M1 | You create, then connect it | None |
| Final original 3D character models (11) | M5–M8 | TBD (Q3) | **Biggest open gap** |
| Final animations (~38 per fighter, ~25 unique) | M5–M8 | TBD (Q3) | **Biggest open gap** |
| Music and SFX | M4+ | TBD (Q10) | Not started |
| Playtesters (3–5 people) | M2, M4 | You | Unknown |
| Google Play developer account | M8, only if publishing | You (needs your approval and payment) | Not needed yet |
| Android signing keystore | M8 | You create and store safely; never share it | Not needed yet |

## 5. What I will **not** do without your explicit approval
- Buy or download paid assets, or sign up for accounts.
- Publish anything to Google Play or any store.
- Create or change external accounts, repos, or services.
- Claim anything compiles, runs, or hits a frame rate without your test results.
