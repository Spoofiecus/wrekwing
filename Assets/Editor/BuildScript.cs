#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using WreckWing.Scenes;

namespace WreckWing.EditorTools
{
    /// <summary>
    /// Build configuration for Unity Cloud Build on Unity 2022.3.
    /// PreExport() is wired as the preExportMethod in the Cloud Build target config
    /// (WreckWing.EditorTools.BuildScript.PreExport). Unity Cloud Build runs it BEFORE its
    /// standard playerExporter, so it must configure the project (ensure the scene asset
    /// exists and is listed in Build Settings, and apply Android PlayerSettings) but must
    /// NOT call BuildPipeline.BuildPlayer and must NOT switch the active build target
    /// (UCB selects that from its own config).
    /// BuildAndroid() is retained for local -executeMethod usage.
    /// </summary>
    public static class BuildScript
    {
        public const string ScenePath = "Assets/Scenes/MainMenu.unity";

        public static void PreExport()
        {
            Debug.Log("[BuildScript] PreExport: Configuring project for Unity Cloud Build...");
            EnsureMainMenuScene();
            ConfigureAndroidPlayerSettings();
            Debug.Log("[BuildScript] PreExport: Configuration complete. Unity Cloud Build will now build the player.");
        }

        private static void EnsureMainMenuScene()
        {
            bool sceneExists = AssetDatabase.LoadMainAssetAtPath(ScenePath) != null;

            if (!sceneExists)
            {
                Debug.Log("[BuildScript] MainMenu scene not present; generating via WreckWing.Scenes.SceneSetup.");
                if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                {
                    AssetDatabase.CreateFolder("Assets", "Scenes");
                }
                SceneSetup.SetupMainMenuScene();
            }
            else
            {
                Debug.Log("[BuildScript] MainMenu scene found; leaving it untouched.");
            }

            EditorBuildSettings.scenes =
                new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static void ConfigureAndroidPlayerSettings()
        {
            PlayerSettings.applicationIdentifier = "com.wreckwing.game";
            PlayerSettings.productName = "Wreck Wing";
            PlayerSettings.companyName = "WreckWing Studios";
            PlayerSettings.defaultInterfaceOrientation = UnityEditor.UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = UnityEditor.AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = UnityEditor.AndroidSdkVersions.AndroidApiLevel33;
            // Build for both 64-bit and 32-bit ABIs. Modern Armv9 flagships (Cortex-X4+)
            // dropped 32-bit execution entirely, so an armeabi-v7a-only APK is rejected on
            // those devices ("not available on the latest version of Android").
            PlayerSettings.Android.targetArchitectures =
                UnityEditor.AndroidArchitecture.ARMv7 | UnityEditor.AndroidArchitecture.ARM64;

            // Let Unity auto-sign with its generated debug keystore.
            PlayerSettings.Android.useCustomKeystore = false;
        }

        public static void BuildAndroid()
        {
            Debug.Log("[BuildScript] Starting local/custom Android build...");
            EnsureMainMenuScene();
            ConfigureAndroidPlayerSettings();

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new string[] { ScenePath },
                locationPathName = "Build/WreckWing.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[BuildScript] BUILD SUCCEEDED: " + summary.totalSize + " bytes at " + options.locationPathName);
            }
            else
            {
                Debug.LogError("[BuildScript] BUILD FAILED: " + summary.result);
                foreach (BuildStep step in report.steps)
                {
                    foreach (BuildStepMessage msg in step.messages)
                    {
                        Debug.LogError("[BuildScript] " + msg.content);
                    }
                }
                EditorApplication.Exit(1);
            }
        }

        public static void Build()
        {
            BuildAndroid();
        }
    }
}
#endif
