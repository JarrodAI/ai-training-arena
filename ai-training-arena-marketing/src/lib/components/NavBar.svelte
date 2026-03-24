<script lang="ts">
  let mobileOpen = false;
  const links = [
    { label: 'How It Works', href: '#how-it-works' },
    { label: 'Agent Classes', href: '#agent-classes' },
    { label: 'Tokenomics', href: '#tokenomics' },
    { label: 'Roadmap', href: '#roadmap' },
    { label: 'FAQ', href: '#faq' },
  ];
</script>

<nav class="fixed top-0 left-0 right-0 z-50 bg-arena-darker/90 backdrop-blur-md border-b border-arena-border">
  <div class="max-w-7xl mx-auto px-4 py-4 flex items-center justify-between">
    <a href="/" class="font-syne font-extrabold text-xl tracking-tight gradient-text">
      AI TRAINING ARENA
    </a>

    <!-- Desktop nav -->
    <div class="hidden md:flex items-center gap-6">
      {#each links as link}
        <a href={link.href} class="text-sm text-white/70 hover:text-white transition-colors font-medium">
          {link.label}
        </a>
      {/each}
      <a href="#join" class="btn-primary text-sm px-4 py-2">
        Join Whitelist
      </a>
    </div>

    <!-- Mobile hamburger -->
    <button
      class="md:hidden text-white"
      on:click={() => (mobileOpen = !mobileOpen)}
      aria-label="Toggle menu"
    >
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        {#if mobileOpen}
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        {:else}
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
        {/if}
      </svg>
    </button>
  </div>

  <!-- Mobile menu -->
  {#if mobileOpen}
    <div class="md:hidden bg-arena-card border-t border-arena-border px-4 py-4 flex flex-col gap-4">
      {#each links as link}
        <a
          href={link.href}
          class="text-white/80 hover:text-white font-medium"
          on:click={() => (mobileOpen = false)}
        >
          {link.label}
        </a>
      {/each}
      <a href="#join" class="btn-primary text-center">Join Whitelist</a>
    </div>
  {/if}
</nav>

<!-- Spacer for fixed nav -->
<div class="h-16"></div>
