# Batch D, part 1: real 3D fighters from Mixamo

Status: written without Unity, so **not compiled or run yet**.

## What's new
- Drop a Mixamo character and its animations into the right folders, run menu item 4 again, and the capsule is replaced by the rigged model.
- The fighter plays real animations: idle, walking and strafing (it blends by direction), the 3-hit chain, the special, hit reaction, knockdown, KO and victory.
- Attack animations are **time-synced to the frame data**: each clip is sped up or slowed so its impact lands on the move's first active frame, so what you see matches when the hit lands. Hit-stop also freezes the pose.
- Import settings are automatic: Humanoid rig, looping on idle/walk/run/victory, root motion baked in place, and materials converted to URP so the model isn't pink.
- If no model is in the folder, you still get the capsule, so nothing breaks.

## 1. Download from mixamo.com (free, Adobe login)
### Characters (one each)
Pick any humanoid you like. Suggestions: a martial-artist type for **Zyron** (for example "Kachujin") and a dark armoured type for **Noxar** (for example "Nightshade" or "Vanguard"). Names on the site may differ a little.

Download settings: **Format: FBX for Unity (.fbx)**, **Pose: T-pose**.

### Animations
Select **the character first**, then search each name below. Any similar clip works.
Download settings: **FBX for Unity**, **Skin: Without Skin**, **30 fps**, tick **In Place** when it's offered.

**Rename each file exactly as in the left column.**

Shared by both fighters, save into `Assets/_Project/Animations/Shared/`:

| File name | Search on Mixamo |
|---|---|
| Idle.fbx | Fighting Idle (or Bouncing Fight Idle) |
| Walk_Forward.fbx | Walking (In Place) |
| Walk_Back.fbx | Walking Backwards |
| Strafe_Left.fbx | Left Strafe Walking |
| Strafe_Right.fbx | Right Strafe Walking |
| Run_Forward.fbx | Running (In Place) |
| Hit_React.fbx | Head Hit (or Stomach Hit) |
| Knockdown.fbx | Knocked Down (or Falling Back) |
| KO.fbx | Dying |
| Victory.fbx | Victory (or Victory Idle) |

Zyron's attacks, save into `Assets/_Project/Animations/Zyron/`:

| File name | Move | Search on Mixamo |
|---|---|---|
| Light1.fbx | Jab | Lead Jab |
| Light2.fbx | Cross | Cross Punch |
| Light3.fbx | Spin Kick | Roundhouse Kick |
| Special1.fbx | Volt Dash | Flying Knee (any forward dashing strike) |

Noxar's attacks, save into `Assets/_Project/Animations/Noxar/`:

| File name | Move | Search on Mixamo |
|---|---|---|
| Light1.fbx | Claw | Hook Punch |
| Light2.fbx | Backhand | Elbow Punch (or a backhand strike) |
| Light3.fbx | Rising Slash | Uppercut |
| Special1.fbx | Shadow Step | Mma Kick (or a spinning kick) |

### Models
- `Assets/_Project/Art/Characters/Zyron/Zyron.fbx`
- `Assets/_Project/Art/Characters/Noxar/Noxar.fbx`

(Folder path on your PC: `C:\data\game\new-game\Assets\_Project\...`)

## 2. In Unity
1. `git pull` first (you should be on `m2-prototype`).
2. Copy the files into the folders above **while Unity is open**. Wait for the import to finish.
3. **Elemental Clash → 4. Build Combat Sandbox** (Save if asked).
4. Check the Console. A yellow line "missing animation files" means a file name doesn't match; missing ones fall back to Idle.
5. Press **Play**, then build to the phone with menu item 3.

## Checkpoint D1: please report
- [ ] No red errors in the Console
- [ ] Both fighters show as real models (not pink, not tiny, not giant), standing on the floor and facing each other
- [ ] Walking and strafing animate; punches/kicks play when you press L and S1
- [ ] The hit visibly connects at the moment damage appears
- [ ] Hit reaction, knockdown, KO and victory play
- [ ] Phone FPS still OK
- [ ] Screenshot or short video if you can

## Tuning
- Select the fighter in the scene → **Fighter Animator** → **Attack Impact**: when the strike lands in each clip (0..1). If a punch looks early, raise it; if late, lower it.
- If a model is pink: select its FBX → **Materials** tab → **Extract Textures**, then run menu item 4 again.
- If a model is in a T-pose: select its FBX → **Rig** tab → Animation Type **Humanoid** → Apply.

## Next (Batch D, part 2)
Lightning and shadow effects for each special, hit sparks, screen shake, sounds, the real HUD, loading screen, "Round 1, Fight!" intro and KO slow-motion camera.

Mixamo animations are royalty-free stock motion capture, so they're fine for an original game.
