/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.cshtml",
    "./Pages/**/*.cs",
    "./Views/**/*.cshtml"
  ],
  theme: {
    extend: {},
  },
  plugins: [require("daisyui")],
  daisyui: {
    themes: [
      {
        azul: {
          "color-scheme": "light",
          "primary": "oklch(78% 0.115 274.713)",
          "primary-content": "oklch(25% 0.09 281.288)",
          "secondary": "oklch(87% 0.01 258.338)",
          "secondary-content": "oklch(13% 0.028 261.692)",
          "accent": "oklch(0% 0 0)",
          "accent-content": "oklch(100% 0 0)",
          "neutral": "oklch(26% 0.051 172.552)",
          "neutral-content": "oklch(97% 0.021 166.113)",
          "base-100": "oklch(97% 0.021 166.113)",
          "base-200": "oklch(95% 0.052 163.051)",
          "base-300": "oklch(90% 0.093 164.15)",
          "base-content": "oklch(37% 0.077 168.94)",
          "info": "oklch(62% 0.214 259.815)",
          "info-content": "oklch(97% 0.014 254.604)",
          "success": "oklch(76% 0.233 130.85)",
          "success-content": "oklch(98% 0.031 120.757)",
          "warning": "oklch(79% 0.184 86.047)",
          "warning-content": "oklch(98% 0.026 102.212)",
          "error": "oklch(64% 0.246 16.439)",
          "error-content": "oklch(96% 0.015 12.422)",
          "--rounded-box": "2rem",
          "--rounded-btn": "2rem",
          "--rounded-badge": "2rem",
          "--animation-btn": "0.25s",
          "--animation-input": "0.2s",
          "--btn-focus-scale": "0.95",
          "--border-btn": "2px",
          "--tab-border": "2px",
          "--tab-radius": "0.5rem",
        },
      },
    ],
  },
}

