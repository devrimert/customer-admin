import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Forward /api to the .NET API. The browser only talks to one address, so no CORS is needed.
    proxy: {
      '/api': 'http://localhost:5261',
    },
  },
})