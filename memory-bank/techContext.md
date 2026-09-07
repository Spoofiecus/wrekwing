# Tech Context

- Unity: project targets 2022.3.55f1; UCB builds with 2022_3_62f3 (latest2022_3). Package: Android.
- Repo: github.com:Spoofiecus/wrekwing.git branch master. Pre-commit gate runs on commit.
- Unity Build Automation: org 11270707303078, project 73c98b36-43ec-481a-ad34-cca6f3b6dfbc,
  target id "wrekwing" (android, win_micro_v1, unityVersion latest2022_3, cleanBuild scheduled builds only).
- Target settings that matter: settings.advanced.unity.preExportMethod =
  WreckWing.EditorTools.BuildScript.PreExport; settings.advanced.unity.playerExporter.sceneList =
  ["Assets/Scenes/MainMenu.unity"] (was [] -> caused "export directory is empty"), playerExporter.export=true.
- API key currently used inline; TODO: move to env var UNITY_API_KEY (termux-expert guidance).
- Local tooling: no Unity Editor can run (aarch64; editor binary is x86). Mono (mcs) available for
  local compile validation against real UnityEngine/UnityEditor DLLs in ~/unity-editor/Editor/Data/Managed
  (pure IL, arch-independent).
