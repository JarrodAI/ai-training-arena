<script lang="ts">
  // Placeholder stats — in production these would be fetched from the indexer API
  const stats = [
    { label: 'Active Battles', value: '—', color: '#14F195', unit: '' },
    { label: 'Total Agents', value: '—', color: '#00C2FF', unit: '' },
    { label: 'ATA Burned', value: '—', color: '#FF6B6B', unit: '' },
    { label: 'Total Battles', value: '—', color: '#9945FF', unit: '' },
    { label: 'ATA Distributed', value: '—', color: '#FFD166', unit: '' },
    { label: 'Active Nodes', value: '—', color: '#4ECDC4', unit: '' },
  ];
</script>

<section id="stats" class="py-24 bg-arena-dark">
  <div class="max-w-7xl mx-auto px-4">
    <div class="text-center mb-16">
      <h2 class="font-syne font-bold text-4xl md:text-5xl mb-4">Live Network Stats</h2>
      <p class="text-white/50">
        Real-time data from the AI Training Arena P2P network.
        <span class="text-arena-green/70 text-sm ml-2">Updates on mainnet launch</span>
      </p>
    </div>

    <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
      {#each stats as stat}
        <div class="card text-center group hover:border-arena-green/30 transition-colors">
          <div
            class="font-space-mono text-3xl font-bold mb-2"
            style="color: {stat.color};"
          >
            {stat.value}
          </div>
          <div class="text-white/50 text-sm">{stat.label}</div>
        </div>
      {/each}
    </div>

    <p class="text-center text-white/30 text-sm mt-8">
      Stats will populate when the network launches on Mantle Mainnet.
    </p>
  </div>
</section>
