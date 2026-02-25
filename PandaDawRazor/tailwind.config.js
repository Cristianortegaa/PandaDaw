/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./Pages/**/*.cshtml", "./Pages/**/*.cs", "./Views/**/*.cshtml"],
  theme: {
    extend: {
      fontFamily: {
        sans: ["Inter", "system-ui", "sans-serif"],
      },
    },
  },
  plugins: [require("daisyui")],
  daisyui: {
    themes: [
      {
        pandadaw: {
          "color-scheme": "light",
          primary: "#166534",
          "primary-content": "#ffffff",
          secondary: "#6b9b7d",
          "secondary-content": "#1a2e22",
          accent: "#c9a84c",
          "accent-content": "#3d3520",
          neutral: "#1a2e22",
          "neutral-content": "#f5f5f5",
          "base-100": "#fcfcfa",
          "base-200": "#f3f5f2",
          "base-300": "#e4e8e2",
          "base-content": "#1a2e22",
          info: "#3b82f6",
          "info-content": "#ffffff",
          success: "#22863a",
          "success-content": "#ffffff",
          warning: "#ca8a04",
          "warning-content": "#3d3520",
          error: "#dc2626",
          "error-content": "#ffffff",
          "--rounded-box": "1rem",
          "--rounded-btn": "0.75rem",
          "--rounded-badge": "0.75rem",
          "--animation-btn": "0.2s",
          "--animation-input": "0.2s",
          "--btn-focus-scale": "0.98",
          "--border-btn": "1px",
          "--tab-border": "1px",
          "--tab-radius": "0.75rem",
        },
      },
    ],
  },
};
