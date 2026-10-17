import { test, expect } from '@playwright/test';

test.describe('Student Booking Flow', () => {
  test('student can browse instructors and open lesson booking modal', async ({ page }) => {
    // 1. Visit landing page
    await page.goto('/');

    // 2. Wait for instructor list to render
    const instructorCards = page.locator('.instructor-card');
    await expect(instructorCards.first()).toBeVisible({ timeout: 10_000 });

    // 3. Search for instructor by name
    const searchInput = page.getByPlaceholder(/search by name/i);
    await searchInput.fill('Abebe');

    // 4. Click book lesson button on the specific filtered card
    const targetCard = page.locator('.instructor-card', { hasText: 'Abebe' });
    await expect(targetCard).toBeVisible();

    const bookBtn = targetCard.getByRole('button', { name: /book lesson/i });
    await bookBtn.click();

    // 5. Assert booking modal opens with instructor name
    const modal = page.locator('.modal-container');
    await expect(modal).toBeVisible();
    await expect(modal).toContainText('Abebe');

    // 6. Close modal cleanly
    const closeBtn = modal.getByRole('button', { name: /cancel|close/i });
    await closeBtn.click();
    await expect(modal).not.toBeVisible();
  });
});
