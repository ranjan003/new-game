# Milestone 2, Batch B: fighters move and hit (editor test)

Status: written without Unity, so **not compiled or run yet**. Your test in step 3 is the first real check.

## 1. Get the code
In PowerShell:
```
cd C:\data\game\new-game
git pull
git checkout m2-prototype
```
Then open the project in Unity (2022.3.62f2) and wait for the spinner in the bottom-right corner to stop.
If there are red errors in **Window → General → Console**, screenshot them and send them to me.

## 2. Build the test scene (one click)
**Elemental Clash → 4. Build Combat Sandbox (Batch B)**

This creates:
- `ScriptableObjects/Fighters/FD_Zyron`, `FD_Noxar`, plus 8 move assets in `ScriptableObjects/Moves/` with the frame data from doc 05
- Placeholder materials and the prefabs `Prefabs/Fighters/Fighter_Zyron`, `Fighter_Noxar` (blue and violet capsules with a white visor showing which way they face)
- `Scenes/Dev/Sandbox_Combat` with a 24 m greybox arena, a follow camera and a test HUD

Running it again rebuilds the scene and prefabs but **keeps your edited fighter and move assets**.

## 3. Play
Press **Play** (▶ at the top). Click once inside the Game view so it gets the keyboard.

| Action | Keyboard | Gamepad |
|---|---|---|
| Move | W A S D | Left stick |
| Light attack (tap up to 3 times for the chain) | J | X / Square |
| Special 1 (Volt Dash) | U | RB / R1 |

Buttons at the bottom of the Game view: **Reset fighters** and **Dummy: passive / attacks** (Noxar throws a light attack every 2 s).

## 4. Checkpoint B: please check and report
- [ ] No red errors in the Console
- [ ] Zyron moves with WASD, keeps facing Noxar, and walks around him
- [ ] Tapping J three times gives Jab → Cross → Spin Kick; Noxar's HP drops and he flashes white and slides back
- [ ] You can feel a short freeze on each hit (hit-stop)
- [ ] J, J, U cancels the Cross into Volt Dash (costs 20 EP, then shows a 4 s cooldown)
- [ ] Turn on **Dummy: attacks** and let Noxar hit you; Zyron gets stunned and pushed back
- [ ] Hitting Noxar until 0 HP shows **K.O.!**; **Reset fighters** brings everything back
- [ ] **Window → General → Test Runner → EditMode → Run All**: all tests green (15)
- [ ] Your opinion: too slow, too fast, or about right?

Tip: in the Scene view (with Gizmos on), the attack hitbox is drawn as an orange sphere during startup and red while it can hit.

## 5. Tuning without code
Select a move asset (for example `MV_Zyron_L1`) and edit startup/active/recovery frames, damage, hitstun, knockback, hitbox size, and so on in the Inspector. Changes apply the next time you press Play.

## What changed in code

| File | Purpose |
|---|---|
| `Scripts/Data/FighterDefinition.cs`, `MoveDefinition.cs` | Fighter and move data assets |
| `Scripts/Combat/Fighter.cs` | State machine (idle, attack, hitstun, knockdown, KO), moves on a 60 Hz tick, hit detection, knockback, hit-stop, hit flash |
| `Scripts/Combat/DamageResolver.cs` | Counter hits, punishes, combo scaling |
| `Scripts/Combat/FighterMeters.cs` | HP, energy, regen and cooldowns |
| `Scripts/Combat/InputBuffer.cs` | 0.15 s press buffer |
| `Scripts/Combat/Hurtbox.cs`, `CombatTypes.cs`, `DummyCommandSource.cs` | Hurtbox, shared types/events, test dummy |
| `Scripts/Input/PlayerInputSource.cs` | Keyboard/gamepad → commands (touch comes in Batch C) |
| `Scripts/Camera/SimpleCombatCamera.cs` | Placeholder follow camera (Cinemachine in Batch C) |
| `Scripts/UI/CombatDebugHUD.cs` | Test HUD |
| `Scripts/Editor/SandboxBuilder.cs` | The menu item that builds everything |
| `Tests/EditMode/CombatCoreTests.cs` | 12 new unit tests |
| `ProjectSettings/TimeManager.asset` | Fixed timestep corrected to 1/60 s (it was still 0.02) |

Simplifications vs doc 05, to revisit after you've played it:
- Volt Dash stops at Noxar instead of passing through him.
- The state machine lives inside `Fighter.cs` for now. It gets split into separate state classes when guard and dodge arrive in M3.
