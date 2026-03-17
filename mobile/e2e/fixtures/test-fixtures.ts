import { test as base, expect, type Page } from '@playwright/test';
import { mockBffAuth, waitForFlutterApp } from './auth-helpers';

type Role = 'anonymous' | 'visitor' | 'user' | 'list_owner' | 'hub_manager';

interface FlutterFixtures {
  authenticatedPage: Page;
}

/**
 * Waits for the Flutter web app to be fully rendered and interactive.
 */
export async function flutterReady(page: Page): Promise<void> {
  // Wait for Flutter engine to initialize — flutter-view is the
  // container rendered by the CanvasKit renderer.
  await page.waitForSelector('flutter-view, flt-glass-pane', {
    state: 'attached',
    timeout: 30_000,
  });

  // Give the framework a moment to finish first frame rendering.
  await page.waitForTimeout(1_000);
}

export const test = base.extend<FlutterFixtures>({
  authenticatedPage: async ({ page }, use) => {
    await mockBffAuth(page, 'user');
    await page.goto('/');
    await flutterReady(page);
    await use(page);
  },
});

export { expect };
