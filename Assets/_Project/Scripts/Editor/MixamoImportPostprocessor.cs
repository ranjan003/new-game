// Location: Assets/_Project/Scripts/Editor/MixamoImportPostprocessor.cs
// Assembly: EC.Editor (Editor only)
// Automatic import settings for character models and animation FBX files dropped into:
//   Assets/_Project/Art/Characters/<Fighter>/<Fighter>.fbx   (model, "With Skin")
//   Assets/_Project/Animations/<Fighter or Shared>/<Clip>.fbx (animations, "Without Skin")
// Sets Humanoid rigs, turns off cameras/lights, and sets looping and root baking on clips
// so fighters stay in place (the CharacterController does the moving).
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ElementalClash.EditorTools
{
    public sealed class MixamoImportPostprocessor : AssetPostprocessor
    {
        private const string CharactersFolder = ProjectSetup.Root + "/Art/Characters/";
        private const string AnimationsFolder = ProjectSetup.Root + "/Animations/";

        private static readonly string[] LoopingClips =
            { "Idle", "Walk_Forward", "Walk_Back", "Strafe_Left", "Strafe_Right", "Run_Forward", "Victory" };

        private static bool IsCharacterModel(string path) =>
            path.StartsWith(CharactersFolder, StringComparison.Ordinal) && IsFbx(path);

        private static bool IsAnimationFile(string path) =>
            path.StartsWith(AnimationsFolder, StringComparison.Ordinal) && IsFbx(path);

        private static bool IsFbx(string path) => path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);

        private void OnPreprocessModel()
        {
            bool model = IsCharacterModel(assetPath);
            bool anim = IsAnimationFile(assetPath);
            if (!model && !anim) return;

            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = model;

            if (model)
            {
                importer.importAnimation = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            }
            else
            {
                importer.importAnimation = true;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
            }
        }

        private void OnPreprocessAnimation()
        {
            if (!IsAnimationFile(assetPath)) return;
            var importer = (ModelImporter)assetImporter;
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0) return;

            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            bool loop = LoopingClips.Contains(fileName, StringComparer.OrdinalIgnoreCase);

            foreach (ModelImporterClipAnimation clip in clips)
            {
                clip.name = fileName;
                clip.loopTime = loop;
                clip.loopPose = loop;
                // Bake root motion into the pose: the fighter stays where gameplay puts it.
                clip.lockRootRotation = true;
                clip.keepOriginalOrientation = true;
                clip.lockRootHeightY = true;
                clip.keepOriginalPositionY = true;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalPositionXZ = true;
            }
            importer.clipAnimations = clips;
        }
    }
}
