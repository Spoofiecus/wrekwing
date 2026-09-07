# Wreck Wing — Project Brief

- Unity 2022.3 Android physics-destruction game ("Wreck Wing"), solo dev on Termux.
- Build exclusively via Unity Build Automation (cloud). Termux = control plane only.
- **Build budget policy (2026-09-06):** 50% of free build minutes consumed. NEVER trigger a cloud build
  unless (a) local compile gate passes (tools/compile-check.sh) AND (b) changes cross-referenced
  against Unity 2022.3 docs. One build = one validated change set.
