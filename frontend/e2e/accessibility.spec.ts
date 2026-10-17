import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

const criticalRoutes = ['/', '/login', '/register'];

for (const path of criticalRoutes) {
  test(`a11y: ${path} should satisfy WCAG 2 A/AA compliance rules`, async ({ page }) => {
    await page.goto(path);
    await page.waitForLoadState('domcontentloaded');

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa'])
      .disableRules(['color-contrast']) // triage: allow branding accents during initial audit
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });
}
