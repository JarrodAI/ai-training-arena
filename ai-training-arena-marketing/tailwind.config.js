/** @type {import('tailwindcss').Config} */
export default {
  content: ['./src/**/*.{html,js,svelte,ts}'],
  theme: {
    extend: {
      colors: {
        // AI Training Arena brand palette
        'arena-green':   '#14F195',
        'arena-blue':    '#00C2FF',
        'arena-purple':  '#9945FF',
        'arena-gold':    '#FFD166',
        'arena-dark':    '#0A0A0F',
        'arena-darker':  '#050508',
        'arena-card':    '#12121A',
        'arena-border':  '#1E1E2E',
      },
      fontFamily: {
        syne: ['Syne', 'sans-serif'],
        'space-mono': ['"Space Mono"', 'monospace'],
      },
      animation: {
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'glow': 'glow 2s ease-in-out infinite alternate',
      },
      keyframes: {
        glow: {
          '0%': { boxShadow: '0 0 5px currentColor' },
          '100%': { boxShadow: '0 0 20px currentColor, 0 0 40px currentColor' },
        }
      }
    },
  },
  plugins: [],
};
