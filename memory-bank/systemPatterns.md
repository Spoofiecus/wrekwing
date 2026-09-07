# System Patterns

- BuildScript.PreExport(): configure-only (ensure scene exists via AssetDatabase.GetAssetOrNull ->
  SceneSetup.SetupMainMenuScene(); set EditorBuildSettings.scenes; Android PlayerSettings). NO
  BuildPipeline.BuildPlayer, NO SwitchActiveBuildTarget inside preExport (UCB owns the target).
- BuildAndroid(): local -executeMethod path only.
- Scene generation: Assets/Scripts/Scenes/SceneSetup.cs builds MainMenu scene at build time.
- Pre-build gate: tools/compile-check.sh (mcs against real Unity DLLs). Run before EVERY cloud build.
