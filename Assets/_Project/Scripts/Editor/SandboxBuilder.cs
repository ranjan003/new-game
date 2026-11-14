// Location: Assets/_Project/Scripts/Editor/SandboxBuilder.cs
// Assembly: EC.Editor (Editor only)
// Menu: Elemental Clash > 4. Build Combat Sandbox (Batch B)
// Creates the Zyron/Noxar data assets (only if missing, so your tuning is kept), placeholder materials,
// fighter prefabs, and the Sandbox_Combat scene with a greybox arena, camera and debug HUD.
using System.IO;
using System.Linq;
using ElementalClash.CameraSystem;
using ElementalClash.Combat;
using ElementalClash.Controls;
using ElementalClash.Core;
using ElementalClash.Data;
using ElementalClash.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace ElementalClash.EditorTools
{
    public static class SandboxBuilder
    {
        private const string Root = ProjectSetup.Root;
        private const string MovesFolder = Root + "/ScriptableObjects/Moves";
        private const string FightersFolder = Root + "/ScriptableObjects/Fighters";
        private const string MaterialsFolder = Root + "/Art/Shared";
        private const string PrefabsFolder = Root + "/Prefabs/Fighters";
        private const string ScenePath = Root + "/Scenes/Dev/Sandbox_Combat.unity";
        private const string InputActionsPath = Root + "/Settings/Input/ControlsActions.inputactions";

        private const float ArenaSize = 24f;
        private const float WallHeight = 1.2f;
        private const float WallThickness = 0.5f;

        private static readonly Color ZyronBlue = new Color32(0x2F, 0x7B, 0xFF, 0xFF);
        private static readonly Color NoxarViolet = new Color32(0x5B, 0x2A, 0x86, 0xFF);

        [MenuItem("Elemental Clash/4. Build Combat Sandbox (Batch B)", priority = 40)]
        public static void Build()
        {
            if (LayerMask.NameToLayer(GameConstants.Layers.Hurtbox) < 0)
            {
                EditorUtility.DisplayDialog("Elemental Clash", "Run 'Elemental Clash > 1. Run Project Setup' first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            foreach (string folder in new[] { MovesFolder + "/Zyron", MovesFolder + "/Noxar", FightersFolder, MaterialsFolder, PrefabsFolder, Path.GetDirectoryName(ScenePath) })
            {
                Directory.CreateDirectory(folder);
            }
            AssetDatabase.Refresh();

            FighterDefinition zyron = CreateZyron();
            FighterDefinition noxar = CreateNoxar();

            Material zyronMat = GetOrCreateMaterial("M_Placeholder_Zyron", ZyronBlue);
            Material noxarMat = GetOrCreateMaterial("M_Placeholder_Noxar", NoxarViolet);
            Material floorMat = GetOrCreateMaterial("M_Greybox_Floor", new Color(0.42f, 0.44f, 0.48f));
            Material wallMat = GetOrCreateMaterial("M_Greybox_Wall", new Color(0.25f, 0.27f, 0.32f));
            Material accentMat = GetOrCreateMaterial("M_Placeholder_Accent", new Color(0.95f, 0.95f, 0.95f));

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (actions == null)
            {
                Debug.LogError("[Elemental Clash] Could not load " + InputActionsPath);
                return;
            }

            GameObject zyronPrefab = BuildFighterPrefab("Fighter_Zyron", zyron, zyronMat, accentMat, isPlayer: true, actions);
            GameObject noxarPrefab = BuildFighterPrefab("Fighter_Noxar", noxar, noxarMat, accentMat, isPlayer: false, null);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.6f, 0.7f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.42f, 0.45f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.22f);

            BuildArena(floorMat, wallMat);

            var player = InstantiateFighter(zyronPrefab, new Vector3(0f, 0f, -4f), Quaternion.identity);
            var opponent = InstantiateFighter(noxarPrefab, new Vector3(0f, 0f, 4f), Quaternion.Euler(0f, 180f, 0f));
            SetRef(player, "_opponent", opponent);
            SetRef(opponent, "_opponent", player);

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraGo.AddComponent<Camera>();
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.transform.position = new Vector3(0f, 3f, -10f);
            cameraGo.transform.LookAt(new Vector3(0f, 1f, 0f));
            var rig = cameraGo.AddComponent<SimpleCombatCamera>();
            rig.SetTargets(player.transform, opponent.transform);
            EditorUtility.SetDirty(rig);

            var hudGo = new GameObject("CombatDebugHUD");
            var hud = hudGo.AddComponent<CombatDebugHUD>();
            SetRef(hud, "_player", player);
            SetRef(hud, "_opponent", opponent);
            SetRef(hud, "_dummy", opponent.GetComponent<DummyCommandSource>());

            var overlay = new GameObject("BuildInfoOverlay (FPS)").AddComponent<BuildInfoOverlay>();
            overlay.enabled = false; // enable in the Inspector if you want the FPS readout here too

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuild(ScenePath);
            AssetDatabase.SaveAssets();

            Debug.Log("[Elemental Clash] Sandbox_Combat built. Press Play. Move WASD, Light J, Special U.");
        }

        // ------------------------------------------------------------------ Data

        private static FighterDefinition CreateZyron()
        {
            string f = MovesFolder + "/Zyron/";
            // Frame data from doc 05 §3. startup, active, recovery, damage, hitstun, knockback
            MoveDefinition l1 = GetOrCreateMove(f + "MV_Zyron_L1.asset", "Jab", MoveKind.Light, 5, 3, 10, 4f, 14, 0.3f,
                cancelFrom: 6, cancelSpecial: false, hitStop: 4, energyGain: 3f, radius: 0.5f, forward: 1.0f, travel: 0.3f);
            MoveDefinition l2 = GetOrCreateMove(f + "MV_Zyron_L2.asset", "Cross", MoveKind.Light, 6, 3, 12, 5f, 16, 0.4f,
                cancelFrom: 7, cancelSpecial: true, hitStop: 4, energyGain: 3f, radius: 0.5f, forward: 1.05f, travel: 0.35f);
            MoveDefinition l3 = GetOrCreateMove(f + "MV_Zyron_L3.asset", "Spin Kick", MoveKind.Light, 9, 4, 20, 8f, 24, 2.0f,
                cancelFrom: 0, cancelSpecial: false, hitStop: 6, energyGain: 3f, radius: 0.6f, forward: 1.1f, travel: 0.5f,
                knockdownOnCounter: true);
            MoveDefinition voltDash = GetOrCreateMove(f + "MV_Zyron_VoltDash.asset", "Volt Dash", MoveKind.Special, 8, 10, 18, 12f, 28, 2.5f,
                cancelFrom: 0, cancelSpecial: false, hitStop: 7, energyGain: 0f, radius: 0.7f, forward: 0.6f, travel: 7f,
                movement: MoveMovement.Dash, energyCost: 20f, cooldown: 4f, tracking: 20f);

            return GetOrCreateFighter(FightersFolder + "/FD_Zyron.asset", "Zyron", ElementPower.Lightning, "Speed melee",
                ZyronBlue, health: 90f, walk: 4f, run: 7f, new[] { l1, l2, l3 }, voltDash);
        }

        private static FighterDefinition CreateNoxar()
        {
            string f = MovesFolder + "/Noxar/";
            MoveDefinition l1 = GetOrCreateMove(f + "MV_Noxar_L1.asset", "Claw", MoveKind.Light, 7, 3, 11, 5f, 15, 0.3f,
                cancelFrom: 8, cancelSpecial: false, hitStop: 4, energyGain: 3f, radius: 0.55f, forward: 1.15f, travel: 0.3f);
            MoveDefinition l2 = GetOrCreateMove(f + "MV_Noxar_L2.asset", "Backhand", MoveKind.Light, 8, 4, 13, 6f, 17, 0.5f,
                cancelFrom: 9, cancelSpecial: true, hitStop: 4, energyGain: 3f, radius: 0.55f, forward: 1.2f, travel: 0.35f);
            MoveDefinition l3 = GetOrCreateMove(f + "MV_Noxar_L3.asset", "Rising Slash", MoveKind.Light, 11, 4, 22, 9f, 26, 1.8f,
                cancelFrom: 0, cancelSpecial: false, hitStop: 6, energyGain: 3f, radius: 0.6f, forward: 1.25f, travel: 0.5f,
                knockdown: true);
            MoveDefinition shadowStep = GetOrCreateMove(f + "MV_Noxar_ShadowStep.asset", "Shadow Step", MoveKind.Special, 14, 4, 16, 10f, 26, 1.5f,
                cancelFrom: 0, cancelSpecial: false, hitStop: 7, energyGain: 0f, radius: 0.7f, forward: 1.0f, travel: 0f,
                movement: MoveMovement.TeleportBeside, energyCost: 20f, cooldown: 4f, tracking: 0f, invulnFrom: 4, invulnTo: 14);

            return GetOrCreateFighter(FightersFolder + "/FD_Noxar.asset", "Noxar", ElementPower.Shadow, "All-rounder",
                NoxarViolet, health: 105f, walk: 3.6f, run: 6.4f, new[] { l1, l2, l3 }, shadowStep);
        }

        private static MoveDefinition GetOrCreateMove(string path, string displayName, MoveKind kind,
            int startup, int active, int recovery, float damage, int hitstun, float knockback,
            int cancelFrom, bool cancelSpecial, int hitStop, float energyGain, float radius, float forward, float travel,
            MoveMovement movement = MoveMovement.Lunge, float energyCost = 0f, float cooldown = 0f, float tracking = 12f,
            bool knockdown = false, bool knockdownOnCounter = false, int invulnFrom = 0, int invulnTo = 0)
        {
            var existing = AssetDatabase.LoadAssetAtPath<MoveDefinition>(path);
            if (existing != null) return existing;

            var move = ScriptableObject.CreateInstance<MoveDefinition>();
            AssetDatabase.CreateAsset(move, path);

            var so = new SerializedObject(move);
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_kind").enumValueIndex = (int)kind;
            so.FindProperty("_startupFrames").intValue = startup;
            so.FindProperty("_activeFrames").intValue = active;
            so.FindProperty("_recoveryFrames").intValue = recovery;
            so.FindProperty("_cancelFromFrame").intValue = cancelFrom;
            so.FindProperty("_canCancelIntoSpecial").boolValue = cancelSpecial;
            so.FindProperty("_damage").floatValue = damage;
            so.FindProperty("_hitstunFrames").intValue = hitstun;
            so.FindProperty("_knockbackDistance").floatValue = knockback;
            so.FindProperty("_causesKnockdown").boolValue = knockdown;
            so.FindProperty("_knockdownOnCounter").boolValue = knockdownOnCounter;
            so.FindProperty("_hitStopFrames").intValue = hitStop;
            so.FindProperty("_energyGainOnHit").floatValue = energyGain;
            so.FindProperty("_energyCost").floatValue = energyCost;
            so.FindProperty("_cooldownSeconds").floatValue = cooldown;
            so.FindProperty("_hitboxRadius").floatValue = radius;
            so.FindProperty("_hitboxForward").floatValue = forward;
            so.FindProperty("_hitboxHeight").floatValue = 1.1f;
            so.FindProperty("_trackingDegreesPerFrame").floatValue = tracking;
            so.FindProperty("_movement").enumValueIndex = (int)movement;
            so.FindProperty("_travelDistance").floatValue = travel;
            so.FindProperty("_invulnerableFromFrame").intValue = invulnFrom;
            so.FindProperty("_invulnerableToFrame").intValue = invulnTo;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(move);
            return move;
        }

        private static FighterDefinition GetOrCreateFighter(string path, string displayName, ElementPower power, string fighterClass,
            Color color, float health, float walk, float run, MoveDefinition[] lightChain, MoveDefinition special1)
        {
            var existing = AssetDatabase.LoadAssetAtPath<FighterDefinition>(path);
            if (existing != null) return existing;

            var fighter = ScriptableObject.CreateInstance<FighterDefinition>();
            AssetDatabase.CreateAsset(fighter, path);

            var so = new SerializedObject(fighter);
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_power").enumValueIndex = (int)power;
            so.FindProperty("_fighterClass").stringValue = fighterClass;
            so.FindProperty("_placeholderColor").colorValue = color;
            so.FindProperty("_maxHealth").floatValue = health;
            so.FindProperty("_walkSpeed").floatValue = walk;
            so.FindProperty("_runSpeed").floatValue = run;
            SerializedProperty chain = so.FindProperty("_lightChain");
            chain.arraySize = lightChain.Length;
            for (int i = 0; i < lightChain.Length; i++)
            {
                chain.GetArrayElementAtIndex(i).objectReferenceValue = lightChain[i];
            }
            so.FindProperty("_special1").objectReferenceValue = special1;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(fighter);
            return fighter;
        }

        private static Material GetOrCreateMaterial(string name, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(lit != null ? lit : Shader.Find("Standard"));
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        // ------------------------------------------------------------------ Fighters

        private static GameObject BuildFighterPrefab(string name, FighterDefinition definition, Material bodyMat,
            Material accentMat, bool isPlayer, InputActionAsset actions)
        {
            string path = $"{PrefabsFolder}/{name}.prefab";
            int fighterLayer = LayerMask.NameToLayer(GameConstants.Layers.Fighter);
            int hurtboxLayer = LayerMask.NameToLayer(GameConstants.Layers.Hurtbox);

            var root = new GameObject(name) { layer = fighterLayer };
            var controller = root.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.slopeLimit = 45f;
            controller.stepOffset = 0.3f;
            controller.skinWidth = 0.05f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
            body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;
            body.layer = fighterLayer;

            // A "visor" so you can see which way the fighter faces.
            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "FacingVisor";
            Object.DestroyImmediate(visor.GetComponent<Collider>());
            visor.transform.SetParent(root.transform, false);
            visor.transform.localPosition = new Vector3(0f, 1.45f, 0.32f);
            visor.transform.localScale = new Vector3(0.5f, 0.12f, 0.2f);
            visor.GetComponent<MeshRenderer>().sharedMaterial = accentMat;
            visor.layer = fighterLayer;

            var hurtboxGo = new GameObject("Hurtbox") { layer = hurtboxLayer };
            hurtboxGo.transform.SetParent(root.transform, false);
            var capsule = hurtboxGo.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.radius = 0.45f;
            capsule.height = 1.8f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            var hurtbox = hurtboxGo.AddComponent<Hurtbox>();

            var fighter = root.AddComponent<Fighter>();
            hurtbox.Owner = fighter;

            if (isPlayer)
            {
                var input = root.AddComponent<PlayerInputSource>();
                var inputSo = new SerializedObject(input);
                inputSo.FindProperty("_actions").objectReferenceValue = actions;
                inputSo.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                root.AddComponent<DummyCommandSource>();
            }

            var fighterSo = new SerializedObject(fighter);
            fighterSo.FindProperty("_definition").objectReferenceValue = definition;
            fighterSo.FindProperty("_hurtbox").objectReferenceValue = hurtbox;
            fighterSo.FindProperty("_bodyRenderer").objectReferenceValue = body.GetComponent<MeshRenderer>();
            fighterSo.ApplyModifiedPropertiesWithoutUndo();

            var hurtboxSo = new SerializedObject(hurtbox);
            hurtboxSo.FindProperty("_owner").objectReferenceValue = fighter;
            hurtboxSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Fighter InstantiateFighter(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance.GetComponent<Fighter>();
        }

        // ------------------------------------------------------------------ Arena

        private static void BuildArena(Material floorMat, Material wallMat)
        {
            int environment = LayerMask.NameToLayer(GameConstants.Layers.Environment);
            int bounds = LayerMask.NameToLayer(GameConstants.Layers.ArenaBounds);

            var arena = new GameObject("Arena_Greybox");

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.layer = environment;
            floor.transform.SetParent(arena.transform, false);
            floor.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            floor.transform.localScale = new Vector3(ArenaSize, 0.5f, ArenaSize);
            floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            float half = ArenaSize * 0.5f;
            CreateWall(arena.transform, "Wall_North", new Vector3(0f, WallHeight * 0.5f, half), new Vector3(ArenaSize, WallHeight, WallThickness), bounds, wallMat);
            CreateWall(arena.transform, "Wall_South", new Vector3(0f, WallHeight * 0.5f, -half), new Vector3(ArenaSize, WallHeight, WallThickness), bounds, wallMat);
            CreateWall(arena.transform, "Wall_East", new Vector3(half, WallHeight * 0.5f, 0f), new Vector3(WallThickness, WallHeight, ArenaSize), bounds, wallMat);
            CreateWall(arena.transform, "Wall_West", new Vector3(-half, WallHeight * 0.5f, 0f), new Vector3(WallThickness, WallHeight, ArenaSize), bounds, wallMat);

            // Invisible tall boundary so nobody gets knocked over the low walls.
            var invisible = new GameObject("Bounds_Invisible") { layer = bounds };
            invisible.transform.SetParent(arena.transform, false);
            AddBoxCollider(invisible, new Vector3(0f, 4f, half + 0.5f), new Vector3(ArenaSize + 2f, 8f, 1f));
            AddBoxCollider(invisible, new Vector3(0f, 4f, -half - 0.5f), new Vector3(ArenaSize + 2f, 8f, 1f));
            AddBoxCollider(invisible, new Vector3(half + 0.5f, 4f, 0f), new Vector3(1f, 8f, ArenaSize + 2f));
            AddBoxCollider(invisible, new Vector3(-half - 0.5f, 4f, 0f), new Vector3(1f, 8f, ArenaSize + 2f));

            // Centre marks to judge spacing.
            for (int i = -2; i <= 2; i++)
            {
                GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mark.name = "SpacingMark";
                Object.DestroyImmediate(mark.GetComponent<Collider>());
                mark.transform.SetParent(arena.transform, false);
                mark.transform.localPosition = new Vector3(0f, 0.005f, i * 2f);
                mark.transform.localScale = new Vector3(1.5f, 0.01f, 0.05f);
                mark.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
            }
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale, int layer, Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.layer = layer;
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
            wall.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void AddBoxCollider(GameObject go, Vector3 center, Vector3 size)
        {
            var box = go.AddComponent<BoxCollider>();
            box.center = center;
            box.size = size;
        }

        // ------------------------------------------------------------------ Utilities

        private static void SetRef(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(property);
            if (p == null)
            {
                Debug.LogError($"[Elemental Clash] Property {property} not found on {target}.");
                return;
            }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddSceneToBuild(string path)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Any(s => s.path == path)) return;
            EditorBuildSettings.scenes = scenes.Append(new EditorBuildSettingsScene(path, true)).ToArray();
        }
    }
}
