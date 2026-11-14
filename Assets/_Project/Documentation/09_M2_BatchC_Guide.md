# Milestone 2, Batch C: phone controls, camera, visuals, AI

Status: written without Unity, so **not compiled or run yet**.

## What's new
- **Noxar fights back.** `AIBrain` with the `AI_Normal` profile. It sees you about 0.25–0.35 s late (it never reads your buttons), walks in while strafing, throws 2- or 3-hit chains, sometimes cancels into Shadow Step, punishes some whiffs, uses Shadow Step to close distance when you stand still far away, pauses after its combos, and backs off after taking a big combo while losing.
- **Touch controls**: a joystick at bottom-left, plus **L** (light) and **S1** (special) buttons at bottom-right.
- **Camera swipe**: drag on the right side of the screen to orbit up to ±60°. It drifts back after 1.5 s. In the editor, drag with the mouse.
- **Readable placeholder visuals**: fists wind up and punch on the real frame data (right hand, left hand, then a spin finisher). Fighters lean back when hit, fall on knockdown or KO, Noxar sinks into the floor before Shadow Step, and the winner raises both fists.
- The HUD buttons moved under the info text, and the dummy toggle is now **AI: fighting / passive**.
- `Sandbox_Combat` is now the **first scene in the build**, so the phone opens straight into the fight.

Changed from the plan: placeholder **3D models from Mixamo** aren't included, because they need your Adobe account and a manual download. The procedural fists make the hits readable until then. We can add models once the feel is right.

## Steps
1. In PowerShell:
   ```
   cd C:\data\game\new-game
   git pull
   ```
   (You should already be on `m2-prototype`. Check with `git branch`.)
2. In Unity, wait for the spinner to stop. Check **Window → General → Console** for red lines.
3. **Elemental Clash → 4. Build Combat Sandbox**. Click **Save** if it asks about the open scene.
4. Press **Play** and test with the keyboard: WASD, J, U, and drag the mouse on the right half to orbit.
5. Plug in the phone and click **Elemental Clash → 3. Build And Run on Phone (USB)**.

## Checkpoint C: please report
- [ ] No red errors in the Console
- [ ] Editor: Noxar walks up to you, attacks with chains, and uses Shadow Step sometimes
- [ ] Editor: fists punch in time with the hits; knockdown and KO look clear
- [ ] Phone: joystick moves Zyron; **L** does the chain; **S1** does Volt Dash
- [ ] Phone: swiping on the right side orbits the camera, and the camera re-centres
- [ ] Phone: can you beat Noxar? Can Noxar beat you if you stand still?
- [ ] Phone FPS reading (enable `BuildInfoOverlay (FPS)` in the scene Hierarchy if you want the counter)
- [ ] **Test Runner → EditMode → Run All**: 19 tests green
- [ ] Feel: is Noxar too easy, too hard, or fair?

## Tuning without code
- `ScriptableObjects/AI/AI_Normal`: reaction time, attack range, chain chances, punish chance, pause lengths
- `ScriptableObjects/Moves/...`: frame data, damage, hitboxes
- Camera: select **Main Camera** → *Simple Combat Camera*: distance, height, max orbit, re-centre delay
- Swipe sensitivity: **TouchControls_Canvas → CameraSwipeArea**

## When it works
Save Unity's generated files:
```
git add -A
git commit -m "Batch C tested: generated meta files and sandbox assets"
git push
```
