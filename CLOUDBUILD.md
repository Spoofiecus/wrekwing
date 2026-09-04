# ☁️ Unity Cloud Build — Ship the APK Without a PC

This project is **fully configured for Unity Cloud Build**. Cloud Build compiles your
Repo **in Unity's data centers** — no CPU/RAM load on your phone, and it produces a
real signed Android APK. You only need a browser + a free Unity account.

> The only prerequisite inside the repo: `Assets/Editor/BuildScript.cs` exists.
> It auto-generates the scene, registers Build Settings, and builds `WreckWing.apk`.
> You do NOT need Unity installed anywhere.

---

## Part 1 — Push the repo to GitHub (5 min)

1. Create a **free GitHub account** (github.com) if you don't have one.
2. GitHub → **New repository** → name it `wreckwing` → **Private** (or Public) →
   **Do NOT** tick "Add a README" (we already have one) → Create.
3. Copy the repo HTTPS or SSH URL shown on the empty repo page (e.g.
   `https://github.com/YOU/wreckwing.git`).
4. On your phone (Termux), still in the project folder:

```bash
cd "$HOME/Wreck Wing"
git remote add origin <THE-URL-YOU-COPIED>
git push -u origin master
```

- If it asks for credentials, GitHub now wants a **Personal Access Token (PAT)**:
  GitHub → Settings → Developer settings → Personal access tokens → Generate new
  (classic), tick `repo`, copy the `ghp_...` string, use it as the password.
- Then push again.

---

## Part 2 — Create a Unity Cloud Build (10 min)

1. Go to **cloud.unity.com** → sign in (free **Unity Personal** — no credit card).
2. Click **New project** → pick your Unity Cloud Project (create one if asked).
3. **Build Targets** → **Add build target**:
   - **Platform:** Android
   - **Source:** your new GitHub repo (`YOU/wreckwing`)
   - Authorize Unity to read the repo (OAuth flow in the browser).
   - **Scene:** leave default.
   - **Build command:** set to **Build with custom script**, method:
     ```
     WreckWing.EditorTools.BuildScript.BuildAndroid
     ```
4. **Keystore:** leave **Automatic** (debug-signed is fine for installs/Play Console
   internal testing).
5. Click **Start Build** → Unity spins up a cloud Linux VM, runs our `BuildScript`,
   and emails you when it finishes (~10–20 min first time).

---

## Part 3 — Get the APK (2 min)

- **Builds** tab → your successful build → **Download** → you get
  `WreckWing.apk` (arm64 + all ABIs).
- Copy to your phone → install (allow "install unknown apps") — or upload to
  **Google Play Console** via the dashboard.

---

## Tuning Your Build

| Setting                               | Where                                                                           |
| ------------------------------------- | ------------------------------------------------------------------------------- |
| Version, icons, orientation           | `ProjectSettings/PlayerSettings.asset` (already portrait, `com.wreckwing.game`) |
| Quality tiers (Low/Med/High)          | `ProjectSettings/QualitySettings.asset`                                         |
| Which scenes build                    | `Assets/Editor/BuildScript.cs` (line: `new[]{ ScenePath }`)                     |
| Gameplay tuning (gravity, thresholds) | `Assets/Scripts/Core/GameSettings.cs`                                           |

> **Tip:** every push to `master` can auto-trigger a new build (toggle in the
> Build Target's settings). Free Personal tier includes a monthly build-minute
> allowance — plenty for iterative dev.

---

## Troubleshooting

- **Build failed / compile errors**: read the log in the Build detail view. Common
  cause = a C# file added on a branch not yet merged. Our pre-commit gate
  (`.githooks/pre-commit`) catches class/file mismatches before they ever reach git.
- **"No Android module"**: Cloud Build includes Android support automatically — the
  only Android files Unity needs at runtime are provided by the build VM.
- **Scene missing**: `BuildScript` calls `SceneSetup.SetupMainMenuScene()` on first
  run, so a bare checkout still builds. Do not delete `Assets/Editor/`.
