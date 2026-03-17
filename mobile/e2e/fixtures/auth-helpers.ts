import type { Page } from '@playwright/test';

type Role = 'anonymous' | 'visitor' | 'user' | 'list_owner' | 'hub_manager';

const MOCK_TOKENS: Record<Role, { accessToken: string; refreshToken: string }> = {
  anonymous: { accessToken: '', refreshToken: '' },
  visitor: {
    accessToken: 'mock-visitor-access-token',
    refreshToken: 'mock-visitor-refresh-token',
  },
  user: {
    accessToken: 'mock-user-access-token',
    refreshToken: 'mock-user-refresh-token',
  },
  list_owner: {
    accessToken: 'mock-list-owner-access-token',
    refreshToken: 'mock-list-owner-refresh-token',
  },
  hub_manager: {
    accessToken: 'mock-hub-manager-access-token',
    refreshToken: 'mock-hub-manager-refresh-token',
  },
};

/**
 * Intercepts BFF auth endpoints and returns mock tokens for the given role.
 * Must be called BEFORE navigating to the app.
 */
export async function mockBffAuth(page: Page, role: Role): Promise<void> {
  const tokens = MOCK_TOKENS[role];

  // Intercept token exchange endpoint
  await page.route('**/api/auth/token', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        access_token: tokens.accessToken,
        refresh_token: tokens.refreshToken,
        expires_in: 3600,
        token_type: 'Bearer',
      }),
    }),
  );

  // Intercept token refresh endpoint
  await page.route('**/api/auth/refresh', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        access_token: tokens.accessToken,
        refresh_token: tokens.refreshToken,
        expires_in: 3600,
        token_type: 'Bearer',
      }),
    }),
  );

  // Intercept user profile / session endpoint
  await page.route('**/api/auth/me', (route) =>
    route.fulfill({
      status: role === 'anonymous' ? 401 : 200,
      contentType: 'application/json',
      body: JSON.stringify(
        role === 'anonymous'
          ? { success: false, error: 'unauthenticated' }
          : {
              success: true,
              user: {
                id: `mock-${role}-id`,
                display_name: `Test ${role}`,
                role,
              },
            },
      ),
    }),
  );
}

/**
 * Waits until the Flutter web app has fully loaded — splash screen gone,
 * main app element present.
 */
export async function waitForFlutterApp(page: Page): Promise<void> {
  // Wait for flutter-view (CanvasKit) or flt-glass-pane (HTML renderer)
  await page.waitForSelector('flutter-view, flt-glass-pane', {
    state: 'attached',
    timeout: 30_000,
  });

  // Ensure the splash/loading indicator has disappeared
  await page.waitForFunction(() => {
    const loading = document.querySelector('#loading');
    return !loading || loading.getAttribute('style')?.includes('display: none');
  }, { timeout: 20_000 });
}
