# AI TRAINING ARENA — MASTER PLATFORM CHAPTER BREAKDOWN
## Full Application · Gas Town Recursive Orchestration · All Codebases
### Version 1.0 — Complete Platform Coverage

---

> **CRITICAL NOTICE TO ROOT MANAGER AGENT:**
> The previous documents (`ai-training-arena-design-doc.md` and `gastown-chapters.md`) covered ONLY the marketing website. This document covers the ENTIRE platform across 4 codebases. Read this document in full before dispatching any sub-manager or agent. Every file in every codebase is enumerated here. Nothing should be invented or inferred.

---

## PART 0 — PLATFORM ARCHITECTURE OVERVIEW

The AI Training Arena platform consists of **4 independent codebases** that must be built and integrated:

```
┌─────────────────────────────────────────────────────────────────┐
│  CODEBASE A: SMART CONTRACTS  (Solidity / Hardhat)              │
│  9 contracts + deployment scripts + tests                        │
│  Deployed to: Mantle Network (EVM)                               │
└─────────────────────────────────────────────────────────────────┘
                              ↕ ABI / Nethereum
┌─────────────────────────────────────────────────────────────────┐
│  CODEBASE B: P2P NODE  (.NET 9 / C#)                            │
│  22 classes across 6 projects + Docker                           │
│  Runs on: Each user's local machine (Windows/Linux/macOS)        │
└─────────────────────────────────────────────────────────────────┘
                              ↕ WebSocket (localhost)
┌─────────────────────────────────────────────────────────────────┐
│  CODEBASE C: FRONTEND APP  (Dioxus / Rust / WASM)               │
│  15 source files + state + API layer                             │
│  Runs in: Browser (WebAssembly) — connects to local P2P Node     │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  CODEBASE D: MARKETING SITE  (Vite + Svelte 5)                  │
│  25 components (see: gastown-chapters.md for full detail)        │
│  Deployed to: Vercel / Cloudflare Pages                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## PART 0A — RECURSIVE MANAGER STRUCTURE

```
ROOT MANAGER
├── reads: this document (master-chapters.md)
├── reads: ai-training-arena-design-doc.md
├── reads: gastown-chapters.md
│
├── SPAWNS: Team Manager A  → Smart Contracts (6 agents)
├── SPAWNS: Team Manager B  → P2P Node (.NET) (6 agents)
├── SPAWNS: Team Manager C  → Frontend App (Dioxus) (6 agents)
└── SPAWNS: Team Manager D  → Marketing Site (Svelte) (6 agents)
           └── (Team Manager D uses gastown-chapters.md as its brief)

Team Manager A reads: PART 1 of this document + shared constants
Team Manager B reads: PART 2 of this document + shared interfaces
Team Manager C reads: PART 3 of this document + shared interfaces
Team Manager D reads: gastown-chapters.md (already complete)
```

**Dependency gates (ROOT MANAGER enforces):**
- Team A (contracts) must complete SC-1 (interfaces) before Teams B and C start their blockchain integration layers
- Team B must complete N-1 (node scaffold) before N-2 through N-6
- Team C must complete F-1 (Rust scaffold) before F-2 through F-6
- Team D has no external dependencies — can start immediately

---

## PART 0B — SHARED DATA MODELS
### Root Manager injects this block into ALL team manager briefs

Every codebase references these canonical data structures. Teams must not invent their own — use these exactly.

```
═══════════════════════════════════════════════════════
CANONICAL SHARED DATA MODELS — INJECT INTO ALL AGENTS
═══════════════════════════════════════════════════════

Agent Classes: A (3B-7B), B (7B-32B), C (32B-70B), D (70B-405B), E (405B+)
NFT Supply:    A=15000, B=6000, C=2500, D=1200, E=300. Total=25000
NFT Prices:    A=10 MNT, B=50 MNT, C=200 MNT, D=800 MNT, E=3000 MNT
Multipliers:   A=1.0x, B=1.2x, C=1.5x, D=2.0x, E=3.0x
Base Rewards:  A=0.041 ATA, B=0.049 ATA, C=0.062 ATA, D=0.082 ATA, E=0.123 ATA

ATA Token:     100,000,000 total supply, ERC-20, 18 decimals
wATA Stake:    A=100, B=500, C=2000, D=8000, E=30000 (wATA required)
Daily Pool:    8,219 ATA/day battle rewards, 8,219 ATA/day staking rewards
Burn Rate:     2% of every battle reward burned
Buyback:       5% of weekly protocol revenue → buy + burn

Round Timing:  3-hour major rounds, 25-min battles, 5-min cooldown, 8 rounds/day
Battle Roles:  PROPOSER (generates questions) vs SOLVER (answers)
Elo System:    Start 1500, K=40 (<30 battles), K=20 (>=30), per-class

IPFS Paths:
  User data:   ipfs://QmUser{nftId}/{battleId}/  (encrypted, owner only)
  DAO data:    ipfs://QmDAO/{date}/aggregate/     (public, anonymized)

Governance:
  1 ATA = 1 vote
  Staked ATA = 2x voting power
  Staked + NFT holder = 3x voting power
  Proposal threshold = 100,000 ATA
  Quorum = 10,000,000 ATA (10%)
  Voting period = 7 days, delay = 1 day
  Multi-sig = 3/5 Gnosis Safe

Telemetry JSON Schema (battle_id, timestamp, class, agents{proposer,solver},
  rounds[{question{hop_count,difficulty,text,answer}, solver_response{answer,
  correct,response_time_ms,search_queries}}], result{winner,proposer_score,
  solver_score,total_questions,correct_answers,avg_difficulty},
  rewards{proposer_reward,solver_reward,burned}, telemetry_ipfs_hash,
  aggregate_included)
```

---

## PART 1 — TEAM A: SMART CONTRACTS
### Team Manager A Brief

**Stack:** Solidity ^0.8.20, Hardhat, OpenZeppelin, TypeScript tests, Mantle Network  
**Repo root:** `ai-training-arena-contracts/`  
**All agents receive:** Shared Data Models block above + this Part 1

---

### CHAPTER SC-0 — PROJECT SCAFFOLD (Agent SC-1 builds this first, all others wait)

**Agent SC-1 Deliverables:**
```
ai-training-arena-contracts/
├── hardhat.config.ts
├── package.json
├── tsconfig.json
├── .env.example
├── contracts/
│   └── interfaces/
│       ├── IATAToken.sol
│       ├── IAgentNFT.sol
│       ├── IBattleVerifier.sol
│       ├── IDataMarketplace.sol
│       ├── IMatchmakingRegistry.sol
│       └── IGovernor.sol
├── scripts/
│   ├── deploy.ts              (deployment orchestrator)
│   └── verify.ts              (Mantle Etherscan verification)
└── test/
    └── helpers/
        ├── fixtures.ts        (shared test fixtures)
        └── utils.ts           (test utilities)
```

**Agent SC-1 Task:**

> Build the Hardhat project scaffold for AI Training Arena smart contracts. Stack: Solidity ^0.8.20, Hardhat 2.x, TypeScript, OpenZeppelin 4.x, ethers.js v6. Target network: Mantle Mainnet (chainId: 5000) and Mantle Testnet (chainId: 5003).
>
> **hardhat.config.ts:** Configure networks for mantle_mainnet (RPC: https://rpc.mantle.xyz), mantle_testnet (RPC: https://rpc.testnet.mantle.xyz), and localhost. Gas settings: gasPrice auto, gasMultiplier 1.2. Enable Solidity optimizer (runs: 200). Add @openzeppelin/contracts dependency.
>
> **contracts/interfaces/IATAToken.sol:**
> ```solidity
> interface IATAToken {
>     function mint(address to, uint256 amount) external;
>     function burn(uint256 amount) external;
>     function transfer(address to, uint256 amount) external returns (bool);
>     function transferFrom(address from, address to, uint256 amount) external returns (bool);
>     function balanceOf(address account) external view returns (uint256);
>     function approve(address spender, uint256 amount) external returns (bool);
>     function totalSupply() external view returns (uint256);
> }
> ```
>
> **contracts/interfaces/IAgentNFT.sol:**
> ```solidity
> interface IAgentNFT {
>     enum AgentClass { A, B, C, D, E }
>     function ownerOf(uint256 tokenId) external view returns (address);
>     function balanceOf(address owner) external view returns (uint256);
>     function getAgentClass(uint256 tokenId) external view returns (AgentClass);
>     function getAgentElo(uint256 tokenId) external view returns (uint256);
>     function updateElo(uint256 tokenId, uint256 newElo) external;
>     function mintAgent(address to, AgentClass class, string calldata modelName) external returns (uint256);
>     function isActive(uint256 tokenId) external view returns (bool);
> }
> ```
>
> **contracts/interfaces/IBattleVerifier.sol:**
> ```solidity
> interface IBattleVerifier {
>     function submitProof(uint256 proposerNFT, uint256 solverNFT, bytes32 merkleRoot, bytes calldata zkProof) external;
>     function verifyBattle(uint256 battleId) external view returns (bool verified, address winner);
>     function challengeBattle(uint256 battleId) external;
>     function resolveDispute(uint256 battleId, address winner) external; // oracle only
> }
> ```
>
> **contracts/interfaces/IDataMarketplace.sol:**
> ```solidity
> interface IDataMarketplace {
>     function listData(uint256 nftId, string calldata ipfsHash, uint256 pricePerAccess) external returns (uint256 listingId);
>     function purchaseDataAccess(uint256 listingId) external payable;
>     function delistData(uint256 listingId) external;
>     function getListingInfo(uint256 listingId) external view returns (address seller, uint256 price, bool active);
> }
> ```
>
> **contracts/interfaces/IMatchmakingRegistry.sol:**
> ```solidity
> interface IMatchmakingRegistry {
>     enum NodeStatus { Offline, Available, InBattle }
>     function announceAvailability(uint256 nftId, uint256 eloRating, NodeStatus status) external;
>     function updateStatus(uint256 nftId, NodeStatus status) external;
>     function getAvailableOpponents(IAgentNFT.AgentClass class, uint256 eloRating) external view returns (uint256[] memory nftIds);
> }
> ```
>
> **test/helpers/fixtures.ts:** Export `deployAllContracts()` function that deploys all contracts in correct order and returns typed contract instances. Export `getSigners()` helper returning { deployer, founder1, founder2, user1, user2, user3, oracle }.
>
> **scripts/deploy.ts:** Deploy in order: ATAToken → AgentNFT → BattleVerifier → WrappedATA → DataMarketplace → AIArenaGovernor → FounderRevenue → MatchmakingRegistry → AITrainingArena (main). Output all addresses to `deployments/{network}.json`. Wire roles after deployment (grant BATTLE_OPERATOR to AITrainingArena, grant DAO_EXECUTOR to AIArenaGovernor, grant ORACLE_ROLE to BattleVerifier).
>
> Confirm with: `SC_AGENT_1_COMPLETE`

---

### CHAPTER SC-1 — ATA TOKEN + AGENT NFT (Agent SC-2)

**Agent SC-2 Deliverables:**
```
contracts/
  ATAToken.sol
  AgentNFT.sol
test/
  ATAToken.test.ts
  AgentNFT.test.ts
```

**Agent SC-2 Task:**

> Build ATAToken.sol and AgentNFT.sol. Import interfaces from contracts/interfaces/. All supply and class data from shared constants block.
>
> **ATAToken.sol** — ERC-20:
> - Inherits: ERC20, ERC20Burnable, AccessControl, Pausable
> - MINTER_ROLE, PAUSER_ROLE
> - Total supply: 100,000,000 × 10^18 (100M tokens, 18 decimals)
> - Constructor mints 0 — all minting done by authorized contracts
> - `mint(address to, uint256 amount)` — MINTER_ROLE only
> - `burn(uint256 amount)` — callable by anyone on own balance
> - `burnFrom(address account, uint256 amount)` — ERC20Burnable
> - `pause()` / `unpause()` — PAUSER_ROLE only
> - Events: Transfer (inherited), Mint(address indexed to, uint256 amount)
> - Deployment distribution (admin executes post-deploy):
>   - 35M locked to NFT vesting contract (set after deploy)
>   - 30M to BattleRewardsPool (set after deploy)
>   - 15M to StakingRewardsPool (set after deploy)
>   - 10M to DAO Treasury
>   - 5M to founders (vested 24mo, 6mo cliff)
>   - 3M to liquidity
>   - 2M to team/advisors
>
> **AgentNFT.sol** — ERC-721:
> - Inherits: ERC721, ERC721Enumerable, AccessControl, ReentrancyGuard
> - MINTER_ROLE (only AITrainingArena can mint), ELO_UPDATER_ROLE (BattleVerifier)
> - Struct Agent: { uint256 nftId, AgentClass class, string modelName, uint256 eloRating, uint256 totalBattles, uint256 wins, uint256 stakedAmount, bool isActive }
> - Enum AgentClass: { A, B, C, D, E }
> - Max supply per class enforced: A=15000, B=6000, C=2500, D=1200, E=300
> - Starting Elo: 1500 for all new agents
> - `mintAgent(address to, AgentClass class, string calldata modelName) returns (uint256)`
> - `updateElo(uint256 tokenId, uint256 newElo)` — ELO_UPDATER_ROLE only
> - `incrementBattles(uint256 tokenId, bool won)` — BATTLE_OPERATOR role
> - `setActive(uint256 tokenId, bool active)` — owner or ADMIN
> - `getAgentClass(uint256 tokenId) returns (AgentClass)`
> - `getAgentElo(uint256 tokenId) returns (uint256)`
> - `getUserAgents(address user) returns (uint256[])`
> - `tokenURI(uint256 tokenId)` — returns JSON metadata with class, elo, battles, wins, modelName
> - Events: AgentMinted(uint256 indexed nftId, address owner, AgentClass class), EloUpdated(uint256 indexed nftId, uint256 oldElo, uint256 newElo)
>
> **ATAToken.test.ts:** Test minting, burning, pausability, access control, supply cap
> **AgentNFT.test.ts:** Test minting per class, class supply caps, Elo updates, tokenURI, getUserAgents
>
> Confirm with: `SC_AGENT_2_COMPLETE`

---

### CHAPTER SC-2 — MAIN ARENA CONTRACT (Agent SC-3)

**Agent SC-3 Deliverables:**
```
contracts/
  AITrainingArena.sol
test/
  AITrainingArena.test.ts
```

**Agent SC-3 Task:**

> Build AITrainingArena.sol — the central orchestrator. This is the most complex contract. Import all interfaces from contracts/interfaces/. Use the shared data models block for all constants.
>
> **AITrainingArena.sol:**
> - Inherits: AccessControl, ReentrancyGuard, Pausable
> - Roles: BATTLE_OPERATOR, DAO_EXECUTOR, ORACLE_ROLE, PAUSER_ROLE
> - State: `IATAToken public ataToken`, `IAgentNFT public agentNFT`, `uint256 public dailyRewardPool = 8219 ether`, `uint256 public lastRewardReset`, `mapping(uint256 => uint256) public classMultiplier` (A=100, B=120, C=150, D=200, E=300 — divide by 100 for actual multiplier)
>
> **Functions:**
>
> `constructor(address _ataToken, address _agentNFT)` — set contracts, setup roles, initialize class multipliers
>
> `mintAgent(IAgentNFT.AgentClass class, string calldata modelName) external payable` — User calls with MNT. Verify price: A=10e18, B=50e18, C=200e18, D=800e18, E=3000e18 MNT. Call agentNFT.mintAgent. Emit AgentPurchased.
>
> `recordBattle(uint256 proposerNFT, uint256 solverNFT, address winner, uint256 proposerScore, uint256 solverScore, string calldata ipfsHash) external onlyRole(BATTLE_OPERATOR) nonReentrant whenNotPaused` — Full implementation from blueprint Section 8.1. Steps: verify both agents active, increment battle counts, calculate Elo changes (K-factor formula from blueprint Section 6.3), calculate rewards using class multiplier, distribute: 90% winner, 10% loser, burn 2%. Emit BattleCompleted.
>
> **Elo calculation (implement inline):**
> ```
> function calculateEloChange(uint256 nftA, uint256 nftB, bool aWon)
>     internal view returns (uint256 newEloA, uint256 newEloB)
> {
>     uint256 eloA = agentNFT.getAgentElo(nftA);
>     uint256 eloB = agentNFT.getAgentElo(nftB);
>     uint256 kFactor = (agentNFT.getBattleCount(nftA) < 30) ? 40 : 20;
>     // Expected score: E = 1/(1+10^((Rb-Ra)/400))
>     // Use integer math approximation — document the approximation
>     // Delta = K * (S - E) where S = 1 (win) or 0 (loss)
> }
> ```
>
> `stakeTokens(uint256 nftId, uint256 amount) external nonReentrant` — verify NFT owner, transferFrom ATA to contract, update agent stakedAmount, emit AgentStaked.
>
> `unstakeTokens(uint256 nftId, uint256 amount) external nonReentrant` — verify owner, 7-day cooldown (store initiatedAt timestamp), after cooldown transfer ATA back, emit AgentUnstaked.
>
> `claimRewards(address user) external nonReentrant` — allow users to claim accumulated ATA rewards.
>
> `executeBuyback(uint256 ataAmount) external onlyRole(DAO_EXECUTOR)` — simulate buying ATA from market and burning: transfer ataAmount to address(0xdead). Emit BuybackExecuted.
>
> `setClassMultiplier(IAgentNFT.AgentClass class, uint256 multiplier) external onlyRole(DAO_EXECUTOR)` — DAO-controlled parameter adjustment.
>
> `pause() / unpause()` — PAUSER_ROLE. Emergency stop.
>
> View functions: `getAgentInfo(uint256 nftId)`, `getUserAgents(address user)`, `getPendingRewards(address user)`, `getClassStats(IAgentNFT.AgentClass class)`
>
> Events: BattleCompleted, AgentPurchased, AgentStaked, AgentUnstaked, RewardsClaimed, BuybackExecuted, ClassMultiplierUpdated
>
> **AITrainingArena.test.ts:** Test battle recording with reward distribution, Elo changes, staking/unstaking with cooldown, buyback, access control, pause/unpause, edge cases (same NFT both sides = revert)
>
> Confirm with: `SC_AGENT_3_COMPLETE`

---

### CHAPTER SC-3 — WRAPPED ATA + BATTLE VERIFIER (Agent SC-4)

**Agent SC-4 Deliverables:**
```
contracts/
  WrappedATA.sol
  BattleVerifier.sol
test/
  WrappedATA.test.ts
  BattleVerifier.test.ts
```

**Agent SC-4 Task:**

> **WrappedATA.sol** — from blueprint Section 8.2 exactly:
> - Inherits: ERC20, ReentrancyGuard
> - Struct TempLicense: { uint256 licenseId, IAgentNFT.AgentClass class, uint256 stakedAmount, uint256 expiryTime, bool active }
> - `mapping(address => TempLicense) public licenses`
> - stakeRequirements: A=100e18, B=500e18, C=2000e18, D=8000e18, E=30000e18
> - `wrap(uint256 amount)` — transferFrom ATA, mint wATA 1:1
> - `unwrap(uint256 amount)` — burn wATA, transfer ATA back
> - `stakeLicense(IAgentNFT.AgentClass class)` — require no active license, require balance >= stakeRequirements[class], lock wATA in contract, create TempLicense, call arena.activateTempAgent
> - `initiateWithdrawal()` — sets expiryTime = block.timestamp + 7 days
> - `completeWithdrawal()` — require cooldown passed, return wATA, deactivate license, call arena.deactivateTempAgent
> - Events: LicenseActivated, WithdrawalInitiated, WithdrawalCompleted
>
> **BattleVerifier.sol** — cryptographic proof verification:
> - Inherits: AccessControl, ReentrancyGuard
> - Roles: ORACLE_ROLE (for dispute resolution)
> - Struct BattleRecord: { uint256 proposerNFT, uint256 solverNFT, bytes32 merkleRoot, uint256 submittedAt, bool verified, bool disputed, address winner, bool settled }
> - `submitProof(uint256 proposerNFT, uint256 solverNFT, bytes32 merkleRoot, bytes calldata zkProof)` — both nodes must submit. When both submitted: compare merkleRoots. If match → verified, call arena.recordBattle. If mismatch → emit DisputeOpened, start 1-hour challenge window.
> - `challengeBattle(uint256 battleId)` — opens dispute, emits OracleRequested
> - `resolveDispute(uint256 battleId, address winner)` — ORACLE_ROLE only. Settles dispute, applies slashing to loser (10% staked ATA sent to DAO treasury), calls arena.recordBattle
> - `verifyMerkleProof(bytes32 root, bytes32 leaf, bytes32[] calldata proof) internal pure returns (bool)` — standard Merkle verification
> - View: `getBattleRecord(uint256 battleId)`, `isPendingDispute(uint256 battleId)`
> - Events: ProofSubmitted, BattleVerified, DisputeOpened, OracleRequested, DisputeResolved
>
> **Tests:** Proof submission flow, dual-submission matching, mismatch → dispute, oracle resolution, slashing
>
> Confirm with: `SC_AGENT_4_COMPLETE`

---

### CHAPTER SC-4 — DATA MARKETPLACE + MATCHMAKING REGISTRY (Agent SC-5)

**Agent SC-5 Deliverables:**
```
contracts/
  DataMarketplace.sol
  MatchmakingRegistry.sol
test/
  DataMarketplace.test.ts
  MatchmakingRegistry.test.ts
```

**Agent SC-5 Task:**

> **DataMarketplace.sol** — from blueprint Section 11.3 exactly, extended:
> - Inherits: ReentrancyGuard, Pausable, AccessControl
> - Struct DataListing: { address seller, uint256 nftId, string ipfsHash, uint256 pricePerAccess, uint256 totalSales, uint256 createdAt, bool active, DataCategory category }
> - Enum DataCategory: { BATTLE_LOG, MODEL_CHECKPOINT, QUESTION_CORPUS, TRAINING_SET }
> - `mapping(uint256 => DataListing) public listings`
> - `mapping(uint256 => mapping(address => bool)) public hasAccess`
> - PLATFORM_FEE = 500 (5% in basis points)
> - `listData(uint256 nftId, string calldata ipfsHash, uint256 pricePerAccess, DataCategory category) external returns (uint256)`
>   — verify agentNFT.ownerOf(nftId) == msg.sender, create listing, emit DataListed
> - `purchaseDataAccess(uint256 listingId) external payable nonReentrant`
>   — require active, require payment >= price, split: seller gets 95%, DAO treasury gets 5%
>   — grant access: hasAccess[listingId][buyer] = true, emit DataPurchased
> - `delistData(uint256 listingId) external` — only seller, sets active=false
> - `updatePrice(uint256 listingId, uint256 newPrice) external` — only seller
> - `hasAccessTo(uint256 listingId, address buyer) external view returns (bool)`
> - `getListingsByNFT(uint256 nftId) external view returns (uint256[])`
> - `getActiveListings(uint256 offset, uint256 limit) external view returns (uint256[])`
>   — pagination for frontend
> - Events: DataListed, DataPurchased, DataDelisted, PriceUpdated, AccessGranted
>
> **MatchmakingRegistry.sol:**
> - Inherits: AccessControl
> - Struct NodeAdvertisement: { uint256 nftId, address owner, IAgentNFT.AgentClass class, uint256 eloRating, NodeStatus status, uint256 lastSeen, string peerId }
> - Enum NodeStatus: { Offline, Available, InBattle }
> - `mapping(uint256 => NodeAdvertisement) public nodes`
> - `mapping(IAgentNFT.AgentClass => uint256[]) private availableByClass`
> - `announceAvailability(uint256 nftId, uint256 eloRating, string calldata peerId)`
>   — verify caller owns nftId, update/create NodeAdvertisement with Available status, emit NodeAvailable
> - `updateStatus(uint256 nftId, NodeStatus status) external` — only NFT owner
> - `getAvailableOpponents(IAgentNFT.AgentClass class, uint256 eloRating, uint256 eloRange) external view returns (uint256[] memory)`
>   — filter by class, filter by eloRating ± eloRange (default 200), filter by Available status, filter lastSeen < 5 min ago
> - `cleanStaleNodes() external` — remove nodes not updated in 10 min (callable by anyone, gas reimbursed from DAO)
> - Events: NodeAvailable, NodeBusy, NodeOffline, StaleNodeRemoved
>
> **Tests:** Full listing/purchase flow with fee verification, access control, pagination, matchmaking filter logic with Elo ranges, stale node cleanup
>
> Confirm with: `SC_AGENT_5_COMPLETE`

---

### CHAPTER SC-5 — DAO GOVERNOR + FOUNDER REVENUE (Agent SC-6)

**Agent SC-6 Deliverables:**
```
contracts/
  AIArenaGovernor.sol
  FounderRevenue.sol
test/
  AIArenaGovernor.test.ts
  FounderRevenue.test.ts
scripts/
  deploy.ts          (complete — wire all contracts)
  verify.ts          (complete)
```

**Agent SC-6 Task:**

> **AIArenaGovernor.sol** — from blueprint Section 12.2:
> - Inherits: Governor, GovernorCountingSimple, GovernorVotes, GovernorTimelockControl
> - VOTING_DELAY = 1 days (in blocks, ~43200 on Mantle)
> - VOTING_PERIOD = 7 days (in blocks, ~302400)
> - PROPOSAL_THRESHOLD = 100_000 ether (100K ATA)
> - `quorum() pure returns (uint256)` → 10_000_000 ether (10M ATA)
> - `getVotes(address account, uint256 blockNumber) view returns (uint256)`:
>   - baseVotes = token.getPastVotes(account, blockNumber)
>   - stakeBonus = stakingContract.stakedBalance(account) (2x multiplier = add stakedAmount once more)
>   - nftBonus = agentNFT.balanceOf(account) > 0 ? stakingContract.stakedBalance(account) : 0 (3x total = base + stake + nftBonus)
>   - return baseVotes + stakeBonus + nftBonus
> - `_execute(...)` — require multiSig.isConfirmed(proposalId, 3) before executing
> - Timelock: 48-hour timelock on all executions after vote passes
> - `propose(...)`, `castVote(...)`, `state(...)` — inherited from Governor
> - Emergency functions: `emergencyPause(address target)` — 2/5 multi-sig, no timelock
>
> **FounderRevenue.sol** — from blueprint Section 13.2:
> - Inherits: ReentrancyGuard
> - `address public founder1`, `address public founder2`
> - Fee shares: BATTLE_FEE_SHARE=2000 (20%), MARKETPLACE_FEE_SHARE=4000 (40%), DATA_FEE_SHARE=3000 (30%)
> - Accumulate: `battleFeesCollected`, `marketplaceFeesCollected`, `dataFeesCollected`
> - `mapping(address => uint256) public withdrawn`
> - `receiveBattleFees() external payable` — add to battleFeesCollected
> - `receiveMarketplaceFees() external payable` — add to marketplaceFeesCollected
> - `receiveDataFees() external payable` — add to dataFeesCollected
> - `availableBalance(address founder) public view returns (uint256)` — exact formula from blueprint 13.2, subtract already withdrawn
> - `withdraw() external onlyFounders nonReentrant` — compute availableBalance, update withdrawn, transfer, emit Withdrawal
> - `updateFounderAddress(address newAddress) external onlyFounders` — allow address rotation (requires both founders' multi-sig in future)
> - `transferFoundership(address newFounder) external onlyFounders` — 7-day timelock
> - Events: FeeReceived, Withdrawal, FounderAddressUpdated
>
> **scripts/deploy.ts (COMPLETE):** Must deploy ALL contracts in order:
> 1. ATAToken → 2. AgentNFT → 3. AITrainingArena → 4. WrappedATA → 5. BattleVerifier → 6. DataMarketplace → 7. AIArenaGovernor → 8. FounderRevenue → 9. MatchmakingRegistry
> Then wire roles: grantRole(BATTLE_OPERATOR, AITrainingArena), grantRole(DAO_EXECUTOR, AIArenaGovernor), grantRole(ORACLE_ROLE to designated oracle address), grantRole(MINTER_ROLE to AITrainingArena), grantRole(ELO_UPDATER_ROLE to BattleVerifier)
> Mint initial ATA supply per distribution (see shared constants).
> Write deployments/{network}.json with all addresses.
>
> **Tests:** Governance proposal lifecycle (propose → vote → queue → execute), voting power multipliers (verify 3x for NFT+staked), founder withdrawal math, timelock on execution
>
> Confirm with: `SC_AGENT_6_COMPLETE`

---

## PART 2 — TEAM B: P2P NODE (.NET 9)
### Team Manager B Brief

**Stack:** .NET 9.0 (C#), ASP.NET Core, LibP2P .NET, Nethereum, ONNX Runtime, IPFS Kubo .NET, SQLite (EF Core), xUnit  
**Repo root:** `AITrainingArena.Node/`  
**All agents receive:** Shared Data Models block (Part 0B) + this Part 2

---

### CHAPTER N-0 — PROJECT SCAFFOLD + NODE CONFIGURATION (Agent N-1)

**Agent N-1 Deliverables:**
```
AITrainingArena.Node/
├── AITrainingArena.Node.sln
├── src/
│   ├── AITrainingArena.Node/
│   │   ├── AITrainingArena.Node.csproj
│   │   ├── Program.cs
│   │   ├── NodeConfiguration.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── ServiceRegistration.cs
│   ├── AITrainingArena.Core/
│   │   ├── AITrainingArena.Core.csproj
│   │   ├── Models/
│   │   │   ├── Agent.cs
│   │   │   ├── Battle.cs
│   │   │   ├── Question.cs
│   │   │   ├── BattleResult.cs
│   │   │   ├── TelemetryRecord.cs
│   │   │   └── NodeAdvertisement.cs
│   │   ├── Enums/
│   │   │   ├── AgentClass.cs
│   │   │   ├── NodeStatus.cs
│   │   │   └── BattleRole.cs
│   │   └── Interfaces/
│   │       ├── INetworkManager.cs
│   │       ├── IBattleOrchestrator.cs
│   │       ├── IDrZeroEngine.cs
│   │       ├── IProofGenerator.cs
│   │       ├── IResultSubmitter.cs
│   │       ├── IIPFSClient.cs
│   │       ├── ITelemetryService.cs
│   │       └── IModelRegistry.cs
│   └── ... (other projects, agent N-2 through N-6 add their projects here)
├── tests/
│   └── AITrainingArena.Node.Tests/
│       ├── AITrainingArena.Node.Tests.csproj
│       └── TestHelpers.cs
└── docker/
    ├── Dockerfile
    └── docker-compose.yml
```

**Agent N-1 Task:**

> Build the .NET 9 solution scaffold and core models for AI Training Arena P2P Node. This is the foundation. Use .NET 9 with C#, Hosted Services pattern, dependency injection, strongly-typed configuration.
>
> **NodeConfiguration.cs** (IOptions<NodeConfiguration> bound from appsettings.json):
> ```csharp
> public class NodeConfiguration {
>     public string WalletAddress { get; set; }         // Ethereum address
>     public string WalletPrivateKey { get; set; }      // encrypted at rest
>     public string ModelPath { get; set; }             // path to ONNX model
>     public AgentClass AgentClass { get; set; }        // A through E
>     public uint NftId { get; set; }                   // NFT token ID
>     public bool AutoBattle { get; set; } = true;      // opt into auto battles
>     public int P2PPort { get; set; } = 4001;
>     public int WebSocketPort { get; set; } = 8080;    // for frontend connection
>     public int HttpApiPort { get; set; } = 8081;      // for frontend REST
>     public string MantleRpcUrl { get; set; } = "https://rpc.mantle.xyz";
>     public string IpfsApiUrl { get; set; } = "/ip4/127.0.0.1/tcp/5001";
>     public string[] BootstrapPeers { get; set; }      // DHT bootstrap nodes
>     public string DataDirectory { get; set; }         // local data storage path
>     public bool CloudFallback { get; set; } = false;  // use cloud GPU if no local
>     public string CloudApiKey { get; set; }           // AWS Bedrock or Azure OAI
> }
> ```
>
> **Core Models (AITrainingArena.Core/Models/):**
> - Agent.cs: NftId, AgentClass Class, string ModelName, int EloRating, int TotalBattles, int Wins, bool IsActive, decimal StakedAmount, string OwnerAddress
> - Battle.cs: Guid BattleId, string ProposerId, string SolverId, BattleRole LocalRole, DateTime StartedAt, BattleStatus Status, List<QuestionAnswerResult> Results, BattleResult? FinalResult
> - Question.cs: string Text, int HopCount, string ExpectedAnswer, decimal DifficultyScore, List<string> HopChain, DateTime GeneratedAt
> - BattleResult.cs: string Winner, decimal ProposerScore, decimal SolverScore, int TotalQuestions, int CorrectAnswers, decimal AvgDifficulty, decimal ProposerReward, decimal SolverReward, decimal BurnAmount, string TelemetryIpfsHash
> - TelemetryRecord.cs: mirrors the telemetry JSON schema from shared data models exactly
> - NodeAdvertisement.cs: string PeerId, AgentClass Class, int EloRating, NodeStatus Status, DateTime LastSeen
>
> **Program.cs:** Register all services via IHostBuilder. Register: NetworkManager, BattleOrchestrator (IHostedService), DrZeroEngine, ProposerService, SolverService, HRPOOptimizer, GRPOOptimizer, MantleRPC, ContractInteraction, ProofGenerator, ResultSubmitter, IPFSClient, TelemetryEncryption, LocalDatabase, PinningService, ModelLoader, InferenceEngine, ModelRegistry. Add WebSocket server (for frontend). Add minimal HTTP API (for frontend REST queries). Add Serilog logging with file sink.
>
> **ServiceRegistration.cs:** Extension method `AddArenaServices(this IServiceCollection services, NodeConfiguration config)` — centralizes all DI registrations.
>
> **appsettings.json:** Complete template with all NodeConfiguration fields, sensible defaults, inline comments explaining each field.
>
> **docker/Dockerfile:** Multi-stage build. Base: mcr.microsoft.com/dotnet/runtime:9.0. Expose ports 4001, 8080, 8081. Copy ONNX model directory as volume mount. Include startup health check.
>
> **docker/docker-compose.yml:** Service for node + service for IPFS daemon (ipfs/kubo). Network: bridge. Volumes for data persistence.
>
> Confirm with: `N_AGENT_1_COMPLETE`

---

### CHAPTER N-1 — P2P NETWORKING (Agent N-2)

**Agent N-2 Deliverables:**
```
src/AITrainingArena.P2P/
  AITrainingArena.P2P.csproj
  NetworkManager.cs
  PeerDiscovery.cs
  GossipSync.cs
  Models/
    PeerConnection.cs
    BattleHandshake.cs
    GossipMessage.cs
    LeaderboardEntry.cs
```

**Agent N-2 Task:**

> Build the LibP2P networking layer. Reference blueprint Section 10.2 for NetworkManager exactly. All interfaces from AITrainingArena.Core.
>
> **NetworkManager.cs** — implements INetworkManager:
> - `Host _p2pHost` — LibP2P Host with TCP + WebSocket transports, Noise encryption, Mplex multiplexing
> - `KademliaProtocol _dht`
> - `Dictionary<string, PeerConnection> _activePeers`
> - `StartAsync()` — start host, bootstrap DHT, announce availability (from blueprint 10.2 exactly)
> - `StopAsync()` — graceful shutdown, announce offline
> - `BootstrapDHTAsync()` — connect to 3 bootstrap nodes from config.BootstrapPeers
> - `AnnounceAvailabilityAsync()` — put NodeAdvertisement to DHT key `/agents/{class}/{peerId}`
> - `FindOpponentAsync(AgentClass myClass, int myElo) Task<PeerId?>` — query DHT, filter ±200 Elo, return closest match. From blueprint 10.2.
> - `EstablishBattleConnectionAsync(PeerId opponentId) Task<BattleConnection>` — open stream on protocol `/arena/battle/1.0.0`, perform BattleHandshake (sign battle intent), verify opponent signature. From blueprint 10.2.
> - `SendToPeerAsync(PeerId peer, byte[] data) Task`
> - `DisconnectPeerAsync(PeerId peer) Task`
>
> **PeerDiscovery.cs** — Kademlia DHT wrapper:
> - `QueryClassAsync(AgentClass class) Task<List<NodeAdvertisement>>`
> - `UpdateMyAdvertisementAsync(NodeAdvertisement ad) Task`
> - `RemoveMyAdvertisementAsync() Task`
> - `GetNetworkSizeEstimate() Task<int>`
>
> **GossipSync.cs** — Gossip protocol for leaderboard:
> - Maintains local leaderboard cache (per class, top 100)
> - On battle completion: gossip new entry to 3 random peers
> - `ReceiveGossip(GossipMessage msg)` — merge into local cache, rebroadcast if newer
> - `GetLeaderboard(AgentClass class, int topN) List<LeaderboardEntry>`
> - `SyncWithPeer(PeerId peer) Task` — full sync on new peer connection
>
> **BattleHandshake.cs:** MyNFTId (uint256), MySignature (byte[]), Timestamp (DateTime) — serializable to byte[]
> **GossipMessage.cs:** MessageType (NewBattleResult, LeaderboardSync), Payload (byte[]), OriginPeer (string), Timestamp (DateTime)
> **LeaderboardEntry.cs:** NftId, ModelName, AgentClass, EloRating, TotalBattles, WinRate, TotalRewardsEarned
>
> Confirm with: `N_AGENT_2_COMPLETE`

---

### CHAPTER N-2 — BATTLE PROTOCOL + BATTLE ORCHESTRATOR (Agent N-3)

**Agent N-3 Deliverables:**
```
src/AITrainingArena.P2P/
  BattleProtocol.cs
src/AITrainingArena.Node/
  BattleOrchestrator.cs
  Models/
    BattleMessage.cs
    BattleConnection.cs
    QuestionAnswerResult.cs
```

**Agent N-3 Task:**

> Build the P2P battle protocol and the main battle orchestration loop. Reference blueprint Sections 10.3 and 3.2 for exact flow.
>
> **BattleMessage.cs:** MessageType enum (Question, Answer, BattleStart, BattleEnd, Heartbeat), byte[] Payload, Guid BattleId, DateTime Timestamp — serialize/deserialize methods.
>
> **BattleProtocol.cs** — P2P battle messaging over LibP2P stream:
> - `SendQuestionAsync(BattleConnection conn, Question q, CancellationToken ct) Task`
> - `ReceiveQuestionAsync(BattleConnection conn, CancellationToken ct) Task<Question>`
> - `SendAnswerAsync(BattleConnection conn, string answer, CancellationToken ct) Task`
> - `ReceiveAnswerAsync(BattleConnection conn, CancellationToken ct) Task<string>`
> - `SendBattleEndAsync(BattleConnection conn, BattleResult result) Task`
> - `ReceiveBattleEndAsync(BattleConnection conn) Task<BattleResult>`
> - `SendHeartbeatAsync(BattleConnection conn) Task`
> - `WaitForHeartbeatAsync(BattleConnection conn, TimeSpan timeout) Task<bool>`
> - Timeout handling: 2-minute timeout per question/answer exchange. On timeout: submit partial results.
>
> **BattleOrchestrator.cs** — implements IHostedService:
> - `ExecuteAsync(CancellationToken ct)` — main battle loop from blueprint 10.3 exactly:
>   1. Check `config.AutoBattle`. If false, sleep 1 min and retry.
>   2. Find opponent via NetworkManager.FindOpponentAsync
>   3. If null: sleep 5 min
>   4. EstablishBattleConnectionAsync
>   5. DetermineRoleAsync (flip coin via shared random seed from handshake — XOR of both signatures)
>   6. If proposer: ExecuteAsProposerAsync
>   7. If solver: ExecuteAsSolverAsync
>   8. GenerateProof
>   9. SubmitBattleProof
>   10. Store telemetry to IPFS
>   11. Log result
>   12. Wait remaining time to next 25-min window
> - `ExecuteAsProposerAsync(BattleConnection conn, CancellationToken ct) Task<BattleResult>` — from blueprint 10.3: generate questions loop for 25 min duration, send via BattleProtocol, receive answers, verify + score, build telemetry
> - `ExecuteAsSolverAsync(BattleConnection conn, CancellationToken ct) Task<BattleResult>` — receive questions, generate answers via InferenceEngine, send back
> - `DetermineRoleAsync(BattleConnection conn) Task<BattleRole>` — cryptographic coin flip
> - `CalculateFinalScores(List<QuestionAnswerResult> results) (decimal proposerScore, decimal solverScore)` — implement proposer_score = difficulty*0.6 + count*0.4, solver_score = correct*1.5 from blueprint 6.1.2
>
> Confirm with: `N_AGENT_3_COMPLETE`

---

### CHAPTER N-3 — DR. ZERO AI ENGINE (Agent N-4)

**Agent N-4 Deliverables:**
```
src/AITrainingArena.BattleEngine/
  AITrainingArena.BattleEngine.csproj
  DrZeroEngine.cs
  ProposerService.cs
  SolverService.cs
  HRPOOptimizer.cs
  GRPOOptimizer.cs
  IPFSSearchEngine.cs
  Models/
    HopChain.cs
    OptimizationResult.cs
    ModelWeightUpdate.cs
```

**Agent N-4 Task:**

> Implement the Dr. Zero AI engine in C#. Reference blueprint Sections 7.1 and 7.2 for exact algorithms. This layer calls InferenceEngine from AITrainingArena.AI project (injected as interface).
>
> **DrZeroEngine.cs** — implements IDrZeroEngine:
> - Constructor: inject IInferenceEngine, IIPFSSearchEngine, ILogger, NodeConfiguration
> - `GenerateQuestionAsync(int hopCount, CancellationToken ct) Task<Question>` — calls ProposerService
> - `VerifyAnswerAsync(Question q, string answer) Task<bool>` — compare against expected answer with fuzzy match (exact + normalized + synonym matching)
> - `CalculateDifficulty(Question q, string answer, bool correct) decimal` — based on hopCount (base difficulty), answer correctness, response time, search complexity
> - `RunTrainingIterationAsync(int numSteps, CancellationToken ct) Task<OptimizationResult>` — full training loop from blueprint 7.1: generate questions → solve → HRPO update → GRPO update
>
> **ProposerService.cs:**
> - `GenerateMultiHopQuestionAsync(int hopCount, CancellationToken ct) Task<Question>` — from blueprint 7.2 exactly:
>   1. Get initial document from IPFSSearchEngine (random Wikipedia article via IPFS search)
>   2. For each hop: ExtractKeyEntity → GenerateSearchQuery → Search → GetNextDocument
>   3. Formulate final question from hop chain
>   4. Store HopChain for verification
> - `ExtractKeyEntityAsync(string document, CancellationToken ct) Task<string>` — use InferenceEngine to extract named entity
> - `GenerateSearchQueryAsync(string entity, int hopNumber) Task<string>` — template-based query generation
>
> **SolverService.cs:**
> - `SolveQuestionAsync(Question q, CancellationToken ct) Task<string>` — use InferenceEngine + IPFSSearchEngine:
>   1. Parse question to identify search strategy
>   2. Up to 5 search turns (max_turns=5 from blueprint)
>   3. Each turn: search → extract answer → verify confidence
>   4. Return best answer when confident or exhausted
> - `AnswerWithSearch(string questionText, int maxTurns, CancellationToken ct) Task<string>`
>
> **HRPOOptimizer.cs** — Hop-grouped Relative Policy Optimization (from blueprint 7.1):
> - `UpdateAsync(List<QuestionAnswerResult> results, IInferenceEngine model) Task<ModelWeightUpdate>`
> - Groups results by hop_count, calculates per-group advantages: `(difficulty - mean) / (std + 1e-8)`
> - Returns gradient update (in practice: saves training log for offline fine-tuning via ONNX)
> - Note: Full RL update requires Python/PyTorch — this C# implementation logs the training signal and queues fine-tuning jobs. Document this limitation clearly.
>
> **GRPOOptimizer.cs** — Group Relative Policy Optimization (from blueprint 7.1):
> - `UpdateAsync(List<QuestionAnswerResult> results, IInferenceEngine model, int groupSize = 5) Task<ModelWeightUpdate>`
> - Batch results in groups of groupSize, calculate advantages per batch
> - Same implementation note as HRPO
>
> **IPFSSearchEngine.cs:**
> - `SearchAsync(string query, int maxResults = 5) Task<List<string>>` — query local IPFS for documents matching query
> - `GetDocumentAsync(string cid) Task<string>` — fetch document from IPFS
> - Falls back to Wikipedia API if IPFS has no results
>
> Confirm with: `N_AGENT_4_COMPLETE`

---

### CHAPTER N-4 — BLOCKCHAIN INTEGRATION (Agent N-5)

**Agent N-5 Deliverables:**
```
src/AITrainingArena.Blockchain/
  AITrainingArena.Blockchain.csproj
  MantleRPC.cs
  ContractInteraction.cs
  ProofGenerator.cs
  ResultSubmitter.cs
  Models/
    BattleProof.cs
    TransactionResult.cs
    ContractAddresses.cs
```

**Agent N-5 Task:**

> Build the Mantle blockchain integration layer using Nethereum. Reference blueprint Section 10 and smart contract ABIs (generated from deployment).
>
> **ContractAddresses.cs:** Strongly-typed config class loaded from `deployments/{network}.json`:
> - string ATAToken, string AgentNFT, string AITrainingArena, string WrappedATA, string BattleVerifier, string DataMarketplace, string AIArenaGovernor, string MatchmakingRegistry
>
> **MantleRPC.cs:**
> - Constructor: inject NodeConfiguration, ILogger
> - `_web3 Web3` — Nethereum Web3 instance, connected to config.MantleRpcUrl
> - `GetBalanceAsync(string address) Task<decimal>` — MNT balance
> - `GetATABalanceAsync(string address) Task<decimal>` — ATA token balance
> - `GetCurrentNonce(string address) Task<int>`
> - `GetGasPrice() Task<BigInteger>`
> - `IsConnected() bool`
> - `GetBlockNumber() Task<ulong>`
> - Health check on startup — emit warning if cannot connect
>
> **ContractInteraction.cs:**
> - Constructor: inject Web3, ContractAddresses, NodeConfiguration, ILogger
> - `GetAgentInfoAsync(uint nftId) Task<Agent>` — call AgentNFT.getAgentInfo
> - `AnnounceAvailabilityAsync(uint nftId, int eloRating, string peerId) Task<TransactionResult>` — call MatchmakingRegistry.announceAvailability
> - `UpdateNodeStatusAsync(uint nftId, NodeStatus status) Task<TransactionResult>` — call MatchmakingRegistry.updateStatus
> - `GetUserAgentsAsync(string address) Task<List<uint>>` — call AgentNFT.getUserAgents
> - `GetPendingRewardsAsync(string address) Task<decimal>` — call AITrainingArena.getPendingRewards
> - `ClaimRewardsAsync() Task<TransactionResult>` — call AITrainingArena.claimRewards, sign with local wallet
> - `StakeTokensAsync(uint nftId, decimal amount) Task<TransactionResult>`
> - `WrapATAAsync(decimal amount) Task<TransactionResult>` — ATA → wATA
> - `StakeLicenseAsync(AgentClass class) Task<TransactionResult>` — wATA temp license
>
> **ProofGenerator.cs:**
> - `GenerateProofAsync(BattleResult result, List<QuestionAnswerResult> qaPairs) Task<BattleProof>`
> - Build Merkle tree from all Q&A pairs (each leaf = hash(question + answer + correct + difficulty))
> - Compute Merkle root
> - Generate ZK proof stub (true ZK proof requires separate ZK circuit — document this: implement as ECDSA signature of Merkle root in Phase 1, upgrade to ZK in Phase 2)
> - Return BattleProof: { ProposerNFT, SolverNFT, MerkleRoot, Signature, Timestamp, TelemetryHash }
>
> **BattleProof.cs:** uint ProposerNFT, uint SolverNFT, byte[] MerkleRoot, byte[] Signature, DateTime Timestamp, string TelemetryIpfsHash, decimal ProposerScore, decimal SolverScore, string WinnerAddress
>
> **ResultSubmitter.cs:**
> - `SubmitBattleProofAsync(BattleProof proof, CancellationToken ct) Task<TransactionResult>`
> - Call BattleVerifier.submitProof with Merkle root + signature
> - Retry up to 3 times with exponential backoff (gas price can spike)
> - On confirmed: log transaction hash, update local battle record
> - `WaitForConfirmation(string txHash, int maxBlocks = 10) Task<bool>` — poll for inclusion
>
> Confirm with: `N_AGENT_5_COMPLETE`

---

### CHAPTER N-5 — STORAGE + AI MODELS + WEBSOCKET API (Agent N-6)

**Agent N-6 Deliverables:**
```
src/AITrainingArena.Storage/
  AITrainingArena.Storage.csproj
  IPFSClient.cs
  TelemetryEncryption.cs
  LocalDatabase.cs
  PinningService.cs
src/AITrainingArena.AI/
  AITrainingArena.AI.csproj
  ModelLoader.cs
  InferenceEngine.cs
  ModelRegistry.cs
src/AITrainingArena.Node/
  WebSocketServer.cs
  HttpApiController.cs
tests/
  StorageTests.cs
  InferenceTests.cs
```

**Agent N-6 Task:**

> Build storage, AI model management, and the WebSocket/HTTP API that the Dioxus frontend connects to.
>
> **IPFSClient.cs** — implements IIPFSClient:
> - Connect to local IPFS daemon at config.IpfsApiUrl (Kubo HTTP API)
> - `AddFileAsync(string localPath) Task<string>` — returns CID
> - `AddContentAsync(byte[] content) Task<string>` — returns CID
> - `GetContentAsync(string cid) Task<byte[]>`
> - `PinAsync(string cid) Task`
> - `UnpinAsync(string cid) Task`
> - `IsAvailableAsync() Task<bool>` — health check
>
> **TelemetryEncryption.cs:**
> - `EncryptForOwner(TelemetryRecord record, string ownerPublicKey) byte[]` — AES-256-GCM with key derived from owner's ETH public key (ECDH key agreement)
> - `DecryptAsOwner(byte[] encrypted, string privateKey) TelemetryRecord`
> - `AnonymizeForDAO(TelemetryRecord record) AnonymizedRecord` — implement exact logic from blueprint 14.3: hash battle_id, strip nft_ids and addresses, keep class + timestamp + aggregate stats only, keep model family not full path
>
> **LocalDatabase.cs** — SQLite via EF Core:
> - Tables: Battles (local battle log), BattleResults, AgentCache (NFT data cache), PendingSubmissions (retry queue), Settings
> - `SaveBattleAsync(Battle b) Task`
> - `GetBattleSummary(int lastN) Task<List<BattleResult>>`
> - `GetPendingSubmissions() Task<List<BattleProof>>` — results not yet submitted on-chain
> - `MarkSubmitted(Guid battleId) Task`
> - `CacheAgentInfo(Agent agent) Task`
> - `GetCachedAgent(uint nftId) Task<Agent?>`
>
> **PinningService.cs:**
> - `PinTelemetryAsync(TelemetryRecord record, string ownerPublicKey) Task<string>` — encrypt + add to IPFS + pin + return CID
> - `SubmitToDAOAsync(TelemetryRecord record) Task<string>` — anonymize + add to IPFS + return CID (DAO archive nodes will pin)
>
> **ModelLoader.cs:**
> - `LoadModelAsync(string modelPath, AgentClass class) Task<IInferenceSession>` — load ONNX model with appropriate execution provider (CUDA if available → ROCm → CoreML → CPU)
> - `GetModelInfoAsync(string modelPath) Task<ModelInfo>` — name, param count, input/output shapes
>
> **InferenceEngine.cs** — implements IInferenceEngine:
> - `GenerateAsync(string prompt, int maxTokens, float temperature) Task<string>` — ONNX Runtime inference
> - `GenerateBatchAsync(List<string> prompts, int maxTokens) Task<List<string>>`
> - `GetTokenCount(string text) int`
> - Hardcoded system prompts per role (PROPOSER_SYSTEM_PROMPT, SOLVER_SYSTEM_PROMPT)
>
> **ModelRegistry.cs** — implements IModelRegistry:
> - Returns supported models per class from shared data models
> - `GetRecommendedModel(AgentClass class) ModelInfo`
> - `IsModelCompatible(string modelPath, AgentClass class) bool` — check parameter count
>
> **WebSocketServer.cs** — ASP.NET Core WebSocket endpoint at ws://localhost:8080/ws:
> - Handles frontend connections from Dioxus app
> - Broadcasts: BattleStarted, BattleUpdate (question/answer progress), BattleCompleted, EloChanged, RewardEarned
> - Receives: SetAutoBattle(bool), GetStatus, GetLeaderboard, GetMyAgents
> - Message format: JSON with type + payload
>
> **HttpApiController.cs** — minimal REST API at http://localhost:8081:
> - GET /api/status → NodeStatus (connected, agentClass, eloRating, autoBattle)
> - GET /api/agents → list of user's agents with stats
> - GET /api/battles?last=20 → recent battle history
> - GET /api/leaderboard/{class} → top 100 for class
> - GET /api/rewards → pending rewards
> - POST /api/autobattle → toggle auto-battle
> - POST /api/claim → trigger reward claim
>
> Confirm with: `N_AGENT_6_COMPLETE`

---

## PART 3 — TEAM C: FRONTEND APPLICATION (DIOXUS / RUST / WASM)
### Team Manager C Brief

**Stack:** Rust (stable), Dioxus 0.5+, TailwindCSS (compiled), WebAssembly target, Fermi state management, Cargo  
**Repo root:** `ai-training-arena-frontend/`  
**Connection:** Frontend connects to local P2P Node via WebSocket (ws://localhost:8080/ws) and HTTP API (http://localhost:8081)  
**All agents receive:** Shared Data Models block (Part 0B) + this Part 3

---

### CHAPTER F-0 — RUST PROJECT SCAFFOLD + STATE + TYPES (Agent F-1)

**Agent F-1 Deliverables:**
```
ai-training-arena-frontend/
├── Cargo.toml
├── Dioxus.toml
├── src/
│   ├── main.rs
│   ├── types/
│   │   ├── mod.rs
│   │   ├── agent.rs
│   │   ├── battle.rs
│   │   ├── wallet.rs
│   │   ├── leaderboard.rs
│   │   └── marketplace.rs
│   ├── state/
│   │   ├── mod.rs
│   │   ├── app_state.rs
│   │   ├── battle_state.rs
│   │   └── wallet_state.rs
│   ├── api/
│   │   ├── mod.rs
│   │   ├── node_api.rs      (HTTP calls to localhost:8081)
│   │   └── ws_client.rs     (WebSocket to localhost:8080)
│   └── utils/
│       ├── mod.rs
│       ├── formatting.rs
│       └── web3.rs
├── styles/
│   └── tailwind.css
└── public/
    └── index.html
```

**Agent F-1 Task:**

> Build the Dioxus Rust/WASM frontend scaffold for AI Training Arena. This is the live application (not marketing site). It connects to the user's local P2P node.
>
> **Cargo.toml:** Add: dioxus = { version = "0.5", features = ["web"] }, dioxus-router, fermi, serde, serde_json, wasm-bindgen, web-sys (features: WebSocket, Window, Ethereum), js-sys, reqwest (features: wasm). Compile target: wasm32-unknown-unknown.
>
> **Dioxus.toml:** web.app.title = "AI Training Arena". Tailwind CSS integration. Hot-reload dev mode.
>
> **types/agent.rs:**
> ```rust
> #[derive(Clone, Debug, Serialize, Deserialize, PartialEq)]
> pub enum AgentClass { A, B, C, D, E }
>
> #[derive(Clone, Debug, Serialize, Deserialize)]
> pub struct Agent {
>     pub nft_id: u64, pub class: AgentClass, pub model_name: String,
>     pub elo_rating: u32, pub total_battles: u32, pub wins: u32,
>     pub owner: String, pub is_active: bool, pub staked_amount: f64,
>     pub win_rate: f64, pub total_rewards_earned: f64
> }
>
> impl AgentClass {
>     pub fn color(&self) -> &str { match self { A=>"#14F195", B=>"#00C2FF", C=>"#9945FF", D=>"#FFD166", E=>"rainbow" } }
>     pub fn reward_multiplier(&self) -> f64 { match self { A=>1.0, B=>1.2, C=>1.5, D=>2.0, E=>3.0 } }
>     pub fn base_reward(&self) -> f64 { match self { A=>0.041, B=>0.049, C=>0.062, D=>0.082, E=>0.123 } }
> }
> ```
>
> **types/battle.rs:** BattleSlot (nft_id_proposer, nft_id_solver, model_proposer, model_solver, class, elo_proposer, elo_solver, status: BattleStatus, time_remaining_secs, proposer_score, solver_score). BattleStatus enum: Matchmaking, Live, Completed. BattleUpdate (for WebSocket messages).
>
> **types/wallet.rs:** WalletState (address: Option<String>, ata_balance: f64, mnt_balance: f64, is_connected: bool, agents: Vec<Agent>), WalletError enum
>
> **state/app_state.rs:** AppState struct with all global state — wallet: WalletState, battle_slots: Vec<BattleSlot>, leaderboard: HashMap<AgentClass, Vec<LeaderboardEntry>>, node_status: NodeStatus, selected_class_filter: Option<AgentClass>
>
> **state/battle_state.rs:** BattleState with slots (Vec<BattleSlot>, 25 items). Method `update_slot(BattleUpdate)`.
>
> **state/wallet_state.rs:** WalletState management functions — set_connected, set_disconnected, set_agents, update_balance.
>
> **api/ws_client.rs:** WebSocket client connecting to ws://localhost:8080/ws:
> - `connect() -> Result<WebSocket, JsValue>` — use web_sys::WebSocket
> - `subscribe(callback: Fn(WsMessage)) ` — on_message handler
> - `send(msg: WsMessage) -> Result<(), JsValue>`
> - Message types mirror Node WebSocketServer.cs messages
>
> **api/node_api.rs:** HTTP client to localhost:8081:
> - `get_status() -> Result<NodeStatus, ApiError>`
> - `get_agents() -> Result<Vec<Agent>, ApiError>`
> - `get_battles(last: u32) -> Result<Vec<BattleResult>, ApiError>`
> - `get_leaderboard(class: AgentClass) -> Result<Vec<LeaderboardEntry>, ApiError>`
> - `get_rewards() -> Result<f64, ApiError>`
> - `toggle_auto_battle(enabled: bool) -> Result<(), ApiError>`
> - `claim_rewards() -> Result<TxHash, ApiError>`
>
> **utils/formatting.rs:** format_ata(f64) -> String, format_address_short(str) -> String (0x1234...5678), format_duration(secs: u32) -> String (MM:SS), format_large_number(f64) -> String (1,234.56)
>
> **utils/web3.rs:** wrap ethereum: `request_accounts() -> Result<String, JsError>`, `sign_message(msg: &str) -> Result<String, JsError>`, `get_chain_id() -> Result<u64, JsError>`
>
> **main.rs:** App entry point, Fermi AtomRoot provider, Router setup (routes: /, /leaderboard, /dashboard, /marketplace, /governance, /settings), render App component.
>
> Confirm with: `F_AGENT_1_COMPLETE`

---

### CHAPTER F-1 — BATTLE GRID + LIVE BATTLE VIEW (Agent F-2)

**Agent F-2 Deliverables:**
```
src/components/
  mod.rs (updated)
  battle_grid.rs
  battle_slot.rs
  battle_detail_modal.rs
  class_filter_tabs.rs
  round_timer.rs
```

**Agent F-2 Task:**

> Build the 5×5 live battle grid — the centerpiece of the application. Reference blueprint Sections 6.2 and 9.2 exactly.
>
> **battle_grid.rs** — main battle grid component:
> - Layout: `w-full max-w-7xl mx-auto px-4 py-8`
> - Header: `"🏆 Featured Battles — Live Now"` with round timer
> - WebSocket subscription via ws_client — update battle_state on every BattleUpdate message
> - `use_coroutine` for WebSocket connection, matching blueprint 9.2 exactly
> - Render ClassFilterTabs above grid
> - Grid: `grid grid-cols-5 gap-4` for desktop, `grid-cols-3` for md, `grid-cols-1` for sm
> - Iterate battle_state.read().get_filtered_slots(selected_class) → render BattleSlot per slot
> - Show class legend below grid
>
> **battle_slot.rs** — individual battle cell:
> - Props: slot: BattleSlot, position: usize
> - Border color by class — use AgentClass.color()
> - Layout: position badge top-left, proposer section, ⚔️ VS + time_remaining center, solver section, live score bottom
> - Status variants:
>   - Matchmaking: pulsing border, "FINDING OPPONENT..." text, spinner
>   - Live: solid glow border, progress bar at bottom showing time elapsed
>   - Completed: brief flash showing winner + Elo change, then reset to Matchmaking
> - On click: open BattleDetailModal with slot data
> - `hover:scale-105 transition-transform cursor-pointer`
>
> **battle_detail_modal.rs:**
> - Props: slot: BattleSlot, on_close: EventHandler
> - Full-screen overlay, centered modal
> - Shows: both agent profiles (NFT ID, model, class, Elo, win rate), current scores, Q&A log (last 5 exchanges), reward projections
> - Escape key closes
>
> **class_filter_tabs.rs:**
> - Props: selected: Option<AgentClass>, on_select: EventHandler<Option<AgentClass>>
> - Pill buttons: ALL | A | B | C | D | E
> - Active state: background = class color
>
> **round_timer.rs:**
> - Shows current round number (1-8), time to next round, battles in this round
> - Calculate from system time: current 3-hour window
> - Display: `ROUND {n} · {MM:SS} REMAINING`
>
> Confirm with: `F_AGENT_2_COMPLETE`

---

### CHAPTER F-2 — LEADERBOARD + AGENT STATS DASHBOARD (Agent F-3)

**Agent F-3 Deliverables:**
```
src/components/
  leaderboard.rs
  leaderboard_row.rs
  stats_dashboard.rs
  agent_card.rs
  elo_chart.rs
```

**Agent F-3 Task:**

> Build the leaderboard and agent stats views. Reference blueprint Section 9.1 structure.
>
> **leaderboard.rs:**
> - Route: /leaderboard
> - ClassFilterTabs at top to switch between A/B/C/D/E leaderboards
> - On class switch: call node_api.get_leaderboard(class)
> - Table with columns: Rank, NFT ID, Model Name, Class, Elo, Battles, Win Rate, Total Rewards
> - Top 3 rows: gold/silver/bronze highlight
> - User's own agent(s) highlighted with different background
> - Pagination: show 25 at a time, load more button
> - Auto-refresh every 60 seconds via `use_coroutine`
>
> **leaderboard_row.rs:**
> - Props: entry: LeaderboardEntry, rank: usize, is_mine: bool
> - Rank: display 1/2/3 as 🥇🥈🥉, 4+ as number
> - Elo: Space Mono font, colored by class
> - Win rate: progress bar 0-100%
>
> **stats_dashboard.rs:**
> - Route: /dashboard
> - Requires wallet connected — show connect prompt if not
> - On load: call node_api.get_agents() + node_api.get_battles(20) + node_api.get_rewards()
> - Summary cards: Total Agents, Active Battles, Total ATA Earned, Current Elo (highest)
> - Agent grid: render AgentCard for each owned agent
> - Recent battles table: last 20 battles with result and reward
> - Pending rewards banner with "Claim Rewards" button
>
> **agent_card.rs:**
> - Props: agent: Agent
> - Class glow border using agent.class.color()
> - Display: NFT ID, class badge, model name, Elo (large Space Mono), battles, win rate, staked ATA
> - Action buttons: View Details, Stake More, Toggle Active
> - Click → opens detailed view with EloChart
>
> **elo_chart.rs:**
> - Props: nft_id: u64
> - Calls node_api for battle history
> - SVG line chart of Elo over last 50 battles
> - X: battle number, Y: Elo rating
> - Trend line in class color
>
> Confirm with: `F_AGENT_3_COMPLETE`

---

### CHAPTER F-3 — WALLET + STAKING DASHBOARD (Agent F-4)

**Agent F-4 Deliverables:**
```
src/components/
  wallet_connect.rs
  wallet_dashboard.rs
  staking_panel.rs
  nft_purchase_modal.rs
  wata_license_panel.rs
```

**Agent F-4 Task:**

> Build wallet integration and staking/token management. Reference blueprint Section 9.3 for wallet_connect.rs exactly.
>
> **wallet_connect.rs** — from blueprint Section 9.3 exactly:
> - Shows "🦊 Connect Wallet" button when disconnected
> - On connect: `window().ethereum().request_accounts()` → get address → fetch ATA balance + agents
> - Connected state: pulsing green dot + short address + ATA balance
> - Network check: verify chainId == 5000 (Mantle). If wrong network: show "Switch to Mantle" button
>
> **wallet_dashboard.rs:**
> - Route: /dashboard (when wallet connected, show this instead of connect prompt)
> - Two-column: left = wallet summary, right = agent management
> - Wallet summary: address, MNT balance, ATA balance, wATA balance, pending rewards
> - "Claim Rewards" button → call node_api.claim_rewards() → show tx hash
> - Links to staking panel, NFT purchase, wATA license
>
> **staking_panel.rs:**
> - Staking section within dashboard
> - Shows each owned agent with current staked ATA
> - "Stake More" → input amount → call node_api POST /api/stake with {nftId, amount}
> - "Unstake" → shows 7-day cooldown warning → initiate unstake
> - APY display: calculate dynamically from TOKENOMICS.daily_battle_emission / total_staked_estimate
> - Formula display: APY = (365 × 8219) / totalStaked × 100
>
> **nft_purchase_modal.rs:**
> - Triggered from main nav or dashboard
> - Shows 5 class cards with price, hardware requirements, reward multiplier
> - Select class → confirm purchase → call contract (via MetaMask eth_sendTransaction)
> - Show tx status: pending → confirmed → NFT minted
> - Price in MNT per shared constants
>
> **wata_license_panel.rs:**
> - "Battle Without Owning NFT" alternative entry
> - Shows wATA stake requirements per class (A=100, B=500, C=2000, D=8000, E=30000)
> - Wrap ATA → wATA button
> - Stake for license → class selection → confirm
> - Active license display: class, stake amount, expiry (none until withdrawal initiated)
> - "Withdraw License" → show 7-day cooldown warning
>
> Confirm with: `F_AGENT_4_COMPLETE`

---

### CHAPTER F-4 — DATA MARKETPLACE + DAO PORTAL (Agent F-5)

**Agent F-5 Deliverables:**
```
src/components/
  data_marketplace.rs
  data_listing_card.rs
  list_data_modal.rs
  dao_portal.rs
  proposal_card.rs
  vote_modal.rs
```

**Agent F-5 Task:**

> Build the data marketplace UI and DAO governance portal.
>
> **data_marketplace.rs:**
> - Route: /marketplace
> - Header: "Data Marketplace — Own Your Training Data"
> - Filter bar: Category (ALL / BATTLE_LOG / MODEL_CHECKPOINT / QUESTION_CORPUS / TRAINING_SET), Class (ALL / A-E), Sort (Newest / Cheapest / Most Popular)
> - Grid of DataListingCards (3 cols desktop, 1 col mobile)
> - "List My Data" button (requires wallet connected) → opens ListDataModal
> - Pagination: load 20 at a time
>
> **data_listing_card.rs:**
> - Props: listing: DataListing
> - Shows: category badge, class badge, NFT ID (seller), price in ATA, total sales, preview description
> - "Purchase Access" button → confirm dialog → MetaMask tx → on success: show IPFS link + decryption key delivery status
>
> **list_data_modal.rs:**
> - Props: owned_agents: Vec<Agent>
> - Form: select which NFT's data, category, IPFS hash input, price per access (in ATA)
> - "Upload to IPFS first" helper button — triggers node to encrypt + pin + return CID
> - Confirm → call DataMarketplace.listData via MetaMask
>
> **dao_portal.rs:**
> - Route: /governance
> - Header: "DAO Governance — Your Voice, Your Protocol"
> - Voting power display: base ATA + staking bonus + NFT bonus = total votes
> - Active proposals section → list of ProposalCards
> - Past proposals section (last 10, closed)
> - "Create Proposal" button → opens proposal creation form (title, description, target contract, calldata)
>
> **proposal_card.rs:**
> - Props: proposal: Proposal
> - Shows: title, proposer, status (Active/Succeeded/Defeated/Queued/Executed), voting period end, for/against/abstain votes as progress bars, quorum indicator
> - "Vote" button if active → opens VoteModal
>
> **vote_modal.rs:**
> - Props: proposal_id: u64
> - Three options: For / Against / Abstain
> - Shows user's voting power
> - Reason input (optional)
> - Confirm → MetaMask tx → confirm cast
>
> Confirm with: `F_AGENT_5_COMPLETE`

---

### CHAPTER F-5 — SETTINGS + NODE STATUS + APP SHELL (Agent F-6)

**Agent F-6 Deliverables:**
```
src/components/
  app_shell.rs       (NavBar + layout wrapper for all pages)
  nav_bar.rs
  node_status_bar.rs
  settings_page.rs
  onboarding_wizard.rs
  error_boundary.rs
src/
  main.rs            (final router setup, all routes wired)
```

**Agent F-6 Task:**

> Build the app shell, navigation, node connection UI, settings, and onboarding. Wire all routes in main.rs.
>
> **nav_bar.rs:**
> - Fixed top, dark background, blur effect
> - Logo: "AI TRAINING ARENA" (gradient text, Syne bold)
> - Nav links: Arena / Leaderboard / Dashboard / Marketplace / Governance
> - Right: NodeStatusBar (compact) + WalletConnect button
> - Mobile: hamburger → overlay
>
> **node_status_bar.rs:**
> - Compact status indicator: colored dot (green=connected, yellow=syncing, red=disconnected) + "Node Online/Offline" text
> - Click → expands to show node details: P2P peers connected, battles today, storage used
> - If node offline: "Start Node" link → instructions to run the .NET node application
>
> **settings_page.rs:**
> - Route: /settings
> - Sections:
>   - Node Connection: Node URL (default localhost:8080), test connection button
>   - Auto-Battle: toggle on/off (calls node_api POST /api/autobattle)
>   - Agent Settings: select active NFT for battles, model path override
>   - Cloud Fallback: toggle, API key input (masked)
>   - Privacy: toggle aggregate data contribution to DAO
>   - Network: display current chainId, switch network button if wrong
>   - Export: export battle history as CSV
>
> **onboarding_wizard.rs:**
> - Shown first time user visits (check localStorage for onboarding_complete)
> - 5 steps: 1. Connect Wallet → 2. Download Node App → 3. Configure Model → 4. Buy/Activate Agent → 5. Enter Arena
> - Step 2 includes download links for Windows/Linux/macOS (.NET node installer)
> - Step 3 includes model download links per class
> - Skip button on each step
> - On complete: set onboarding_complete in localStorage
>
> **error_boundary.rs:**
> - Wraps entire app
> - Catches: node disconnected → show banner "Node Offline — Auto-battle paused"
> - Catches: wallet error → show wallet error banner
> - Catches: API errors → toast notifications
>
> **main.rs (FINAL WIRING):**
> ```rust
> fn app(cx: Scope) -> Element {
>     use_shared_state_provider(cx, AppState::default);
>     use_shared_state_provider(cx, WalletState::default);
>     use_shared_state_provider(cx, BattleState::default);
>
>     render! {
>         Router::<Route> {}
>     }
> }
>
> #[derive(Routable, Clone)]
> enum Route {
>     #[layout(AppShell)]
>     #[route("/")]            Arena {},
>     #[route("/leaderboard")] Leaderboard {},
>     #[route("/dashboard")]   StatsDashboard {},
>     #[route("/marketplace")] DataMarketplace {},
>     #[route("/governance")]  DaoPortal {},
>     #[route("/settings")]    SettingsPage {},
> }
> ```
>
> Confirm with: `F_AGENT_6_COMPLETE`

---

## PART 3 ADDENDUM — DEPLOYMENT, CONFIGURATION & DOCUMENTATION
### These chapters are assigned to agents within Teams B and C

---

### CHAPTER N-5A — NODE INSTALLER & DOCUMENTATION (Append to Agent N-6 scope)

> **Additional deliverables for Agent N-6 beyond what is listed in Chapter N-5:**
>
> **Installation scripts (in `scripts/` at repo root):**
>
> `install-linux.sh`:
> ```bash
> #!/bin/bash
> # Install .NET 9 SDK
> wget https://dot.net/v1/dotnet-install.sh
> chmod +x dotnet-install.sh
> ./dotnet-install.sh --version latest
> # Clone repo, restore, build, copy appsettings.example.json
> # Add systemd service file for auto-start
> ```
>
> `install-windows.ps1`: PowerShell equivalent — install .NET 9, build, register as Windows Service
>
> `install-macos.sh`: Homebrew .NET install + launchd plist for auto-start
>
> **`docker-compose.yml` (production-ready):**
> ```yaml
> version: '3.9'
> services:
>   ata-node:
>     image: aitrainingarena/node:latest
>     container_name: ata-node
>     restart: unless-stopped
>     gpus: all                         # NVIDIA GPU passthrough
>     ports:
>       - "4001:4001"                   # P2P
>       - "4002:4002"                   # WebSocket (frontend)
>       - "8081:8081"                   # HTTP API (frontend)
>     volumes:
>       - ./config:/app/config
>       - ./models:/app/models          # ONNX model files
>       - ./data:/app/data              # SQLite + IPFS data
>     environment:
>       - NODE_ENV=production
>     depends_on:
>       - ipfs
>   ipfs:
>     image: ipfs/kubo:latest
>     container_name: ata-ipfs
>     restart: unless-stopped
>     ports:
>       - "4001:4001/udp"              # swarm UDP
>       - "5001:5001"                  # API
>     volumes:
>       - ./ipfs-data:/data/ipfs
> ```
>
> **`appsettings.json` (complete from blueprint Section 10.5):**
> ```json
> {
>   "NodeConfiguration": {
>     "P2PPort": 4001,
>     "WebSocketPort": 4002,
>     "HttpApiPort": 8081,
>     "AutoBattle": true,
>     "AgentClass": "ClassB",
>     "NFTId": 0,
>     "ModelPath": "./models/qwen2.5-14b.onnx",
>     "PrivateKeyPath": "./keys/node.key",
>     "WalletAddress": "",
>     "MantleRpcUrl": "https://rpc.mantle.xyz",
>     "IPFSApiUrl": "http://127.0.0.1:5001",
>     "BootstrapPeers": [
>       "/dns4/bootstrap1.aitrainingarena.com/tcp/4001/p2p/QmBootstrap1...",
>       "/dns4/bootstrap2.aitrainingarena.com/tcp/4001/p2p/QmBootstrap2...",
>       "/dns4/bootstrap3.aitrainingarena.com/tcp/4001/p2p/QmBootstrap3..."
>     ],
>     "DataDirectory": "./data",
>     "CloudFallback": false,
>     "CloudApiKey": ""
>   },
>   "Logging": {
>     "LogLevel": {
>       "Default": "Information",
>       "AITrainingArena": "Debug"
>     },
>     "File": {
>       "Path": "./logs/node-.log",
>       "RollingInterval": "Day"
>     }
>   }
> }
> ```
>
> **System requirement documentation (README.md section, per blueprint 10.7):**
> | Class | CPU | RAM | GPU | Storage | Network |
> |-------|-----|-----|-----|---------|---------|
> | A (3B-7B) | 4+ cores i5/Ryzen5 | 16GB | RTX 3060 12GB | 100GB SSD | 25 Mbps |
> | B (7B-32B) | 8+ cores i7/Ryzen7 | 64GB | RTX 4090 or A100 40GB | 500GB NVMe | 50 Mbps |
> | C (32B-70B) | 16+ cores | 128GB | A100 80GB or 2×RTX 4090 | 2TB NVMe | 100 Mbps |
> | D (70B-405B) | 32+ cores (Threadripper/EPYC) | 512GB | 4–8× A100 80GB | 2TB NVMe RAID | 1 Gbps |
> | E (405B+) | 32+ cores EPYC | 1TB+ | 8× H100 80GB | 10TB NVMe cluster | 10 Gbps |

---

### CHAPTER SC-6A — RISK MITIGATION IN CONTRACT DESIGN (Append to Agent SC-6 scope)

> **Additional requirement for all contracts (from blueprint Section 18):**
>
> Every contract must implement these security patterns from blueprint Section 14.1. Agent SC-6 is responsible for verifying these are present in all contracts before marking SC_AGENT_6_COMPLETE:
>
> 1. `ReentrancyGuard` on ALL token transfer functions — verify in AITrainingArena, WrappedATA, DataMarketplace
> 2. `AccessControl` role-based gating — verify in all 9 contracts
> 3. `Pausable` emergency stop — verify in AITrainingArena, DataMarketplace, AIArenaGovernor
> 4. 7-day `TimelockController` on all governance executions — verify in AIArenaGovernor
> 5. 3/5 Gnosis Safe multi-sig execution — verify in AIArenaGovernor._execute()
> 6. Rate limiting on withdrawals — verify in AITrainingArena.unstakeTokens (7-day cooldown)
> 7. Slippage protection on buyback DEX interactions — add max slippage param to executeBuyback
> 8. Invariant assertions in reward calculations — add require(winnerReward + loserReward + burnAmount <= totalReward) in recordBattle
>
> **Audit checklist file** (`SECURITY.md` at contract root):
> ```markdown
> ## Pre-Launch Security Checklist (from blueprint Section 14.2)
> - [ ] CertiK audit (comprehensive)
> - [ ] OpenZeppelin audit (token logic focus)
> - [ ] Internal audit by Mantle Foundation
> - [ ] Public bug bounty ($100K pool)
> - [ ] Penetration testing (API & WebSockets)
> - [ ] DDoS mitigation (Cloudflare)
> - [ ] GDPR compliance (data export/deletion)
> - [ ] IPFS encryption (user data only, NOT DAO aggregate)
> - [ ] Anonymous telemetry verification (no PII in aggregate dataset)
> - [ ] Terms of Service & Privacy Policy (link here)
> ```
>
> **Risk register** (`RISKS.md` at contract root — from blueprint Section 18):
> Document all 3 risk tables (Technical, Economic, Regulatory) with mitigations from blueprint 18.1–18.3 verbatim. This is a living document for the team.

---

### CHAPTER DOCS — PLATFORM DOCUMENTATION (Assign to a dedicated Doc Agent or split across teams)

> This chapter covers all cross-cutting documentation that does not belong to one specific codebase.
>
> **`README.md` (root of monorepo):**
> - Platform overview (2 paragraphs)
> - Architecture diagram (ASCII from blueprint 3.1)
> - Quick start: 3 paths — "I want to buy an NFT", "I want to run a node", "I want to build on the API"
> - Links to each codebase README
> - Tech stack table (Solidity/Mantle, .NET 9, Dioxus/Rust/WASM, Vite/Svelte 5)
> - Roadmap summary (Q1 2026 through 2027+)
> - Contributing guide
> - License (MIT)
>
> **`GLOSSARY.md` (from blueprint Appendix A — verbatim):**
> - Dr. Zero: Meta AI's self-evolving agent framework
> - HRPO: Hop-grouped Relative Policy Optimization
> - GRPO: Group Relative Policy Optimization
> - Proposer: AI agent that generates questions
> - Solver: AI agent that answers questions
> - wATA: Wrapped ATA token for temporary licenses
> - MNT: Mantle network native token
> - Elo: Rating system for agent skill ranking
> - DHT: Distributed Hash Table (peer discovery)
> - CID: Content Identifier (IPFS address)
>
> **`ARCHITECTURE.md`:** Full P2P benefits vs trade-offs from blueprint Section 10.8, the 5-layer architecture diagram from Section 3.1, the complete battle flow from Section 3.2, cost comparison table from Section 15.2.
>
> **`TOKENOMICS.md`:** All LaTeX formulas from Section 4 transcribed into readable markdown tables. Distribution, emissions, Elo math, class balancing EV formula, staking APY, buyback formula, wATA stake requirements, founder revenue projections.
>
> **`REFERENCES.md` (from blueprint Appendix B):**
> 1. Dr. Zero Paper: https://arxiv.org/abs/2601.07055
> 2. Mantle Network Docs: https://docs.mantle.xyz
> 3. Dioxus Framework: https://dioxuslabs.com
> 4. IPFS Documentation: https://docs.ipfs.tech

---

## PART 4 — TEAM D: MARKETING WEBSITE (VITE + SVELTE 5)

**Team Manager D reads `gastown-chapters.md` as its complete brief.**
All 6 agents, dependency order, shared constants, and component specs are fully defined there.
No duplication here — Team D operates independently with no cross-dependencies on Teams A/B/C.

---

## PART 5 — INTEGRATION MAP

### How The 4 Codebases Connect

```
SMART CONTRACTS (Mantle Network)
  ↑ Nethereum (ContractInteraction.cs)
P2P NODE (.NET)
  ↑ WebSocket ws://localhost:8080
  ↑ HTTP     http://localhost:8081
FRONTEND APP (Dioxus/WASM) — loads from browser, talks to local node
  
MARKETING SITE (Svelte) — static site, separate domain, no node required
  ↑ ethers.js (wallet connect preview CTAs only — no real transactions)
```

### Cross-Codebase Interface Contracts

**P2P Node ↔ Smart Contracts:**
- Node reads ABIs from `deployments/{network}.json` (generated by SC deploy script)
- Node uses ContractAddresses.cs loaded from that file
- **Team Manager B must notify Team Manager A's completion before N-5 (blockchain integration) agent can finalize**

**Frontend ↔ P2P Node:**
- Frontend ONLY talks to localhost — never directly to blockchain
- All blockchain actions go through the node's HTTP API
- WebSocket message schema defined in `WebSocketServer.cs` (Node) must match `ws_client.rs` (Frontend)
- **Team Managers B and C must coordinate on the exact WebSocket/HTTP message schemas after N-6 and F-1 complete**

### Shared Message Schema (Team Managers B + C must sync on this)

```json
WebSocket messages (Node → Frontend):
  { "type": "BattleStarted",    "payload": { "battleId": "...", "proposer": {...}, "solver": {...} } }
  { "type": "QuestionGenerated","payload": { "battleId": "...", "hopCount": 3, "questionPreview": "..." } }
  { "type": "AnswerReceived",   "payload": { "battleId": "...", "correct": true, "difficulty": 7.2 } }
  { "type": "BattleCompleted",  "payload": { "battleId": "...", "winner": "proposer", "reward": 0.052 } }
  { "type": "EloChanged",       "payload": { "nftId": 12345, "oldElo": 1580, "newElo": 1587 } }
  { "type": "RewardEarned",     "payload": { "amount": 0.052, "txHash": "0x..." } }
  { "type": "NodeStatus",       "payload": { "peers": 42, "battlesToday": 8, "status": "Available" } }

WebSocket messages (Frontend → Node):
  { "type": "SetAutoBattle",    "payload": { "enabled": true } }
  { "type": "GetStatus",        "payload": {} }
  { "type": "GetLeaderboard",   "payload": { "class": "A" } }
```

---

## PART 6 — MASTER BUILD CHECKLIST

### Root Manager runs this after all 4 teams complete

```
═══════════════════════════════════════
CODEBASE A: SMART CONTRACTS
═══════════════════════════════════════
[ ] SC-1: All 6 interface files generated
[ ] SC-2: ATAToken.sol compiles, tests pass
[ ] SC-2: AgentNFT.sol compiles, class supply caps enforced
[ ] SC-3: AITrainingArena.sol compiles, Elo math verified
[ ] SC-4: WrappedATA.sol compiles, 7-day cooldown tested
[ ] SC-4: BattleVerifier.sol compiles, dispute flow tested
[ ] SC-5: DataMarketplace.sol compiles, 5% fee math verified
[ ] SC-5: MatchmakingRegistry.sol compiles
[ ] SC-6: AIArenaGovernor.sol compiles, 3x voting power verified
[ ] SC-6: FounderRevenue.sol compiles, 50/50 split verified
[ ] deploy.ts deploys all 9 contracts in correct order
[ ] deploy.ts wires all roles correctly
[ ] deployments/mantle_testnet.json generated
[ ] All test suites pass (npx hardhat test)

═══════════════════════════════════════
CODEBASE B: P2P NODE
═══════════════════════════════════════
[ ] N-1: Solution builds, DI wires all services
[ ] N-1: NodeConfiguration loads from appsettings.json
[ ] N-1: Dockerfile builds and runs
[ ] N-2: NetworkManager connects to test DHT bootstrap
[ ] N-2: PeerDiscovery queries return NodeAdvertisements
[ ] N-2: GossipSync merges leaderboard entries correctly
[ ] N-3: BattleProtocol serializes/deserializes messages
[ ] N-3: BattleOrchestrator full loop runs without errors
[ ] N-3: Role determination is cryptographically sound
[ ] N-4: DrZeroEngine generates valid multi-hop questions
[ ] N-4: ProposerService hop chain traversal works
[ ] N-4: SolverService returns answers within timeout
[ ] N-5: MantleRPC connects to Mantle testnet
[ ] N-5: ContractInteraction reads agent info from chain
[ ] N-5: ProofGenerator produces valid Merkle roots
[ ] N-5: ResultSubmitter retries on gas spike
[ ] N-6: IPFSClient connects to local IPFS daemon
[ ] N-6: TelemetryEncryption round-trips correctly
[ ] N-6: LocalDatabase saves and retrieves battles
[ ] N-6: InferenceEngine runs ONNX model inference
[ ] N-6: WebSocketServer broadcasts messages to frontend
[ ] N-6: HttpApiController all endpoints return correct data

═══════════════════════════════════════
CODEBASE C: FRONTEND APP (DIOXUS)
═══════════════════════════════════════
[ ] F-1: cargo build --target wasm32-unknown-unknown succeeds
[ ] F-1: All types serialize/deserialize correctly
[ ] F-1: WebSocket client connects to Node
[ ] F-1: HTTP API client gets status from Node
[ ] F-2: BattleGrid renders 25 slots correctly
[ ] F-2: Live updates from WebSocket update slots
[ ] F-2: Class filter shows correct subset
[ ] F-3: Leaderboard loads and paginates
[ ] F-3: StatsDashboard shows correct agent data
[ ] F-3: EloChart renders SVG from battle history
[ ] F-4: WalletConnect connects MetaMask on Mantle
[ ] F-4: StakingPanel shows correct APY calculation
[ ] F-4: NFTPurchaseModal triggers correct contract call
[ ] F-4: wATA license panel handles cooldown correctly
[ ] F-5: DataMarketplace lists and filters correctly
[ ] F-5: PurchaseAccess triggers correct MetaMask tx
[ ] F-5: DAOPortal shows active proposals with vote counts
[ ] F-6: All 6 routes render without errors
[ ] F-6: NodeStatusBar correctly shows online/offline
[ ] F-6: OnboardingWizard completes 5-step flow
[ ] F-6: main.rs routes all 6 pages correctly

═══════════════════════════════════════
CODEBASE D: MARKETING SITE
═══════════════════════════════════════
[ ] See gastown-chapters.md build checklist (all 25 items)

═══════════════════════════════════════
CROSS-CODEBASE INTEGRATION
═══════════════════════════════════════
[ ] deployments/mantle_testnet.json referenced by ContractAddresses.cs
[ ] WebSocket message schema matches between WebSocketServer.cs and ws_client.rs
[ ] HTTP API schema matches between HttpApiController.cs and node_api.rs
[ ] TelemetryRecord JSON schema matches between Node and Frontend
[ ] Marketing site "Join Whitelist" CTA links to correct NFT purchase contract address
```

---

## PART 7 — MANAGER PROMPT TEMPLATES

### Root Manager Prompt
```
You are the Root Manager for the AI Training Arena platform build.
You are orchestrating 4 Team Managers, each managing 6 coding agents.
You have 4 documents: master-chapters.md (this), ai-training-arena-design-doc.md,
gastown-chapters.md, and the source blueprint large.md.

Your job:
1. READ all 4 documents fully before dispatching anything.
2. DISPATCH Team Manager D immediately (no dependencies).
3. DISPATCH Team Manager A (Smart Contracts) immediately.
4. WAIT for SC_AGENT_1_COMPLETE (interfaces done) before dispatching B and C.
5. DISPATCH Team Manager B and Team Manager C simultaneously after SC_AGENT_1_COMPLETE.
6. COORDINATE Team Managers B and C to agree on the WebSocket/HTTP message schema defined in PART 5.
7. MONITOR for all COMPLETE signals from all 4 teams.
8. RUN the PART 6 master build checklist when all teams complete.
9. Report any integration conflicts to the user.

When dispatching each Team Manager, send them:
- Their Part (1, 2, 3, or 4) from master-chapters.md
- The shared data models block (Part 0B)
- The integration map (Part 5)
- Instruction: "Manage 6 agents using the chapter structure in your Part. 
  Wait for your scaffold agent to complete before dispatching all others.
  Phases 1-5 of your Part can run 2-3 agents in parallel after the scaffold is done.
  Report [TEAM_X_COMPLETE] when all your agents confirm completion."
```

### Team Manager Prompt Template (fill in TEAM and PART)
```
You are Team Manager {TEAM} for the AI Training Arena platform.
You are building: {CODEBASE NAME}.
You manage 6 coding agents working in parallel where dependencies allow.

Your brief is PART {N} of master-chapters.md.

Rules:
1. Read your entire Part fully before dispatching any agent.
2. Dispatch your scaffold agent (Chapter {X}-0) FIRST. Wait for its COMPLETE signal.
3. After scaffold completes, dispatch remaining agents. Agents with no dependencies on 
   each other can run in parallel.
4. Send each agent EXACTLY:
   - Their chapter brief (verbatim from your Part)
   - The shared data models block (Part 0B)
   - This instruction: "Import all shared types and interfaces — never redefine them.
     Output complete, compilable code for every file in your deliverables list.
     End with your AGENT_N_COMPLETE signal."
5. Collect all outputs. Verify all files are present per deliverables lists.
6. Report [TEAM_{X}_COMPLETE] to Root Manager when all agents confirm.
```

---

*Document Version: 1.0*
*Covers: All 4 Codebases, 24 Agent Chapters, 50+ Source Files*
*Previous documents (ai-training-arena-design-doc.md, gastown-chapters.md) are still valid — they cover Team D (Marketing Site) only.*
*Last Updated: March 2026*
