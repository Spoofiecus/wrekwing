# 🔬 Task Breakdown Template — Wreck Wing

> **How to use:** Copy this file to `tasks/WW-<number>-<slug>.md` for every feature ticket.
> **Rule:** A task is NOT allowed into a sprint until every micro-step has a file path, precise action, and a _terminal-provable_ validation command. "It should work" is not a validation step.
> **Rule:** If a micro-step takes >2 hours, it is not a micro-step. Split it.

---

## Ticket Header

| Field                  | Value                                                  |
| ---------------------- | ------------------------------------------------------ |
| **Ticket ID**          | WW-\_\_\_\_                                            |
| **Title**              | (one sentence, verb-first)                             |
| **Requested by**       | (name / ticket link)                                   |
| **Estimate**           | (total hours — must equal sum of micro-steps)          |
| **Priority**           | P0 blocker / P1 this release / P2 next / P3 backlog    |
| **Depends on**         | (WW-\_\_\_ or none)                                    |
| **Definition of Done** | Link: `DEVELOPER_PLAYBOOK.md §3` — all gates must pass |

## Impact Contract (fill before coding)

- **Files created:** (exact paths)
- **Files modified:** (exact paths)
- **Public APIs added/changed:** (signatures)
- **Events fired:** (name, payload, who subscribes)
- **Perf budget impact:** (allocations/frame, draw calls, particles at Low tier)
- **Save-data migration needed:** yes/no (if yes → version + migration step required)

---

## Micro-Step Breakdown

> Fill one block per micro-step. Do the steps **in order**. Never proceed to the
> next step while the current validation command fails.

### Step 1 — (imperative title)

- **File(s):** `Assets/Scripts/...` or `native-android/src/...`
- **Action:** (the precise edit: paste the function signature / config line / command)
- **Validate:**
  ```bash
  # exact terminal command + expected output
  ```
- **Works?** ☐

### Step 2 — (imperative title)

- **File(s):** ...
- **Action:** ...
- **Validate:**
  ```bash
  ...
  ```
- **Works?** ☐

### Step 3 — Editor-only steps (Unity tasks only)

- **In Unity Editor:** (menu path / Inspector fields to set / prefab wiring)
- **Validate:** (what you visually see in Play Mode; screenshot pasted in ticket)
- **Works?** ☐

_(add steps as needed — target 4–10 steps total)_

---

## Regression Sweep (run before opening PR)

```bash
# 1. Full gate (same as pre-commit, standalone):
bash .githooks/pre-commit || true   # after staging: expect all PASS

# 2. Java prototype compiles + APK builds:
cd native-android && <paste §2.5/2.6 commands from DEVELOPER_PLAYBOOK>

# 3. Manual smoke matrix (tick each):
#    ☐ MainMenu loads    ☐ Fly + steer     ☐ Crash + shake
#    ☐ Score increments  ☐ Combo shows     ☐ GameOver → retry
#    ☐ Pause/resume      ☐ Back button     ☐ Low-tier device OK
```

## Rollback Plan

- Revert command: `git revert <sha>` — safe because (state why: no schema change / additive PlayerPrefs / etc.)
- If save-data migration: describe downgrade behavior.

## Retrospective (fill after merge)

- Estimate vs actual: **_h vs _**h
- Steps that were wrongly scoped: (list — feeds future estimation)
- Guardrail gaps found: (propose new gate to add to `.githooks/pre-commit`)
