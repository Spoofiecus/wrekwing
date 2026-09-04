#!/data/data/com.termux/files/usr/bin/sh
# One-time activation of Wreck Wing quality gates. Safe to re-run.
set -e
cd "$(git rev-parse --show-toplevel 2>/dev/null || echo .)"

# Ensure git repo exists
if ! git rev-parse --git-dir >/dev/null 2>&1; then
  echo "[setup] Initializing git repository..."
  git init -q
fi

# Point git at versioned hooks (auto-works for every future clone)
git config core.hooksPath .githooks
chmod +x .githooks/pre-commit 2>/dev/null || true

# Local env config for Android/Termux line endings
git config core.autocrlf input
git config core.longpaths true 2>/dev/null || true

echo "[setup] Pre-commit gate ACTIVE (core.hooksPath=.githooks)"
echo "[setup] Try it:  git commit --allow-empty -m 'gate test'"
