#!/bin/bash
# =============================================================================
# AI Training Arena — Single Command Full Launch
# Usage: bash start-arena.sh
#
# Opens 5 Windows Terminal tabs:
#   MAYOR      — orchestrates all phases, runs build gates
#   SCAFFOLD   — Phase 0: scaffolds all 4 codebases
#   CONTRACTS  — Phase 1: Solidity smart contracts
#   NODE       — Phase 1: .NET 9 P2P node
#   FRONTEND   — Phase 2: Dioxus/Rust WASM app
# =============================================================================

set -euo pipefail

PROJ="C:\\Users\\David48\\Downloads\\code\\ai-training-arena"
PROJ_BASH="/c/Users/David48/Downloads/code/ai-training-arena"
GT="/c/Users/David48/.local/bin/gt.exe"
BD="/c/Users/David48/go/bin/bd"

echo "=== AI Training Arena — Full Launch ==="
echo ""

# ─── Step 1: Install formula to HQ if not installed ──────────────────────────
echo "[1/3] Installing arena-polecat-work formula..."
FORMULA_SRC="$PROJ_BASH/gastown/arena-polecat-work.formula.toml"
FORMULA_DST="/c/Users/David48/gt/.beads/formulas/arena-polecat-work.formula.toml"

mkdir -p "/c/Users/David48/gt/.beads/formulas"
if [ ! -f "$FORMULA_DST" ]; then
  cp -f "$FORMULA_SRC" "$FORMULA_DST"
  echo "  Installed formula."
else
  echo "  Formula already installed."
fi

# ─── Step 2: Bootstrap beads (idempotent — skips if beads already exist) ─────
echo ""
echo "[2/3] Bootstrapping beads..."
cd "$PROJ_BASH"
EXISTING=$($BD ready 2>/dev/null | wc -l | tr -dc '0-9' || echo "0")
EXISTING=${EXISTING:-0}
if [ "$EXISTING" -le 2 ]; then
  echo "  Running setup-beads.sh to create all 18 chapter beads..."
  bash gastown/setup-beads.sh
else
  echo "  Beads already exist ($EXISTING items). Skipping setup."
  $BD ready 2>/dev/null | head -20
fi

# ─── Step 3: Open Windows Terminal with 5 agent tabs ─────────────────────────
echo ""
echo "[3/3] Launching 5 agent windows..."
echo ""

BASH="C:\\Program Files\\Git\\bin\\bash.exe"

# Each tab: wt opens bash running the script directly — no inline semicolons
wt.exe -w 0 new-tab --title "MAYOR"     -- "$BASH" -l "$PROJ\\mayor\\run-loop.sh"
sleep 0.5
wt.exe -w 0 new-tab --title "SCAFFOLD"  -- "$BASH" -l "$PROJ\\polecats\\scaffold\\run-loop.sh"
sleep 0.5
wt.exe -w 0 new-tab --title "CONTRACTS" -- "$BASH" -l "$PROJ\\polecats\\contracts\\run-loop.sh"
sleep 0.5
wt.exe -w 0 new-tab --title "NODE"      -- "$BASH" -l "$PROJ\\polecats\\node\\run-loop.sh"
sleep 0.5
wt.exe -w 0 new-tab --title "FRONTEND"  -- "$BASH" -l "$PROJ\\polecats\\frontend\\run-loop.sh"

echo "All 5 agent windows launched."
echo ""
echo "  MAYOR      — orchestrates phases, runs build gates"
echo "  SCAFFOLD   — Phase 0"
echo "  CONTRACTS  — Phase 1A (parallel)"
echo "  NODE       — Phase 1B (parallel)"
echo "  FRONTEND   — Phase 2"
echo ""
echo "Monitor progress:  gt.exe ready"
echo "Watch builds:      bash gastown/master-build-loop.sh"
