# Progress

- Code: all game scripts compile clean on UCB (only BuildScript ever errored). Scene/UI managers present.
- Builds 8-18: all failed. Root causes found+fixed: empty sceneList (config), 3 waves of C# errors in
  BuildScript, malformed .meta batch.
- Local compile gate: script ready (tools/compile-check.sh), mono install in progress.
- Known issues: none open besides pending build #19.
- TODO: move API key to env var; commit tools/compile-check.sh; update docs/ via documentation-keeper.

- Pre-build validation: DONE (tools/compile-check, mono/Roslyn installed). Policy: no cloud build without COMPILE OK.
- Build #19: queued on 6f56422 (first build passing local gate).
