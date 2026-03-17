import { test, expect } from '@playwright/test';
import { waitForFlutterApp } from '../fixtures/auth-helpers';
import { flutterReady } from '../fixtures/test-fixtures';

test.describe('Flutter Web — Smoke Tests', () => {
  test('app loads and renders flutter-view', async ({ page }) => {
    await page.goto('/');
    await flutterReady(page);

    const flutterView = page.locator('flutter-view, flt-glass-pane');
    await expect(flutterView).toBeAttached();
  });

  test('unauthenticated user sees initial screen', async ({ page }) => {
    // Mock auth endpoints to reject — anonymous user
    await page.route('**/api/auth/me', (route) =>
      route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({ success: false, error: 'unauthenticated' }),
      }),
    );

    await page.goto('/');
    await waitForFlutterApp(page);

    // Flutter web renders inside a shadow DOM canvas — verify the view exists
    // and the page title matches the app
    const flutterView = page.locator('flutter-view, flt-glass-pane');
    await expect(flutterView).toBeAttached();
    await expect(page).toHaveTitle(/Musicratic/i);
  });
});
