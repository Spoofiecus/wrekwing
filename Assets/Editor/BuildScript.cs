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
    /// Build configuration for Unity Cloud Build.
    /// PreExport() configures the project (scene, PlayerSettings, Build Settings).
    /// Unity Cloud Build then automatically calls BuildPipeline.BuildPlayer.
    /// BuildAndroid() is kept for local -executeMethod usage.
    /// </summary>
    public static class BuildScript
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";

        /// <summary>
        /// Called by Unity Cloud Build as preExportMethod.
        /// Configures the project but does NOT build — UCB handles the build.
        /// </summary>
        public static void PreExport()
        {
            Debug.Log("[BuildScript] PreExport: Configuring project for Unity Cloud Build...");

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
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.applicationIdentifier = "com.wreckwing.game";
            PlayerSettings.productName = "Wreck Wing";
            PlayerSettings.companyName = "WreckWing Studios";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            // Unity 2022.3: minSdkVersion and targetSdkVersion use AndroidSdkVersions enum.
            // AndroidApiLevel24 = API 24 (Android 7.0 Nougat) — minimum supported
            // AndroidApiLevel33 = API 33 (Android 13 Tiramisu) — Google Play minimum target as of Aug 2024
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;

            // 4. Auto-sign with a debug keystore (Unity generates on first build).
            PlayerSettings.Android.useCustomKeystore = false;

            Debug.Log("[BuildScript] PreExport: Configuration complete. Unity Cloud Build will now build the player.");
        }

        /// <summary>Called by Unity Cloud Build when configured as the build method (legacy/custom).</summary>
        public static void BuildAndroid()
        {
            PreExport(); // Configure first
            
            Debug.Log("[BuildScript] Starting build (local/custom)...");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Build/WreckWing.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] BUILD SUCCEEDED: {summary.totalSize} bytes at {options.locationPathName}");
            }
            else
            {
                Debug.LogError($"[BuildScript] BUILD FAILED: {summary.result}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        Debug.LogError($"[BuildScript] {msg.content}");
                    }
                }
                EditorApplication.Exit(1);
            }
        }

        /// <summary>Local convenience + default UCC fallback.</summary>
        public static void Build()
        {
            BuildAndroid();
        }
    }
}
#endif
