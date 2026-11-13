// Location: Assets/_Project/Scripts/Editor/ProjectSetup.cs
// Assembly: EC.Editor (Editor only)
// One-click project configuration for Milestone 1.
// Menu: Elemental Clash > 1. Run Project Setup
// Every step is safe to run again: it only creates what is missing and re-applies settings.
using System.IO;
using ElementalClash.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ElementalClash.EditorTools
{
    public static class ProjectSetup
    {
        public const string Root = "Assets/_Project";
        public const string CompanyName = "ArenaLegends";
        public const string ProductName = "Elemental Clash";
        // Placeholder package id. Can be changed freely until the first Google Play upload.
        public const string AndroidPackageName = "com.arenalegends.elementalclash";

        private const string RenderingFolder = Root + "/Settings/Rendering";
        private const string BootScenePath = GameConstants.Scenes.Folder + "/" + GameConstants.Scenes.Boot + ".unity";
        private const string BootCubeMaterialPath = Root + "/Art/Shared/M_BootCube.mat";
        private const string PostProcessDataPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset";

        private static readonly string[] Folders =
        {
            "Art/Characters/Zyron", "Art/Characters/Noxar", "Art/Arenas", "Art/Shared", "Art/UI",
            "Animations/Shared", "Animations/Zyron", "Animations/Noxar", "Animations/Controllers",
            "Audio/Music", "Audio/SFX", "Audio/Voice", "Audio/Mixers",
            "VFX/Shared", "VFX/Lightning", "VFX/Shadow",
            "Prefabs/Fighters", "Prefabs/Projectiles", "Prefabs/Arenas", "Prefabs/Systems", "Prefabs/UI",
            "Scenes/Arenas", "Scenes/Dev",
            "ScriptableObjects/Fighters", "ScriptableObjects/Moves/Zyron", "ScriptableObjects/Moves/Noxar",
            "ScriptableObjects/Elements", "ScriptableObjects/AI", "ScriptableObjects/Arenas",
            "ScriptableObjects/Match", "ScriptableObjects/Story",
            "Scripts/Core", "Scripts/Data", "Scripts/Combat/States", "Scripts/Input", "Scripts/AI",
            "Scripts/Camera", "Scripts/Match", "Scripts/UI", "Scripts/Save", "Scripts/Presentation",
            "Scripts/Editor",
            "Settings/Input", "Settings/Rendering",
            "Tests/EditMode", "Tests/PlayMode",
            "Documentation",
        };

        [MenuItem("Elemental Clash/1. Run Project Setup", priority = 1)]
        public static void RunAll()
        {
            bool ok = EditorUtility.DisplayDialog(
                "Elemental Clash setup",
                "This creates the folder tree, URP mobile settings, physics layers, Android player settings " +
                "and a Boot test scene, then switches the platform to Android (this can take a few minutes).\n\nContinue?",
                "Run setup", "Cancel");
            if (!ok) return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            CreateFolders();
            ConfigureLayers();
            ConfigurePhysicsMatrix();
            ConfigureTime();
            ConfigurePlayerSettings();
            ConfigureUrp();
            CreateBootScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SwitchToAndroid();

            Debug.Log("[Elemental Clash] Project setup finished. Next: Elemental Clash > 2. Build Android Dev APK " +
                      "(or File > Build Profiles > Android > Build And Run). If Unity asked to restart for the " +
                      "Input System, restart it first.");
        }

        [MenuItem("Elemental Clash/Setup Steps/Create Folders", priority = 100)]
        public static void CreateFolders()
        {
            foreach (string relative in Folders)
            {
                string path = Path.Combine(Root, relative).Replace('\\', '/');
                Directory.CreateDirectory(path);
                // Hidden files are ignored by Unity but keep empty folders in Git.
                string keep = Path.Combine(path, ".gitkeep");
                if (!File.Exists(keep)) File.WriteAllText(keep, string.Empty);
            }
            AssetDatabase.Refresh();
            Debug.Log("[Elemental Clash] Folders ready.");
        }

        [MenuItem("Elemental Clash/Setup Steps/Configure Layers", priority = 101)]
        public static void ConfigureLayers()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogError("[Elemental Clash] Could not load TagManager.asset.");
                return;
            }

            var tagManager = new SerializedObject(assets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            for (int i = 0; i < GameConstants.Layers.All.Length; i++)
            {
                int index = GameConstants.Layers.FirstUserLayer + i;
                string wanted = GameConstants.Layers.All[i];
                SerializedProperty element = layers.GetArrayElementAtIndex(index);
                if (string.IsNullOrEmpty(element.stringValue))
                {
                    element.stringValue = wanted;
                }
                else if (element.stringValue != wanted)
                {
                    Debug.LogWarning($"[Elemental Clash] Layer {index} is '{element.stringValue}', expected '{wanted}'. " +
                                     "Left unchanged; please rename it in Project Settings > Tags and Layers.");
                }
            }
            tagManager.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[Elemental Clash] Layers ready.");
        }

        [MenuItem("Elemental Clash/Setup Steps/Configure Physics Matrix", priority = 102)]
        public static void ConfigurePhysicsMatrix()
        {
            int fighter = GameConstants.Layers.IndexOf(GameConstants.Layers.Fighter);
            int hurtbox = GameConstants.Layers.IndexOf(GameConstants.Layers.Hurtbox);
            int hitbox = GameConstants.Layers.IndexOf(GameConstants.Layers.Hitbox);
            int projectile = GameConstants.Layers.IndexOf(GameConstants.Layers.Projectile);
            int environment = GameConstants.Layers.IndexOf(GameConstants.Layers.Environment);
            int bounds = GameConstants.Layers.IndexOf(GameConstants.Layers.ArenaBounds);
            const int defaultLayer = 0;

            int[] ours = { fighter, hurtbox, hitbox, projectile, environment, bounds };

            // Hurtboxes and hitboxes are found with overlap queries, never by physics contacts.
            bool Allowed(int a, int b)
            {
                bool Pair(int x, int y) => (a == x && b == y) || (a == y && b == x);
                return Pair(fighter, defaultLayer) || Pair(fighter, fighter) || Pair(fighter, environment) ||
                       Pair(fighter, bounds) ||
                       Pair(projectile, defaultLayer) || Pair(projectile, environment) || Pair(projectile, bounds) ||
                       Pair(environment, defaultLayer) || Pair(environment, environment);
            }

            foreach (int a in ours)
            {
                for (int b = 0; b < 32; b++)
                {
                    Physics.IgnoreLayerCollision(a, b, !Allowed(a, b));
                }
            }
            Debug.Log("[Elemental Clash] Physics collision matrix ready.");
        }

        [MenuItem("Elemental Clash/Setup Steps/Configure Time (60 Hz)", priority = 103)]
        public static void ConfigureTime()
        {
            Time.fixedDeltaTime = GameConstants.CombatFixedDeltaTime;
            Debug.Log("[Elemental Clash] Fixed timestep set to 1/60 s.");
        }

        [MenuItem("Elemental Clash/Setup Steps/Configure Player Settings (Android)", priority = 104)]
        public static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.colorSpace = ColorSpace.Linear;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidPackageName);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode = 1;

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
                new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });

            SetActiveInputHandlerToInputSystem();
            Debug.Log("[Elemental Clash] Player settings ready (" + AndroidPackageName + ").");
        }

        private static void SetActiveInputHandlerToInputSystem()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return;

            var so = new SerializedObject(assets[0]);
            SerializedProperty handler = so.FindProperty("activeInputHandler");
            if (handler == null) return;

            const int inputSystemPackageOnly = 1;
            if (handler.intValue == inputSystemPackageOnly) return;

            handler.intValue = inputSystemPackageOnly;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.LogWarning("[Elemental Clash] Active Input Handling set to 'Input System Package'. " +
                             "Restart Unity for this to take effect.");
        }

        [MenuItem("Elemental Clash/Setup Steps/Configure URP (Mobile Low/Medium/High)", priority = 105)]
        public static void ConfigureUrp()
        {
            Directory.CreateDirectory(RenderingFolder);
            AssetDatabase.Refresh();

            UniversalRenderPipelineAsset low = CreateTier("Low", renderScale: 0.75f, msaa: 1, hdr: false, shadowDistance: 20f);
            UniversalRenderPipelineAsset medium = CreateTier("Medium", renderScale: 0.85f, msaa: 2, hdr: false, shadowDistance: 30f);
            UniversalRenderPipelineAsset high = CreateTier("High", renderScale: 1f, msaa: 2, hdr: true, shadowDistance: 40f);

            GraphicsSettings.defaultRenderPipeline = medium;

            // Map Unity's quality levels (normally 6: Very Low..Ultra) onto our three tiers.
            string[] names = QualitySettings.names;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < names.Length; i++)
            {
                float t = names.Length <= 1 ? 1f : i / (names.Length - 1f);
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = t < 0.34f ? low : (t < 0.67f ? medium : high);
            }
            QualitySettings.SetQualityLevel(current, false);

            AssetDatabase.SaveAssets();
            Debug.Log("[Elemental Clash] URP mobile tiers ready and assigned.");
        }

        private static UniversalRenderPipelineAsset CreateTier(string tier, float renderScale, int msaa, bool hdr,
            float shadowDistance)
        {
            string rendererPath = $"{RenderingFolder}/URP_Mobile_{tier}_Renderer.asset";
            string assetPath = $"{RenderingFolder}/URP_Mobile_{tier}.asset";

            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                renderer.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(PostProcessDataPath);
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }

            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
            if (asset == null)
            {
                asset = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            asset.renderScale = renderScale;
            asset.msaaSampleCount = msaa;
            asset.supportsHDR = hdr;
            asset.shadowDistance = shadowDistance;
            asset.shadowCascadeCount = 1;
            asset.supportsCameraDepthTexture = false;
            asset.supportsCameraOpaqueTexture = false;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        [MenuItem("Elemental Clash/Setup Steps/Create Boot Test Scene", priority = 106)]
        public static void CreateBootScene()
        {
            Directory.CreateDirectory(GameConstants.Scenes.Folder);
            Directory.CreateDirectory(Path.GetDirectoryName(BootCubeMaterialPath));
            AssetDatabase.Refresh();

            var material = AssetDatabase.LoadAssetAtPath<Material>(BootCubeMaterialPath);
            if (material == null)
            {
                Shader lit = Shader.Find("Universal Render Pipeline/Lit");
                if (lit == null)
                {
                    Debug.LogError("[Elemental Clash] URP Lit shader not found. Is the URP package installed?");
                    return;
                }
                material = new Material(lit);
                material.SetColor("_BaseColor", new Color32(0x2F, 0x7B, 0xFF, 0xFF)); // Zyron electric blue
                AssetDatabase.CreateAsset(material, BootCubeMaterialPath);
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "SpinningCube";
            cube.transform.position = new Vector3(0f, 1f, 0f);
            cube.transform.localScale = Vector3.one * 2f;
            cube.GetComponent<MeshRenderer>().sharedMaterial = material;

            var overlayObject = new GameObject("BuildInfoOverlay");
            var overlay = overlayObject.AddComponent<BuildInfoOverlay>();
            var so = new SerializedObject(overlay);
            so.FindProperty("_spinTarget").objectReferenceValue = cube.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, BootScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BootScenePath, true) };
            Debug.Log("[Elemental Clash] Boot scene saved and added to the build list.");
        }

        [MenuItem("Elemental Clash/Setup Steps/Switch Platform to Android", priority = 107)]
        public static void SwitchToAndroid()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) return;

            bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            if (!switched)
            {
                Debug.LogError("[Elemental Clash] Could not switch to Android. In Unity Hub, check that Android Build " +
                               "Support (with OpenJDK and Android SDK & NDK Tools) is installed for this editor.");
            }
        }
    }
}
