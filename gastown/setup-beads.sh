#!/bin/bash
# =============================================================================
# AI Training Arena — Full Beads Setup Script
# Creates all chapter beads, sets dependencies, assigns to polecats
# Run from: C:\Users\David48\Downloads\code\ai-training-arena
# =============================================================================

BD="/c/Users/David48/go/bin/bd"
GT="/c/COneect/gastown/gt.exe"

echo "=== AI Training Arena: Beads Setup ==="
echo ""

# ─────────────────────────────────────────────────────────────────────────────
# PHASE 0 — SCAFFOLD (polecat: scaffold)
# ─────────────────────────────────────────────────────────────────────────────

echo "[Phase 0] Creating scaffold beads..."

SC0=$($BD create \
  --title "SC-0: Scaffold all 4 codebases" \
  --description "POLECAT-1: Create foundational project scaffolds for all 4 codebases. Hardhat contracts, .NET 9 solution, Dioxus/Rust frontend, Svelte marketing site. All must compile before Phase 1 starts. See gastown/master-chapters.md CHAPTER SC-0 for full spec." \
  --type feature --priority 0 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $SC0 (scaffold all 4 codebases)"

# ─────────────────────────────────────────────────────────────────────────────
# PHASE 1A — SMART CONTRACTS (polecat: contracts)
# ─────────────────────────────────────────────────────────────────────────────

echo ""
echo "[Phase 1A] Creating contracts beads..."

SC1=$($BD create \
  --title "SC-1: ATAToken + AgentNFT" \
  --description "POLECAT-2: Implement ATAToken.sol (ERC-20, 100M supply, burn/mint roles) and AgentNFT.sol (ERC-721, 5 classes A-E, max supply per class, Elo system). OpenZeppelin 5.x. 100% hardhat test coverage. See master-chapters.md CHAPTER SC-1." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $SC1 (ATAToken + AgentNFT)"

SC2=$($BD create \
  --title "SC-2: AITrainingArena battle orchestrator" \
  --description "POLECAT-2: Implement AITrainingArena.sol — main battle orchestrator. Handles matchmaking calls, battle initiation, round lifecycle, reward distribution (2% burn on each reward). ELO update triggers. MINTs battle rewards via ATAToken. Checks-Effects-Interactions. See master-chapters.md CHAPTER SC-2." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $SC2 (AITrainingArena)"

SC3=$($BD create \
  --title "SC-3: WrappedATA + BattleVerifier" \
  --description "POLECAT-2: WrappedATA.sol — temporary wATA stake wrapper for battle access (class minimums: A=100, B=500, C=2000, D=8000, E=30000 wATA). BattleVerifier.sol — ZK battle proof submission, challenge window, oracle dispute resolution. See master-chapters.md CHAPTER SC-3." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $SC3 (WrappedATA + BattleVerifier)"

SC4=$($BD create \
  --title "SC-4: DataMarketplace + MatchmakingRegistry" \
  --description "POLECAT-2: DataMarketplace.sol — IPFS-backed training data listing, purchase, access control. 5% protocol fee on sales. MatchmakingRegistry.sol — P2P node registration, availability announcements, ELO-bracket opponent discovery. See master-chapters.md CHAPTER SC-4." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $SC4 (DataMarketplace + MatchmakingRegistry)"

SC5=$($BD create \
  --title "SC-5: AIArenaGovernor + FounderRevenue" \
  --description "POLECAT-2: AIArenaGovernor.sol — OpenZeppelin Governor DAO. 1 ATA=1 vote, staked=2x, staked+NFT=3x. 100k ATA proposal threshold, 10M quorum, 7-day voting, 1-day delay. FounderRevenue.sol — 3/5 Gnosis Safe multi-sig, weekly 5% buyback+burn. See master-chapters.md CHAPTER SC-5." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $SC5 (AIArenaGovernor + FounderRevenue)"

# ─────────────────────────────────────────────────────────────────────────────
# PHASE 1B — P2P NODE (polecat: node)
# ─────────────────────────────────────────────────────────────────────────────

echo ""
echo "[Phase 1B] Creating P2P node beads..."

N1=$($BD create \
  --title "N-1: .NET 9 solution scaffold" \
  --description "POLECAT-3: Create AITrainingArena.sln with 6 projects: Domain, Application, Infrastructure, API, BattleEngine, Blockchain. Directory.Build.props with shared versions. dotnet restore && dotnet build must pass. See master-chapters.md PART 2." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $N1 (node scaffold)"

N2=$($BD create \
  --title "N-2: Domain layer — entities, value objects, events" \
  --description "POLECAT-3: Implement domain entities: Agent, Battle, Stake. Value objects: EloRating, AgentClass, WalletAddress, BattleProof. Domain events: BattleCompleted, AgentRegistered, StakeCreated. Interfaces (ports): IBattleRepository, IAgentRepository, IBlockchainBridge. Zero external dependencies. See master-chapters.md N-2." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $N2 (domain layer)"

N3=$($BD create \
  --title "N-3: Application layer — CQRS commands, queries, handlers" \
  --description "POLECAT-3: MediatR CQRS. Commands: StartBattle, RegisterAgent, StakeTokens, ClaimRewards. Queries: GetBattleHistory, GetLeaderboard, GetAgentStats. FluentValidation validators. AutoMapper DTOs. Depends only on Domain. See master-chapters.md N-3." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $N3 (application layer)"

N4=$($BD create \
  --title "N-4: Infrastructure — MySQL, blockchain bridge, IPFS, NATS" \
  --description "POLECAT-3: EF Core + Pomelo MySQL (one schema per domain module, no cross-schema FKs). Nethereum blockchain bridge. IPFS adapter (Kubo HTTP API). NATS JetStream publisher. Azure Key Vault secret provider. dotnet test >= 80% coverage. See master-chapters.md N-4." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $N4 (infrastructure layer)"

N5=$($BD create \
  --title "N-5: API layer — REST, gRPC, WebSocket" \
  --description "POLECAT-3: ASP.NET Core REST controllers (thin, delegate to MediatR). gRPC services for inter-node communication. SignalR WebSocket hub for real-time battle updates. Health check endpoints (/health/live, /health/ready). See master-chapters.md N-5." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $N5 (API layer)"

N6=$($BD create \
  --title "N-6: Battle engine — Dr. Zero Proposer-Solver + ELO" \
  --description "POLECAT-3: Dr. Zero Proposer-Solver implementation. ELO calculator (K=40 <30 battles, K=20 >=30, per-class). Battle proof generator (Merkle tree). Model inference orchestrator. 3-hour rounds, 25-min battles, 5-min cooldown, 8 rounds/day. See master-chapters.md N-6." \
  --type feature --priority 1 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $N6 (battle engine)"

# ─────────────────────────────────────────────────────────────────────────────
# PHASE 2 — FRONTEND WASM (polecat: frontend) — needs SC-1 interfaces
# ─────────────────────────────────────────────────────────────────────────────

echo ""
echo "[Phase 2] Creating frontend beads..."

F1=$($BD create \
  --title "F-1: Dioxus/Rust WASM scaffold" \
  --description "POLECAT-4: Create ai-training-arena-frontend/ with Cargo.toml, Dioxus.toml, tailwind.config.js. Dioxus 0.5+ with WASM target. wasm-pack build --target web must pass. cargo check must pass. See master-chapters.md F-1." \
  --type feature --priority 2 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $F1 (WASM scaffold)"

F2=$($BD create \
  --title "F-2: State management + API client" \
  --description "POLECAT-4: Fermi atoms/signals state: WalletState, BattleState, AgentState, AppState. REST client for P2P node API. WebSocket subscription for real-time battle updates. Contract ABI calls via web3/ethers-rs. See master-chapters.md F-2." \
  --type feature --priority 2 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $F2 (state + API client)"

F3=$($BD create \
  --title "F-3: Core UI components" \
  --description "POLECAT-4: Reusable Dioxus components: AgentCard (NFT display with class/Elo/stats), BattleGrid (2D grid of active battles), Leaderboard (top agents by Elo), WalletConnect (MetaMask/WalletConnect), StakingPanel (stake/unstake/rewards), BattleReplay (step-by-step visualization). See master-chapters.md F-3." \
  --type feature --priority 2 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $F3 (UI components)"

F4=$($BD create \
  --title "F-4: Pages — Home, Mint, Staking, Battles, Marketplace, Governance, Dashboard" \
  --description "POLECAT-4: All 7 app pages using Dioxus Router. Home (hero + live battles), Mint (agent class selection + cost), Staking (stake/unstake + rewards), Battles (matchmaking + spectating), Marketplace (data listings), Governance (proposals + voting), Dashboard (user stats + earnings). See master-chapters.md F-4." \
  --type feature --priority 2 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $F4 (pages)"

F5=$($BD create \
  --title "F-5: WASM security modules" \
  --description "POLECAT-4: 4 WASM security modules compiled from Rust: wasm-battle-validator (client-side battle proof validation), wasm-crypto-signer (local tx signing, keys never in JS), wasm-data-integrity (Merkle proof gen/verify), wasm-sandbox-runtime (sandboxed model inference per battle). All code-signed and hash-verified on load. See master-chapters.md F-5." \
  --type feature --priority 2 \
  --json 2>/dev/null | python3 -c "import sys,json; print(json.load(sys.stdin).get('id',''))" 2>/dev/null)

echo "  Created: $F5 (WASM security modules)"

# ─────────────────────────────────────────────────────────────────────────────
# DEPENDENCY GRAPH
# ─────────────────────────────────────────────────────────────────────────────

echo ""
echo "[Dependencies] Wiring dependency graph..."

# Everything in Phase 1 depends on scaffold
$BD dep add "$SC1" "$SC0" 2>/dev/null && echo "  $SC0 → $SC1"
$BD dep add "$SC2" "$SC0" 2>/dev/null && echo "  $SC0 → $SC2"
$BD dep add "$SC3" "$SC0" 2>/dev/null && echo "  $SC0 → $SC3"
$BD dep add "$SC4" "$SC0" 2>/dev/null && echo "  $SC0 → $SC4"
$BD dep add "$SC5" "$SC0" 2>/dev/null && echo "  $SC0 → $SC5"
$BD dep add "$N1"  "$SC0" 2>/dev/null && echo "  $SC0 → $N1"
$BD dep add "$N2"  "$N1"  2>/dev/null && echo "  $N1  → $N2"
$BD dep add "$N3"  "$N2"  2>/dev/null && echo "  $N2  → $N3"
$BD dep add "$N4"  "$N3"  2>/dev/null && echo "  $N3  → $N4"
$BD dep add "$N5"  "$N4"  2>/dev/null && echo "  $N4  → $N5"
$BD dep add "$N6"  "$N5"  2>/dev/null && echo "  $N5  → $N6"

# Contracts chain
$BD dep add "$SC2" "$SC1" 2>/dev/null && echo "  $SC1 → $SC2"
$BD dep add "$SC3" "$SC2" 2>/dev/null && echo "  $SC2 → $SC3"
$BD dep add "$SC4" "$SC3" 2>/dev/null && echo "  $SC3 → $SC4"
$BD dep add "$SC5" "$SC4" 2>/dev/null && echo "  $SC4 → $SC5"

# Frontend needs scaffold + SC interfaces
$BD dep add "$F1" "$SC0" 2>/dev/null && echo "  $SC0 → $F1"
$BD dep add "$F2" "$F1"  2>/dev/null && echo "  $F1  → $F2"
$BD dep add "$F2" "$SC1" 2>/dev/null && echo "  $SC1 → $F2 (interfaces)"
$BD dep add "$F3" "$F2"  2>/dev/null && echo "  $F2  → $F3"
$BD dep add "$F4" "$F3"  2>/dev/null && echo "  $F3  → $F4"
$BD dep add "$F5" "$F4"  2>/dev/null && echo "  $F4  → $F5"

# ─────────────────────────────────────────────────────────────────────────────
# ASSIGN TO POLECATS
# ─────────────────────────────────────────────────────────────────────────────

echo ""
echo "[Assignments] Assigning beads to polecats..."

$BD update "$SC0" --assignee "arena/polecats/scaffold" 2>/dev/null && echo "  scaffold ← $SC0"
$BD update "$SC1" --assignee "arena/polecats/contracts" 2>/dev/null && echo "  contracts ← $SC1"
$BD update "$SC2" --assignee "arena/polecats/contracts" 2>/dev/null && echo "  contracts ← $SC2"
$BD update "$SC3" --assignee "arena/polecats/contracts" 2>/dev/null && echo "  contracts ← $SC3"
$BD update "$SC4" --assignee "arena/polecats/contracts" 2>/dev/null && echo "  contracts ← $SC4"
$BD update "$SC5" --assignee "arena/polecats/contracts" 2>/dev/null && echo "  contracts ← $SC5"
$BD update "$N1"  --assignee "arena/polecats/node" 2>/dev/null && echo "  node ← $N1"
$BD update "$N2"  --assignee "arena/polecats/node" 2>/dev/null && echo "  node ← $N2"
$BD update "$N3"  --assignee "arena/polecats/node" 2>/dev/null && echo "  node ← $N3"
$BD update "$N4"  --assignee "arena/polecats/node" 2>/dev/null && echo "  node ← $N4"
$BD update "$N5"  --assignee "arena/polecats/node" 2>/dev/null && echo "  node ← $N5"
$BD update "$N6"  --assignee "arena/polecats/node" 2>/dev/null && echo "  node ← $N6"
$BD update "$F1"  --assignee "arena/polecats/frontend" 2>/dev/null && echo "  frontend ← $F1"
$BD update "$F2"  --assignee "arena/polecats/frontend" 2>/dev/null && echo "  frontend ← $F2"
$BD update "$F3"  --assignee "arena/polecats/frontend" 2>/dev/null && echo "  frontend ← $F3"
$BD update "$F4"  --assignee "arena/polecats/frontend" 2>/dev/null && echo "  frontend ← $F4"
$BD update "$F5"  --assignee "arena/polecats/frontend" 2>/dev/null && echo "  frontend ← $F5"

# ─────────────────────────────────────────────────────────────────────────────
# SUMMARY
# ─────────────────────────────────────────────────────────────────────────────

echo ""
echo "=== SETUP COMPLETE ==="
echo ""
echo "Created beads:"
echo "  Phase 0 (scaffold):   $SC0"
echo "  Phase 1A (contracts): $SC1 $SC2 $SC3 $SC4 $SC5"
echo "  Phase 1B (node):      $N1 $N2 $N3 $N4 $N5 $N6"
echo "  Phase 2 (frontend):   $F1 $F2 $F3 $F4 $F5"
echo ""
echo "To start work:"
echo "  cd ~/gt"
echo "  gt sling $SC0 arena/polecats/scaffold --create --formula arena-polecat-work"
echo ""
echo "After scaffold completes, launch Phase 1 in parallel:"
echo "  gt sling $SC1 arena/polecats/contracts --formula arena-polecat-work"
echo "  gt sling $N1  arena/polecats/node      --formula arena-polecat-work"
echo ""
$BD list 2>/dev/null | head -30
