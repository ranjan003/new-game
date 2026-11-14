# Elemental Clash — First Playable Prototype: Zyron vs Noxar (M1 + M2)

Version 0.1 (draft for approval) · 13 Nov 2026
Status: **Plan only. No scripts written yet; they come after your approval.**

## 1. Goal
Prove the core loop is **fun and responsive on a real Android phone** before building anything else:
> Walk up, land a light combo, cancel into a special, take a hit, finish the opponent, see a winner, press Rematch.

## 2. Scope

### In
- 1 greybox arena (Sky Sanctuary layout, 24 m ring, boundary).
- 2 fighters: **Zyron** (player default) and **Noxar** (AI default). A dev toggle swaps sides so you can play Noxar too.
- Third-person combat camera framing both fighters, swipe to orbit.
- Touch controls: floating joystick, Light, Heavy*, Special 1, Dodge*, Guard* buttons. Keyboard/gamepad in editor.
- Light 3-hit chain per fighter; Special 1 per fighter (Volt Dash, Shadow Step).
- HP, damage, hitstun, knockback, hit-stop, KO.
- EP bar with Special 1 cost and cooldown display.
- Basic AI (one "Normal" profile, reaction delay, approach/attack/retreat).
- Single round; victory and defeat animations; result panel with **Rematch** and **Quit**.
- Placeholder models and animations, simple hit spark VFX, 2 hit sounds.

\* Heavy, Dodge, Guard are **stretch goals** for M2. They are planned in M3 but the architecture supports them from day one. If M2 runs smoothly, I'll add simple versions so the prototype has a defensive option.

### Out (deferred to M3+)
Special 2, defensive abilities, ultimates, rounds/timer, AI difficulty selection, menus, other fighters, other arenas, saves, story.

## 3. Prototype frame data (starting values @60 Hz, tuned in play)

### Zyron (HP 90, walk 4.0 m/s, run 7.0 m/s)

| Move | Startup | Active | Recovery | Damage | Hitstun | Knockback | Range | Notes |
|---|---|---|---|---|---|---|---|---|
| L1 Jab | 5f | 3f | 10f | 4 | 14f | 0.3 m | 1.4 m | Cancel to L2 from frame 6 |
| L2 Cross | 6f | 3f | 12f | 5 | 16f | 0.4 m | 1.5 m | Cancel to L3 or Volt Dash |
| L3 Spin kick | 9f | 4f | 20f | 8 | 24f | 2.0 m | 1.7 m | Knockdown on counter hit |
| Volt Dash (S1) | 8f | 10f travel | 18f | 12 | 28f | 2.5 m | 7 m dash | 20 EP, 4 s cooldown, passes through target, punishable on guard (-12f) |

### Noxar (HP 105, walk 3.6 m/s, run 6.4 m/s)

| Move | Startup | Active | Recovery | Damage | Hitstun | Knockback | Range | Notes |
|---|---|---|---|---|---|---|---|---|
| L1 Claw | 7f | 3f | 11f | 5 | 15f | 0.3 m | 1.6 m | Slightly longer reach than Zyron |
| L2 Backhand | 8f | 4f | 13f | 6 | 17f | 0.5 m | 1.7 m | Cancel to L3 or Shadow Step |
| L3 Rising slash | 11f | 4f | 22f | 9 | 26f | 1.8 m (launch) | 1.8 m | |
| Shadow Step (S1) | 14f (sink, invulnerable frames 4–14) | 4f | 16f | 10 | 26f | 1.5 m | Teleport to opponent's side (stick picks side) | 20 EP, 4 s cooldown. Visible sink VFX = telegraph |

Global: hit-stop light 4f / special 7f; input buffer 9f; EP regen 6/s after 1 s; +3 EP per light hit landed.

Design intent: Zyron is faster and wins scrambles; Noxar has more HP and reach, and Shadow Step beats Zyron's predictable dashes. Neither should win more than 60% of mirror-skill AI-vs-AI test fights.

## 4. Scenes and objects

`Assets/_Project/Scenes/Dev/Sandbox_Combat.unity`
```
Sandbox_Combat
├─ --- Systems ---
│  ├─ GameBootstrap        (registers services, sets fixedDeltaTime = 1/60, targetFrameRate)
│  ├─ MatchManager         (MatchRules_Single, FD_Zyron, FD_Noxar, spawn points)
│  ├─ PoolManager
│  └─ TimeController       (hit-stop)
├─ --- Arena ---
│  ├─ Arena_Greybox        (ProBuilder ring + ArenaBounds collider + baked light)
│  ├─ SpawnPoint_P1  (0, 0, -5)
│  └─ SpawnPoint_P2  (0, 0,  5)
├─ --- Camera ---
│  ├─ Main Camera          (CinemachineBrain)
│  └─ CM_CombatCamera      (CinemachineCamera + TargetGroup + CombatCameraController + ImpulseListener)
├─ --- UI ---
│  ├─ HUD_Canvas           (HP/EP bars, KO banner, result panel)
│  └─ TouchControls_Canvas (OnScreenStick, OnScreenButtons)
└─ EventSystem             (InputSystemUIInputModule)
```

## 5. Scripts to write (in this order), with exact locations
No code yet; this is the build list. Each will be delivered as a complete file with Inspector instructions.

| # | Script | Folder | Purpose |
|---|---|---|---|
| 1 | `GameEvents.cs` | `Scripts/Core/` | Static events: OnHit, OnKO, OnMatchEnd |
| 2 | `TimeController.cs` | `Scripts/Core/` | Hit-stop (per-fighter freeze), KO slow-mo |
| 3 | `FighterDefinition.cs` | `Scripts/Data/` | Fighter SO |
| 4 | `MoveDefinition.cs` (+ `HitboxWindow`, `HitProperties`) | `Scripts/Data/` | Move SO |
| 5 | `MatchRules.cs`, `AIProfile.cs` | `Scripts/Data/` | SOs |
| 6 | `IFighterCommandSource.cs`, `FighterCommand.cs` | `Scripts/Combat/` | Shared player/AI command contract |
| 7 | `FighterResources.cs` | `Scripts/Combat/` | HP, EP, cooldowns |
| 8 | `DamageResolver.cs` | `Scripts/Combat/` | Pure damage math |
| 9 | `Hurtbox.cs`, `HitboxQuery.cs` | `Scripts/Combat/` | Overlap-based hit detection |
| 10 | `FighterMotor.cs` | `Scripts/Combat/` | CharacterController movement, knockback, facing |
| 11 | `FighterStateMachine.cs` + states (`IdleMoveState`, `AttackState`, `HitstunState`, `KnockdownState`, `KOState`, `VictoryState`) | `Scripts/Combat/States/` | Behavior |
| 12 | `MoveExecutor.cs` | `Scripts/Combat/` | Frame-stepping moves, cancels, hitbox windows |
| 13 | `Fighter.cs` | `Scripts/Combat/` | Composition root on fighter prefab |
| 14 | `ControlsActions.inputactions` + generated class | `Settings/Input/` | Input map |
| 15 | `InputBuffer.cs`, `PlayerInputSource.cs` | `Scripts/Input/` | Touch/keyboard → commands |
| 16 | `AIPerception.cs`, `AIBrain.cs` | `Scripts/AI/` | Basic AI |
| 17 | `CombatCameraController.cs` | `Scripts/Camera/` | Swipe orbit + framing |
| 18 | `FighterAnimationDriver.cs` | `Scripts/Presentation/` | State → Animator |
| 19 | `CombatFeedback.cs` | `Scripts/Presentation/` | Sparks, sounds, shake |
| 20 | `MatchManager.cs` | `Scripts/Match/` | Spawn, fight, KO, result, rematch |
| 21 | `HUDController.cs`, `ResultPanel.cs` | `Scripts/UI/` | Bars, KO banner, buttons |
| 22 | `DamageResolverTests.cs`, `FighterResourcesTests.cs` | `Tests/EditMode/` | First unit tests |
| 23 | `MatchFlowTests.cs` | `Tests/PlayMode/` | Spawn → hit → KO → rematch |

About 25 files. Delivered in 4 batches so each batch compiles and can be tested before the next.

## 6. Build order with checkpoints

**Batch A — Project skeleton (M1)**
1. Create project from Unity Hub: Unity 6000.0.67f1, "Universal 3D" template, name `ElementalClash`, in a folder you choose (doc 06 Q4).
2. Install packages: Input System, Cinemachine, ProBuilder, Test Framework, TextMeshPro essentials.
3. Create folder tree (I'll provide an Editor script that creates it in one click).
4. Player Settings: package name, landscape, IL2CPP, ARM64, Vulkan + GLES3, min API 26.
5. Build an empty scene to your phone.
✅ **Checkpoint A:** Empty app installs and opens on your phone in landscape.

**Batch B — Fighters move and hit (no AI)**
6. Scripts 1–13, greybox arena, capsule placeholders with colored materials (blue Zyron, violet Noxar).
7. Keyboard controls in editor; Noxar as a passive dummy.
✅ **Checkpoint B:** In the editor, Zyron's L1→L2→L3 chain hits the dummy, HP drops, hitstun and knockback visible, hit-stop felt. EditMode tests pass.

**Batch C — Touch, camera, animation, AI**
8. Scripts 14–19. Import placeholder humanoid models + animations, set up base Animator + override controllers.
9. Touch controls canvas; camera swipe.
10. Noxar AI "Normal".
✅ **Checkpoint C:** On your phone: move with joystick, attack, Volt Dash, AI fights back with combos and Shadow Step. FPS shown on screen.

**Batch D — Match loop and feedback**
11. Scripts 20–23. KO → slow-mo → victory/defeat animation → result panel → Rematch resets both fighters fully.
12. Hit sparks, two hit sounds, camera impulse.
✅ **Checkpoint D (M2 done):** Acceptance criteria below all pass on device.

## 7. AI for the prototype ("Normal" only)
Decision tick 10 Hz; perception delay 300 ms ± 50 ms.

| State | Enter when | Does |
|---|---|---|
| Approach | Distance > preferred range (2.0 m) | Run toward player, occasional strafe |
| Pressure | In range, own move ready | L1–L2 (60%), full chain (25%), chain → S1 (15% if EP ≥ 20) |
| Punish | Saw player in recovery (delayed snapshot) | Fastest move that reaches, 30% chance |
| Retreat | Own HP < player HP and just took a combo | Back off 1–2 s, then re-approach |
| Recover | After own combo | Pause 0.3–0.8 s (gives the player an opening) |
| Shadow Step | Player > 5 m away and stationary for 1 s, EP ≥ 20 | Teleport-in attack (35% chance per check) |

The AI never reads the input buffer and never reacts faster than its perception delay.

## 8. Acceptance criteria (M2 done = all pass on your phone)

| # | Criterion | How we check |
|---|---|---|
| 1 | App launches straight into the Sandbox match in landscape | Install APK, open |
| 2 | Joystick moves Zyron camera-relative, no drift | Manual |
| 3 | Light chain L1→L2→L3 comes out every time with rhythmic taps | 20 tries, 20 successes |
| 4 | Volt Dash spends 20 EP, shows cooldown, cannot be used without EP | Manual |
| 5 | Every landed hit shows spark, plays sound, shakes HP bar, and has hit-stop | Manual |
| 6 | Noxar AI approaches, attacks, uses Shadow Step, and can beat a passive player | Stand still 60 s |
| 7 | Active player can beat AI; AI can sometimes beat a casual player | 5 matches each by 2 people |
| 8 | KO triggers victory/defeat animations and result panel | Manual |
| 9 | Rematch resets HP, EP, cooldowns, positions, and AI with no leftovers | 10 rematches in a row |
| 10 | ≥30 FPS sustained during combat on your test phone (Profiler / on-screen counter) | 3 minute recording |
| 11 | No errors in Logcat / Console during 10 matches | `adb logcat -s Unity` |
| 12 | EditMode + PlayMode tests green | Test Runner |
| 13 | It feels responsive: input to visible reaction < 100 ms (slow-motion phone video) | 240 fps phone camera test |

## 9. What you'll need to do on your side
- Run Unity and Android builds (I cannot run Unity from this cloud environment; see doc 06).
- Optionally let me write files straight into your project folder on your PC through the folder connection, so you don't copy scripts by hand.
- Connect a phone with USB debugging enabled for Checkpoints A, C, D.
- Report back: Console errors (copy text), what felt wrong, FPS reading.

## 10. Prototype risks
- **Placeholder animations don't sell the hits.** Hit-stop, sparks, and sound carry the feel early; don't judge art yet.
- **Camera in a 3D 1v1 can lose the opponent.** Target-group framing + auto-recenter; tune at Checkpoint C.
- **Touch "fat finger" input.** Large buttons, generous hit areas, input buffer.
