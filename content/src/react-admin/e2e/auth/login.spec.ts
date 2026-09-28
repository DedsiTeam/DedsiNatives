import { expect, test } from '@playwright/test';

test('登录页展示必填校验，不发送空账号请求', async ({ page }) => {
  await page.goto('/login');

  await expect(page.getByRole('heading', { name: 'Dedsi Admin' })).toBeVisible();
  await page.getByRole('button', { name: '立即登录' }).click();

  await expect(page.getByText('请输入您的账号名')).toBeVisible();
  await expect(page.getByText('请输入您的登录密码')).toBeVisible();
  await expect(page).toHaveURL(/\/login$/);
});
