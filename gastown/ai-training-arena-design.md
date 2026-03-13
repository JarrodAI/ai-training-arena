# AI TRAINING ARENA — GAS TOWN CHAPTER BREAKDOWN
### Manager Agent Reference · 6 Coding Agents · Recursive Orchestration

---

## HOW TO USE THIS DOCUMENT

This document is the **manager agent's source of truth**. It defines:
1. The dependency order (what must be built before what)
2. The exact scope each coding agent receives
3. The shared constants/tokens every agent must import (never reinvent)
4. The assembly instructions once all agents finish

The **full design spec** is in `ai-training-arena-design-doc.md`. The manager agent reads both. Coding agents receive ONLY their chapter brief + the shared constants file.

---

## DEPENDENCY ORDER (CRITICAL — DO NOT VIOLATE)

```
Phase 0 — AGENT 1 must finish before any other agent starts
  └─ globals.css, tailwind config, fonts, Svelte project scaffold, constants.js

Phase 1 — AGENTS 2 + 3 can run in PARALLEL after Phase 0
  └─ Agent 2: NavBar + Hero + How It Works
  └─ Agent 3: Agent Classes + Battle Arena Preview

Phase 2 — AGENTS 4 + 5 can run in PARALLEL after Phase 0
  └─ Agent 4: Dr. Zero Explainer + Tokenomics + Architecture
  └─ Agent 5: Staking Calculator + DAO Governance + Roadmap

Phase 3 — AGENT 6 runs after Phase 0 (no dependency on 2–5)
  └─ Agent 6: Investor Metrics + FAQ + Footer + App.svelte assembly

Phase 4 — MANAGER assembles all outputs, resolves import paths, runs final build check
```

---

## SHARED CONSTANTS (manager injects into every agent brief)

The manager must paste the following block into every agent's task. This prevents each agent from inventing their own color values or token data.

```js
// src/lib/constants.js — DO NOT MODIFY, import from here
export const COLORS = {
  bgVoid:        '#040B14',
  bgSurface:     '#080F1C',
  bgElevated:    '#0D1828',
  bgBorder:      '#1A2840',
  solPurple:     '#9945FF',
  solPurpleDim:  '#6B2FCC',
  solGreen:      '#14F195',
  solGreenDim:   '#0BBF76',
  electricBlue:  '#00C2FF',
  flameOrange:   '#FF6B35',
  gold:          '#FFD166',
  dangerRed:     '#FF4444',
  textPrimary:   '#F0F4FF',
  textSecondary: '#8899BB',
  textMuted:     '#4A5A7A',
}

export const AGENT_CLASSES = [
  {
    id: 'A', name: 'Entry Gladiators', params: '3B–7B', nfts: 15000,
    price_mnt: 10, price_usd: 5, multiplier: 1.0, base_reward_ata: 0.041,
    hardware: 'RTX 3060+', glow: '#14F195',
    models: ['Qwen 2.5 3B/7B', 'Llama 3.2 3B'],
  },
  {
    id: 'B', name: 'Mid-Tier Gladiators', params: '7B–32B', nfts: 6000,
    price_mnt: 50, price_usd: 25, multiplier: 1.2, base_reward_ata: 0.049,
    hardware: 'RTX 4090 / A100 40GB', glow: '#00C2FF',
    models: ['Qwen 2.5 14B/32B', 'Mistral 7B/22B'],
  },
  {
    id: 'C', name: 'Elite Strategists', params: '32B–70B', nfts: 2500,
    price_mnt: 200, price_usd: 100, multiplier: 1.5, base_reward_ata: 0.062,
    hardware: 'A100 80GB / 2x RTX 4090', glow: '#9945FF',
    models: ['Qwen 2.5 72B', 'Llama 3.1 70B'],
  },
  {
    id: 'D', name: 'Master Architects', params: '70B–405B', nfts: 1200,
    price_mnt: 800, price_usd: 400, multiplier: 2.0, base_reward_ata: 0.082,
    hardware: '4–8x A100 80GB', glow: '#FFD166',
    models: ['Llama 3.1 405B', 'Qwen 2.5 235B'],
  },
  {
    id: 'E', name: 'Titan Sentinels', params: '405B+', nfts: 300,
    price_mnt: 3000, price_usd: 1500, multiplier: 3.0, base_reward_ata: 0.123,
    hardware: '8x H100 80GB (cloud only)', glow: 'rainbow',
    models: ['Llama 3.1 405B full', 'GPT-4 class'],
  },
]

export const TOKENOMICS = {
  total_supply: 100_000_000,
  symbol: 'ATA',
  chain: 'Mantle Network (EVM)',
  distribution: [
    { label: 'NFT Sales (locked)',    pct: 35, color: '#9945FF' },
    { label: 'Battle Rewards Pool',   pct: 30, color: '#14F195' },
    { label: 'Staking Rewards',       pct: 15, color: '#00C2FF' },
    { label: 'DAO Treasury',          pct: 10, color: '#FFD166' },
    { label: 'Founders (vested)',      pct:  5, color: '#FF6B35' },
    { label: 'Liquidity',             pct:  3, color: '#8899BB' },
    { label: 'Team & Advisors',       pct:  2, color: '#4A5A7A' },
  ],
  daily_battle_emission: 8219,
  staking_pool: 15_000_000,
}

export const ROADMAP = [
  {
    quarter: 'Q1 2026', status: 'complete', label: 'Research & Design',
    items: ['Dr. Zero framework analysis','Tokenomics modeling','Architecture design'],
  },
  {
    quarter: 'Q2 2026', status: 'active', label: 'Smart Contracts & Node Alpha',
    items: ['ERC-721 NFT contract','ERC-20 ATA token','Battle Verifier','DAO Governor','CertiK audit'],
  },
  {
    quarter: 'Q3 2026', status: 'upcoming', label: 'Mainnet Launch',
    items: ['Whitelist sale (1K NFTs)','Public sale (24K NFTs)','Staking activation','Data Marketplace','CMC/CoinGecko listings'],
  },
  {
    quarter: 'Q4 2026', status: 'upcoming', label: 'Cross-Chain & Expansion',
    items: ['Ethereum L1 bridge','Polygon bridge','Custom model uploads','Advanced analytics'],
  },
  {
    quarter: '2027+', status: 'future', label: 'Vision',
    items: ['Mobile app','$1M prize tournaments','Enterprise API','Decentralized compute network'],
  },
]

export const DISCLAIMER = `This website is for informational purposes only and does not constitute financial advice, investment advice, or a solicitation to invest. Cryptocurrency and digital asset investments carry significant risk including total loss of capital. Past projections are not indicative of future results. Consult qualified legal and financial advisors before participating. Geographic restrictions may apply.`
```

---

## CHAPTER 1 — AGENT 1: PROJECT SCAFFOLD & DESIGN SYSTEM

**Agent 1 is the foundation. No other agent starts until Agent 1 outputs are confirmed.**

### Deliverables

```
vite.config.js
tailwind.config.js
postcss.config.js
index.html
src/globals.css              ← ALL CSS custom properties live here
src/lib/constants.js         ← paste shared constants block above
src/lib/utils.js
src/components/ui/
  ExplainerCard.svelte        ← most important shared component
  ScrollReveal.svelte
  WalletConnectButton.svelte
  CountdownTimer.svelte
```

### Exact Task Brief for Agent 1

> You are coding Agent 1 for the AI Training Arena marketing site (Vite + Svelte 5 with runes). Your job is ONLY the project scaffold and shared design system. Do not code any page sections.
>
> **1. Vite + Svelte 5 setup**
> - Vite 5, Svelte 5 (runes: `$state`, `$derived`, `$effect`)
> - Tailwind CSS v4 via PostCSS plugin
> - Path alias: `$lib` → `src/lib`, `$components` → `src/components`
>
> **2. `src/globals.css`** — define ALL CSS custom properties:
> ```css
> :root {
>   --bg-void: #040B14; --bg-surface: #080F1C; --bg-elevated: #0D1828;
>   --bg-border: #1A2840; --sol-purple: #9945FF; --sol-purple-dim: #6B2FCC;
>   --sol-green: #14F195; --sol-green-dim: #0BBF76; --electric-blue: #00C2FF;
>   --flame-orange: #FF6B35; --gold: #FFD166; --danger-red: #FF4444;
>   --text-primary: #F0F4FF; --text-secondary: #8899BB; --text-muted: #4A5A7A;
>   --gradient-hero: linear-gradient(135deg, #9945FF 0%, #14F195 100%);
>   --gradient-solana: linear-gradient(90deg, #9945FF, #00C2FF, #14F195);
>   --font-display: 'Syne', sans-serif;
>   --font-mono: 'Space Mono', monospace;
>   --font-body: 'Inter', sans-serif;
> }
> ```
> - Global body: `background: var(--bg-void); color: var(--text-primary); font-family: var(--font-body);`
> - Add Google Fonts link in `index.html`: Syne (400,500,700,800), Space Mono (400,700), Inter (300,400,500,600)
>
> **3. `ExplainerCard.svelte`** — the most important shared component:
> - Props: `trigger` (string), `title` (string), `body` (string), `learnMoreUrl` (optional string)
> - Renders inline as: `<span class="explainer-trigger">[?]</span>` in `--sol-green`
> - On click: opens a slide-in panel (right side, `position: fixed`) with `--bg-elevated` background, blur overlay
> - Shows `title`, `body`, optional "Learn more →" link
> - Escape key closes. Returns focus to trigger on close.
> - ARIA: `role="dialog"`, `aria-modal="true"`, `aria-labelledby`
>
> **4. `ScrollReveal.svelte`** — wrapper component:
> - Uses IntersectionObserver
> - Props: `delay` (ms, default 0), `direction` ('up'|'left'|'right', default 'up')
> - Wraps children, applies `transform: translateY(40px) → 0, opacity 0 → 1` on entry
> - Respects `prefers-reduced-motion`
>
> **5. `WalletConnectButton.svelte`**:
> - Shows "Connect Wallet" with MetaMask icon
> - On click: calls `window.ethereum.request({ method: 'eth_requestAccounts' })` if available
> - If unavailable: shows "Install MetaMask" link
> - Connected state: shows truncated address `0x1234...5678` in `--sol-green`
>
> **6. `CountdownTimer.svelte`**:
> - Props: `targetDate` (ISO string)
> - Displays: `DD HH MM SS` in Space Mono font
> - Updates every second via `setInterval` in `$effect`
>
> **7. `src/lib/constants.js`**: paste the shared constants block exactly as provided.
>
> **8. `src/lib/utils.js`**:
> ```js
> export const truncateAddress = (addr) => `${addr.slice(0,6)}...${addr.slice(-4)}`
> export const formatATA = (n) => n.toFixed(3) + ' ATA'
> export const formatUSD = (n) => '$' + n.toLocaleString('en-US', {minimumFractionDigits: 2})
> ```
>
> Output all files. Confirm with: `AGENT_1_COMPLETE`.

---

## CHAPTER 2 — AGENT 2: NAVBAR + HERO + HOW IT WORKS

**Depends on: Agent 1 complete**

### Deliverables

```
src/components/sections/NavBar.svelte
src/components/sections/Hero.svelte
src/components/sections/HowItWorks.svelte
```

### Exact Task Brief for Agent 2

> You are coding Agent 2 for the AI Training Arena marketing site (Vite + Svelte 5 runes). Import shared components from `$components/ui/` and constants from `$lib/constants.js`. Do NOT redefine CSS variables — they are in globals.css.
>
> **1. `NavBar.svelte`**
> - Fixed top, full width, `z-index: 100`
> - Default: fully transparent
> - On scroll (Y > 60px): `background: rgba(4,11,20,0.85); backdrop-filter: blur(20px);` — use `$effect` + scroll listener
> - Left: Logo text `AI TRAINING ARENA` — Syne 700, gradient text using `--gradient-solana` via `background-clip: text`
> - Center: Nav links — How It Works · Agent Classes · Tokenomics · Roadmap · DAO — Inter 500, color `--text-secondary`, hover `--sol-green`, 150ms transition
> - Right: Ghost button "View Whitepaper" + Primary button "Join Whitelist" (background `--sol-green`, text `--bg-void`, Syne 600)
> - Mobile (< 768px): hide center + right, show hamburger → full-screen overlay, same links stacked, `$state` for open/closed
>
> **2. `Hero.svelte`**
> - Full viewport height (`min-height: 100vh`)
> - Background: `--bg-void` with subtle radial gradient `radial-gradient(ellipse at 30% 50%, rgba(153,69,255,0.15) 0%, transparent 60%)` and second one `radial-gradient(ellipse at 70% 50%, rgba(20,241,149,0.1) 0%, transparent 60%)`
> - **Left column (55% desktop, full width mobile)**:
>   - Eyebrow: `BY 4TH SYSTEMS` — Space Mono, `--sol-green`, 0.8rem, letter-spacing 0.15em
>   - H1: `Where AI Agents Fight to Evolve` — Syne 800, `clamp(2.5rem, 6vw, 5.5rem)`, gradient text `--gradient-hero`
>   - H2: `Own. Battle. Earn. Build the world's largest decentralized AI training dataset.` — Inter 300, `clamp(1rem, 2vw, 1.25rem)`, `--text-secondary`, max-width 520px
>   - Stats row: 3 items — `25,000 NFTs | 200,000 battles/day | $50M+ TVL target` — Space Mono, `--sol-green`, count-up animation on mount (use `$effect`)
>   - CTA row: Primary "Join Whitelist →" + Ghost "Read the Blueprint"
>   - Annotation line below stats (small, `--text-muted`): *"NFTs are your AI agents. Battles are training sessions. Rewards are ATA tokens."*
> - **Right column (45% desktop, hidden on mobile)**:
>   - SVG/CSS animation: two hexagonal nodes connected by dashed animated paths, particles orbiting, green and purple glow pulsing — use CSS `@keyframes` only (no Three.js needed)
>   - Node labels: `PROPOSER` and `SOLVER` in Space Mono
>   - Animated data stream between them (dashed stroke-dashoffset animation)
> - Scroll indicator bottom-center: `Explore the Arena ↓` in `--text-muted`, bobbing animation
> - Page load stagger: eyebrow 200ms → H1 400ms → H2 700ms → stats 900ms → CTAs 1100ms (CSS animation-delay)
>
> **3. `HowItWorks.svelte`**
> - Section padding: `padding: 7rem 2rem`
> - Section label: `HOW IT WORKS` — Space Mono, `--sol-green`, uppercase, letter-spacing
> - H2: `Four Steps to the Arena` — Syne 700
> - Layout: horizontal flex (desktop) / vertical stack (mobile), connected by `--sol-purple` line
> - **4 step cards**, each with:
>   - Large step number (Syne 800, `--bg-border` color, very large, behind card as decorative element)
>   - Icon (use inline SVG or Lucide): 🤖 ⚔️ 🧠 💰
>   - Title: Syne 600, `--text-primary`
>   - Body: Inter 400, `--text-secondary`, ~3 sentences
>   - `<ExplainerCard>` component with the explainer text below (see design doc Section 2 for copy)
> - Step data (hardcode in component):
>   - Step 1: "Buy an AI Agent NFT" — explainer: what an AI Agent NFT actually is
>   - Step 2: "Enter the Battle Arena" — explainer: what P2P means
>   - Step 3: "Your Agent Gets Smarter" — explainer: Dr. Zero self-evolution
>   - Step 4: "Claim ATA Token Rewards" — explainer: what ATA is and how to use it
> - Use `<ScrollReveal>` on each card with 100ms stagger delay
>
> Output all three files. Confirm with: `AGENT_2_COMPLETE`.

---

## CHAPTER 3 — AGENT 3: AGENT CLASSES + BATTLE ARENA PREVIEW

**Depends on: Agent 1 complete**

### Deliverables

```
src/components/sections/AgentClasses.svelte
src/components/ui/ClassCard.svelte
src/components/sections/BattleArena.svelte
```

### Exact Task Brief for Agent 3

> You are coding Agent 3 for the AI Training Arena marketing site (Vite + Svelte 5 runes). Import AGENT_CLASSES from `$lib/constants.js`. Import ExplainerCard and ScrollReveal from `$components/ui/`.
>
> **1. `ClassCard.svelte`**
> - Props: `classData` (one item from AGENT_CLASSES array), `selected` (boolean)
> - Card styles: `background: var(--bg-surface); border: 1px solid var(--bg-border); border-radius: 16px; padding: 1.5rem;`
> - Glow border on hover and when `selected`: `box-shadow: 0 0 20px {classData.glow}40, inset 0 0 20px {classData.glow}10; border-color: {classData.glow};`
> - Class E gets animated rainbow border: CSS `@keyframes rainbow-border` cycling hue
> - Contents (top to bottom):
>   - Class badge: `CLASS {id}` — Space Mono, `--text-muted`, small
>   - Class name: Syne 700, `--text-primary`
>   - Parameter range: Space Mono, glow color
>   - NFT count + price: two inline pills
>   - Divider line
>   - Hardware requirement with 💻 icon
>   - Reward multiplier: large Space Mono number in glow color (`{multiplier}x rewards`)
>   - Model list: small `--text-muted` tags
>   - `[Select Class]` button: full width, background = glow color at 20% opacity, border = glow color, text = glow color. On click: emits `select` event
>   - One `<ExplainerCard>` per card explaining what that class's parameter range means practically
>
> **2. `AgentClasses.svelte`**
> - Section label + H2: `AGENT CLASSES` / `Choose Your Fighter`
> - Renders 5 `<ClassCard>` components using `{#each AGENT_CLASSES as cls}`
> - `$state selectedClass = null` — clicking a card sets it
> - Layout: CSS grid, `grid-template-columns: repeat(5, 1fr)` on desktop → `repeat(3,1fr)` on md → `1fr` on sm
> - Section-level `<ExplainerCard>` buttons (3 total, displayed as inline pill buttons below the headline):
>   - "What are Parameters?" — body: plain-English explanation of model parameters
>   - "What is MNT?" — body: Mantle token explanation, where to buy
>   - "What does Reward Multiplier mean?" — body: how rewards scale per class
> - Scroll reveal on each card with staggered delay (0, 100, 200, 300, 400ms)
>
> **3. `BattleArena.svelte`**
> - Full-width section, `background: var(--bg-surface)`
> - Header: `THE ARENA — LIVE BATTLES` (Syne 700) + fake live timer (CountdownTimer with target 3h from now)
> - Filter tabs: `ALL | CLASS A | CLASS B | CLASS C | CLASS D | CLASS E` — pill buttons, active state in `--sol-green`
> - **5×5 battle grid**: CSS grid `grid-template-columns: repeat(5, 1fr)`, gap 12px
> - Each of 25 cells is a "BattleCell" (inline component or separate file):
>   - Shows: `{agentA} vs {agentB}`, class icon, "● LIVE" badge (pulsing red dot via CSS), Elo ratings, progress bar
>   - Progress bar: CSS animation, random duration 10–25s via inline style
>   - Cell cycles through states: `matchmaking → live → result` every 8–15s using `setInterval` in `$effect`
>   - Generate fake agent IDs like `A-3K`, `B-1H` etc. for all 25 cells on mount
>   - `result` state briefly shows winner ("A-3K WINS +7 ELO") then resets
> - Filter logic: `$derived filteredCells` — when filter != 'ALL', show only 5 cells of that class in a 1×5 row
> - Section-level explainers:
>   - "What is Elo?" pill button
>   - "What is the 5×5 Grid?" pill button
> - Mobile: 3×3 grid + "View All Battles" button (scrolls to full grid)
>
> Output all three files. Confirm with: `AGENT_3_COMPLETE`.

---

## CHAPTER 4 — AGENT 4: DR. ZERO + TOKENOMICS + ARCHITECTURE

**Depends on: Agent 1 complete**

### Deliverables

```
src/components/sections/DrZeroExplainer.svelte
src/components/sections/Tokenomics.svelte
src/components/ui/TokenomicsChart.svelte
src/components/sections/Architecture.svelte
```

### Exact Task Brief for Agent 4

> You are coding Agent 4 for the AI Training Arena marketing site (Vite + Svelte 5 runes). Import TOKENOMICS from `$lib/constants.js`. Import ExplainerCard, ScrollReveal from `$components/ui/`.
>
> **1. `DrZeroExplainer.svelte`**
> - 2-column layout (desktop): animated diagram left (45%), copy right (55%) → stacked on mobile
> - **Left — Loop Diagram (SVG, inline in component)**:
>   - Three rounded rectangles: PROPOSER (top), SOLVER (middle), BOTH LEARN (bottom)
>   - Colors: PROPOSER = `--sol-purple` border, SOLVER = `--electric-blue` border, BOTH LEARN = `--sol-green` border
>   - Dashed animated arrows between boxes: use `stroke-dashoffset` CSS animation (`@keyframes dash`) to make lines "travel" continuously
>   - Labels on arrows: "Question (multi-hop)" and "Answer + Score"
>   - All SVG, no canvas
> - **Right — Copy**:
>   - Section label: `DR. ZERO FRAMEWORK` in Space Mono `--sol-green`
>   - H2: `Self-Evolution Without Human Labels` — Syne 700
>   - Three paragraphs (Inter 400, `--text-secondary`, line-height 1.7):
>     - Para 1: The Problem (cost of human-labeled data)
>     - Para 2: The Dr. Zero Solution (adversarial self-play)
>     - Para 3: Why this matters for the user (data ownership + earnings)
>   - `<ExplainerCard>` trigger: "HRPO & GRPO" — explain these are the learning algorithms Dr. Zero uses
>   - `<ExplainerCard>` trigger: "What is a multi-hop question?" — explain with the Massey University example
>   - **Investor callout box**: dark bordered box (`border: 1px solid --sol-purple`, background `--bg-elevated`, padding 1.5rem, border-radius 12px):
>     - Label: `FOR INVESTORS` — Space Mono, `--sol-purple`, small caps
>     - Body: explain the data flywheel — 200K battles/day = ~$40M/year equivalent data production
>
> **2. `TokenomicsChart.svelte`** (sub-component)
> - Props: `distribution` (array from TOKENOMICS.distribution)
> - Renders an animated SVG donut chart
> - Each segment: use SVG `circle` with `stroke-dasharray` and `stroke-dashoffset` to draw segments
> - On mount (`$effect`): animate each segment drawing in sequence, 100ms stagger
> - Center label: `100M ATA` in Space Mono
> - Legend below chart: color dot + label + percentage for each segment
>
> **3. `Tokenomics.svelte`**
> - Section label + H2: `TOKENOMICS` / `The ATA Economy`
> - **Legal disclaimer** (required — use DISCLAIMER from constants.js): small text, `--text-muted`, top of section, above chart
> - 2-column layout: `<TokenomicsChart>` left, breakdown cards right
> - Breakdown cards: 7 cards (one per distribution item), each with color dot, label, %, lock icon if applicable, one-line description
> - **Deflationary Mechanics sub-section** (below 2-col):
>   - H3: `Designed to Get Scarcer`
>   - 3 mechanism cards in a row:
>     - Battle Burn: "2% of every battle reward permanently burned"
>     - Buyback & Burn: "5% of weekly protocol revenue → market buy → burn"
>     - Supply Cliff: "30M battle reward tokens emit over 10 years, not all at once"
>   - Each card: icon + title + body, border `--danger-red` left-accent
> - **wATA explainer**: `<ExplainerCard>` trigger "What is wATA / No-NFT Entry?" — explain stake-to-play without buying NFT
> - **Investor callout box**: price targets table ($0.50 M6 → $1.50 M12 → $5 M24 → $12 M36) + disclaimer these are targets not guarantees
>
> **4. `Architecture.svelte`**
> - Section label + H2: `ARCHITECTURE` / `Truly Decentralized`
> - **Cost comparison banner** (hero stat): centered, large Space Mono:
>   - `$630/month` in `--sol-green` (huge) vs `$43,000/month` in `--danger-red` with strikethrough
>   - Sub-label: `98.5% infrastructure cost reduction`
> - **5-layer architecture diagram** (SVG or HTML/CSS):
>   - Layer 1 (top): USER INTERFACE — Svelte Frontend — `--electric-blue` band
>   - Layer 2: LOCAL P2P NODE — .NET 9, AI Model, LibP2P, IPFS, Blockchain RPC — `--sol-purple` band
>   - Layer 3: P2P NETWORK MESH — Node A ↔ Node B ↔ Node C + DHT — `--bg-elevated` band
>   - Layer 4: BLOCKCHAIN (MANTLE) — 5 contract boxes — `--sol-green` band
>   - Layer 5: IPFS STORAGE — Private + Public datasets — `--gold` band
>   - Animated vertical arrows between layers (same dash animation as DrZero)
> - **Network effect callout** (3-row table):
>   ```
>   1 user    = 1x compute    → $630/mo cost
>   1,000     = 1,000x compute → $630/mo cost
>   25,000    = 25,000x compute → $630/mo cost
>   ```
>   Style as monospace table with `--sol-green` highlight on the cost column (all same)
> - Explainer: "What does running a node mean?" — plain English for non-technical users
> - Investor callout: "Why P2P Architecture is a Moat" — scaling economics explanation
>
> Output all four files. Confirm with: `AGENT_4_COMPLETE`.

---

## CHAPTER 5 — AGENT 5: STAKING CALCULATOR + GOVERNANCE + ROADMAP

**Depends on: Agent 1 complete**

### Deliverables

```
src/components/sections/StakingCalculator.svelte
src/stores/calculator.js
src/components/sections/Governance.svelte
src/components/sections/Roadmap.svelte
```

### Exact Task Brief for Agent 5

> You are coding Agent 5 for the AI Training Arena marketing site (Vite + Svelte 5 runes). Import AGENT_CLASSES, ROADMAP, TOKENOMICS from `$lib/constants.js`. Import ExplainerCard, ScrollReveal from `$components/ui/`.
>
> **1. `src/stores/calculator.js`** (Svelte 5 store using runes):
> ```js
> // All calculation logic lives here, imported by StakingCalculator
> export function createCalculatorStore() {
>   let selectedClass = $state('A')
>   let numAgents = $state(1)
>   let battlesPerDay = $state(8)
>   let ataPrice = $state(1.00)
>
>   const classMultipliers = { A:1.0, B:1.2, C:1.5, D:2.0, E:3.0 }
>   const baseRewards = { A:0.041, B:0.049, C:0.062, D:0.082, E:0.123 }
>
>   const dailyATA = $derived(
>     baseRewards[selectedClass] * classMultipliers[selectedClass] * battlesPerDay * numAgents
>   )
>   const dailyUSD = $derived(dailyATA * ataPrice)
>   const monthlyATA = $derived(dailyATA * 30)
>   const monthlyUSD = $derived(monthlyASD * ataPrice) // fix: dailyUSD * 30
>   const stakingAPY = $derived(
>     ((365 * 8219) / 10_000_000) * 100  // assumes 10M staked
>   )
>
>   return { selectedClass, numAgents, battlesPerDay, ataPrice, dailyATA, dailyUSD, monthlyATA, monthlyUSD, stakingAPY }
> }
> ```
> Note: fix the monthlyUSD derivation to `dailyATA * 30 * ataPrice`.
>
> **2. `StakingCalculator.svelte`**
> - Section label + H2: `EARNINGS CALCULATOR` / `Project Your Returns`
> - Background: `--bg-surface`, full-width
> - 2-column layout: inputs left, outputs right → stacked on mobile
> - **Left — Inputs**:
>   - Agent Class: 5 pill toggle buttons (A B C D E), `$state` bound, active = `--sol-green` bg
>   - Number of agents: range slider 1–10, current value displayed in Space Mono
>   - Daily battles per agent: range slider 1–8, current value displayed
>   - ATA price assumption: number input, `$`, default 1.00
>   - All inputs update the store in real time
> - **Right — Outputs** (updates live via `$derived`):
>   - 4 output cards: Daily ATA, Daily USD, Monthly ATA, Monthly USD — Space Mono large number, Inter label
>   - Staking APY card: `~30% APY` at assumed 10M ATA staked
>   - Small `--text-muted` line: "Assumes 10M ATA staked. APY adjusts dynamically."
> - **Disclaimer** below outputs (required): use first 2 sentences of DISCLAIMER constant
> - `<ExplainerCard>` trigger "How are these calculated?" — explain the formula plainly
>
> **3. `Governance.svelte`**
> - Section label + H2: `DAO GOVERNANCE` / `You Own the Protocol`
> - 3-column feature grid (desktop) → 1-col (mobile):
>   - Card 1 — Voting Rights: icon 🏛️, title, body (1 ATA = 1 vote, 1% to propose, 10% quorum)
>   - Card 2 — Treasury Control: icon 🔐, title, body (10M ATA, multi-sig + timelock, community vote)
>   - Card 3 — Telemetry Governance: icon 📊, title, body (DAO decides data access and pricing)
> - Card styles: `--bg-surface`, `--sol-purple` left border (4px), padding 2rem
> - **Investor callout box** (below grid):
>   - Label: `FOR INVESTORS` — `--sol-purple`
>   - Founder vesting: 5% / 24 months / 6-month cliff / unvested tokens non-voting
>   - DAO treasury: 10M ATA / on-chain / 10% quorum required to spend
> - `<ExplainerCard>` trigger "What can I vote on?" — list: reward rates, new classes, treasury spending, protocol upgrades, data marketplace pricing
>
> **4. `Roadmap.svelte`**
> - Section label + H2: `ROADMAP` / `The Path to the Arena`
> - Desktop: horizontal timeline — flex row, each phase is a vertical card connected by a horizontal line
> - Mobile: vertical accordion — each phase is a collapsible item, `$state openPhase`
> - Data: iterate over ROADMAP constant (5 phases)
> - Phase card styles by status:
>   - `complete`: `--sol-green` left border, ✓ checkmark badge
>   - `active`: `--electric-blue` left border, animated pulsing dot badge
>   - `upcoming`: `--sol-purple` left border, clock icon
>   - `future`: `--text-muted` border, dimmed text
> - Each card: quarter label (Space Mono), status badge, phase title (Syne 600), bullet list of items
> - Timeline connector line (desktop only): `--bg-border` horizontal line, progress highlight in `--sol-green` up to the active phase
>
> Output all four files. Confirm with: `AGENT_5_COMPLETE`.

---

## CHAPTER 6 — AGENT 6: INVESTOR METRICS + FAQ + FOOTER + APP ASSEMBLY

**Depends on: Agents 1–5 complete (for App.svelte assembly)**
**Can start Investor Metrics, FAQ, Footer immediately after Agent 1**

### Deliverables

```
src/components/sections/InvestorMetrics.svelte
src/components/sections/FAQ.svelte
src/components/sections/Footer.svelte
src/App.svelte                         ← assembles all sections
```

### Exact Task Brief for Agent 6

> You are coding Agent 6 for the AI Training Arena marketing site (Vite + Svelte 5 runes). Import DISCLAIMER from `$lib/constants.js`. Import all section components. Your final task is assembling `App.svelte`.
>
> **1. `InvestorMetrics.svelte`**
> - Section label + H2: `BY THE NUMBERS` / `Built for Scale`
> - **Top stats row** — 6 cards in a grid (`repeat(6,1fr)` → `repeat(3,2fr)` → `repeat(2,1fr)` on mobile):
>   - `25,000` Total NFTs
>   - `200,000` Battles/Day (at capacity)
>   - `$1.4M` NFT Sale Revenue
>   - `100M+` Training Interactions
>   - `$50M+` Target TVL
>   - `$630/mo` Infrastructure Cost
>   - Each card: big Space Mono number in `--sol-green`, Inter label below
>   - Animate count-up on scroll entry
> - **Revenue projection table** (HTML table, custom styled):
>   ```
>   | Revenue Stream        | Month 6    | Month 12    | Month 24    |
>   | Battle Entry Fees     | $3,000/day | $10,000/day | $15,000/day |
>   | NFT Royalties         | $125/day   | $300/day    | $500/day    |
>   | Data Marketplace      | $100/day   | $2,000/day  | $10,000/day |
>   | Staking Protocol Fees | $100/day   | $500/day    | $2,000/day  |
>   | TOTAL PROTOCOL        | ~$3,325/day| ~$12,800/day| ~$27,500/day|
>   ```
>   - Table styles: `--bg-surface` rows, `--bg-elevated` header, `--sol-green` on total row, Space Mono for numbers
> - **Investor callout box** below table: explain assumptions (5K/15K/25K active agents), data marketplace market size ($20B+ by 2027), link to blueprint for full model
> - **Legal disclaimer** (required, verbatim from DISCLAIMER constant): below investor callout, `--text-muted`, small
>
> **2. `FAQ.svelte`**
> - Section label + H2: `FAQ` / `Common Questions`
> - 10 accordion items — use `$state openIndex = null`
> - Each item: question in Syne 600, answer in Inter 400 `--text-secondary`
> - Open item: `--bg-elevated` background, `--sol-green` left border, smooth height transition
> - Questions and answers (use exactly these):
>   1. Is this a real AI or just a game?
>   2. Do I need technical skills to participate?
>   3. What blockchain is this on?
>   4. How are battles verified? Can someone cheat?
>   5. What happens if I close my computer mid-battle?
>   6. Who owns the training data generated by my agent?
>   7. Is ATA listed on exchanges?
>   8. What are the risks?
>   9. Can I have multiple agents?
>   10. Is this connected to Solana?
>   (Full answer copy is in the design doc Section 12 — paste verbatim)
>
> **3. `Footer.svelte`**
> - Background: `--bg-surface`, top border `--bg-border`
> - 4-column grid (desktop) → 2-col → 1-col:
>   - Col 1: Logo + tagline "Train the future. Own the data." + "Built by 4th Systems" + "Deployed on Mantle Network"
>   - Col 2: Links — Whitepaper, Smart Contracts (Mantle Explorer), GitHub, Brand Assets
>   - Col 3: Community — Discord, Twitter/X, Telegram, DAO Portal
>   - Col 4: Legal — DISCLAIMER text (full, small, `--text-muted`) + Terms of Service + Privacy Policy
> - Bottom bar: `© 2026 AI Training Arena · Built by 4th Systems · All Rights Reserved` — centered, `--text-muted`, Space Mono
>
> **4. `App.svelte`** — FINAL ASSEMBLY
> ```svelte
> <script>
>   import NavBar from '$components/sections/NavBar.svelte'
>   import Hero from '$components/sections/Hero.svelte'
>   import HowItWorks from '$components/sections/HowItWorks.svelte'
>   import AgentClasses from '$components/sections/AgentClasses.svelte'
>   import BattleArena from '$components/sections/BattleArena.svelte'
>   import DrZeroExplainer from '$components/sections/DrZeroExplainer.svelte'
>   import Tokenomics from '$components/sections/Tokenomics.svelte'
>   import Architecture from '$components/sections/Architecture.svelte'
>   import StakingCalculator from '$components/sections/StakingCalculator.svelte'
>   import Governance from '$components/sections/Governance.svelte'
>   import Roadmap from '$components/sections/Roadmap.svelte'
>   import InvestorMetrics from '$components/sections/InvestorMetrics.svelte'
>   import FAQ from '$components/sections/FAQ.svelte'
>   import Footer from '$components/sections/Footer.svelte'
> </script>
>
> <NavBar />
> <main>
>   <Hero />
>   <HowItWorks />
>   <AgentClasses />
>   <BattleArena />
>   <DrZeroExplainer />
>   <Tokenomics />
>   <Architecture />
>   <StakingCalculator />
>   <Governance />
>   <Roadmap />
>   <InvestorMetrics />
>   <FAQ />
> </main>
> <Footer />
> ```
>
> Output all four files. Confirm with: `AGENT_6_COMPLETE`.

---

## MANAGER AGENT PROMPT TEMPLATE

Use this as the **exact system prompt / task** for your Gas Town manager agent:

```
You are the manager agent for the AI Training Arena marketing site build.
You are orchestrating 6 coding agents. Your job is:

1. READ both documents in full:
   - ai-training-arena-design-doc.md  (visual + content spec)
   - gastown-chapters.md              (this document — task breakdown)

2. DISPATCH agents in this order:
   - Phase 0: Dispatch Agent 1. WAIT for AGENT_1_COMPLETE before proceeding.
   - Phase 1: Dispatch Agent 2 AND Agent 3 simultaneously.
   - Phase 2: Dispatch Agent 4 AND Agent 5 simultaneously.
   - Phase 3: Dispatch Agent 6 (can start section coding after Phase 0, assembles App.svelte after all agents complete).

3. For each agent dispatch, send EXACTLY:
   - The agent's Chapter brief (copy verbatim from gastown-chapters.md)
   - The SHARED CONSTANTS block (copy verbatim from the top of gastown-chapters.md)
   - This instruction: "Do not define any CSS variables. They exist in globals.css. Import from $lib/constants.js. Import shared UI components from $components/ui/. Output complete file code for every deliverable listed. End your response with your AGENT_N_COMPLETE signal."

4. COLLECT all agent outputs and write them to the correct file paths.

5. RESOLVE any import path conflicts:
   - All section imports use $components/sections/
   - All UI component imports use $components/ui/
   - All store imports use $stores/
   - All lib imports use $lib/

6. RUN final verification:
   - Does App.svelte import all 13 section components? ✓
   - Does every section that uses ExplainerCard import it? ✓
   - Are legal disclaimers present in Tokenomics, InvestorMetrics, and Footer? ✓
   - Does globals.css exist with all CSS variables? ✓
   - Does constants.js export AGENT_CLASSES, TOKENOMICS, ROADMAP, DISCLAIMER? ✓

7. OUTPUT a build-ready file tree when all checks pass.
```

---

## QUICK REFERENCE — SECTION → AGENT MAPPING

| Section | Agent | File |
|---|---|---|
| NavBar | 2 | NavBar.svelte |
| Hero | 2 | Hero.svelte |
| How It Works | 2 | HowItWorks.svelte |
| Agent Classes | 3 | AgentClasses.svelte + ClassCard.svelte |
| Battle Arena Preview | 3 | BattleArena.svelte |
| Dr. Zero Explainer | 4 | DrZeroExplainer.svelte |
| Tokenomics | 4 | Tokenomics.svelte + TokenomicsChart.svelte |
| Architecture | 4 | Architecture.svelte |
| Staking Calculator | 5 | StakingCalculator.svelte + calculator.js |
| DAO Governance | 5 | Governance.svelte |
| Roadmap | 5 | Roadmap.svelte |
| Investor Metrics | 6 | InvestorMetrics.svelte |
| FAQ | 6 | FAQ.svelte |
| Footer | 6 | Footer.svelte |
| App Assembly | 6 | App.svelte |
| Design System | 1 | globals.css + constants.js + UI components |

**Total files: 22 Svelte components + 3 JS/CSS files = 25 deliverables across 6 agents.**
