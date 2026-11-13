# Milestone 1 Setup Guide — Open the project and put a test build on your phone

Target: Unity **6000.0.67f1**, Windows, test phone **iQOO Z7s**.
Project location: `C:\data\game\new-game`

Status: these files were written without access to Unity, so **none of this has been compiled or run yet**. Step 4 is the first real test. Please paste any red Console errors back to me.

## 1. One-time tools check (5 min)
1. Unity Hub → Installs → 6000.0.67f1 → ⚙ → Add modules. Make sure **Android Build Support** and both sub-items **OpenJDK** and **Android SDK & NDK Tools** are ticked.
2. Git for Windows is installed. Open *Git Bash* or *PowerShell* and run once:
   ```
   git lfs install
   ```

## 2. Get the project
```
cd C:\data\game
git clone https://github.com/ranjan003/new-game
cd new-game
git checkout m1-project-skeleton
```
(After the pull request is merged you can skip the `checkout` line; `main` will have everything.)

## 3. Open in Unity
1. Unity Hub → Projects → **Add** → **Add project from disk** → select `C:\data\game\new-game`.
2. Make sure the editor version shows **6000.0.67f1**, then open it. The first import takes several minutes; Unity creates `Library/` and the rest of `ProjectSettings/` itself.
3. If Unity asks *"enable the new Input System backends?"* click **Yes**. Unity restarts.
4. If the Package Manager reports a version error for a package (most likely URP), open **Window → Package Manager**, select the package, and install the version it recommends. Tell me which one so I can fix the manifest.

## 4. Run the setup script
1. Top menu: **Elemental Clash → 1. Run Project Setup** → **Run setup**.
2. This creates the folder tree, layers (Fighter, Hurtbox, Hitbox, Projectile, Environment, ArenaBounds), physics collision matrix, 60 Hz fixed timestep, three URP mobile quality tiers, Android player settings (package `com.arenalegends.elementalclash`, landscape, IL2CPP, ARM64, Vulkan + GLES3, Android 8.0+), a **Boot** test scene, and switches the platform to Android.
3. If the Console says to restart for the Input System, restart Unity.
4. Check: **Window → General → Test Runner → EditMode → Run All**. Expect 3 green tests.

## 5. Build to the iQOO Z7s
1. On the phone: Settings → About phone → Software info → tap **Software version** 7 times to unlock Developer options. Then Settings → System → Developer options → enable **USB debugging**. (iQOO menu names can vary slightly by Funtouch/Origin OS version.)
2. Connect by USB, accept the "Allow USB debugging" prompt on the phone.
3. Unity: **Elemental Clash → 3. Build And Run on Phone (USB)**.
   Or **2. Build Android Dev APK** to get `Builds/Android/ElementalClash_dev.apk` and install it yourself.

## 6. Checkpoint A (M1 done) — please report back
- [ ] Project opens with no red Console errors
- [ ] 3 EditMode tests pass
- [ ] App installs and opens on the iQOO Z7s in **landscape**
- [ ] A blue cube spins and the overlay shows FPS, device name and graphics API
- [ ] FPS reading at the **60 cap**: ____  and at the **30 cap**: ____
- [ ] Graphics API shown (Vulkan or OpenGLES3): ____

Then commit Unity's generated files (the `.meta` files and `ProjectSettings/`):
```
git add -A
git commit -m "M1: Unity-generated project settings and meta files"
git push
```

## What's in this milestone

| File | What it does |
|---|---|
| `Packages/manifest.json` | URP, Input System, Cinemachine, Test Framework, uGUI, Timeline |
| `ProjectSettings/ProjectVersion.txt` | Pins Unity 6000.0.67f1 |
| `.gitignore`, `.gitattributes` | Unity ignores, Git LFS for models, textures, audio |
| `Scripts/Core/GameConstants.cs` | Tick rate, layer names, scene paths |
| `Scripts/Core/BuildInfoOverlay.cs` | On-device FPS / device overlay with 30/60 cap and quality buttons |
| `Scripts/Editor/ProjectSetup.cs` | The one-click setup (each step also under *Elemental Clash → Setup Steps*) |
| `Scripts/Editor/BuildMenu.cs` | Build APK / Build And Run menu items |
| `Settings/Input/ControlsActions.inputactions` | Combat input map (keyboard, gamepad, and on-screen touch controls) |
| `Tests/EditMode/GameConstantsTests.cs` | First tests |

Editor keyboard controls (used from M2): move **WASD**, Light **J**, Heavy **K**, Special 1 **U**, Special 2 **I**, Guard **L**, Dodge **Space**, Ultimate **O**, Pause **Esc**.
