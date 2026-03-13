---
name: AI Training Arena — Gas Town Master Agent Orchestration
description: >
  Recursive multi-agent orchestration config for AI Training Arena.
  1 Manager Agent (Mayor) controls 7 Sub-Agents (Polecats) via Gas Town.
  Covers all 4 codebases: Smart Contracts (Solidity), P2P Node (.NET 9),
  Frontend App (Dioxus/Rust/WASM), Marketing Site (Svelte 5).
  Hosted on Azure with MySQL Enterprise Edition.
applyTo: "**/*"
---

# AI TRAINING ARENA — GAS TOWN RECURSIVE ORCHESTRATION

> **CRITICAL**: This file defines the MANDATORY recursive agent loop, agent roles,
> dependency gates, build/test requirements, and venv protocol. All agents MUST
> follow this document. Violations cause immediate task rejection.

---

## 0. GAS TOWN INSTALLATION & PREREQUISITES

Gas Town is installed at: `C:\COneect\gastown`
Executable: `C:\COneect\gastown\gt.exe`

### Required Tools

| Tool | Version | Purpose |
|------|---------|---------|
| `gt` (Gas Town) | Latest from `C:\COneect\gastown\gt.exe` | Agent orchestration |
| `bd` (Beads) | 0.55.4+ | Issue tracking |
| Node.js | 20+ | Marketing site, contract tests |
| .NET SDK | 9.0+ | P2P Node (Codebase B) |
| Rust + wasm-pack | Latest stable | Frontend WASM app (Codebase C) |
| Go | 1.23+ | Gas Town itself |
| Python | 3.12+ | Integration tests, venv |
| Solidity | ^0.8.20 | Smart contracts (Codebase A) |
| Hardhat | 2.x | Contract compilation + testing |
| MySQL | 8.0 Enterprise | Production database (Azure) |
| Docker / Podman | Latest | Containerization |
| tmux | 3.0+ | Agent session management |

### Azure Hosting Requirements

- **Azure Kubernetes Service (AKS)** — P2P Node orchestration
- **Azure Database for MySQL — Enterprise** — All persistent data
- **Azure Static Web Apps** — Marketing site (Codebase D)
- **Azure Container Apps** — Frontend WASM hosting (Codebase C)
- **Azure Key Vault** — Secrets, contract private keys, API keys
- **Azure Monitor + Application Insights** — Telemetry
- **Azure Blob Storage** — IPFS gateway cache, model checkpoints

---

## 1. AGENT TOPOLOGY — 1 MANAGER + 7 SUB-AGENTS

```
╔══════════════════════════════════════════════════════════════════╗
║                    MAYOR (Manager Agent)                         ║
║  Role: Global coordinator, dispatcher, build verifier            ║
║  Reads: This file + master-chapters.md + design-doc.md           ║
║  Controls: All 7 sub-agents via gt sling / gt nudge              ║
║  Runs: Master build loop after each phase                        ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  ┌─ POLECAT-1 (Scaffold Agent)                                  ║
║  │  Phase 0 — MUST complete before all others                    ║
║  │  Scope: Project scaffolds for ALL 4 codebases                ║
║  │  Deliverables: package.json, hardhat.config, .csproj,        ║
║  │    Cargo.toml, vite.config, shared constants, interfaces     ║
║  │                                                               ║
║  ├─ POLECAT-2 (Smart Contracts Agent)                           ║
║  │  Phase 1 — After POLECAT-1 completes                         ║
║  │  Scope: Codebase A chapters SC-1 through SC-5                ║
║  │  Deliverables: All 9 Solidity contracts + Hardhat tests      ║
║  │                                                               ║
║  ├─ POLECAT-3 (P2P Node Core Agent)                             ║
║  │  Phase 1 — After POLECAT-1 completes (parallel with 2)       ║
║  │  Scope: Codebase B — .NET 9 P2P node, core services          ║
║  │  Deliverables: Network layer, battle engine, IPFS adapter    ║
║  │                                                               ║
║  ├─ POLECAT-4 (Frontend WASM Agent)                             ║
║  │  Phase 2 — After POLECAT-1 completes + SC interfaces ready   ║
║  │  Scope: Codebase C — Dioxus/Rust WASM frontend               ║
║  │  Deliverables: All UI components, state management, API layer║
║  │                                                               ║
║  ├─ POLECAT-5 (Marketing Site Agent)                            ║
║  │  Phase 1 — After POLECAT-1 completes (parallel, no deps)     ║
║  │  Scope: Codebase D — Svelte 5 marketing/investor site        ║
║  │  Deliverables: All sections per gastown/ai-training-arena-*  ║
║  │                                                               ║
║  ├─ POLECAT-6 (Infrastructure Agent)                            ║
║  │  Phase 2 — After POLECAT-2 + POLECAT-3 complete              ║
║  │  Scope: Azure infra, MySQL schema, Docker/Podman,            ║
║  │    CI/CD pipelines, monitoring, deployment scripts            ║
║  │                                                               ║
║  └─ POLECAT-7 (Security + Testing Agent)                        ║
║     Phase 3 — Final phase, after all others                      ║
║     Scope: WASM security modules, integration tests, pen tests, ║
║       Python venv test suite, E2E tests, security audit          ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## 2. DEPENDENCY GATES (MANAGER ENFORCES — NO EXCEPTIONS)

```
Phase 0 ──── POLECAT-1 (Scaffold)
              │
              ├──── MUST pass: npm install, dotnet restore, cargo check, npx hardhat compile
              │
Phase 1 ──── POLECAT-2 (Contracts) ─────────── PARALLEL
         ├── POLECAT-3 (P2P Node)   ─────────── PARALLEL
         └── POLECAT-5 (Marketing)  ─────────── PARALLEL
              │
              ├──── GATE: All Phase 1 agents report COMPLETE
              ├──── MANAGER runs MASTER BUILD LOOP (see Section 4)
              │
Phase 2 ──── POLECAT-4 (Frontend WASM) ─────── PARALLEL
         └── POLECAT-6 (Infrastructure) ─────── PARALLEL
              │
              ├──── GATE: All Phase 2 agents report COMPLETE
              ├──── MANAGER runs MASTER BUILD LOOP
              │
Phase 3 ──── POLECAT-7 (Security + Testing)
              │
              ├──── GATE: POLECAT-7 reports COMPLETE
              ├──── MANAGER runs FINAL BUILD LOOP + ALL TESTS
              │
Phase 4 ──── MANAGER: Assembly, integration, deploy prep
```

---

## 3. RECURSIVE AGENT PROTOCOL

Every agent (Manager and all Polecats) follows this recursive loop:

```
╔══════════════════════════════════════════════════════════════════╗
║               RECURSIVE AGENT WORK LOOP (GUPP)                   ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  1. CHECK HOOK — Is there work assigned to me?                   ║
║     └─ If no: IDLE (wait for gt sling / gt nudge)               ║
║     └─ If yes: PROCEED                                           ║
║                                                                  ║
║  2. READ CHAPTER — Load my assigned chapter from master-chapters ║
║     └─ Parse deliverables list                                   ║
║     └─ Parse exact task brief                                    ║
║     └─ Load shared constants (NEVER reinvent)                    ║
║                                                                  ║
║  3. IMPLEMENT — Write code per chapter spec                      ║
║     └─ Follow coding rules (ai-training-arena-rules.md)         ║
║     └─ One file at a time, test as you go                       ║
║     └─ Use $lib imports, respect module boundaries                ║
║                                                                  ║
║  4. BUILD + COMPILE — MANDATORY after every file group           ║
║     └─ Smart Contracts: npx hardhat compile                      ║
║     └─ P2P Node: dotnet build                                    ║
║     └─ Frontend: cargo check && wasm-pack build                  ║
║     └─ Marketing: npm run build                                  ║
║     └─ If build fails: FIX IMMEDIATELY, do not proceed           ║
║                                                                  ║
║  5. RUN TESTS — MANDATORY after build succeeds                   ║
║     └─ Smart Contracts: npx hardhat test                         ║
║     └─ P2P Node: dotnet test                                     ║
║     └─ Frontend: cargo test                                      ║
║     └─ Marketing: npm run build (no runtime tests yet)           ║
║     └─ Python integration: activate venv, pytest                 ║
║     └─ If tests fail: FIX IMMEDIATELY, do not proceed            ║
║                                                                  ║
║  6. REPORT COMPLETION — Signal to Manager                        ║
║     └─ gt nudge mayor "POLECAT-N CHAPTER-X COMPLETE"             ║
║     └─ bd close <bead-id>                                        ║
║                                                                  ║
║  7. LOOP — Go back to step 1                                     ║
║     └─ Check for more work on hook                               ║
║     └─ If no more work: gt nudge mayor "POLECAT-N ALL COMPLETE"  ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## 4. MASTER BUILD LOOP (MANAGER RUNS AFTER EACH PHASE)

The Manager Agent executes this loop after every phase gate:

```bash
#!/bin/bash
# === MASTER BUILD LOOP ===
# Manager runs this after each phase completes
# ALL steps must pass or the phase is REJECTED

set -euo pipefail

echo "=== PHASE GATE: MASTER BUILD LOOP ==="
FAIL=0

# --- Codebase A: Smart Contracts ---
echo "[A] Compiling Smart Contracts..."
cd ai-training-arena-contracts/
npm ci
npx hardhat compile || { echo "FAIL: Hardhat compile"; FAIL=1; }
npx hardhat test || { echo "FAIL: Hardhat tests"; FAIL=1; }
cd ..

# --- Codebase B: P2P Node (.NET 9) ---
echo "[B] Building P2P Node..."
cd ai-training-arena-node/
dotnet restore
dotnet build --configuration Release || { echo "FAIL: dotnet build"; FAIL=1; }
dotnet test --configuration Release --no-build || { echo "FAIL: dotnet test"; FAIL=1; }
cd ..

# --- Codebase C: Frontend WASM (Dioxus/Rust) ---
echo "[C] Building Frontend WASM..."
cd ai-training-arena-frontend/
cargo check || { echo "FAIL: cargo check"; FAIL=1; }
cargo test || { echo "FAIL: cargo test"; FAIL=1; }
wasm-pack build --target web || { echo "FAIL: wasm-pack build"; FAIL=1; }
cd ..

# --- Codebase D: Marketing Site ---
echo "[D] Building Marketing Site..."
cd aitrainingarena.com/
npm ci
npm run build || { echo "FAIL: vite build"; FAIL=1; }
cd ..

# --- Python Integration Tests ---
echo "[PYTHON] Running integration tests..."
cd tests/
source pyenv/bin/activate 2>/dev/null || {
  python -m venv pyenv
  python pyenv/bin/python -m ensurepip --upgrade
  source pyenv/bin/activate
  pip install -r requirements.txt
}
pytest -v --tb=short || { echo "FAIL: Python integration tests"; FAIL=1; }
deactivate
cd ..

# --- Final Verdict ---
if [ $FAIL -ne 0 ]; then
  echo "=== PHASE GATE FAILED — DO NOT PROCEED ==="
  exit 1
fi
echo "=== PHASE GATE PASSED ==="
```

### Windows PowerShell Equivalent

```powershell
# === MASTER BUILD LOOP (Windows) ===
$ErrorActionPreference = "Continue"
$fail = 0

Write-Host "=== PHASE GATE: MASTER BUILD LOOP ===" -ForegroundColor Cyan

# --- Codebase A ---
Push-Location ai-training-arena-contracts
npm ci; if ($LASTEXITCODE -ne 0) { $fail = 1 }
npx hardhat compile; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: Hardhat compile" -ForegroundColor Red }
npx hardhat test; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: Hardhat tests" -ForegroundColor Red }
Pop-Location

# --- Codebase B ---
Push-Location ai-training-arena-node
dotnet restore
dotnet build --configuration Release; if ($LASTEXITCODE -ne 0) { $fail = 1 }
dotnet test --configuration Release --no-build; if ($LASTEXITCODE -ne 0) { $fail = 1 }
Pop-Location

# --- Codebase C ---
Push-Location ai-training-arena-frontend
cargo check; if ($LASTEXITCODE -ne 0) { $fail = 1 }
cargo test; if ($LASTEXITCODE -ne 0) { $fail = 1 }
wasm-pack build --target web; if ($LASTEXITCODE -ne 0) { $fail = 1 }
Pop-Location

# --- Codebase D ---
Push-Location aitrainingarena.com
npm ci; npm run build; if ($LASTEXITCODE -ne 0) { $fail = 1 }
Pop-Location

# --- Python Integration Tests ---
Push-Location tests
if (-not (Test-Path "pyenv")) {
    python -m venv pyenv
    & .\pyenv\Scripts\python.exe -m ensurepip --upgrade
}
& .\pyenv\Scripts\Activate.ps1
pip install -r requirements.txt -q
pytest -v --tb=short; if ($LASTEXITCODE -ne 0) { $fail = 1 }
deactivate
Pop-Location

if ($fail -ne 0) {
    Write-Host "=== PHASE GATE FAILED ===" -ForegroundColor Red
    exit 1
}
Write-Host "=== PHASE GATE PASSED ===" -ForegroundColor Green
```

---

## 5. PYTHON VENV PROTOCOL (MANDATORY)

### Venv Location

```
<project-root>/tests/pyenv/
```

### Activation Rules

1. **EVERY terminal session** that runs Python MUST activate the venv FIRST
2. NEVER install packages globally — always inside the venv
3. If `pyenv` does not exist, CREATE IT before proceeding
4. **Python 3.14+ on Windows**: Run `python -m ensurepip --upgrade` inside the venv after creation

```bash
# Linux/macOS
python -m venv tests/pyenv
source tests/pyenv/bin/activate

# Windows (PowerShell)
python -m venv tests\pyenv
& tests\pyenv\Scripts\python.exe -m ensurepip --upgrade
& tests\pyenv\Scripts\Activate.ps1

# Windows (CMD)
python -m venv tests\pyenv
tests\pyenv\Scripts\python.exe -m ensurepip --upgrade
tests\pyenv\Scripts\activate.bat
```

### Required Packages (tests/requirements.txt)

```
pytest>=8.0
pytest-asyncio>=0.24
pytest-cov>=5.0
pytest-timeout>=2.3
web3>=7.0
eth-brownie>=1.20
mysql-connector-python>=9.0
azure-identity>=1.19
azure-keyvault-secrets>=4.9
azure-storage-blob>=12.24
httpx>=0.27
pydantic>=2.10
python-dotenv>=1.0
```

### Auto-Activate on Terminal Start

Add to your shell profile:

```bash
# ~/.bashrc or ~/.zshrc
if [ -f "tests/pyenv/bin/activate" ]; then
    source tests/pyenv/bin/activate
fi
```

```powershell
# $PROFILE (PowerShell)
if (Test-Path "tests\pyenv\Scripts\Activate.ps1") {
    & tests\pyenv\Scripts\Activate.ps1
}
```

---

## 6. MYSQL ENTERPRISE EDITION — SCHEMA PER MODULE

### Connection Config (.env)

```
MYSQL_HOST=ai-training-arena-mysql.mysql.database.azure.com
MYSQL_PORT=3306
MYSQL_USER=arena_admin@ai-training-arena-mysql
MYSQL_PASSWORD=<from Azure Key Vault>
MYSQL_SSL_MODE=REQUIRED
MYSQL_SSL_CA=/path/to/DigiCertGlobalRootCA.crt.pem
```

### Schema Isolation (One Schema Per Domain Module)

```sql
-- Module: Battles
CREATE SCHEMA IF NOT EXISTS battles;
CREATE TABLE battles.battle_records (...);
CREATE TABLE battles.battle_telemetry (...);
CREATE TABLE battles.elo_history (...);

-- Module: Agents
CREATE SCHEMA IF NOT EXISTS agents;
CREATE TABLE agents.agent_profiles (...);
CREATE TABLE agents.agent_stats (...);
CREATE TABLE agents.model_configs (...);

-- Module: Marketplace
CREATE SCHEMA IF NOT EXISTS marketplace;
CREATE TABLE marketplace.data_listings (...);
CREATE TABLE marketplace.purchases (...);
CREATE TABLE marketplace.access_grants (...);

-- Module: Staking
CREATE SCHEMA IF NOT EXISTS staking;
CREATE TABLE staking.stakes (...);
CREATE TABLE staking.rewards (...);
CREATE TABLE staking.withdrawal_requests (...);

-- Module: Governance
CREATE SCHEMA IF NOT EXISTS governance;
CREATE TABLE governance.proposals (...);
CREATE TABLE governance.votes (...);
CREATE TABLE governance.execution_queue (...);

-- Module: Analytics
CREATE SCHEMA IF NOT EXISTS analytics;
CREATE TABLE analytics.node_metrics (...);
CREATE TABLE analytics.battle_analytics (...);
CREATE TABLE analytics.revenue_reports (...);

-- Module: Identity
CREATE SCHEMA IF NOT EXISTS identity;
CREATE TABLE identity.users (...);
CREATE TABLE identity.api_keys (...);
CREATE TABLE identity.sessions (...);
```

### Cross-Schema Rules

```
╔══════════════════════════════════════════════════════════════════╗
║  EACH MODULE OWNS ITS SCHEMA — SACRED BOUNDARY                  ║
║                                                                  ║
║  FORBIDDEN: Cross-schema foreign keys                            ║
║  FORBIDDEN: Cross-schema joins in queries                        ║
║  FORBIDDEN: Cross-schema transactions                            ║
║                                                                  ║
║  ALLOWED: Denormalized read models                               ║
║  ALLOWED: Event-driven data synchronization                      ║
║  ALLOWED: API calls between modules for data                     ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## 7. DESKTOP + MOBILE APPLICATION (Node Bot Manager)

Users configure, run, and view analytics of their node bots through:

### Desktop App (Blazor WASM + .NET MAUI)

```
ai-training-arena-desktop/
├── Domain/
│   ├── Entities/        (NodeConfig, BotProfile, BattleResult)
│   ├── ValueObjects/    (EloRating, AgentClass, WalletAddress)
│   └── Interfaces/
├── Application/
│   ├── Commands/        (StartNode, StopNode, ConfigureBot)
│   ├── Queries/         (GetBotStatus, GetAnalytics, GetBattleHistory)
│   └── ViewModels/      (DashboardVM, SettingsVM, AnalyticsVM)
├── Infrastructure/
│   ├── NodeBridge/      (localhost WebSocket to P2P Node)
│   ├── AzureClient/     (analytics API, telemetry upload)
│   └── MySqlAdapter/    (local cache + Azure MySQL)
├── Presentation/
│   ├── Pages/           (Dashboard, BotConfig, Analytics, Battles)
│   └── Components/      (Shared Razor components)
└── Platforms/
    ├── Windows/
    ├── macOS/
    └── Android/ + iOS/  (MAUI mobile targets)
```

### App Features

1. **Dashboard** — Live bot status, active battles, Elo rating chart
2. **Bot Configuration** — Select agent class, model, hardware profile
3. **Battle History** — Past battles with replay data, scores
4. **Analytics** — Earnings chart, win rate, Elo progression, data sales
5. **Settings** — Wallet connection, node settings, auto-battle config
6. **Staking Manager** — Stake/unstake ATA, view rewards, claim

---

## 8. WASM SECURITY MODULES

```
╔══════════════════════════════════════════════════════════════════╗
║               WASM SECURITY ARCHITECTURE                         ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  Module: wasm-battle-validator                                   ║
║  Purpose: Client-side battle proof validation                    ║
║  Compiled from: Rust → WASM via wasm-pack                        ║
║  Runs in: Browser sandbox (Codebase C frontend)                  ║
║  Security: Memory-safe, no host filesystem access                ║
║                                                                  ║
║  Module: wasm-crypto-signer                                      ║
║  Purpose: Local transaction signing, key derivation              ║
║  Compiled from: Rust → WASM                                      ║
║  Security: Keys never leave WASM memory boundary                 ║
║  Uses: ring crate for cryptographic operations                   ║
║                                                                  ║
║  Module: wasm-data-integrity                                     ║
║  Purpose: Merkle proof generation/verification                   ║
║  Compiled from: Rust → WASM                                      ║
║  Security: Deterministic, verifiable outputs                     ║
║                                                                  ║
║  Module: wasm-sandbox-runtime                                    ║
║  Purpose: Sandboxed model inference for battle execution         ║
║  Compiled from: Rust → WASM (wasmtime runtime)                   ║
║  Security: Per-agent resource limits (CPU, memory, time)         ║
║  Isolation: Each battle runs in separate WASM instance           ║
║                                                                  ║
╠══════════════════════════════════════════════════════════════════╣
║  RULES:                                                          ║
║  • ALL crypto operations MUST run inside WASM boundary           ║
║  • Private keys NEVER touch JavaScript/managed runtime           ║
║  • Battle proofs MUST be verifiable in both WASM and on-chain    ║
║  • Model execution MUST be sandboxed with hard resource limits   ║
║  • WASM modules MUST be code-signed and hash-verified on load    ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## 9. GAS TOWN COMMANDS REFERENCE

```bash
# Manager starts work
gt convoy create "Phase 0: Scaffold" <bead-ids> --notify mayor

# Assign work to agents
gt sling <bead-id> polecat-1
gt sling <bead-id> polecat-2

# Check agent status
gt nudge polecat-1 "Status report?"

# Agent reports completion
gt nudge mayor "POLECAT-1 PHASE-0 COMPLETE — all scaffolds pass build"

# Manager checks convoy progress
gt convoy status <convoy-id>

# Inter-agent communication
gt nudge polecat-3 "SC interfaces ready — you can start blockchain integration"

# Build verification
gt nudge mayor "MASTER BUILD LOOP: ALL PASS"
```

---

## 10. FAILURE PROTOCOL

```
╔══════════════════════════════════════════════════════════════════╗
║  FAIL FAST — DO NOT PROCEED ON FAILURE                           ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  Build failure:                                                  ║
║  1. Agent stops ALL work                                         ║
║  2. Agent analyzes error output                                  ║
║  3. Agent fixes the issue                                        ║
║  4. Agent re-runs build                                          ║
║  5. If 3 consecutive failures: ESCALATE to Manager               ║
║     gt nudge mayor "ESCALATION: Build failing after 3 attempts"  ║
║                                                                  ║
║  Test failure:                                                   ║
║  1. Agent reads test output carefully                            ║
║  2. Agent fixes the failing test OR the code under test           ║
║  3. NEVER disable, skip, or delete a failing test                ║
║  4. If test is genuinely wrong: gt nudge mayor for review        ║
║                                                                  ║
║  Dependency conflict:                                            ║
║  1. gt nudge <blocking-agent> "Blocked on: <description>"        ║
║  2. Work on non-blocked items from your chapter                   ║
║  3. If blocked >30 min: gt nudge mayor for reassignment          ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```
