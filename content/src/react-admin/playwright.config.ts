import { defineConfig, devices } from '@playwright/test';

const port = 11027;

/**
 * 前端 E2E 使用独立 Vite 端口，避免误连其他工作区的开发服务。
 */
export default defineConfig({
  testDir: './e2e',
  testMatch: '**/*.spec.ts',
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: `http://127.0.0.1:${port}`,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    headless: process.env.HEADED !== '1',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    command: `bun run dev -- --host 127.0.0.1 --port ${port} --strictPort`,
    url: `http://127.0.0.1:${port}/login`,
    reuseExistingServer: false,
    timeout: 60_000,
  },
});
