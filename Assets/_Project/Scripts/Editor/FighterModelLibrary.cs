// Location: Assets/_Project/Scripts/Editor/FighterModelLibrary.cs
// Assembly: EC.Editor (Editor only)
// Finds a fighter's rigged model and animation clips, builds its Animator Controller,
// and fixes materials so they render in URP. Used by SandboxBuilder; when no model is found the
// capsule placeholder is used instead, so nothing breaks before you download models.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ElementalClash.EditorTools
{
    public static class FighterModelLibrary
    {
        public const string CharactersFolder = ProjectSetup.Root + "/Art/Characters";
        public const string AnimationsFolder = ProjectSetup.Root + "/Animations";
        public const string ControllersFolder = AnimationsFolder + "/Controllers";
        public const float TargetHeight = 1.8f;

        /// <summary>Clips every fighter shares (looked up per fighter first, then in Shared).</summary>
        public static readonly string[] SharedClipNames =
        {
            "Idle", "Walk_Forward", "Walk_Back", "Strafe_Left", "Strafe_Right", "Run_Forward",
            "Hit_React", "Knockdown", "KO", "Victory",
        };

        /// <summary>Attack clips in FighterAnimator slot order.</summary>
        public static readonly string[] AttackClipNames = { "Light1", "Light2", "Light3", "Special1" };

        public sealed class Result
        {
            public GameObject Model;
            public Avatar Avatar;
            public AnimatorController Controller;
            public float[] AttackClipLengths;
            public List<string> Missing = new List<string>();
        }

        /// <summary>Returns null when Art/Characters/{fighter}/{fighter}.fbx doesn't exist.</summary>
        public static Result TryLoad(string fighterName)
        {
            string modelPath = $"{CharactersFolder}/{fighterName}/{fighterName}.fbx";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) return null;

            var result = new Result
            {
                Model = model,
                Avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault(),
            };
            if (result.Avatar == null || !result.Avatar.isHuman)
            {
                Debug.LogWarning($"[Elemental Clash] {modelPath} has no Humanoid avatar. Select it, open the Rig tab, " +
                                 "set Animation Type to Humanoid and click Apply.");
            }

            var clips = new Dictionary<string, AnimationClip>();
            foreach (string clipName in SharedClipNames.Concat(AttackClipNames))
            {
                AnimationClip clip = FindClip(fighterName, clipName);
                if (clip == null) result.Missing.Add(clipName);
                clips[clipName] = clip;
            }

            AnimationClip idle = clips["Idle"];
            AnimationClip Fallback(string n) => clips[n] != null ? clips[n] : idle;

            result.Controller = BuildController(fighterName, clips, Fallback);
            result.AttackClipLengths = AttackClipNames.Select(n => Fallback(n) != null ? Fallback(n).length : 1f).ToArray();

            if (result.Missing.Count > 0)
            {
                Debug.LogWarning($"[Elemental Clash] {fighterName}: missing animation files: {string.Join(", ", result.Missing)}. " +
                                 $"Put them in {AnimationsFolder}/{fighterName}/ or {AnimationsFolder}/Shared/ (see 10_M2_BatchD1_Mixamo_Guide). " +
                                 "Missing ones fall back to Idle.");
            }
            return result;
        }

        private static AnimationClip FindClip(string fighterName, string clipName)
        {
            foreach (string folder in new[] { $"{AnimationsFolder}/{fighterName}", $"{AnimationsFolder}/Shared" })
            {
                string path = $"{folder}/{clipName}.fbx";
                AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<AnimationClip>()
                    .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
                if (clip != null) return clip;
            }
            return null;
        }

        private static AnimatorController BuildController(string fighterName, Dictionary<string, AnimationClip> clips,
            System.Func<string, AnimationClip> fallback)
        {
            Directory.CreateDirectory(ControllersFolder);
            string path = $"{ControllersFolder}/AC_{fighterName}.controller";
            AssetDatabase.DeleteAsset(path);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveZ", AnimatorControllerParameterType.Float);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            // Locomotion: 2D blend on local velocity (m/s). Fighters always face the opponent, so they strafe a lot.
            AnimatorState locomotion = controller.CreateBlendTreeInController("Locomotion", out BlendTree tree, 0);
            tree.blendType = BlendTreeType.FreeformDirectional2D;
            tree.blendParameter = "MoveX";
            tree.blendParameterY = "MoveZ";
            void AddMotion(string clipName, Vector2 pos)
            {
                AnimationClip c = clips[clipName];
                if (c != null) tree.AddChild(c, pos);
            }
            AddMotion("Idle", Vector2.zero);
            AddMotion("Walk_Forward", new Vector2(0f, 3.5f));
            AddMotion("Walk_Back", new Vector2(0f, -3.5f));
            AddMotion("Strafe_Left", new Vector2(-3.5f, 0f));
            AddMotion("Strafe_Right", new Vector2(3.5f, 0f));
            AddMotion("Run_Forward", new Vector2(0f, 7f));
            if (tree.children.Length == 0 && clips["Idle"] != null) locomotion.motion = clips["Idle"];
            sm.defaultState = locomotion;

            float y = 80f;
            void AddState(string stateName, string clipName)
            {
                AnimatorState s = sm.AddState(stateName, new Vector3(550f, y));
                s.motion = fallback(clipName);
                y += 60f;
            }
            AddState("Light1", "Light1");
            AddState("Light2", "Light2");
            AddState("Light3", "Light3");
            AddState("Special1", "Special1");
            AddState("HitReact", "Hit_React");
            AddState("Knockdown", "Knockdown");
            AddState("KO", "KO");
            AddState("Victory", "Victory");

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        /// <summary>
        /// Instantiates the model under parent, scales it to TargetHeight, and swaps any non-URP material
        /// for a URP Lit copy (keeps the texture), so it isn't pink. Returns the instance and its main renderer.
        /// </summary>
        public static GameObject InstantiateModel(Result result, string fighterName, Transform parent, int layer,
            out Renderer mainRenderer)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(result.Model);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            instance.name = "Model";
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            foreach (Transform t in instance.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            mainRenderer = renderers.OrderByDescending(r => r.bounds.size.sqrMagnitude).FirstOrDefault();

            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
                if (b.size.y > 0.01f)
                {
                    float s = TargetHeight / b.size.y;
                    if (Mathf.Abs(s - 1f) > 0.05f) instance.transform.localScale = Vector3.one * s;
                }
            }

            FixMaterials(renderers, fighterName);

            var animator = instance.GetComponent<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();
            animator.avatar = result.Avatar;
            animator.runtimeAnimatorController = result.Controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return instance;
        }

        private static void FixMaterials(Renderer[] renderers, string fighterName)
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) return;
            string folder = $"{CharactersFolder}/{fighterName}/Materials";
            var converted = new Dictionary<Material, Material>();

            foreach (Renderer r in renderers)
            {
                Material[] mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    Material m = mats[i];
                    if (m == null || m.shader.name.StartsWith("Universal Render Pipeline")) continue;
                    if (!converted.TryGetValue(m, out Material urp))
                    {
                        Directory.CreateDirectory(folder);
                        string path = $"{folder}/{Sanitize(m.name)}_URP.mat";
                        urp = AssetDatabase.LoadAssetAtPath<Material>(path);
                        if (urp == null)
                        {
                            urp = new Material(lit);
                            Texture tex = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null;
                            if (tex != null) urp.SetTexture("_BaseMap", tex);
                            Color c = m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;
                            urp.SetColor("_BaseColor", c);
                            urp.SetFloat("_Smoothness", 0.35f);
                            AssetDatabase.CreateAsset(urp, path);
                        }
                        converted[m] = urp;
                    }
                    mats[i] = urp;
                    changed = true;
                }
                if (changed) r.sharedMaterials = mats;
            }
        }

        private static string Sanitize(string s) =>
            new string(s.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) || ch == ':' ? '_' : ch).ToArray());
    }
}
