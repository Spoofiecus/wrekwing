#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WreckWing.Scenes;

namespace WreckWing.EditorTools
{
    /// <summary>
    /// Entry point used by Unity Cloud Build (and local -executeMethod).
    /// Generates the scene, wires Build Settings, and produces an Android APK.
    /// </summary>
    public static class BuildScript
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";

        /// <summary>Called by Unity Cloud Build when configured to use this method.</summary>
        public static void BuildAndroid()
        {
            int code = DoConfigureAndBuild(BuildTarget.Android);
            if (code != 0)
            {
                EditorApplication.Exit(1);
            }
        }

        /// <summary>Local convenience + default UCC fallback.</summary>
        public static void Build()
        {
            BuildAndroid();
        }

        private static int DoConfigureAndBuild(BuildTarget target)
        {
            Debug.Log("[BuildScript] Configuring project...");

            // 1. Generate the MainMenu scene (idempotent).
            if (!System.IO.File.Exists(ScenePath))
            {
                // Use fully qualified name to avoid ambiguity with UnityEditor.SceneManagement.SceneSetup
                WreckWing.Scenes.SceneSetup.SetupMainMenuScene();
            }
            else
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                EditorSceneManager.SaveScene(scene);
            }

            // 2. Ensure the scene is the sole entry in Build Settings.
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            // 3. Android platform + identification.
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, target);
            PlayerSettings.applicationIdentifier = "com.wreckwing.game";
            PlayerSettings.productName = "Wreck Wing";
            PlayerSettings.companyName = "WreckWing Studios";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            // Unity 2022.3+: BOTH minSdkVersion and targetSdkVersion are integers (API levels), not enums.
            // API 24 = Android 7.0 (Nougat) — minimum supported
            // API 33 = Android 13 (Tiramisu) — Google Play minimum target as of Aug 2024
            PlayerSettings.Android.minSdkVersion = 24;
            PlayerSettings.Android.targetSdkVersion = 33;

            // 4. Auto-sign with a debug keystore (Unity generates on first build).
            PlayerSettings.Android.useCustomKeystore = false;

            Debug.Log("[BuildScript] Starting build...");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Build/WreckWing.apk",
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] BUILD SUCCEEDED: {summary.totalSize} bytes at {options.locationPathName}");
                return 0;
            }

            Debug.LogError($"[BuildScript] BUILD FAILED: {summary.result}");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    Debug.LogError($"[BuildScript] {msg.content}");
                }
            }
            return 1;
        }
    }
}
#endif