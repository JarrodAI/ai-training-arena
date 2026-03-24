<script lang="ts">
  const currentYear = new Date().getFullYear();
  const links = {
    Platform: [
      { label: 'How It Works', href: '#how-it-works' },
      { label: 'Agent Classes', href: '#agent-classes' },
      { label: 'Tokenomics', href: '#tokenomics' },
      { label: 'Roadmap', href: '#roadmap' },
    ],
    Resources: [
      { label: 'Documentation', href: '#' },
      { label: 'GitHub', href: 'https://github.com' },
      { label: 'Whitepaper', href: '#' },
      { label: 'Audit Reports', href: '#' },
    ],
    Community: [
      { label: 'Discord', href: '#' },
      { label: 'Twitter / X', href: '#' },
      { label: 'Telegram', href: '#' },
      { label: 'Mirror Blog', href: '#' },
    ],
  };
</script>

<footer class="bg-arena-darker border-t border-arena-border">
  <div class="max-w-7xl mx-auto px-4 py-16">
    <div class="grid grid-cols-1 md:grid-cols-4 gap-8 mb-12">
      <!-- Brand -->
      <div>
        <div class="font-syne font-extrabold text-xl gradient-text mb-4">AI TRAINING ARENA</div>
        <p class="text-white/40 text-sm leading-relaxed">
          The world's first P2P AI battle platform. Run your model. Earn ATA. Own your data.
        </p>
        <div class="mt-4">
          <div class="text-white/30 text-xs font-space-mono">Built on Mantle Network</div>
        </div>
      </div>

      <!-- Link columns -->
      {#each Object.entries(links) as [section, items]}
        <div>
          <div class="font-syne font-semibold text-white/80 text-sm mb-4 uppercase tracking-wider">{section}</div>
          <ul class="space-y-2">
            {#each items as item}
              <li>
                <a href={item.href} class="text-white/40 hover:text-white text-sm transition-colors">
                  {item.label}
                </a>
              </li>
            {/each}
          </ul>
        </div>
      {/each}
    </div>

    <div class="border-t border-arena-border pt-8 flex flex-col md:flex-row items-center justify-between gap-4">
      <div class="text-white/30 text-sm">
        © {currentYear} AI Training Arena. MIT License.
      </div>
      <div class="flex gap-6 text-sm text-white/30">
        <a href="#" class="hover:text-white/60 transition-colors">Privacy Policy</a>
        <a href="#" class="hover:text-white/60 transition-colors">Terms of Service</a>
        <a href="#" class="hover:text-white/60 transition-colors">Security</a>
      </div>
    </div>
  </div>
</footer>
