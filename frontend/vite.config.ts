import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig(({ command }) => ({
  plugins: [react(), tailwindcss()],
  // En build (GitHub Pages) el sitio se sirve en /FirmaDigital/, no en la raíz del dominio.
  base: command === 'build' ? '/FirmaDigital/' : '/',
  server: {
    port: 5173,
  },
}))
