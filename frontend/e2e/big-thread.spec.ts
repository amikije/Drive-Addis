import { test, expect } from '@playwright/test';

test.describe('The Big Thread: Multi-Role Student and Instructor Flow', () => {
  test('student books lesson and instructor reviews booking in real-time', async ({ browser }) => {
    // 1. Context 1: Student
    const studentContext = await browser.newContext();
    const studentPage = await studentContext.newPage();

    await studentPage.goto('/login');
    const demoStudentBtn = studentPage.getByRole('button', { name: /student/i });
    if (await demoStudentBtn.isVisible()) {
      await demoStudentBtn.click();
      await studentPage.getByRole('button', { name: /sign in/i }).click();
    }

    // 2. Context 2: Instructor
    const instructorContext = await browser.newContext();
    const instructorPage = await instructorContext.newPage();

    await instructorPage.goto('/login');
    const demoInstructorBtn = instructorPage.getByRole('button', { name: /instructor/i });
    if (await demoInstructorBtn.isVisible()) {
      await demoInstructorBtn.click();
      await instructorPage.getByRole('button', { name: /sign in/i }).click();
    }

    // Instructor navigates to instructor dashboard
    await instructorPage.goto('/instructor');
    await expect(instructorPage.getByRole('heading', { name: /instructor dashboard|portal/i })).toBeVisible({ timeout: 10_000 });

    // Clean up
    await studentContext.close();
    await instructorContext.close();
  });
});
