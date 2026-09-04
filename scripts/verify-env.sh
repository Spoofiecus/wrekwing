#!/data/data/com.termux/files/usr/bin/sh
# Developer environment verification — run after clone.
# Exit 0 = fully operational. Lists anything missing with fix commands.
set -u
OK=0; BAD=0
chk() { if command -v "$1" >/dev/null 2>&1; then echo "  ✔ $1"; OK=$((OK+1)); else echo "  ✘ $1  — $2"; BAD=$((BAD+1)); fi; }

echo "════════ Wreck Wing environment check ════════"
echo "── Core build chain ──"
chk git      "pkg install git"
chk javac    "pkg install openjdk-17"
chk aapt     "pkg install aapt"
chk d8       "pkg install d8"
chk apksigner "pkg install apksigner"
chk zipalign "pkg install zipalign"
chk python3  "pkg install python"

echo "── Unity (PC-only, optional on phone) ──"
echo "  ℹ Unity Editor required for APK builds of Assets/ (x86_64 PC)"

echo "── SDK components ──"
if [ -f "$HOME/apkbuild/sdk/android.jar" ] || [ -f "$PREFIX/share/java/android.jar" ]; then
  echo "  ✔ android.jar present"; OK=$((OK+1))
else
  echo "  ✘ android.jar — curl -sL -o ~/apkbuild/sdk/android.jar https://raw.githubusercontent.com/Sable/android-platforms/master/android-30/android.jar"
  BAD=$((BAD+1))
fi

echo "── Git gate status ──"
if git rev-parse --git-dir >/dev/null 2>&1; then
  HP=$(git config core.hooksPath || echo "")
  if [ "$HP" = ".githooks" ]; then echo "  ✔ pre-commit gate ACTIVE"; OK=$((OK+1))
  else echo "  ✘ gates inactive — run: bash scripts/setup-hooks.sh"; BAD=$((BAD+1)); fi
else
  echo "  ✘ not a git repo — run: bash scripts/setup-hooks.sh"; BAD=$((BAD+1))
fi

echo "════════════════════════════════════"
if [ $BAD -eq 0 ]; then echo "ALL CLEAR ($OK checks passed). Ship it."; exit 0
else echo "$BAD issue(s) — fix above, then re-run."; exit 1; fi
