import { test, expect } from './fixtures/test-fixtures';

test.describe('Smoke Tests', () => {
  test('app loads and shows root element', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('app-root')).toBeAttached();
  });

  test('unauthenticated user lands on hub discovery page', async ({ page }) => {
    await page.goto('/');

    // The default route redirects to /hub which shows hub discovery
    await expect(page).toHaveURL(/\/hub/);
  });

  test('unauthenticated user sees shell layout', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('app-shell')).toBeVisible();
  });

  test('login page is accessible', async ({ page }) => {
    await page.goto('/login');
    await expect(page).toHaveURL(/\/login/);
  });

  test('protected route redirects unauthenticated user to login', async ({ page }) => {
    await page.goto('/economy');

    // Auth guard should redirect to /login
    await expect(page).toHaveURL(/\/login/);
  });

  test('auth redirect is intercepted correctly', async ({ page }) => {
    const { mockAuthentikLogin } = await import('./fixtures/auth-helpers');
    await mockAuthentikLogin(page, 'visitor');

    // Navigate to login — the OIDC authorize call should be intercepted
    await page.goto('/login');
    await expect(page).toHaveURL(/\/login/);
  });

  test('authenticated user can access protected routes', async ({ authenticatedPage }) => {
    await authenticatedPage.goto('/hub');
    await expect(authenticatedPage.locator('app-root')).toBeAttached();
  });
});
