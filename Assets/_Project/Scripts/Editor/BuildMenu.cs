// Location: Assets/_Project/Scripts/Editor/BuildMenu.cs
// Assembly: EC.Editor (Editor only)
// Menu: Elemental Clash > 2. Build Android Dev APK / 3. Build And Run on Phone
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ElementalClash.EditorTools
{
    public static class BuildMenu
    {
        private const string OutputFolder = "Builds/Android";
        private const string ApkName = "ElementalClash_dev.apk";

        [MenuItem("Elemental Clash/2. Build Android Dev APK", priority = 20)]
        public static void BuildDevApk() => Build(runOnDevice: false);

        [MenuItem("Elemental Clash/3. Build And Run on Phone (USB)", priority = 21)]
        public static void BuildAndRun() => Build(runOnDevice: true);

        private static void Build(bool runOnDevice)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android &&
                !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("[Elemental Clash] Android platform is not available. Install Android Build Support in Unity Hub.");
                return;
            }

            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[Elemental Clash] No scenes in the build list. Run 'Elemental Clash > 1. Run Project Setup' first.");
                return;
            }

            Directory.CreateDirectory(OutputFolder);
            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(OutputFolder, ApkName),
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development | (runOnDevice ? BuildOptions.AutoRunPlayer : BuildOptions.None),
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Elemental Clash] Build succeeded: {summary.outputPath} " +
                          $"({summary.totalSize / (1024f * 1024f):0.0} MB, {summary.totalTime.TotalSeconds:0} s)");
                if (!runOnDevice) EditorUtility.RevealInFinder(summary.outputPath);
            }
            else
            {
                Debug.LogError($"[Elemental Clash] Build {summary.result} with {summary.totalErrors} error(s). " +
                               "See the Console above for details.");
            }
        }
    }
}
