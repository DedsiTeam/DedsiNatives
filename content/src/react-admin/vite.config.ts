import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd())
  const target = env.VITE_API_SERVICE_URL || 'http://localhost:12256'

  return {
    plugins: [react()],
    server: {
      port: 11026,
      proxy: {
        '/api': {
          target,
          changeOrigin: true,
        },
      },
    },
  }
})
