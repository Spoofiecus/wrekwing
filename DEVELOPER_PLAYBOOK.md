# 🛩️ Wreck Wing — Developer Playbook

> **Read this before writing a single line of code.** Single source of truth for architecture, workflows, and quality gates. DoD violations get PRs rejected.

## 1. Architecture Map

### 1.1 Repository Topology

```
Wreck Wing/                          ← GIT REPOSITORY ROOT
│
├── Assets/                          ← UNITY PROJECT (primary game, C#)
│   ├── Scripts/
│   │   ├── GameManager.cs           ← ENTRY: state machine singleton
│   │   ├── Core/                    ← GameSettings, ObjectPooler, QualityManager
│   │   ├── Gameplay/                ← Domain: flight, destruction, economy
│   │   │   ├── PlaneController.cs   ←   Flight physics + 4 input schemes
│   │   │   ├── PlaneData.cs         ←   ScriptableObject: stats/unlocks
│   │   │   ├── PlaneSpawner.cs      ←   Spawns planes above stadium
│   │   │   ├── WindSystem.cs        ←   Perlin-noise gusts
│   │   │   ├── DestructionManager.cs←   Impact → chain-reaction collapse
│   │   │   ├── DestructibleStructure.cs ← Load-bearing sections
│   │   │   ├── DebrisObject.cs      ←   Pooled physics chunks
│   │   │   ├── StadiumSection.cs    ←   Health-based section damage
│   │   │   ├── ScoreManager.cs      ←   Score + combo + high score
│   │   │   ├── ProgressionManager.cs←   Currency / XP / unlocks facade
│   │   │   ├── BattlePassManager.cs ←   100-tier seasonal pass
│   │   │   ├── DailyChallengeManager.cs / DailyChallengeData.cs
│   │   │   ├── AchievementManager.cs
│   │   │   ├── SlowMotionEffect.cs / ScreenShake.cs
│   │   │   └── Plane/StadiumVisualController.cs
│   │   ├── UI/                      ← UIManager (router), HUD, MainMenu,
│   │   │                              PauseMenu, GameOver controllers
│   │   ├── Audio/AudioManager.cs    ← Music + SFX buses
│   │   ├── VFX/                     ← VFXManager, VFXQualityScaler
│   │   ├── Data/                    ← CurrencyData, PlayerProgress,
│   │   │                              PlaneUnlockData (PlayerPrefs)
│   │   └── Scenes/SceneSetup.cs     ← EDITOR-ONLY bootstrap
│   │        (menu: WreckWing → Setup Main Menu Scene)
│   └── Plugins/Android/             ← AndroidManifest, Gradle templates
│
├── native-android/                  ← STANDALONE 2D PROTOTYPE (Java)
│   ├── AndroidManifest.xml          ← portrait, minSdk 24, target 30
│   └── src/com/wreckwing/game/
│       ├── MainActivity.java        ← Fullscreen activity host
│       ├── GameView.java            ← Loop: READY→FLYING→IMPACT→GAMEOVER
│       ├── Plane.java               ← Flight + steering + impact force
│       └── Stadium.java             ← Sections, health, debris physics
│
├── ProjectSettings/                 ← Unity player/quality/input/tags
├── Packages/manifest.json           ← URP, Cinemachine, Input System, TMP
├── PROGRESS.md                      ← Live build status
├── DEVELOPER_PLAYBOOK.md            ← THIS FILE
└── .github/TASK_BREAKDOWN_TEMPLATE.md
```

### 1.2 Runtime Data Flow (Unity)

```
   Touch / Gyro ──▶ PlaneController (tilt|swipe|stick|tap)
                          │ velocity, impactForce()
                          ▼
   Collision tag:"Stadium" ──▶ OnCrash() ──▶ OnCrashed event
                          │
                          ▼
              DestructionManager.ProcessImpact()
              ├─ sort structures by distance
              ├─ force ≥ BreakThreshold → BreakStructure()
              │     ├─▶ DebrisObject (via ObjectPooler)
              │     └─▶ VFXManager (dust/fire) + ScreenShake
              └─ chain reaction (delay 0.3s) → cascade collapse
                          │
        OnStructureDestroyed / OnDestructionPercentChanged
                          │
   ┌──────────────┬───────┴────────┬──────────────────┐
   ▼              ▼                ▼                  ▼
ScoreManager  HUDController  ProgressionManager  BattlePass/Daily
(combo mult)  (live HUD)     (coins/XP/unlocks)  (challenges)
```

### 1.3 Build & Deliver Flow

```
 DEVELOP                     BUILD GATE                       SHIP
┌──────────────┐   ┌──────────────────────────┐   ┌────────────────────┐
│ Edit C#/Java │──▶│ git commit → pre-commit  │──▶│ PC + Unity 2022.3  │
│ in repo      │   │ javac | py_compile |     │   │ → open Assets      │
└──────────────┘   │ hygiene checks           │   │ → WreckWing menu   │
        │          └──────────────────────────┘   │ → Build Android APK│
        ▼                                         └─────────┬──────────┘
 native-android/ (Termux)                                   ▼
 javac → d8 → aapt → zipalign                     Copy APK → phone → store
 → apksigner → APK to Downloads
```

### 1.4 Critical Invariants

1. **Namespace lock**: `WreckWing.Gameplay` / `.Core` / `.UI` / `.Data` / `.VFX`; editor code in `WreckWing.Scenes` behind `#if UNITY_EDITOR`.
2. **Event contract**: `ScoreManager.OnScoreChanged(Action<int>)`, `OnComboChanged(Action<float>)`; `DestructionManager.OnDestructionPercentChanged(Action<float>)`; `GameManager.OnGameStateChanged(Action<GameState>)`. Subscribe in `Start()`, **unsubscribe in `OnDestroy()`** — no exceptions.
3. **Singletons** (Instance + DontDestroyOnLoad): GameManager, ScoreManager, DestructionManager, AudioManager, UIManager, VFXManager, ProgressionManager, AchievementManager, DailyChallengeManager, BattlePassManager. Never `new` them; never add duplicates to a scene.
4. **Pooling mandate**: runtime-spawned objects (debris, VFX, popups) go through `ObjectPooler`. Never `Instantiate`/`Destroy` inside `Update()` — GC spikes drop frames on Snapdragon 6xx targets.
5. **File↔Class name lock** (Unity requirement): `PlaneController.cs` contains `class PlaneController`. Enforced by the pre-commit gate.
6. **Vertical-first**: all UI targets 1080×1920 portrait. No landscape code paths.

## 2. Workflow Boilerplates (copy-paste ready)

### 2.1 Spin Up the Local Environment (fresh clone)

```bash
# 1. Clone and enter
git clone <repo-url> "Wreck Wing" && cd "Wreck Wing"

# 2. Activate quality gates (one-time; safe to re-run)
bash scripts/setup-hooks.sh

# 3. Verify your environment (all should pass)
bash scripts/verify-env.sh
```

### 2.2 Add a New Unity Plane (C#)

```bash
PLANE=StealthBomber   # PascalCase, no spaces

# a. Data: plane stats ScriptableObject
cp Assets/Scripts/Gameplay/PlaneData.cs "Assets/Scripts/Gameplay/${PLANE}Data.cs"
#    → edit class name to ${PLANE}Data, adjust stat fields

# b. Validate BEFORE committing:
grep -q "class ${PLANE}Data" "Assets/Scripts/Gameplay/${PLANE}Data.cs" \
  || echo "FAIL: class/file name mismatch"
```

In Unity Editor: right-click Project → Create → WreckWing → PlaneData, fill stats, assign prefab.

### 2.3 Add a New Destructible Structure (C#)

```bash
cat > Assets/Scripts/Gameplay/Floodlight.cs << 'EOF'
using UnityEngine;
namespace WreckWing.Gameplay {
    public class Floodlight : DestructibleStructure { }
}
EOF
# In Unity: add to scene, configure thresholds + connectedStructures in Inspector
```

### 2.4 Add a New Manager (C# singleton)

```bash
cat > Assets/Scripts/Gameplay/MySystem.cs << 'EOF'
using UnityEngine;
namespace WreckWing.Gameplay {
    public class MySystem : MonoBehaviour {
        public static MySystem Instance { get; private set; }
        private void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
EOF
# REQUIRED: register in Scenes/SceneSetup.cs (AddComponent) or it won't exist at runtime
```

### 2.5 Localized Compile Test (native Android)

```bash
cd native-android && mkdir -p build/classes && javac --release 8 \
  -classpath ~/apkbuild/sdk/android.jar -d build/classes $(find src -name "*.java") \
  && echo "JAVA GATE: PASS"
```

## 3. Definition of Done (DoD) — PR Entry Checklist

**A PR may only be opened when EVERY box below is checked by the author.**
Reviewers must verify boxes 1–6 themselves; lying on a checklist = PR closed on sight.

### Gate 0 — Commit Hygiene (AUTOMATED, cannot lie)

- [ ] `git commit` succeeded — the pre-commit gate **physically blocked** any broken state:
  - [ ] Java sources compile (`javac` gate PASS)
  - [ ] No file >10MB, no `.apk`/`.so`/binaries committed to source paths
  - [ ] No secrets/keys in diff (`password|secret|apikey` pattern scan clean)
  - [ ] C# class names match file names
  - [ ] Python compiles / shell scripts pass `bash -n` (if applicable)
  - [ ] Markdown/YAML/JSON formatted with Prettier
- If the hook blocked you: fix the reported file, `git add` again, re-commit. **Never** use `--no-verify` without a lead's explicit written approval in the ticket.

### Gate 1 — Scope & Architecture

- [ ] Code lands in the correct namespace + folder per §1.1 (no "temporary" misplaced files)
- [ ] New MonoBehaviours that persist across scenes are registered in `Scenes/SceneSetup.cs`
- [ ] New runtime-spawned objects use `ObjectPooler` (no raw Instantiate/Destroy in Update)
- [ ] Event subscriptions paired with unsubscriptions in `OnDestroy()`
- [ ] No changes to `Packages/manifest.json` unless the ticket explicitly requires it (call out in PR body)

### Gate 2 — Correctness

- [ ] Feature works in the declared game states: `MainMenu → PlaneSelect → Playing → Paused → GameOver`
- [ ] Pausing mid-action does not break the feature (timeScale = 0 respected)
- [ ] No new compiler warnings introduced (C# console output / javac output clean)
- [ ] Save data changes are backward-compatible (PlayerPrefs keys versioned or additive)
- [ ] Null-safety: all public event raisers tolerate zero subscribers; UI null-checks `Instance`

### Gate 3 — Performance (mobile targets: Snapdragon 6xx, 3GB RAM)

- [ ] No per-frame allocations in `Update()`/`FixedUpdate()` (no `new` in hot paths, no string concat in HUD)
- [ ] Particle/debris counts respect `VFXQualityScaler` budgets
- [ ] No new unbatched draw calls (textures atlas-able, no per-object materials in loops)
- [ ] Verified at Low quality tier (or reasoning provided why tier doesn't apply)

### Gate 4 — UX & Platform

- [ ] Verified in portrait 1080×1920; safe-area respected (no HUD under camera notch)
- [ ] Touch targets ≥ 88px; back button behaves correctly (pause, not quit)
- [ ] Text via TextMeshPro; no legacy UI.Text
- [ ] Haptics/audio respect user volume settings (AudioManager buses only)

### Gate 5 — Reviewability

- [ ] PR title: `TYPE(scope): summary` — e.g. `feat(destruction): floodlight chain reactions`
- [ ] PR body links the task file (from `.github/TASK_BREAKDOWN_TEMPLATE.md`) with all micro-step boxes ticked
- [ ] Diff is ≤ 400 lines OR ticketed as a large change with pre-agreed breakdown
- [ ] No commented-out code, no dead files, no TODO without a ticket reference (`// TODO(WW-123): ...`)
- [ ] `PROGRESS.md` updated if the change alters build/run instructions

### Gate 6 — Final Manual Verification (author runs these)

```bash
# All must pass on your machine before opening the PR:
bash scripts/verify-env.sh                                     # environment OK
cd native-android && javac --release 8 -classpath \
  ~/apkbuild/sdk/android.jar -d /tmp/ww-check $(find src -name "*.java") \
  && echo "JAVA PASS"                                          # Java compiles
git diff --cached --stat                                       # review your own diff first
```

**Failure to complete any gate = PR returned without review. Repeat offenses = pairing session with lead.**

### 2.6 Build the Native APK (Termux, full pipeline)

```bash
cd native-android && mkdir -p build/gen build/classes build
AAPT=aapt; SDK=~/apkbuild/sdk/android.jar; KS=~/apkbuild/smoke/build/debug.keystore
aapt package -f -m -I $SDK -J build/gen -M AndroidManifest.xml -S res
javac --release 8 -classpath $SDK -d build/classes \
  build/gen/com/wreckwing/game/R.java $(find src -name "*.java")
d8 --release --min-api 24 --output build build/classes/com/wreckwing/game/*.class
aapt package -f -F build/unsigned.apk -I $SDK -M AndroidManifest.xml -S res
(cd build && jar -uf unsigned.apk classes.dex && zipalign -f 4 unsigned.apk aligned.apk \
 && apksigner sign --ks $KS --ks-pass pass:android --key-pass pass:android \
 --out WreckWing.apk aligned.apk && apksigner verify WreckWing.apk)
cp build/WreckWing.apk /storage/emulated/0/Download/ && echo "APK in Downloads"
```

### 2.7 Unity Build (PC only — reference)

```
1. Unity Hub → Unity 2022.3 LTS + Android Build Support (+SDK/NDK)
2. Open repo root as project (imports Assets/, Packages/, ProjectSettings/)
3. Menu: WreckWing → Setup Main Menu Scene   (generates MainMenu.unity)
4. Assign prefabs/audio/textures to manager Inspector fields
5. File → Build Settings → Android → Switch Platform → Build
```
