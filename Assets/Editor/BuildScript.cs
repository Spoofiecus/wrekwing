#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WreckWing.EditorTools
{
    /// <summary>
    /// Build configuration for Unity Cloud Build on Unity 2022.3.
    ///
    /// PreExport() (wired as preExportMethod in UCB: WreckWing.EditorTools.BuildScript.PreExport)
    /// expands the scene list and applies platform PlayerSettings; UCB then runs the standard
    /// playerExporter, which drives BuildPipeline.BuildPlayer itself. PreExport therefore must
    /// NOT call BuildPipeline.BuildPlayer, and must NOT touch the active build target (UCB sets
    /// it from its own config).
    ///
    /// BuildAndroid() is retained for local `-executeMethod` usage: it applies the same
    /// configuration and drives BuildPipeline.BuildPlayer directly, logging totalSize and
    /// exiting non-zero on failure.
    /// </summary>
    public static class BuildScript
    {
        public const string ScenePath = "Assets/Scenes/MainMenu.unity";

        /// <summary>
        /// Unity Cloud Build preExportMethod entry point.
        /// Idempotent: only regenerates the MainMenu scene if it is missing from this checkout;
        /// an existing committed scene is left byte-for-byte untouched (no reserialize, no repo
        /// noise).
        /// </summary>
        public static void PreExport()
        {
            Debug.Log("[BuildScript] PreExport: Configuring project for Unity Cloud Build...");

            EnsureMainMenuScene();
            ConfigureAndroidPlayerSettings();

            Debug.Log("[BuildScript] PreExport: Configuration complete. Unity Cloud Build will now build the player.");
        }

        /// <summary>
        /// Open-or-generate scene bootstrap.
        /// Opening the committed scene validates that it exists without touching any file-system
        /// API and without dirtying a present, committed scene. If it is missing (fresh checkout
        /// predating the committed scene), we generate+save it via SceneSetup and then point Build
        /// Settings at it.
        /// </summary>
        private static void EnsureMainMenuScene()
        {
            bool sceneExists = false;
            try
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                sceneExists = true;
                Debug.Log("[BuildScript] MainMenu scene found; leaving it untouched.");
            }
            catch (Exception)
            {
                sceneExists = false;
            }

            if (!sceneExists)
            {
                Debug.Log("[BuildScript] MainMenu scene not present; generating via WreckWing.Scenes.SceneSetup.");

                // Ensure Assets/Scenes exists before SceneSetup saves into it, without using
                // any (non-compiling) file-system API.
                if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                {
                    AssetDatabase.CreateFolder("Assets/Scenes");
                }

                WreckWing.Scenes.SceneSetup.SetupMainMenuScene();
            }

            // Ensure Build Settings lists it as the sole enabled scene.
            EditorBuildSettings.scenes =
                new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        /// <summary>
        /// Android identification + SDK policy, shared by UCB PreExport and local builds.
        /// </summary>
        private static void ConfigureAndroidPlayerSettings()
        {
            PlayerSettings.applicationIdentifier = "com.wreckwing.game";
            PlayerSettings.productName = "Wreck Wing";
            PlayerSettings.companyName = "WreckWing Studios";
            PlayerSettings.defaultInterfaceOrientation = UnityEditor.UIOrientation.Portrait;

            // Unity 2022.3: min/target SDK versions use the AndroidSdkVersions enum.
            // AndroidApiLevel24 = API 24 (Android 7.0 Nougat)  - minimum supported.
            // AndroidApiLevel33 = API 33 (Android 13)           - modern Play-store baseline.
            PlayerSettings.Android.minSdkVersion = UnityEditor.AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = UnityEditor.AndroidSdkVersions.AndroidApiLevel33;

            // Let Unity auto-sign with its generated debug keystore.
            PlayerSettings.Android.useCustomKeystore = false;
        }

        /// <summary>
        /// Local/custom build for `-executeMethod WreckWing.EditorTools.BuildScript.BuildAndroid`.
        /// Applies the same configuration, then drives BuildPipeline.BuildPlayer directly,
        /// reports totalSize and exits non-zero on failure.
        /// </summary>
        public static void BuildAndroid()
        {
            Debug.Log("[BuildScript] Starting local/custom Android build...");

            EnsureMainMenuScene();
            ConfigureAndroidPlayerSettings();

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new String[] { ScenePath },
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

        /// <summary>Local convenience alias for the default custom build method.</summary>
        public static void Build()
        {
            BuildAndroid();
        }
    }
}
#endif
