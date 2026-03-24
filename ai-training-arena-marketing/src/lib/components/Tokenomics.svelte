<script lang="ts">
  // Canonical tokenomics from Part 0B shared data models
  const distribution = [
    { label: 'Battle Rewards Pool',  pct: 30, color: '#14F195' },
    { label: 'NFT Vesting',          pct: 35, color: '#9945FF' },
    { label: 'Staking Rewards',      pct: 15, color: '#00C2FF' },
    { label: 'DAO Treasury',         pct: 10, color: '#FFD166' },
    { label: 'Founders (24mo vest)', pct: 5,  color: '#FF6B6B' },
    { label: 'Liquidity',            pct: 3,  color: '#4ECDC4' },
    { label: 'Team & Advisors',      pct: 2,  color: '#95A5A6' },
  ];

  const mechanics = [
    { icon: '🔥', title: 'Burn Mechanism', desc: '2% of every battle reward is burned permanently. Supply decreases with every battle.' },
    { icon: '💰', title: 'Buyback', desc: '5% of weekly protocol revenue buys ATA from the market and burns it.' },
    { icon: '🏦', title: 'Staking', desc: 'Stake ATA alongside your NFT to earn staking rewards. 7-day unstaking cooldown.' },
    { icon: '🗳️', title: 'Governance', desc: 'Staked ATA = 2× voting power. NFT + staked = 3× voting power.' },
  ];
</script>

<section id="tokenomics" class="py-24 bg-arena-darker">
  <div class="max-w-7xl mx-auto px-4">
    <div class="text-center mb-16">
      <h2 class="font-syne font-bold text-4xl md:text-5xl mb-4">
        <span class="gradient-text">ATA Tokenomics</span>
      </h2>
      <p class="text-white/50 text-lg">100,000,000 ATA total supply. Deflationary. DAO-governed.</p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-12 items-start">
      <!-- Distribution table -->
      <div>
        <h3 class="font-syne font-bold text-xl mb-6 text-white">Token Distribution</h3>
        <div class="space-y-3">
          {#each distribution as item}
            <div class="flex items-center gap-3">
              <div class="w-3 h-3 rounded-full flex-shrink-0" style="background: {item.color};"></div>
              <div class="flex-1 text-white/70 text-sm">{item.label}</div>
              <div class="font-space-mono text-sm" style="color: {item.color};">{item.pct}%</div>
              <!-- Bar -->
              <div class="w-24 bg-arena-border rounded-full h-1.5">
                <div class="h-1.5 rounded-full" style="width: {item.pct}%; background: {item.color};"></div>
              </div>
            </div>
          {/each}
        </div>

        <div class="mt-8 card">
          <div class="font-space-mono text-sm text-white/40 mb-2">DAILY EMISSION</div>
          <div class="flex justify-between items-center">
            <div>
              <div class="font-space-mono text-xl text-arena-green">8,219 ATA</div>
              <div class="text-white/50 text-xs mt-0.5">Battle rewards / day</div>
            </div>
            <div class="text-right">
              <div class="font-space-mono text-xl text-arena-blue">8,219 ATA</div>
              <div class="text-white/50 text-xs mt-0.5">Staking rewards / day</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Mechanics cards -->
      <div>
        <h3 class="font-syne font-bold text-xl mb-6 text-white">Token Mechanics</h3>
        <div class="space-y-4">
          {#each mechanics as item}
            <div class="card flex gap-4">
              <span class="text-2xl">{item.icon}</span>
              <div>
                <div class="font-syne font-semibold text-white mb-1">{item.title}</div>
                <div class="text-white/55 text-sm">{item.desc}</div>
              </div>
            </div>
          {/each}
        </div>
      </div>
    </div>
  </div>
</section>
