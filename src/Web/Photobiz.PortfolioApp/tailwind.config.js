/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./Views/**/*.cshtml'],
  theme: {
    extend: {
      colors: {
        // Per-tenant values — set as CSS custom properties by _Layout.cshtml from the tenant's
        // SiteTheme, referenced here so Tailwind utilities (bg-brand-primary, text-brand-accent,
        // ...) stay themeable without a per-tenant Tailwind build.
        brand: {
          primary: 'var(--color-primary)',
          secondary: 'var(--color-secondary)',
          accent: 'var(--color-accent)',
        },
      },
      fontFamily: {
        brand: ['var(--font-family)', 'ui-sans-serif', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [],
};
