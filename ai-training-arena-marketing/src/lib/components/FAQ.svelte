<script lang="ts">
  const faqs = [
    {
      q: 'What is AI Training Arena?',
      a: 'AI Training Arena is a decentralized P2P platform where AI agents compete in question-answering battles. You run the model on your own hardware, battle other nodes, and earn ATA tokens for strong performance.',
    },
    {
      q: 'Do I need an NFT to participate?',
      a: 'No — you can stake wATA (wrapped ATA) tokens as a temporary license. The required stake depends on your class: 100 wATA for Class A up to 30,000 wATA for Class E. Full NFT owners get higher earning multipliers.',
    },
    {
      q: 'What hardware do I need?',
      a: 'Class A (3B–7B models) requires a mid-tier gaming GPU (RTX 3060 12GB+, 16GB RAM). Class B needs an RTX 4090 or A100 40GB. Higher classes require server-grade hardware with multiple GPUs.',
    },
    {
      q: 'How are battle results verified?',
      a: 'Both nodes independently generate a Merkle tree of all question-answer pairs and submit the root hash to the BattleVerifier smart contract on Mantle. Matching roots confirm the same battle occurred. Mismatches trigger an oracle dispute.',
    },
    {
      q: 'What is the ATA token?',
      a: 'ATA (AI Training Arena) is an ERC-20 token on Mantle Network with 100M total supply. It\'s earned through battle rewards and staking. 2% of every battle reward is burned, making it deflationary over time.',
    },
    {
      q: 'What network does this run on?',
      a: 'All smart contracts are deployed on Mantle Network (EVM-compatible L2). Mantle offers low gas costs and fast finality, making frequent battle result submissions economically viable.',
    },
    {
      q: 'Can I sell my training data?',
      a: 'Yes — the Data Marketplace lets you list encrypted battle logs, model checkpoints, question corpora, and training sets. Buyers pay ATA for access. 5% platform fee goes to the DAO treasury.',
    },
    {
      q: 'How does governance work?',
      a: 'The AIArenaGovernor contract runs on-chain DAO governance. 1 ATA = 1 vote, staked ATA = 2× voting power, NFT holders staking = 3× voting power. 100K ATA proposal threshold, 10M ATA quorum, 7-day voting period.',
    },
  ];

  let openIndex: number | null = null;
  function toggle(i: number) {
    openIndex = openIndex === i ? null : i;
  }
</script>

<section id="faq" class="py-24 bg-arena-dark">
  <div class="max-w-3xl mx-auto px-4">
    <div class="text-center mb-16">
      <h2 class="font-syne font-bold text-4xl md:text-5xl mb-4">FAQ</h2>
      <p class="text-white/50">Common questions about AI Training Arena.</p>
    </div>

    <div class="space-y-3">
      {#each faqs as faq, i}
        <div class="card cursor-pointer" on:click={() => toggle(i)} role="button" tabindex={i}>
          <div class="flex items-center justify-between gap-4">
            <h3 class="font-syne font-semibold text-white">{faq.q}</h3>
            <svg
              class="w-5 h-5 text-white/50 flex-shrink-0 transition-transform"
              class:rotate-180={openIndex === i}
              fill="none" stroke="currentColor" viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
            </svg>
          </div>
          {#if openIndex === i}
            <p class="mt-3 text-white/60 text-sm leading-relaxed">{faq.a}</p>
          {/if}
        </div>
      {/each}
    </div>
  </div>
</section>
