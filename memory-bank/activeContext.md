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
