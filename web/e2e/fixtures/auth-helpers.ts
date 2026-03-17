import { Page, Route } from '@playwright/test';

/** Roles matching the backend role hierarchy (docs/07-user-roles.md). */
export type TestRole = 'anonymous' | 'visitor' | 'user' | 'list_owner' | 'hub_manager';

interface TestTokenPayload {
  sub: string;
  role: TestRole;
  hub_id?: string;
  exp: number;
  iat: number;
}

const BFF_BASE = 'http://localhost:5010/api/web';
const OIDC_AUTHORITY = 'http://localhost:9000/application/o/musicratic';

/**
 * Creates a fake Base64-encoded token for test purposes.
 * Not a real JWT — just enough structure to satisfy the app's auth flow.
 */
export function createTestToken(userId: string, role: TestRole, hubId?: string): string {
  const header = { alg: 'none', typ: 'JWT' };
  const payload: TestTokenPayload = {
    sub: userId,
    role,
    hub_id: hubId,
    exp: Math.floor(Date.now() / 1000) + 3600,
    iat: Math.floor(Date.now() / 1000),
  };

  const encode = (obj: object): string =>
    Buffer.from(JSON.stringify(obj)).toString('base64url');

  return `${encode(header)}.${encode(payload)}.test-signature`;
}

/**
 * Intercepts Authentik OIDC authorize and token routes, returning mock responses
 * so tests never hit a real identity provider.
 */
export async function mockAuthentikLogin(page: Page, role: TestRole = 'visitor'): Promise<void> {
  const userId = `test-user-${role}`;
  const token = createTestToken(userId, role);

  // Intercept the OIDC authorize redirect — respond with a redirect back to callback
  await page.route(`${OIDC_AUTHORITY}/authorize**`, async (route: Route) => {
    const url = new URL(route.request().url());
    const state = url.searchParams.get('state') ?? 'test-state';
    const callbackUrl = `http://localhost:4200/callback?code=mock-code&state=${state}`;
    await route.fulfill({
      status: 302,
      headers: { Location: callbackUrl },
    });
  });

  // Intercept BFF auth callback — return mock tokens and user info
  await page.route(`${BFF_BASE}/auth/callback`, async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        accessToken: token,
        refreshToken: 'mock-refresh-token',
        expiresIn: 3600,
        user: {
          id: userId,
          displayName: `Test ${capitalize(role)}`,
          email: `${role}@test.musicratic.local`,
          avatarUrl: null,
        },
      }),
    });
  });
}

/**
 * Sets up route intercepts so that any BFF auth-related endpoint returns
 * a valid response for the given role. Covers:
 * - /auth/callback
 * - /auth/refresh
 * - /auth/me
 */
export async function setupAuthIntercept(page: Page, role: TestRole = 'visitor'): Promise<void> {
  const userId = `test-user-${role}`;
  const token = createTestToken(userId, role);

  await page.route(`${BFF_BASE}/auth/callback`, async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        accessToken: token,
        refreshToken: 'mock-refresh-token',
        expiresIn: 3600,
        user: {
          id: userId,
          displayName: `Test ${capitalize(role)}`,
          email: `${role}@test.musicratic.local`,
          avatarUrl: null,
        },
      }),
    });
  });

  await page.route(`${BFF_BASE}/auth/refresh`, async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        accessToken: token,
        expiresIn: 3600,
      }),
    });
  });

  await page.route(`${BFF_BASE}/auth/me`, async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: userId,
        displayName: `Test ${capitalize(role)}`,
        email: `${role}@test.musicratic.local`,
        avatarUrl: null,
        role,
      }),
    });
  });
}

function capitalize(s: string): string {
  return s.charAt(0).toUpperCase() + s.slice(1);
}
