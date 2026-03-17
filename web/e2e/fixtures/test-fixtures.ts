import { test as base, expect, Page } from '@playwright/test';
import {
  createTestToken,
  mockAuthentikLogin,
  setupAuthIntercept,
  type TestRole,
} from './auth-helpers';

/** Options accepted by custom fixtures. */
interface TestFixtureOptions {
  /** Role for the authenticated page fixture. Defaults to 'visitor'. */
  role: TestRole;
  /** Hub ID for the hubPage fixture. */
  hubId: string;
}

/** Custom fixtures available in every test. */
interface TestFixtures {
  /** A page with mock auth already set up (localStorage tokens + BFF intercepts). */
  authenticatedPage: Page;
  /** A page authenticated and navigated to a specific hub page. */
  hubPage: Page;
}

/**
 * Extended Playwright `test` with Musicratic-specific fixtures.
 *
 * Usage:
 *   import { test, expect } from '../fixtures/test-fixtures';
 *   test('my test', async ({ authenticatedPage }) => { ... });
 */
export const test = base.extend<TestFixtures & TestFixtureOptions>({
  role: ['visitor', { option: true }],
  hubId: ['test-hub-001', { option: true }],

  authenticatedPage: async ({ page, role }, use) => {
    // Set up route intercepts for auth endpoints
    await setupAuthIntercept(page, role);
    await mockAuthentikLogin(page, role);

    // Seed localStorage with tokens so the app considers the user authenticated
    const token = createTestToken(`test-user-${role}`, role);
    await page.addInitScript(
      ({ accessToken, role: userRole }: { accessToken: string; role: string }) => {
        localStorage.setItem('musicratic_access_token', accessToken);
        localStorage.setItem('musicratic_refresh_token', 'mock-refresh-token');
        localStorage.setItem(
          'musicratic_expires_at',
          String(Date.now() + 3_600_000),
        );
        localStorage.setItem(
          'musicratic_user',
          JSON.stringify({
            id: `test-user-${userRole}`,
            displayName: `Test ${userRole.charAt(0).toUpperCase() + userRole.slice(1)}`,
            email: `${userRole}@test.musicratic.local`,
            avatarUrl: null,
          }),
        );
      },
      { accessToken: token, role },
    );

    await use(page);
  },

  hubPage: async ({ authenticatedPage, hubId }, use) => {
    // Intercept hub detail API so navigation doesn't fail
    await authenticatedPage.route(
      `**/api/web/hubs/${hubId}`,
      async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            success: true,
            total_items_in_response: 1,
            has_more_items: false,
            items: [
              {
                id: hubId,
                name: 'Test Hub',
                description: 'A hub for E2E tests',
                isActive: true,
                memberCount: 1,
              },
            ],
            audit: { timestamp: new Date().toISOString() },
          }),
        });
      },
    );

    await authenticatedPage.goto(`/hub/${hubId}`);
    await use(authenticatedPage);
  },
});

export { expect };
