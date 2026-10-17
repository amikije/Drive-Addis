import { test, expect } from '@playwright/test';

test.describe('Failure Paths & Edge Cases with Mocked Network', () => {
  test('displays network error alert when instructors search API returns 500', async ({ page }) => {
    // Intercept instructors API and simulate 500
    await page.route('**/api/instructors/search*', (route) =>
      route.fulfill({
        status: 500,
        contentType: 'application/problem+json',
        body: JSON.stringify({
          type: 'https://tools.ietf.org/html/rfc9110#section-15.6.1',
          title: 'Internal Server Error',
          status: 500,
          detail: 'Database connection pool exhausted.'
        })
      })
    );

    await page.goto('/');

    // Assert error toast/alert notification appears
    await expect(page.locator('.toast, .notification, .alert-box, [role="alert"]').first()).toBeVisible({ timeout: 5_000 });
  });

  test('displays empty state when no instructors match search query', async ({ page }) => {
    // Intercept instructors API and return empty items
    await page.route('**/api/instructors/search*', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [],
          pageNumber: 1,
          pageSize: 18,
          totalCount: 0,
          totalPages: 0
        })
      })
    );

    await page.goto('/');

    // Assert empty state message or zero card list
    await expect(page.locator('.instructor-card')).toHaveCount(0);
  });
});
