# Active Context (2026-09-06)

Goal: first green cloud build + APK.

- Build #15: "export directory is empty" -> root cause: playerExporter.sceneList empty. FIXED via API PUT
  (sceneList=[Assets/Scenes/MainMenu.unity]).
- Build #16: CS1002/CS1525 Java for-each in BuildScript. FIXED -> foreach.
- Build #17: CS0246 (Exception/String), CS7036 (CreateFolder 1-arg), CS0234. FIXED -> clean C# rewrite
  (GetAssetOrNull, CreateFolder("Assets","Scenes"), using WreckWing.Scenes).
- Build #18: ONLY CS0234 remained -> caused by 35 hand-authored malformed .meta files (m_ObjectHideFlags:1
  hid scripts from compilation). FIXED: all .meta removed (commit a3d057a). Lesson: never hand-author .meta;
  let Unity generate them.
- NEXT: run tools/compile-check.sh (mono installing); only if COMPILE OK, trigger build #19.

## Update (build gate established)

- Local compile gate now live: tools/compile-check (Roslyn csc 3.9 + netstandard2.1 profile from editor install).
  Usage: bash tools/compile-check -> "COMPILE OK - safe to build" required before ANY cloud build.
- Caught+fixed via gate before spending a build: AssetDatabase.GetAssetOrNull does not exist in 2022.3 ->
  LoadMainAssetAtPath. Commit 6f56422.
- Build #19 triggered (incremental, cleanBuild:false) on 6f56422 after COMPILE OK.
- NOTE: .sh files are POSIX-checked by pre-commit hook -> keep bash scripts extension-less (tools/compile-check).

## Update (build #19 RCA + Android config fixes, commit 99166b6)

- Build #19 FATAL: UnityException - custom gradleTemplate.properties lacked mandatory markers
  (unityStreamingAssets=.unity3d**STREAMING_ASSETS**, **ADDITIONAL_PROPERTIES**, **JVM_HEAP_SIZE**).
- FIXED: markers restored; settings.gradle deleted (wrong filename, Unity expects settingsTemplate.gradle -> inert);
  AndroidManifest replaced with minimal compliant version (no package/uses-sdk/unused required-true features).
- Verified: all markers present, XML valid, manifest lint clean, compile gate OK. NOT built yet - awaiting user approval.
- Facts: editor here is x86-64 ELF (e_machine 0x3E), device aarch64, no ARM64 Linux Unity editor exists ->
  UCB is the only build plane; keep local editor for compile-gate DLLs. Proot ubuntu container exists.
- Game code uses NO vibration/mic/network/UnityServices (grep-verified); INTERNET perm kept for Analytics module.

## Update (docs cross-reference audit, pre-build #20)

- Manifest docs: custom manifest MERGES over Unity Library Manifest; permissions auto-injected (INTERNET via Analytics).
  Minimal manifest is compliant. Gradle docs: unityTemplateVersion/unityProjectPath injected via **ADDITIONAL_PROPERTIES**;
  Unity throws if template version mismatches -> marker mandatory (validated our fix).
- API audit vs editor XML docs: PlayerSettings.Android.min/targetSdkVersion, AndroidSdkLevels 24/33, useCustomKeystore,
  defaultInterfaceOrientation+UIOrientation, AssetDatabase.LoadMainAssetAtPath, EditorBuildSettings.scenes all present;
  ctor/build APIs proven by gate compilation against real DLLs.
- UCB target: bundleId casing fixed Com.wrekwing.game -> com.wreckwing.game (matches PlayerSettings).
  androidSDK android_sdk_35 (build machine) >= targetSdk 33 = compatible. unityVersion latest2022_3 (62f3) opens
  55f1 project (patch-line compatible).
- PLAY SUBMISSION TODO (not build-blocking): bump targetSdkVersion 33 -> 34+ per Play policy at submission time.

## Update (build #20 SUCCEEDED -> APK installed; no-icon bug fixed, commit b512a5f)

- FIRST GREEN BUILD. APK installed on device but no launcher icon.
- RCA: minimal custom AndroidManifest overrode Unity Library Manifest template -> removed <application> icon attr
  and UnityPlayerActivity MAIN/LAUNCHER intent-filter -> installed but no home-screen entry.
- FIX: deleted custom manifest entirely -> Unity default restores icon+launcher. Compile gate OK. Pushed b512a5f.
- NEXT (needs approval): build #21. After: uninstall/reinstall or update-in-place restores icon.
- TODO later: add real game icon (PlayerSettings > Icon; needs icon assets committed) - currently Unity default icon.

## Update (build #21 green APK but launch rejected on device; commit 7addb2a)

- APK installed but showed "This app is not available on the latest version of Android. Check for an update or
  contact the developer" at LAUNCH.
- Device: Android 16 / API 36 / arm64-v8a / security patch 2026-05. Modern Armv9 flagships (Cortex-X4+)
  dropped AArch32 (32-bit) execution entirely -> 64-bit-only devices reject 32-bit-only APKs.
- RCA: downloaded APK was armeabi-v7a-ONLY (no arm64 libunity.so). PlayerSettings.targetArchitectures
  absent in source -> Unity defaulted to ARMv7-only. Manifest itself was fine (minSdk 24, targetSdk 33).
- FIX: PreExport now sets PlayerSettings.Android.targetArchitectures = ARMv7 | ARM64 (Mono backend
  supports arm64, no IL2CPP needed). Gate OK. Pushed 7addb2a. Awaiting build #22 approval.
- NOTE on downloaded APKs: partial dl timed out at 21MB/23.7MB; used curl -C - resume in background.
