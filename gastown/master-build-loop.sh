#!/bin/bash
# =============================================================================
# MASTER BUILD LOOP — AI Training Arena
# Run by Mayor after each phase gate AND by Polecats after every 10 tasks
# ALL steps must pass or the phase is REJECTED
# =============================================================================

set -euo pipefail

PROJ_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$PROJ_ROOT"

echo "=== MASTER BUILD LOOP: AI Training Arena ==="
echo "Directory: $PROJ_ROOT"
FAIL=0

# --- Codebase A: Smart Contracts ---
if [ -d "ai-training-arena-contracts" ]; then
  echo ""
  echo "[A] Compiling Smart Contracts..."
  cd "$PROJ_ROOT/ai-training-arena-contracts"
  npm ci --silent || { echo "FAIL: npm ci (contracts)"; FAIL=1; }
  npx hardhat compile || { echo "FAIL: hardhat compile"; FAIL=1; }
  npx hardhat test || { echo "FAIL: hardhat tests"; FAIL=1; }
  cd "$PROJ_ROOT"
else
  echo "[A] SKIP: ai-training-arena-contracts not scaffolded yet"
fi

# --- Codebase B: P2P Node (.NET 9) ---
if [ -d "ai-training-arena-node" ]; then
  echo ""
  echo "[B] Building P2P Node (.NET 9)..."
  cd "$PROJ_ROOT/ai-training-arena-node"
  dotnet restore --verbosity quiet || { echo "FAIL: dotnet restore"; FAIL=1; }
  dotnet build --configuration Release --no-restore || { echo "FAIL: dotnet build"; FAIL=1; }
  dotnet test --configuration Release --no-build || { echo "FAIL: dotnet test"; FAIL=1; }
  cd "$PROJ_ROOT"
else
  echo "[B] SKIP: ai-training-arena-node not scaffolded yet"
fi

# --- Codebase C: Frontend WASM (Dioxus/Rust) ---
if [ -d "ai-training-arena-frontend" ]; then
  echo ""
  echo "[C] Building Frontend WASM..."
  cd "$PROJ_ROOT/ai-training-arena-frontend"
  cargo check || { echo "FAIL: cargo check"; FAIL=1; }
  cargo test || { echo "FAIL: cargo test"; FAIL=1; }
  wasm-pack build --target web || { echo "FAIL: wasm-pack build"; FAIL=1; }
  cd "$PROJ_ROOT"
else
  echo "[C] SKIP: ai-training-arena-frontend not scaffolded yet"
fi

# --- Codebase D: Marketing Site ---
if [ -d "aitrainingarena.com" ]; then
  echo ""
  echo "[D] Building Marketing Site..."
  cd "$PROJ_ROOT/aitrainingarena.com"
  npm ci --silent || { echo "FAIL: npm ci (marketing)"; FAIL=1; }
  npm run build || { echo "FAIL: vite build"; FAIL=1; }
  cd "$PROJ_ROOT"
else
  echo "[D] SKIP: aitrainingarena.com not found"
fi

# --- Python Integration Tests ---
if [ -d "tests" ]; then
  echo ""
  echo "[PYTHON] Running integration tests..."
  cd "$PROJ_ROOT/tests"
  if [ ! -d "pyenv" ]; then
    python -m venv pyenv
    pyenv/bin/python -m ensurepip --upgrade
    pyenv/bin/pip install -r requirements.txt -q
  fi
  source pyenv/bin/activate 2>/dev/null || true
  pytest -v --tb=short || { echo "FAIL: Python integration tests"; FAIL=1; }
  deactivate 2>/dev/null || true
  cd "$PROJ_ROOT"
else
  echo "[PYTHON] SKIP: tests/ not created yet"
fi

echo ""
echo "============================================"
if [ $FAIL -ne 0 ]; then
  echo "=== PHASE GATE FAILED — DO NOT PROCEED ==="
  echo "============================================"
  exit 1
fi
echo "=== PHASE GATE PASSED ==="
echo "============================================"
