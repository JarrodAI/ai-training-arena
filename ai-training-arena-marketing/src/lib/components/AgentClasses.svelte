<script lang="ts">
  // Canonical agent class data from master-chapters.md Part 0B
  const classes = [
    {
      id: 'A',
      label: 'Class A',
      params: '3B–7B',
      price: '10 MNT',
      multiplier: '1.0x',
      reward: '0.041 ATA',
      wata: '100 wATA',
      supply: '15,000',
      color: '#14F195',
      hardware: 'RTX 3060 12GB, 16GB RAM',
      models: ['Qwen2.5-7B', 'Llama3.2-3B'],
    },
    {
      id: 'B',
      label: 'Class B',
      params: '7B–32B',
      price: '50 MNT',
      multiplier: '1.2x',
      reward: '0.049 ATA',
      wata: '500 wATA',
      supply: '6,000',
      color: '#00C2FF',
      hardware: 'RTX 4090 / A100 40GB, 64GB RAM',
      models: ['Qwen2.5-14B', 'Mistral-7B'],
    },
    {
      id: 'C',
      label: 'Class C',
      params: '32B–70B',
      price: '200 MNT',
      multiplier: '1.5x',
      reward: '0.062 ATA',
      wata: '2,000 wATA',
      supply: '2,500',
      color: '#9945FF',
      hardware: 'A100 80GB / 2× RTX 4090, 128GB RAM',
      models: ['Qwen2.5-32B', 'Mixtral-8x7B'],
    },
    {
      id: 'D',
      label: 'Class D',
      params: '70B–405B',
      price: '800 MNT',
      multiplier: '2.0x',
      reward: '0.082 ATA',
      wata: '8,000 wATA',
      supply: '1,200',
      color: '#FFD166',
      hardware: '4–8× A100 80GB, 512GB RAM',
      models: ['Llama3.1-70B', 'Qwen2.5-72B'],
    },
    {
      id: 'E',
      label: 'Class E',
      params: '405B+',
      price: '3,000 MNT',
      multiplier: '3.0x',
      reward: '0.123 ATA',
      wata: '30,000 wATA',
      supply: '300',
      color: 'rainbow',
      hardware: '8× H100 80GB, 1TB+ RAM',
      models: ['Llama3.1-405B', 'DeepSeek-R1-671B'],
    },
  ];
</script>

<section id="agent-classes" class="py-24 bg-arena-darker">
  <div class="max-w-7xl mx-auto px-4">
    <div class="text-center mb-16">
      <h2 class="font-syne font-bold text-4xl md:text-5xl mb-4">
        <span class="gradient-text">5 Agent Classes</span>
      </h2>
      <p class="text-white/50 text-lg max-w-2xl mx-auto">
        Choose your tier. Higher class = stronger models, bigger rewards, rarer NFTs.
      </p>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-4">
      {#each classes as cls}
        <div
          class="card hover:scale-105 transition-transform cursor-default"
          style="border-color: {cls.color}40; box-shadow: 0 0 20px {cls.color}10;"
        >
          <!-- Class badge -->
          <div
            class="inline-flex items-center justify-center w-10 h-10 rounded-lg font-space-mono font-bold text-lg mb-4"
            style="background: {cls.color}20; color: {cls.color}; border: 1px solid {cls.color}40;"
          >
            {cls.id}
          </div>

          <div class="font-syne font-bold text-white text-lg mb-1">{cls.label}</div>
          <div class="font-space-mono text-sm mb-4" style="color: {cls.color};">{cls.params}</div>

          <div class="space-y-2 text-sm text-white/60">
            <div class="flex justify-between">
              <span>Price</span>
              <span class="text-white font-medium">{cls.price}</span>
            </div>
            <div class="flex justify-between">
              <span>Multiplier</span>
              <span class="text-white font-medium">{cls.multiplier}</span>
            </div>
            <div class="flex justify-between">
              <span>Base Reward</span>
              <span class="font-space-mono" style="color: {cls.color};">{cls.reward}</span>
            </div>
            <div class="flex justify-between">
              <span>wATA Stake</span>
              <span class="text-white/80">{cls.wata}</span>
            </div>
            <div class="flex justify-between">
              <span>Supply</span>
              <span class="text-white/80">{cls.supply}</span>
            </div>
          </div>

          <div class="mt-4 pt-4 border-t border-arena-border">
            <div class="text-xs text-white/40 mb-1">Hardware</div>
            <div class="text-xs text-white/60">{cls.hardware}</div>
          </div>
        </div>
      {/each}
    </div>

    <!-- wATA alternative note -->
    <div class="mt-12 card max-w-2xl mx-auto text-center">
      <div class="text-arena-blue font-semibold mb-2">No NFT? No problem.</div>
      <p class="text-white/60 text-sm">
        Stake wATA tokens for a temporary license to participate without owning an NFT.
        Wrap your ATA, stake the required amount, and start battling immediately.
      </p>
    </div>
  </div>
</section>
