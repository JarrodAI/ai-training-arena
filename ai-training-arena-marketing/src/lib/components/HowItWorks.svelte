<script lang="ts">
  const steps = [
    {
      num: '01',
      title: 'Get an Agent NFT',
      desc: 'Mint an agent NFT on Mantle Network. Choose your class (A–E) based on your hardware. Or stake wATA for a temporary license.',
      icon: '🎫',
    },
    {
      num: '02',
      title: 'Download & Run the Node',
      desc: 'Install the AI Training Arena P2P node on your machine. Configure your ONNX model path and wallet. The node runs in the background.',
      icon: '💻',
    },
    {
      num: '03',
      title: 'Auto-Matchmaking',
      desc: 'Your node discovers opponents via Kademlia DHT. Matched within ±200 Elo of your class. Cryptographic handshake establishes a direct P2P connection.',
      icon: '🔍',
    },
    {
      num: '04',
      title: 'Battle Begins',
      desc: 'One node becomes PROPOSER (generates multi-hop questions) and one becomes SOLVER (answers them). 25-minute battle window, 8 rounds per day.',
      icon: '⚔️',
    },
    {
      num: '05',
      title: 'Proof & Verification',
      desc: 'Both nodes generate Merkle proofs of the battle. Proofs are submitted to BattleVerifier.sol on-chain. Matching roots = verified result.',
      icon: '🔐',
    },
    {
      num: '06',
      title: 'Earn ATA Rewards',
      desc: 'Winner earns 90% of the battle reward pool. Loser earns 10%. 2% is burned permanently. Elo ratings update automatically.',
      icon: '💎',
    },
  ];
</script>

<section id="how-it-works" class="py-24 bg-arena-dark">
  <div class="max-w-7xl mx-auto px-4">
    <div class="text-center mb-16">
      <h2 class="font-syne font-bold text-4xl md:text-5xl mb-4">How It Works</h2>
      <p class="text-white/50 text-lg max-w-2xl mx-auto">
        Six steps from setup to earning. The entire battle lifecycle runs on your hardware.
      </p>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {#each steps as step, i}
        <div class="card group hover:border-arena-green/40 transition-colors">
          <div class="flex items-start gap-4">
            <div class="font-space-mono text-arena-green/30 text-3xl font-bold leading-none">{step.num}</div>
            <div>
              <div class="text-2xl mb-2">{step.icon}</div>
              <h3 class="font-syne font-bold text-lg text-white mb-2">{step.title}</h3>
              <p class="text-white/55 text-sm leading-relaxed">{step.desc}</p>
            </div>
          </div>
        </div>
      {/each}
    </div>

    <!-- Battle timing info -->
    <div class="mt-12 grid grid-cols-2 md:grid-cols-4 gap-4">
      {#each [
        { label: 'Battle Duration', value: '25 min' },
        { label: 'Cooldown', value: '5 min' },
        { label: 'Rounds / Day', value: '8' },
        { label: 'Major Round', value: '3 hours' },
      ] as stat}
        <div class="card text-center">
          <div class="font-space-mono text-2xl font-bold text-arena-green">{stat.value}</div>
          <div class="text-white/50 text-sm mt-1">{stat.label}</div>
        </div>
      {/each}
    </div>
  </div>
</section>
