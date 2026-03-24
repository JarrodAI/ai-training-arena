# AI Training Arena — Put It All Together
**Date:** 2026-03-24

---

## FEATURE SUGGESTION / HONEST TAKE

**Is this a good idea?** Yes — with real caveats.

**What's good:** The core loop (AI agents battle → earn tokens → sell training data) is genuinely novel. The onchain data marketplace is the most interesting part — nobody is doing "pay for AI training telemetry" at this scale. The Gas Town recursive agent orchestration is itself a product demo.

**What's risky:** Blockchain gaming has a brutal graveyard. The moat is not the token — it's the training data quality. If battles don't produce *actually useful* training signal, the data marketplace is worthless and the whole economy collapses.

**Futuristic ideas worth pursuing:**

1. **AI Model Rental Market** — Let agents rent their trained weights to other agents (pay-per-inference, onchain). Turn the platform into a model marketplace, not just a game.
2. **Federated Fine-Tuning** — Pool anonymized battle data → auto-fine-tune open models (Mistral, LLaMA) → sell improved checkpoints. Real ML value, not just speculation.
3. **Cross-Chain Agent Passports** — AgentNFTs that carry Elo/history across chains. Become the "LinkedIn for AI agents."
4. **Human-vs-Agent Battles** — Open the arena to human players. Humans earn by beating agents; agents learn from losses. Viral growth mechanic.
5. **Enterprise Training Contracts** — Let companies post bounties for agents to solve specific domain problems. Agent earns ATA; company gets labeled data. B2B revenue without tokens being the exit.

---

## WHAT WAS BUILT

| Codebase | Tech | Status |
|---|---|---|
| Smart Contracts | Solidity 0.8.28, Hardhat, Mantle EVM | Scaffolded, partial impl |
| P2P Node | .NET 9, ASP.NET Core, SignalR, CQRS | Scaffolded, partial impl |
| Frontend App | Rust/Dioxus, WASM | ICO page done, arena UI not started |
| Marketing Site | Svelte 5, Tailwind 4, Vite | All sections built, needs API wiring |

**13 Smart Contracts:** ATAToken, AgentNFT, AITrainingArena, BattleVerifier, WrappedATA, DataMarketplace, MatchmakingRegistry, AIArenaGovernor, FounderRevenue, NodeSaleICO, VestingSchedule, NodeTrainerNFT + 2 mocks

**Orchestration:** Gas Town (Mayor + 7 Polecats) + Beads issue tracker. Phase 0 done, Phase 1 in progress, Phases 2-3 not started.

---

## WHAT CONNECTS TO WHAT

```
Marketing Site (Svelte)
  → MetaMask/WalletConnect → Smart Contracts (Mantle)
  → [MISSING] No live API integration yet

Frontend App (Dioxus WASM)
  → WebSocket → P2P Node (.NET)     [NOT WIRED YET]
  → ethers.js → Smart Contracts     [NOT WIRED YET]

P2P Node (.NET)
  → Nethereum → Smart Contracts     [NOT WIRED YET]
  → IPFS/Azure Blob → Data          [NOT IMPLEMENTED]
  → MatchmakingRegistry → Other Nodes [NOT IMPLEMENTED]

Smart Contracts
  → Chainlink Oracle (mock only)    [REAL ORACLE NOT INTEGRATED]
  → Mantle Network                  [NOT DEPLOYED]
```

---

## WHAT DOES NOT CONNECT (YET)

- .NET node ↔ Smart contracts (ABI calls not wired)
- Dioxus frontend ↔ .NET node (WebSocket not connected)
- Marketing site ↔ Any backend (fully static)
- IPFS: no client code in node yet
- Chainlink: only a mock oracle exists
- Contract addresses in `shared/constants.json` are all `0x0000...` (not deployed)

---

## HOW TO GET IT LIVE

### Step 1 — Finish the contracts
```bash
cd ai-training-arena-contracts
npm install
npx hardhat compile
npx hardhat test
# Set .env: PRIVATE_KEY, MANTLE_RPC_URL, ETHERSCAN_API_KEY
npx hardhat run scripts/deploy.ts --network mantleTestnet
# Copy deployed addresses into shared/constants.json
```

### Step 2 — Deploy the .NET node
```bash
cd ai-training-arena-node
dotnet restore && dotnet build
# Set env vars: contract addresses, RPC URL, IPFS endpoint, Azure connection strings
dotnet run --project src/AITrainingArena.API
# OR: docker build + push to Azure Container Apps / AKS
```

### Step 3 — Build & deploy the WASM frontend
```bash
cd ai-training-arena-frontend
cargo build --target wasm32-unknown-unknown
# OR: wasm-pack build
# Point WASM to node's WebSocket URL in config
# Deploy pkg/ to Azure Container Apps or Cloudflare Pages
```

### Step 4 — Deploy the marketing site
```bash
cd ai-training-arena-marketing
pnpm install && pnpm run build
vercel --prod
# Update contract addresses in marketing site's ethers.js config
```

### Step 5 — Infrastructure
- Provision Azure: AKS, MySQL Enterprise, Key Vault, Blob Storage, App Insights
- Set up IPFS node (or Pinata/Infura IPFS gateway)
- Configure Chainlink oracle on Mantle (replace MockChainlinkOracle)
- Set up Gnosis Safe multi-sig (3/5) for DAO

---

## WHAT'S NEEDED BEFORE MAINNET

| Item | Priority |
|---|---|
| Complete contract implementations (BattleVerifier, DataMarketplace, MatchmakingRegistry, AIArenaGovernor) | CRITICAL |
| P2P node battle engine + IPFS adapter | CRITICAL |
| Dioxus WASM arena UI (battle page, leaderboard, staking) | CRITICAL |
| Wire WebSocket: frontend ↔ node | CRITICAL |
| Wire Nethereum: node ↔ contracts | CRITICAL |
| Real Chainlink oracle integration | HIGH |
| Smart contract audit | HIGH (do NOT skip) |
| Load test node matchmaking at scale | HIGH |
| Marketing site live stats wired to real API | MEDIUM |
| Azure infra fully provisioned | MEDIUM |
| Tokenomics vesting/lockup enforcement tested | MEDIUM |

---

## WHAT WILL REQUIRE ONGOING UPDATES

- **ATA tokenomics** — Emission rates, burn %, buyback % will need governance tuning once live
- **Elo K-factor** — May need adjustment as real battle data comes in
- **Agent class pricing** — MNT price is volatile; consider USD-pegged pricing oracle
- **WASM frontend** — Dioxus 0.6 is new; expect API churn with future releases
- **Mantle network** — Still maturing; RPC reliability, gas pricing will evolve
- **Data marketplace pricing** — Hard to know right value until real buyers show up
- **Smart contract upgrades** — Contracts are NOT upgradeable by default; plan for this
- **Node discovery** — P2P matchmaking at scale is an unsolved performance problem

---

## QUICK REFERENCE: KEY FILES

| Purpose | File |
|---|---|
| Full spec (read this) | `gastown/master-chapters.md` |
| Shared constants / addresses | `shared/constants.json` |
| Contract deploy script | `ai-training-arena-contracts/scripts/deploy.ts` |
| Node entry point | `ai-training-arena-node/src/AITrainingArena.API/Program.cs` |
| WASM app entry | `ai-training-arena-frontend/src/main.rs` |
| Marketing homepage | `ai-training-arena-marketing/src/routes/+page.svelte` |
| Build verification | `gastown/master-build-loop.sh` |
| Agent rules | `gastown/ai-training-arena-rules.instructions.md` |

---

## ENVIRONMENT VARIABLES NEEDED

```env
# Contracts
PRIVATE_KEY=
MANTLE_RPC_URL=https://rpc.mantle.xyz
MANTLE_TESTNET_RPC_URL=https://rpc.testnet.mantle.xyz
ETHERSCAN_API_KEY=

# Node (.NET)
ATA_TOKEN_ADDRESS=
AGENT_NFT_ADDRESS=
ARENA_ADDRESS=
BATTLE_VERIFIER_ADDRESS=
MANTLE_RPC=
IPFS_ENDPOINT=
AZURE_STORAGE_CONNECTION_STRING=
AZURE_MYSQL_CONNECTION_STRING=

# Frontend (WASM config)
NODE_WEBSOCKET_URL=ws://localhost:5000/arena

# Marketing (Svelte)
PUBLIC_ARENA_CONTRACT=
PUBLIC_MANTLE_CHAIN_ID=5000
```
