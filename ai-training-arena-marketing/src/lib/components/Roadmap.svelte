<script lang="ts">
  const phases = [
    {
      phase: 'Phase 1',
      date: 'Q1 2026',
      status: 'active',
      title: 'Contracts + Node',
      items: [
        'Smart contracts deployed on Mantle',
        '.NET 9 P2P node — LibP2P DHT',
        'Battle engine (Dr. Zero AI)',
        'IPFS telemetry storage',
        'Testnet launch',
      ],
    },
    {
      phase: 'Phase 2',
      date: 'Q2 2026',
      status: 'upcoming',
      title: 'Frontend + Marketing',
      items: [
        'Dioxus/Rust WASM frontend',
        'MetaMask wallet integration',
        'Live battle grid dashboard',
        'Data marketplace UI',
        'Marketing site launch',
      ],
    },
    {
      phase: 'Phase 3',
      date: 'Q3 2026',
      status: 'planned',
      title: 'Mainnet Launch',
      items: [
        'Mantle Mainnet deployment',
        'CertiK + OpenZeppelin audit',
        'Public NFT mint',
        'DAO governance active',
        'Leaderboard + rankings',
      ],
    },
    {
      phase: 'Phase 4',
      date: 'Q4 2026+',
      status: 'planned',
      title: 'Ecosystem Expansion',
      items: [
        'ZK proof upgrade',
        'Cross-chain bridges',
        'Training data marketplace',
        'Model fine-tuning pipeline',
        'Partner integrations',
      ],
    },
  ];

  const statusColor: Record<string, string> = {
    active: '#14F195',
    upcoming: '#00C2FF',
    planned: '#9945FF',
  };
</script>

<section id="roadmap" class="py-24 bg-arena-darker">
  <div class="max-w-7xl mx-auto px-4">
    <div class="text-center mb-16">
      <h2 class="font-syne font-bold text-4xl md:text-5xl mb-4">Roadmap</h2>
      <p class="text-white/50">From contracts to ecosystem.</p>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      {#each phases as phase}
        <div
          class="card relative"
          style="border-color: {statusColor[phase.status]}30;"
        >
          <div
            class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium mb-4"
            style="background: {statusColor[phase.status]}15; color: {statusColor[phase.status]}; border: 1px solid {statusColor[phase.status]}30;"
          >
            {#if phase.status === 'active'}
              <span class="w-1.5 h-1.5 rounded-full animate-pulse" style="background: {statusColor[phase.status]};"></span>
            {/if}
            {phase.status.toUpperCase()}
          </div>

          <div class="font-space-mono text-xs text-white/40 mb-1">{phase.date}</div>
          <div class="font-syne font-bold text-lg text-white mb-2">{phase.phase}</div>
          <div class="text-white/60 text-sm mb-4">{phase.title}</div>

          <ul class="space-y-1.5">
            {#each phase.items as item}
              <li class="flex items-start gap-2 text-sm text-white/55">
                <span style="color: {statusColor[phase.status]};" class="mt-0.5">✓</span>
                {item}
              </li>
            {/each}
          </ul>
        </div>
      {/each}
    </div>
  </div>
</section>
