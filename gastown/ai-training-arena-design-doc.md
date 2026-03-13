# AI TRAINING ARENA — MARKETING SITE DESIGN DOCUMENT
### For Gas Town Agents · Vite + Svelte · Version 1.0

---

> **Document Purpose:** This is a full front-end design specification for the AI Training Arena marketing/investor site. It covers visual language, component architecture, copy strategy, section-by-section layout instructions, and annotated investor/user callout requirements. Gas Town agents should treat every section marked `[EXPLAIN TO USER]` or `[EXPLAIN TO INVESTOR]` as a required callout — either a tooltip, accordion, modal, or explanatory sidebar — so that no concept is left unexplained for a non-technical audience.

---

## 0. PLATFORM CONTEXT FOR AGENTS

**4th Systems** is the builder behind AI Training Arena — an AI-native development studio operating across **Solana** (pump.fun ecosystem, meme coins, DeFi) and **EVM chains** (Mantle Network, Arbitrum, Base, zkSync). The marketing site must:

- Speak to **two audiences simultaneously**: crypto/DeFi investors and AI enthusiasts
- Position AI Training Arena as the first **gamified, decentralized AI training economy**
- Launch on **Mantle Network** (EVM-compatible, low gas, fast finality) with future Solana/multi-chain support
- Reflect the **Solana color palette** while being technically deployed on Mantle

---

## 1. TECH STACK REQUIREMENTS

```
Framework:     Vite 5+ + Svelte 5 (use runes syntax: $state, $derived, $effect)
Styling:       Tailwind CSS v4 (PostCSS plugin, not CDN)
Animations:    @motionone/svelte OR CSS custom properties + keyframes
3D / Canvas:   Three.js (r128) via CDN — optional for hero section
Icons:         Lucide Svelte
Fonts:         Google Fonts (specified below)
Wallet:        ethers.js v6 (for MetaMask/WalletConnect preview CTAs)
Build:         Vite with static adapter (output: /dist)
Deployment:    Vercel or Cloudflare Pages
```

### File Structure

```
/src
  /components
    /sections
      Hero.svelte
      HowItWorks.svelte
      AgentClasses.svelte
      Tokenomics.svelte
      BattleArena.svelte
      DrZeroExplainer.svelte
      Architecture.svelte
      Governance.svelte
      Roadmap.svelte
      Investors.svelte
      FAQ.svelte
      Footer.svelte
    /ui
      NavBar.svelte
      ExplainerCard.svelte        ← reusable "explain this" tooltip/modal
      TokenomicsChart.svelte
      ClassCard.svelte
      CountdownTimer.svelte
      WalletConnectButton.svelte
      ScrollReveal.svelte
  /stores
    wallet.js
    countdown.js
  /lib
    constants.js                  ← token supply, class data, roadmap items
    utils.js
  App.svelte
  main.js
/public
  /fonts
  /images
  /videos
vite.config.js
tailwind.config.js
```

---

## 2. VISUAL IDENTITY

### 2.1 Color Palette — Near-Solana

Solana's canonical colors are `#9945FF` (purple) and `#14F195` (green). We shift slightly toward a **deep space / electric arena** aesthetic while staying in that family.

```css
/* globals.css — paste into app root */
:root {
  /* Backgrounds */
  --bg-void:        #040B14;   /* near-black with blue tint — page background */
  --bg-surface:     #080F1C;   /* card/section backgrounds */
  --bg-elevated:    #0D1828;   /* modals, tooltips */
  --bg-border:      #1A2840;   /* subtle borders */

  /* Solana-inspired Primary */
  --sol-purple:     #9945FF;   /* primary brand — Solana purple */
  --sol-purple-dim: #6B2FCC;   /* darker variant */
  --sol-green:      #14F195;   /* primary accent — Solana green */
  --sol-green-dim:  #0BBF76;   /* darker variant */

  /* Extended palette */
  --electric-blue:  #00C2FF;   /* secondary accent, data viz */
  --flame-orange:   #FF6B35;   /* warnings, battle intensity */
  --gold:           #FFD166;   /* top-tier rewards, Class E */
  --danger-red:     #FF4444;   /* burn/deflation indicators */

  /* Text */
  --text-primary:   #F0F4FF;
  --text-secondary: #8899BB;
  --text-muted:     #4A5A7A;

  /* Gradients */
  --gradient-hero:    linear-gradient(135deg, #9945FF 0%, #14F195 100%);
  --gradient-card:    linear-gradient(160deg, #0D1828 0%, #080F1C 100%);
  --gradient-burn:    linear-gradient(90deg, #FF4444, #FF6B35);
  --gradient-solana:  linear-gradient(90deg, #9945FF, #00C2FF, #14F195);
}
```

### 2.2 Typography

```css
/* Import in index.html <head> */
@import url('https://fonts.googleapis.com/css2?family=Syne:wght@400;500;600;700;800&family=Space+Mono:ital,wght@0,400;0,700;1,400&family=Inter:wght@300;400;500;600&display=swap');

/* Usage */
--font-display:  'Syne', sans-serif;         /* headlines, section titles */
--font-mono:     'Space Mono', monospace;    /* numbers, code, stats, addresses */
--font-body:     'Inter', sans-serif;        /* body copy, descriptions */
```

**Rationale:** Syne has an angular, technological character that reads as futuristic without being cliché. Space Mono grounds the crypto-native numbers in a familiar typewriter feel. Inter handles readability at small sizes.

### 2.3 Visual Language Principles

1. **Deep space with neon edges.** Cards float on a near-black void. Borders glow with `--sol-purple` or `--sol-green`. No white backgrounds.
2. **Data is art.** Every number, chart, and formula gets display treatment — large, glowing, monospaced.
3. **Motion signals life.** The arena is alive. Particle fields in the hero, pulsing borders on active battles, staggered reveals on scroll.
4. **Annotations are first-class.** Every complex concept has a visible `[?]` icon that opens a plain-language explainer. These are never hidden.

---

## 3. GLOBAL COMPONENTS

### 3.1 NavBar.svelte

**Behavior:** Fixed top, transparent on hero → blurs to `rgba(4, 11, 20, 0.85)` with `backdrop-filter: blur(20px)` on scroll.

**Items:**
- Logo (left): `AI TRAINING ARENA` — display font, gradient text using `--gradient-solana`
- Nav links (center): How It Works · Agent Classes · Tokenomics · Roadmap · DAO
- CTAs (right): `View Whitepaper` (ghost button) + `Join Whitelist` (primary button — `--sol-green` background, `--bg-void` text)

**Mobile:** Hamburger → full-screen overlay with the same links stacked.

---

### 3.2 ExplainerCard.svelte (Reusable)

This is the most important UI component. Any time a concept needs explanation, render this.

```svelte
<!-- Usage example -->
<ExplainerCard
  trigger="Dr. Zero Framework"
  icon="?"
  title="What is Dr. Zero?"
  body="Dr. Zero is a self-evolution AI framework from Meta Research. 
        Instead of needing human-labeled data, AI agents learn by 
        competing — one agent asks hard questions, another answers them. 
        Over thousands of battles, both agents get smarter. 
        AI Training Arena is the first platform to make this process 
        publicly playable and financially rewarding."
  learnMoreUrl="https://arxiv.org/abs/2601.07055"
/>
```

**Visual:** Inline `?` badge in `--sol-green`. Click/tap triggers a slide-in panel or modal with dark glass background. Never a browser tooltip.

---

## 4. SECTIONS — FULL SPECIFICATION

---

### SECTION 1 — HERO

**Goal:** Establish the product in 5 seconds. Communicate: *Own an AI agent. Train it by fighting. Earn real rewards.*

#### Layout

```
[NavBar]

[LEFT 55%]                          [RIGHT 45%]
EYEBROW: "BY 4TH SYSTEMS"          [3D/Canvas: Battle Animation]
                                     Two agent nodes connected by
H1: "Where AI Agents                 arcing data streams. Particles
     Fight to Evolve"                orbit the nodes. Green and
                                     purple glow pulses on "attack"
H2: "Own. Battle. Earn.             cycles. Can be Three.js canvas
     Build the world's              or CSS/SVG animation if perf
     largest decentralized          is a concern.
     AI training dataset."

[Stats Row]
25,000 NFTs  |  200,000 battles/day  |  $50M+ TVL target

[CTA Row]
[Join Whitelist →]  [Read the Blueprint]

[Scroll indicator: "Explore the Arena ↓"]
```

#### Copy Notes

- H1 must be large — `clamp(3rem, 7vw, 6rem)`, display font, gradient text
- Stats row uses Space Mono, `--sol-green` color, animated count-up on load

#### `[EXPLAIN TO USER]` Annotations in Hero

Place a small annotation line beneath the stats:
> *"NFTs are your AI agents. Battles are training sessions. Rewards are ATA tokens. See how it works ↓"*

---

### SECTION 2 — HOW IT WORKS

**Goal:** Walk a non-technical user through the full loop in 4 steps. Must be scannable in under 60 seconds.

#### Layout: Horizontal timeline (desktop) / Vertical steps (mobile)

```
STEP 1                STEP 2               STEP 3               STEP 4
──────────────────────────────────────────────────────────────────────
[Icon: 🤖]           [Icon: ⚔️]           [Icon: 🧠]           [Icon: 💰]

Buy an               Enter the             Your Agent            Claim ATA
AI Agent NFT         Battle Arena          Gets Smarter          Token Rewards

Each NFT contains    Match against         Every battle          Win battles,
a real AI model.     other agents          generates real        earn ATA tokens.
Your agent runs      of the same           training data.        Sell training
on your hardware     skill class.          Your agent            data. Stake for
or cloud.            Battles are P2P       evolves with          passive income.
                     and verifiable        each fight.
                     on-chain.
```

#### `[EXPLAIN TO USER]` callouts per step

**Step 1 — NFT + AI Model:**
> *An "AI Agent NFT" isn't just a picture — it's a live AI model (like a small version of ChatGPT) that you own. The NFT proves ownership on the blockchain. You run the model yourself, which means you control it and keep any training data it generates.*

**Step 2 — P2P Battle:**
> *"Peer-to-peer" means your computer connects directly to your opponent's computer — no central server in the middle. This cuts costs by 98.5% compared to traditional platforms and means no single company can shut down your battle.*

**Step 3 — Self-Evolution (Dr. Zero):**
> *This is the breakthrough. Most AI systems need expensive human-labeled training data. Dr. Zero (from Meta Research) lets agents teach themselves by competing. Your agent generates hard questions for opponents, opponents solve them, and both sides improve. The harder the question, the more both agents learn.*

**Step 4 — Token Rewards:**
> *ATA is the platform's native token (ERC-20 on Mantle blockchain). You earn it by winning battles, contributing training data, and staking. It has real utility: governance voting, tournament entry, and data marketplace transactions.*

---

### SECTION 3 — AGENT CLASSES

**Goal:** Let users self-select their hardware tier and understand the investment/reward trade-off.

#### Layout: 5-column card grid

Each card (`ClassCard.svelte`) represents one agent class.

```
CLASS A             CLASS B             CLASS C             CLASS D             CLASS E
──────────────────────────────────────────────────────────────────────────────────────
ENTRY GLADIATORS    MID-TIER            ELITE               MASTER              TITAN
                    GLADIATORS          STRATEGISTS         ARCHITECTS          SENTINELS

3B–7B               7B–32B              32B–70B             70B–405B            405B+
Parameters          Parameters          Parameters          Parameters          Parameters

15,000 NFTs         6,000 NFTs          2,500 NFTs          1,200 NFTs          300 NFTs
10 MNT (~$5)        50 MNT (~$25)       200 MNT (~$100)     800 MNT (~$400)     3,000 MNT (~$1,500)

RTX 3060+           RTX 4090            A100 40GB           4–8x A100           8x H100
Consumer GPU        High-end GPU        Data Center         GPU Cluster         Cloud Only

1.0x Rewards        1.2x Rewards        1.5x Rewards        2.0x Rewards        3.0x Rewards

[Select Class]      [Select Class]      [Select Class]      [Select Class]      [Select Class]
```

**Card Design:**
- Each card has a unique border glow color:
  - Class A: `--sol-green`
  - Class B: `--electric-blue`
  - Class C: `--sol-purple`
  - Class D: `--gold`
  - Class E: animated rainbow border cycling through `--gradient-solana`
- Hover state: card lifts with box-shadow, brief scale(1.03) transform
- Active/Selected: thick colored border + checkmark badge

#### `[EXPLAIN TO USER]` Callouts

**"What are Parameters?"**
> *Parameters are how AI scientists measure model size and capability. A 3B model has 3 billion mathematical values it uses to think. A 405B model has 135x more. Bigger isn't always better — a well-trained small model often beats a lazy large one. Class-based matchmaking ensures you only fight agents your own size.*

**"What is MNT?"**
> *MNT is the native token of Mantle Network, the blockchain AI Training Arena runs on. It's used to purchase NFT agents. At current prices, 10 MNT ≈ $5 USD. You can acquire MNT on major exchanges like Bybit and Bitget.*

**"What does Reward Multiplier mean?"**
> *Every battle awards ATA tokens. Class E agents earn 3x more per battle than Class A. This compensates for the higher hardware cost of running larger models. All classes are designed to be economically viable when you account for data marketplace earnings.*

**"What hardware do I need?"**
> *Class A runs on a gaming GPU you might already own (RTX 3060 or better). Classes D and E require data center hardware — most players in those tiers use cloud GPU services like RunPod or Vast.ai rather than owning the hardware outright.*

---

### SECTION 4 — BATTLE ARENA PREVIEW (LIVE DEMO MOCKUP)

**Goal:** Show the 5×5 battle grid. Make it feel alive even though it's a mockup on the marketing site.

#### Layout

Full-width dark section. Title: `"THE ARENA — LIVE BATTLES"` with a live-looking timer.

Render a **5×5 animated grid** where each cell shows:
- Two agent IDs (`A3K vs A7K`)
- A class icon
- A pulsing "LIVE" badge
- An animated progress bar (fake timer ticking down)
- Simulated Elo ratings

Each cell cycles through fake battle states: `MATCHMAKING → LIVE → RESULT` every 8–12 seconds (randomized per cell with `setInterval`).

Add a "Class Filter" tab row above: `ALL | CLASS A | CLASS B | CLASS C | CLASS D | CLASS E`

#### `[EXPLAIN TO USER]` Callouts

**"What is Elo?"**
> *Elo is a rating system originally invented for chess. A win against a higher-rated opponent gives you more points than a win against a weaker one. AI Training Arena uses per-class Elo — so a Class A agent's rating only competes against other Class A agents. Starting Elo is 1500 for all new agents.*

**"What is the 5×5 Grid?"**
> *The arena surfaces 25 featured battles at any time (5 per class). These are the highest-stakes matches happening live on the P2P network. Any player can watch any featured battle in real time. Think of it as the main stage of a sports arena — most battles happen off-screen, but the best ones get spotlighted here.*

---

### SECTION 5 — DR. ZERO EXPLAINER

**Goal:** Explain the core technical innovation to both technical and non-technical audiences. This is the biggest differentiator and must be communicated clearly.

#### Layout: Split — Visual left, Copy right

**Left:** Animated diagram showing the Proposer → Solver loop:
```
    ┌─────────────┐
    │  PROPOSER   │  ← generates hard questions
    │  (Agent A)  │
    └──────┬──────┘
           │  Question (3-hop difficulty)
           ▼
    ┌─────────────┐
    │   SOLVER    │  ← attempts to answer
    │  (Agent B)  │
    └──────┬──────┘
           │  Answer + Score
           ▼
    ┌─────────────┐
    │  BOTH LEARN │  ← HRPO + GRPO policy update
    │  FROM THIS  │
    └─────────────┘
```
Animate with dashed lines traveling between boxes. Use CSS animation or Motion.

**Right copy:**

**Headline:** `Self-Evolution Without Human Labels`

**Body (3 short paragraphs):**

Para 1 — The Problem:
> *Training AI models traditionally costs millions of dollars in human-labeled data. Someone has to manually write thousands of questions and correct answers to teach an AI system anything.*

Para 2 — The Dr. Zero Solution:
> *Meta Research's Dr. Zero framework discovered that AI agents can teach themselves through adversarial competition. One agent (the Proposer) generates questions designed to be hard. Another agent (the Solver) tries to answer them. Both agents update their internal weights based on how the battle went. No human labels required.*

Para 3 — Why This Matters for You:
> *AI Training Arena is the first platform to open this process to the public — and pay you for participating. Every battle your agent fights generates real training data that researchers, companies, and DAO members can purchase. You own that data. You set the price.*

#### `[EXPLAIN TO INVESTOR]` Block

> **Investment Significance:**
> The Dr. Zero framework transforms every user's battle into a data production event. With 200,000 battles per day at full capacity, the platform generates what would cost ~$40M/year in traditional human-labeled data — decentrally, at near-zero marginal cost. The DAO data marketplace monetizes this directly. Early investors gain exposure to this data flywheel before it reaches scale.

---

### SECTION 6 — TOKENOMICS

**Goal:** Present the $ATA token economy transparently. Investors must see supply, distribution, and deflationary mechanics clearly.

#### Layout: Two-column — Chart left, Breakdown right

**Left: Donut Chart** (`TokenomicsChart.svelte` using SVG or Canvas)

```
Total Supply: 100,000,000 ATA

Distribution:
  35% — NFT Sales (locked)          #9945FF  ████████████████
  30% — Battle Rewards Pool         #14F195  █████████████
  15% — Staking Rewards             #00C2FF  ██████
  10% — DAO Treasury                #FFD166  ████
   5% — Founders (vested)           #FF6B35  ██
   3% — Liquidity                   #8899BB  █
   2% — Team & Advisors             #4A5A7A  █
```

Animate the donut drawing itself on scroll entry.

**Right: Breakdown Cards**

Each allocation gets a small card with:
- Color dot + label + percentage
- One-line description
- Lock icon if vested/locked

```
🔒 NFT Sales (35%) — Distributed to NFT buyers as part of purchase price. Locked per vesting schedule.
⚔️  Battle Rewards (30%) — Paid out over 10 years. 8,219 ATA/day at launch.
📈 Staking Rewards (15%) — Earn passive yield by staking ATA. APY adjusts dynamically.
🏛️  DAO Treasury (10%) — Controlled by ATA holders via on-chain governance.
👥 Founders (5%) — 24-month linear vest, 6-month cliff. Fully transparent on-chain.
💧 Liquidity (3%) — DEX liquidity pools on launch day.
🛠️  Team & Advisors (2%) — 12-month vest.
```

#### Deflationary Mechanics Sub-Section

Headline: `"Designed to Get Scarcer"`

Three mechanism cards:

**Card 1 — Battle Burn**
> *2% of every battle reward is permanently burned. At 200,000 battles/day, this removes tokens from circulation daily.*

**Card 2 — Buyback & Burn**
> *5% of all protocol revenue is used weekly to buy ATA from the open market and burn it permanently. At $100K/week revenue (Month 12 projection), that's 5,000 ATA burned weekly.*

**Card 3 — Supply Cliff**
> *30M tokens in the battle rewards pool emit over 10 years, not all at once. Early participation has favorable supply/demand dynamics.*

#### `[EXPLAIN TO INVESTOR]` Block

> **Token Economics Summary:**
> ATA is not a speculation token — it is utility-first. It pays for: tournament entry fees, wATA staking licenses (non-NFT access), DAO governance, and data marketplace transactions. The deflationary model means that as platform usage grows, circulating supply decreases. Conservative price targets based on comparable DeFi/GameFi protocols: $0.50 (Month 6) → $1.50 (Month 12) → $5.00 (Month 24) → $12.00 (Month 36). These are targets, not guarantees.

#### `[EXPLAIN TO USER]` Block — "wATA / No NFT Entry"

> *Don't want to buy an NFT right away? You can stake ATA tokens to get a temporary battle license (wATA). Stake 100 ATA → fight in Class A. No upfront NFT purchase required. This lowers the barrier to entry for new participants and keeps the token in demand.*

---

### SECTION 7 — ARCHITECTURE (TECHNICAL CREDIBILITY)

**Goal:** Prove to technical investors and developers that the platform is genuinely decentralized, not theater.

#### Layout: Vertical flow diagram on dark background

Render the P2P architecture as a visual flow. Use SVG or canvas — NOT a screenshot of an ASCII diagram.

**Four Layers (top to bottom):**

```
LAYER 1: USER INTERFACE
[Svelte Frontend] — Connects to user's local P2P node via WebSocket

LAYER 2: LOCAL P2P NODE (.NET 9)
[AI Model Execution] [LibP2P Networking] [IPFS Client] [Blockchain RPC]

LAYER 3: P2P NETWORK MESH
[Node A] ←→ [Node B] ←→ [Node C]
         ↓ Kademlia DHT ↓
      [Matchmaking Registry]

LAYER 4: BLOCKCHAIN (MANTLE)
[NFT Contract] [ATA Token] [Battle Verifier] [DAO Governor]
                    ↓
LAYER 5: DECENTRALIZED STORAGE (IPFS)
[User Private Data] | [DAO Aggregate Dataset]
```

Each layer is a horizontal band with labeled boxes connected by animated arrows.

#### Key Stats to Display Here

```
$630/month infrastructure cost   ←→   $43,000/month (centralized equivalent)
         98.5% cost reduction
         
25,000 nodes = 25,000x compute
Linear cost, exponential value
```

#### `[EXPLAIN TO INVESTOR]` Block

> **Why P2P Architecture is a Moat:**
> Every new user who joins brings their own compute — the platform scales for free. Traditional AI training platforms must raise capital to buy more servers as they grow. AI Training Arena's marginal infrastructure cost is zero. This creates sustainable unit economics from Day 1 and eliminates the "death by success" scaling problem that kills most Web3 projects.

#### `[EXPLAIN TO USER]` Block

> **"What does running a node mean?"**
> *When you join, you download a small .NET application (Windows, Mac, or Linux). This is your "node." It runs your AI model locally, connects to other players' nodes to set up battles, and submits results to the blockchain. You don't need to know anything about blockchain or networking — the node handles all of it automatically. Think of it like a gaming client that also mines tokens.*

---

### SECTION 8 — STAKING & REWARDS CALCULATOR (INTERACTIVE)

**Goal:** Let users see their potential earnings based on class and participation level. Drives conversion.

#### Layout: Full-width interactive section

**Inputs (left panel):**
- Agent Class selector (A–E, button group)
- Number of agents: slider (1–10)
- Daily battles per agent: slider (1–8)
- ATA Price assumption: text input (default: $1.00)

**Outputs (right panel, updates live):**
```
Daily Battle Rewards:     X.XX ATA    ($X.XX USD)
Monthly Battle Rewards:   X.XX ATA    ($X.XX USD)
Estimated Annual Yield:   X.XX%
Data Marketplace (est.):  $X.XX/month
Staking APY (30% pool):   30% APY
```

**Implementation note:** All calculations are deterministic and run client-side in Svelte stores. No API calls needed.

**Formula to encode:**
- `daily_reward = class_base_reward × reward_multiplier × battles_per_day × num_agents`
- Class A base: 0.041 ATA. Multipliers: A=1.0x, B=1.2x, C=1.5x, D=2.0x, E=3.0x
- Staking APY = `(365 × 8219) / total_staked` (show at assumed 10M staked = 30%)

#### `[EXPLAIN TO USER]`

> *These projections assume full battle participation and a healthy marketplace. Real earnings depend on your win rate, network participation, ATA market price, and data sales. Past performance of comparable platforms is not a guarantee of future returns. Treat this calculator as a planning tool, not a promise.*

---

### SECTION 9 — DAO GOVERNANCE

**Goal:** Explain decentralized governance simply. Investors want to see that founders can't rug. Users want to know they have a voice.

#### Layout: Three-column feature grid

**Column 1 — Voting Rights**
Icon: 🏛️
> *Every ATA token = 1 vote. Hold more ATA or stake longer for amplified voting power. Proposals require 1% of supply to submit and 10% quorum to pass.*

**Column 2 — Treasury Control**
Icon: 🔐
> *The 10M ATA DAO treasury is on-chain. No single wallet controls it. Multi-sig + timelock on all fund movements. Spend proposals require community vote.*

**Column 3 — Telemetry Governance**
Icon: 📊
> *The DAO decides what aggregate AI training data is made public, who can access it, and at what price. This protects users while enabling research partnerships.*

#### `[EXPLAIN TO INVESTOR]`

> **Governance as Risk Mitigation:**
> Founder allocation is 5% (5M ATA), vested over 24 months with a 6-month cliff. Founders cannot vote with unvested tokens. The DAO treasury (10M ATA) cannot be accessed without a passed on-chain vote with 10% participation quorum. This structure prevents founder capture and aligns long-term incentives.

#### `[EXPLAIN TO USER]` — "What can I vote on?"

> *You can vote on: new agent class introductions, reward rate adjustments, treasury spending (grants, audits, marketing), data marketplace pricing, and protocol upgrades. Voting happens at dao.aitrainingarena.com. Every passed proposal is executed automatically by smart contracts — no humans in the middle.*

---

### SECTION 10 — ROADMAP

**Goal:** Show credibility through a phased, realistic timeline.

#### Layout: Horizontal timeline (desktop) / Accordion (mobile)

**Q1 2026 (Jan–Mar) — COMPLETE ✓**
- Research & architecture design
- Dr. Zero framework analysis
- Tokenomics modeling

**Q2 2026 (Apr–Jun) — IN PROGRESS**
- Smart contract development (ERC-721 NFT, ERC-20 ATA, Battle Verifier, DAO Governor)
- .NET P2P node alpha
- Security audits (CertiK)

**Q3 2026 (Jul–Sep) — UPCOMING**
- Mantle testnet launch → public mainnet
- Whitelist NFT sale (1,000 agents) → Public sale (24,000 agents)
- Staking activation
- Data Marketplace launch
- CoinGecko / CMC listings

**Q4 2026 (Oct–Dec) — PLANNED**
- Cross-chain bridge (Ethereum L1, Polygon)
- Custom model uploads
- Advanced analytics dashboard

**2027+ — VISION**
- Mobile app
- Tournament system ($1M prize pools)
- Enterprise API
- Decentralized compute network (GPU sharing)

**Visual treatment:**
- Each phase is a card with colored left-border (green = done, blue = in progress, purple = upcoming, muted = future)
- Completed items have a ✓ checkmark
- In-progress items have an animated pulsing dot

---

### SECTION 11 — INVESTOR METRICS (DEDICATED BLOCK)

**Goal:** Aggregate all key financial/growth metrics in one scannable section for investors.

#### Layout: Stats grid + Revenue table

**Top stats row (6 cards):**
```
25,000       |  200,000    |  $1.4M       |  100M+      |  $50M+     |  $630/mo
Total NFTs   |  Battles/Day|  NFT Sale    |  Training   |  Target    |  Infra Cost
             |  at capacity|  Revenue     |  Interactions|  TVL       |
```

**Revenue table:**

| Revenue Stream | Month 6 | Month 12 | Month 24 |
|---|---|---|---|
| Battle Entry Fees | $3,000/day | $10,000/day | $15,000/day |
| NFT Royalties | $125/day | $300/day | $500/day |
| Data Marketplace | $100/day | $2,000/day | $10,000/day |
| Staking Protocol Fees | $100/day | $500/day | $2,000/day |
| **Total Protocol** | **~$3,325/day** | **~$12,800/day** | **~$27,500/day** |

#### `[EXPLAIN TO INVESTOR]`

> *Revenue projections assume gradual ecosystem adoption: 5,000 active agents at Month 6, 15,000 at Month 12, 25,000 at Month 24. The data marketplace is the highest-margin revenue stream and scales with AI industry demand for training data — a market projected at $20B+ by 2027. All revenue projections are conservative estimates, not guarantees. A full financial model is available in the master blueprint document.*

---

### SECTION 12 — FAQ

**Goal:** Pre-empt the top 10 questions. Accordion component.

```
Q: Is this a real AI or just a game?
A: Both. Your NFT contains a real, running AI model (e.g., Qwen 2.5 7B or Llama 3). The "game" is a structured framework for generating AI training data. The AI genuinely learns from each battle.

Q: Do I need technical skills to participate?
A: For Class A, no. Download the node application, connect your wallet, and the node handles the rest. For Class D/E (data center scale), some familiarity with cloud GPU services is helpful.

Q: What blockchain is this on?
A: Mantle Network (EVM-compatible). Smart contracts are Solidity. Future bridges to Ethereum L1, Polygon, and Solana are planned for 2027.

Q: How are battles verified? Can someone cheat?
A: Both nodes independently calculate results and submit cryptographic proofs to the BattleVerifier smart contract. If proofs don't match, a 1-hour challenge period begins with oracle arbitration. Dishonest parties face slashing (loss of staked tokens).

Q: What happens if I close my computer mid-battle?
A: Your node will attempt to gracefully submit partial results. Repeated disconnections lower your node's reliability score, reducing your matchmaking priority. A reconnect-and-resume protocol is in development.

Q: Who owns the training data generated by my agent?
A: You do. All training data is encrypted with your public key and pinned to your IPFS node. You can sell it via the data marketplace or keep it private forever.

Q: Is ATA listed on exchanges?
A: Not yet. ATA launches alongside the mainnet in Q3 2026. Listings on Bybit and decentralized exchanges (Mantle DEXes) are targeted for launch week.

Q: What are the risks?
A: Smart contract exploits (mitigated by CertiK audit + bug bounty), token price volatility, regulatory changes, and technical failures. Read the full risk analysis in the blueprint. Crypto investments carry significant risk.

Q: Can I have multiple agents?
A: Yes. You can own and operate multiple NFT agents across different classes. Each runs as a separate process on your node. Hardware limits how many you can run simultaneously.

Q: Is this connected to Solana?
A: The current version launches on Mantle (EVM). Solana integration is on the 2027 roadmap. The visual aesthetic reflects Solana's brand as a nod to the broader Web3 culture.
```

---

### SECTION 13 — FOOTER

**Layout:** 4-column grid

**Col 1 — Brand:**
- Logo + tagline: *"Train the future. Own the data."*
- Built by 4th Systems
- Deployed on Mantle Network

**Col 2 — Links:**
- Whitepaper (PDF)
- Smart Contracts (Mantle Explorer)
- GitHub (open-source components)
- Brand Assets

**Col 3 — Community:**
- Discord
- Twitter/X (@AITrainingArena)
- Telegram
- DAO Portal

**Col 4 — Legal:**
- *This site is for informational purposes only and does not constitute financial advice. Cryptocurrency investments carry significant risk. Consult legal and financial advisors before participating.*
- Terms of Service
- Privacy Policy

**Bottom bar:** `© 2026 AI Training Arena · Built by 4th Systems · All Rights Reserved`

---

## 5. ANIMATIONS & INTERACTIONS SPEC

### Page Load Sequence
1. Nav fades in (0ms → 300ms)
2. Hero eyebrow slides in from left (200ms)
3. H1 fades up, letters stagger (400ms–800ms)
4. H2 fades up (700ms)
5. Stats counter-up (900ms)
6. CTAs fade in (1100ms)
7. Hero canvas/animation starts (500ms)

### Scroll Animations
Use IntersectionObserver or `@motionone/svelte` `inView`.

- Section headings: `translateY(40px) → 0, opacity 0 → 1`
- Cards: staggered, 100ms delay per card
- Charts: draw-on animation triggered by visibility
- Stats: count-up triggered by visibility

### Battle Grid (Section 4)
- Each cell: pulsing border glow (CSS `@keyframes pulse-border`)
- State transitions: `opacity 0.3s ease`
- "LIVE" badge: blinking red dot
- Progress bars: CSS animation, `animation-duration` randomized between 10s–25s

### Hover States
- Buttons: scale(1.02), brightness(1.1), 150ms ease
- Class cards: translateY(-4px), enhanced box-shadow, 200ms ease
- Nav links: color transition to `--sol-green`, 150ms
- FAQ items: background color shift to `--bg-elevated`

---

## 6. ACCESSIBILITY & PERFORMANCE

### Accessibility
- All images: `alt` attributes required
- Color contrast: minimum 4.5:1 for body text, 3:1 for large text
- Focus rings: visible, custom `--sol-green` outline, never `outline: none`
- Keyboard navigation: full keyboard access for all interactive elements
- ExplainerCard: Escape key closes, returns focus to trigger
- `prefers-reduced-motion`: wrap all animations in `@media (prefers-reduced-motion: no-preference)`

### Performance
- Fonts: `font-display: swap`
- Images: WebP format, lazy loading
- Three.js canvas: load only if `window.innerWidth > 768` (skip on mobile)
- Vite: code splitting per route/section
- Target: Lighthouse 90+ on mobile

---

## 7. RESPONSIVE BREAKPOINTS

```css
/* Tailwind custom breakpoints */
sm:  640px   — stacks most grids to 1 col
md:  768px   — 2-col where appropriate
lg:  1024px  — full desktop layout
xl:  1280px  — max-width container
2xl: 1536px  — hero canvas gets bigger
```

Key responsive rules:
- Hero: 2-col (desktop) → 1-col with canvas below fold (mobile)
- Agent class grid: 5-col → 3-col (md) → 1-col (sm)
- Battle grid: 5×5 → 3×3 with "View All" (sm)
- Roadmap: horizontal timeline → vertical accordion (sm)
- Stats row: 6-col → 3×2 grid (sm)

---

## 8. COPY TONE GUIDELINES

- **Not hype. Not dry.** Speak like a brilliant friend who understands both AI and crypto.
- **No jargon without immediate explanation.** Every technical term used must be followed by a plain-English definition on first use.
- **Active voice.** "Your agent earns tokens" not "tokens are earned by agents."
- **Specific numbers beat vague claims.** "$630/month infrastructure" beats "very low cost."
- **Respect the reader's intelligence.** Don't condescend, but don't assume knowledge.
- **Urgency without FOMO-bait.** "Whitelist spots are limited" is fine. "LAST CHANCE 🔥🔥" is not.

---

## 9. LEGAL DISCLAIMER PLACEMENT

The following disclaimer must appear in **three locations**:
1. Footer (always visible)
2. Tokenomics section (inline, above the chart)
3. Investor Metrics section (inline, beneath the revenue table)

**Standard disclaimer text:**
> *This document and website are for informational purposes only. Nothing here constitutes financial advice, investment advice, or a solicitation to invest. Cryptocurrency and digital asset investments carry significant risk including total loss of capital. Past projections are not indicative of future results. Consult qualified legal and financial advisors before participating. Geographic restrictions may apply.*

---

## 10. GAS TOWN AGENT HANDOFF NOTES

### What Agents Should NOT Change
- Color variables in `:root` — these define the brand
- The `ExplainerCard` component behavior — tooltips must open on click, not hover
- Legal disclaimer text — use verbatim
- Font families

### What Agents CAN Customize
- Animation timings (adjust for performance)
- Card layout density (compress if too much whitespace)
- Chart library (recharts, Chart.js, D3, or SVG all acceptable)
- Icon set (Lucide recommended but Heroicons or Phosphor acceptable)

### Build Checklist for Agents

```
[ ] Vite project initialized with Svelte 5 + Tailwind CSS v4
[ ] All CSS variables defined in globals.css
[ ] Fonts loaded via Google Fonts (Syne, Space Mono, Inter)
[ ] NavBar — sticky, blur-on-scroll, mobile hamburger
[ ] Hero — H1 gradient text, animated stats, 3D canvas or fallback SVG
[ ] ExplainerCard component — all [EXPLAIN] annotations wired up
[ ] Section 2 (How It Works) — 4 steps, all annotations present
[ ] Section 3 (Agent Classes) — 5 cards, correct data, tooltips
[ ] Section 4 (Battle Grid) — animated 5×5 grid, filter tabs
[ ] Section 5 (Dr. Zero) — animated loop diagram, investor block
[ ] Section 6 (Tokenomics) — animated donut chart, mechanism cards
[ ] Section 7 (Architecture) — layered diagram, cost comparison
[ ] Section 8 (Calculator) — working Svelte store, live output
[ ] Section 9 (Governance) — 3-col grid, investor block
[ ] Section 10 (Roadmap) — 4 phases, correct dates
[ ] Section 11 (Investor Metrics) — stats grid, revenue table
[ ] Section 12 (FAQ) — 10 items, accordion
[ ] Section 13 (Footer) — 4-col, legal disclaimer
[ ] Legal disclaimers in 3 locations
[ ] All ExplainerCard [?] badges present and functional
[ ] Responsive breakpoints tested on 375px, 768px, 1280px
[ ] Lighthouse mobile score ≥ 90
[ ] prefers-reduced-motion respected
[ ] Keyboard navigation functional
[ ] Deploy to Vercel or Cloudflare Pages
```

---

*Design Document Version: 1.0*
*Prepared by: 4th Systems / Three.group*
*Platform: AI Training Arena*
*Target Chains: Mantle (EVM) + Solana (future)*
*Last Updated: March 2026*

---

> **Final Note to Gas Town Agents:** Every `[EXPLAIN TO USER]` and `[EXPLAIN TO INVESTOR]` annotation in this document represents a required UI element — not optional copy. The platform's success with non-technical investors and first-time crypto participants depends on zero-friction comprehension. When in doubt: add the explainer, not remove it.
