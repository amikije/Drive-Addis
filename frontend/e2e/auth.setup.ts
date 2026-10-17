import { test as setup, expect } from '@playwright/test';
import * as fs from 'fs';

const authDir = 'playwright/.auth';

setup('authenticate as student', async ({ page }) => {
  if (!fs.existsSync(authDir)) {
    fs.mkdirSync(authDir, { recursive: true });
  }

  await page.goto('/login');
  await expect(page.getByRole('heading', { name: /welcome back/i })).toBeVisible();

  // Use demo student button or fill credentials
  const demoStudentBtn = page.getByRole('button', { name: /student/i });
  if (await demoStudentBtn.isVisible()) {
    await demoStudentBtn.click();
  } else {
    await page.getByLabel(/phone number/i).fill(process.env.TEST_STUDENT_PHONE ?? '+251922000001');
    await page.getByLabel(/password/i).fill(process.env.TEST_STUDENT_PASS ?? 'StudentPass123!');
  }

  await page.getByRole('button', { name: /sign in/i }).click();

  // Ensure redirected away from login to home or student dashboard
  await expect(page).not.toHaveURL(/\/login/);
  await page.context().storageState({ path: `${authDir}/student.json` });
});
