# Elemental Clash — Unity Project Architecture

Version 0.1 (draft for approval) · 13 Nov 2026
Status: **Plan only. The Unity project has not been created yet.**

## 1. Engine and packages

| Item | Choice | Why |
|---|---|---|
| Unity | **Unity 6 LTS, 6000.0.67f1** (already on your PC, see doc 06 for an Android module check) | Current LTS, URP Render Graph, best Android tooling |
| Fallback | 2022.3.62f3 LTS (also installed, Android module present) | If Unity 6 Android module is broken |
| Template | "Universal 3D" (URP) | |
| Render pipeline | URP with a **Mobile** renderer asset + separate **PC** renderer asset for editor testing | |
| Input | Input System 1.x (`com.unity.inputsystem`) incl. On-Screen Stick/Button | Same actions for touch, keyboard, gamepad |
| Camera | Cinemachine 3.x (`com.unity.cinemachine`) | Target group framing, impulse shake, cinematic ultimates |
| UI | uGUI + TextMeshPro for HUD (fast, battle-tested on mobile); UI Toolkit optional for menus later | |
| Animation | Mecanim Animator with Humanoid rigs; animation events only for VFX/SFX, **never** for hit timing | |
| Pooling | `UnityEngine.Pool.ObjectPool<T>` (built-in) | No plugins |
| Tests | Unity Test Framework (EditMode + PlayMode) | |
| Version control | Git + Git LFS (recommended, see doc 06) | |
| Paid plugins | None | |

## 2. Architecture principles
1. **Data-driven combat.** Fighters, moves, AI, arenas, and match rules are ScriptableObjects. Balancing = editing assets, not code.
2. **Code owns timing, Animator owns visuals.** The combat state machine advances move phases (startup/active/recovery) on a fixed tick. The Animator plays clips in sync, but hitboxes are driven by move data. This keeps hits reliable at 30 FPS on slow phones.
3. **One command interface for player and AI.** `IFighterCommandSource` is implemented by `PlayerInputSource` (touch/keyboard) and `AIBrain`. Fighters never know who controls them.
4. **Events over references.** Systems talk through C# events / a small `GameEvents` hub (OnHit, OnKO, OnRoundEnd…). HUD, audio, VFX, and camera listen; combat never references UI.
5. **Small assemblies.** Assembly Definitions per module for fast compiles and enforced dependencies.
6. **Mobile budget from day one.** Pools, no per-frame allocations in combat code, no `Find`/`GetComponent` in Update.

## 3. Folder structure

```
ElementalClash/                         (Unity project root)
├─ Assets/
│  └─ _Project/                         (everything we own lives here; third-party stays outside)
│     ├─ Art/
│     │  ├─ Characters/<FighterName>/   Models, Textures, Materials
│     │  ├─ Arenas/<ArenaName>/
│     │  ├─ Shared/                     Toon shader, ramp textures, common materials
│     │  └─ UI/                         Portraits, icons, button sprites, fonts
│     ├─ Animations/
│     │  ├─ Shared/                     Locomotion, hit reactions, getup
│     │  ├─ <FighterName>/              Attacks, specials, ultimate, victory, defeat
│     │  └─ Controllers/                Base AnimatorController + per-fighter Override Controllers
│     ├─ Audio/                         Music/, SFX/<Element>/, Voice/, Mixers/
│     ├─ VFX/                           Shared/, <Element>/ (particle prefabs, textures, shaders)
│     ├─ Prefabs/
│     │  ├─ Fighters/                   Fighter_Zyron.prefab, Fighter_Noxar.prefab …
│     │  ├─ Projectiles/
│     │  ├─ Arenas/
│     │  ├─ Systems/                    MatchManager, CameraRig, PoolManager, HUD
│     │  └─ UI/
│     ├─ Scenes/
│     │  ├─ Boot.unity                  Loads services, then MainMenu
│     │  ├─ MainMenu.unity
│     │  ├─ CharacterSelect.unity
│     │  ├─ Battle.unity                Match systems + HUD (arena loaded additively)
│     │  ├─ Arenas/Arena_SkySanctuary.unity, Arena_MoltenCrater.unity …
│     │  ├─ Training.unity
│     │  └─ Dev/Sandbox_Combat.unity    Prototype test scene
│     ├─ ScriptableObjects/
│     │  ├─ Fighters/                   FD_Zyron.asset …
│     │  ├─ Moves/<FighterName>/        MV_Zyron_Light1.asset …
│     │  ├─ Elements/                   EL_Lightning.asset … + AffinityTable.asset
│     │  ├─ AI/                         AI_Easy.asset, AI_Normal.asset, AI_Hard.asset, Boss phases
│     │  ├─ Arenas/
│     │  ├─ Match/                      MatchRules_Single.asset, _Bo3, _Bo5
│     │  └─ Story/                      Chapters, dialogue
│     ├─ Scripts/                       (see §4)
│     ├─ Settings/                      URP assets, Input Actions, Quality presets
│     ├─ Tests/
│     │  ├─ EditMode/
│     │  └─ PlayMode/
│     └─ Documentation/                 Copies of these design docs
├─ Packages/
└─ ProjectSettings/
```

## 4. Code modules (Assembly Definitions and namespaces)

| Assembly | Namespace | Depends on | Contents |
|---|---|---|---|
| `EC.Core` | `ElementalClash.Core` | — | GameEvents, ServiceLocator (tiny), SceneLoader, TimeController (hit-stop, slow-mo), Pools, utilities |
| `EC.Data` | `ElementalClash.Data` | Core | All ScriptableObject definitions |
| `EC.Combat` | `ElementalClash.Combat` | Core, Data | Fighter runtime, state machine, hitboxes, damage, resources |
| `EC.Input` | `ElementalClash.Input` | Core, Combat | Input actions wrapper, PlayerInputSource, input buffer |
| `EC.AI` | `ElementalClash.AI` | Core, Data, Combat | AIBrain, perception, utility scoring, boss phases |
| `EC.Camera` | `ElementalClash.CameraSystem` | Core, Combat | Combat camera, swipe orbit, shake, cinematic hooks |
| `EC.Match` | `ElementalClash.Match` | Core, Data, Combat | MatchManager, round flow, timer, results |
| `EC.UI` | `ElementalClash.UI` | Core, Data, Combat, Match | HUD, menus, character select, settings, layout editor |
| `EC.Save` | `ElementalClash.Save` | Core, Data | SaveSystem, SettingsData, UnlockData |
| `EC.Presentation` | `ElementalClash.Presentation` | Core, Combat | VFX/SFX routers, animation driver |
| `EC.Tests.EditMode` / `.PlayMode` | — | All | Tests |

### 4.1 Key classes

**Data (ScriptableObjects)**
- `FighterDefinition` — name, power, class, stats (HP, speed, guard stamina, EP regen), prefab, portrait, AnimatorOverrideController, move set references, unlock rule.
- `MoveDefinition` — frame data, damage, hit properties, hitbox windows, costs, cancel rules, VFX/SFX/camera cues (see GDD §3.3).
- `HitboxWindow` (serializable struct) — start/end frame, shape, bone/offset, radius/size.
- `ProjectileDefinition` — speed, lifetime, hit data, pool size, prefab.
- `ElementDefinition` — color palette, icon, hit VFX family; `AffinityTable` — favored pairs and modifiers.
- `AIProfile` — reaction time range, probabilities, combo limits, preferred range.
- `MatchRules` — rounds to win, timer, affinity toggle, meter carry-over.
- `ArenaDefinition` — scene reference, spawn points, thumbnail, hazard prefab.

**Combat runtime**
- `Fighter` (MonoBehaviour, root of fighter prefab) — composes everything below, exposes read-only state.
- `FighterMotor` — CharacterController-based movement, gravity, dash, knockback, facing.
- `FighterStateMachine` — states: `Idle`, `Move`, `Attack`, `Special`, `Ultimate`, `Guard`, `Dodge`, `Hitstun`, `Blockstun`, `Knockdown`, `Getup`, `Stunned`, `Victory`, `Defeat`. Each state is a plain C# class with Enter/Tick/Exit.
- `MoveExecutor` — runs the active `MoveDefinition` frame by frame, opens/closes hitboxes, handles cancels.
- `Hitbox` / `Hurtbox` — physics overlap queries (`Physics.OverlapCapsuleNonAlloc`) on dedicated layers; each attack instance hits a target once.
- `DamageResolver` (pure C#, unit-tested) — damage, chip, counter/punish bonus, combo scaling, affinity.
- `FighterResources` — HP, EP, ultimate meter, guard stamina, cooldown timers.
- `ComboTracker` — hit count, scaling, counter display.
- `Projectile` + `ProjectilePool`.

**Input**
- `ControlsActions.inputactions` — Move, Look, Light, Heavy, Special1, Special2, Guard, Dodge, Ultimate, Pause.
- `PlayerInputSource : IFighterCommandSource` — reads actions, writes into `InputBuffer` (buffers presses for 0.15 s so early taps still come out).
- `TouchControlsLayout` — positions and scales on-screen controls from saved layout.

**AI**
- `AIBrain : IFighterCommandSource` — tick at 10–20 Hz, chooses actions with utility scores.
- `AIPerception` — delayed snapshot ring buffer of opponent state.
- `BossController` — phase thresholds → swaps `AIProfile`, enables moves, fires events.

**Match / flow**
- `MatchManager` — spawns fighters from selection, runs round states (`Intro`, `Fighting`, `RoundEnd`, `MatchEnd`), timer, pause.
- `MatchSetup` (static or SO) — carries selections between scenes.
- `SceneLoader` — async loads Battle + arena additively, with loading screen.

**Presentation**
- `FighterAnimationDriver` — maps state machine + move to Animator states (CrossFade by hash).
- `CombatFeedback` — hit-stop, flashes, camera impulse, hit sparks, damage numbers, SFX routing.

## 5. Runtime flow (a single hit)

```
Touch "L" → PlayerInputSource → InputBuffer → FighterStateMachine (Idle → Attack)
 → MoveExecutor ticks frames → frame 5: Hitbox opens → overlap finds opponent Hurtbox
 → DamageResolver computes result → target FighterStateMachine → Hitstun
 → GameEvents.OnHit → CombatFeedback (hit-stop, VFX, SFX, shake) + HUD (HP bar) + ComboTracker
```

Combat logic runs in `FixedUpdate` at **60 Hz** (Time.fixedDeltaTime = 1/60) independent of render FPS, so 30 FPS devices still get correct frame data.

## 6. Physics layers

| Layer | Collides with |
|---|---|
| `Fighter` (CharacterController) | Environment, ArenaBounds |
| `Hurtbox` | queried by Hitbox overlaps only |
| `Hitbox` | none (query-only) |
| `Projectile` | Hurtbox (query), Environment |
| `Environment`, `ArenaBounds` | Fighter, Projectile |

## 7. Animator setup
- One **Base Fighter Animator Controller** with shared state layout: Locomotion blend tree (speed, direction), Guard, Dodge (4-dir), Hit reactions (light/heavy/front/back), Knockdown/Getup, Victory, Defeat, and generic slots `Attack_L1..L3`, `Heavy`, `Finisher`, `Special1`, `Special2`, `Defense`, `Ultimate`.
- Each fighter gets an **AnimatorOverrideController** that swaps the clips. No per-fighter controllers to maintain.
- Root motion OFF; movement comes from `FighterMotor` and move data (lunge distance curves).

## 8. Rendering and Android settings

| Setting | Value |
|---|---|
| Color space | Linear |
| Graphics API | Vulkan + OpenGLES3 fallback |
| Scripting backend | IL2CPP, ARM64 only |
| Min API level | Android 8.0 (API 26); Target: latest installed |
| Orientation | Landscape Left + Right |
| Texture compression | ASTC (6×6 default, 4×4 for UI) |
| URP mobile | HDR off on Low, MSAA 2× (Med/High), render scale 0.75/0.85/1.0, main light shadows 1 cascade, soft shadows off on Low, no additional light shadows |
| Post FX | Bloom (High only), color grading (all), no SSAO/DoF |
| Lighting | Baked lightmaps + light probes per arena, one realtime directional |
| Shader | Custom lightweight toon shader (Shader Graph, 2-band ramp + rim) |
| Target FPS | `Application.targetFrameRate` = 30 or 60 from settings |

### 8.1 Budgets (per scene, to be verified with the Profiler on device)
- Characters: 15–20k tris each, 1 material (2 max), 1024² atlas (2048² on High).
- Arena: under 80k tris visible, under 120 draw calls total frame (SRP Batcher on).
- Particles: under 300 live particles per effect burst, no overdraw-heavy full-screen effects on Low.
- Memory: under 600 MB total on device.
- GC: 0 B allocated per frame during combat.

## 9. Save system
- `SaveData` (serializable): version, unlocks, win counts, story progress, settings, control layout.
- JSON via `JsonUtility`, write to temp then rename (crash-safe), keep one backup.
- Versioned migration function for future changes.

## 10. Testing strategy
- **EditMode:** DamageResolver (counter, punish, scaling, affinity), FighterResources (EP regen, cooldowns), AffinityTable symmetry (every power has exactly one favored and one unfavored matchup), MatchRules round logic, SaveData round-trip.
- **PlayMode:** spawn two fighters, simulate inputs, assert a light hit lands on the expected frame; KO ends round; rematch resets state.
- **Manual device test sheet:** per milestone (doc 05 §8).
- I'll only report tests as passing after they actually run in your Unity editor (or a CI runner if we set one up).

## 11. Coding standards
- C# 9, `namespace ElementalClash.<Module>`, PascalCase public, `_camelCase` private fields, `[SerializeField] private` over public fields.
- No `GameObject.Find`, no `SendMessage`, no LINQ in per-frame code.
- Every public class has a one-line summary comment; every script header names its folder location.
