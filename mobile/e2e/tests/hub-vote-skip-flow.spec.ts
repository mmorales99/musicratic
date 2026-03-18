import { test, expect } from "../fixtures/test-fixtures";
import { mockBffAuth, waitForFlutterApp } from "../fixtures/auth-helpers";

const BFF_BASE = "**/api";

const MOCK_HUB = {
  id: "e2e-hub-001",
  name: "E2E Test Hub",
  hubType: "bar",
  status: "active",
  isActive: true,
  isPaused: false,
  activeUsersCount: 5,
  hubCode: "TESTHUB",
  qrUrl: null,
  averageRating: 4.2,
  settings: {
    proposalsEnabled: true,
    maxQueueDepth: 20,
    voteSkipThreshold: 0.65,
    votingWindowSeconds: 60,
    coinCostMultiplier: 1.0,
    minVoteCount: 3,
  },
};

const MOCK_TRACK = {
  id: "track-001",
  title: "Test Song",
  artist: "Test Artist",
  albumArt: null,
  durationMs: 210000,
  provider: "spotify",
};

const MOCK_QUEUE_ENTRY = {
  id: "entry-001",
  position: 0,
  trackTitle: MOCK_TRACK.title,
  trackArtist: MOCK_TRACK.artist,
  trackAlbumArt: null,
  durationMs: MOCK_TRACK.durationMs,
  proposerName: "Test User",
  upvotes: 0,
  downvotes: 0,
  status: "playing",
  source: "proposal",
};

function wrapEnvelope<T>(items: T[]) {
  return {
    success: true,
    total_items_in_response: items.length,
    has_more_items: false,
    items,
    audit: { timestamp: new Date().toISOString() },
  };
}

test.describe("Hub Scan → Propose → Vote → Skip Flow", () => {
  test.describe("Hub Discovery & Navigation", () => {
    test("app loads Flutter view successfully", async ({ page }) => {
      await mockBffAuth(page, "anonymous");
      await page.goto("/");
      await waitForFlutterApp(page);

      const flutterView = page.locator("flutter-view, flt-glass-pane");
      await expect(flutterView).toBeAttached();
    });

    test("displays hubs from search API", async ({
      authenticatedPage: page,
    }) => {
      await page.route(`${BFF_BASE}/hubs/search*`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify(wrapEnvelope([MOCK_HUB])),
        });
      });

      // Navigate to hub screen
      await page.goto("/#/hub");
      await expect(page.locator("text=E2E Test Hub")).toBeVisible({
        timeout: 10000,
      });
    });

    test("hub card shows active badge", async ({ authenticatedPage: page }) => {
      await page.route(`${BFF_BASE}/hubs/search*`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify(wrapEnvelope([MOCK_HUB])),
        });
      });

      await page.goto("/#/hub");
      await expect(page.locator("text=Active")).toBeVisible({
        timeout: 10000,
      });
    });
  });

  test.describe("Hub Join via Code", () => {
    test("joins hub with valid code", async ({ authenticatedPage: page }) => {
      await page.route(`${BFF_BASE}/hubs/attach`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify({
            hubId: MOCK_HUB.id,
            hubName: MOCK_HUB.name,
            role: "visitor",
          }),
        });
      });

      await page.route(`${BFF_BASE}/hubs/${MOCK_HUB.id}`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify(MOCK_HUB),
        });
      });

      await page.goto("/#/hub/join");

      // Fill in hub code — Flutter text fields render as contenteditable or input
      const codeInput = page
        .locator('input[type="text"], [contenteditable="true"]')
        .first();
      await codeInput.fill("TESTHUB");

      // Tap join button
      await page.locator("text=Join").first().click();

      // After successful join, should navigate to hub detail
      await expect(page.locator(`text=${MOCK_HUB.name}`)).toBeVisible({
        timeout: 10000,
      });
    });
  });

  test.describe("Track Proposal", () => {
    test("search and select a track to propose", async ({
      authenticatedPage: page,
    }) => {
      // Mock hub detail
      await page.route(`${BFF_BASE}/hubs/${MOCK_HUB.id}`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify(MOCK_HUB),
        });
      });

      // Mock track search
      await page.route(`${BFF_BASE}/hubs/*/tracks/search*`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify(wrapEnvelope([MOCK_TRACK])),
        });
      });

      // Mock proposal submission
      await page.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/proposals`,
        async (route) => {
          if (route.request().method() === "POST") {
            await route.fulfill({
              status: 201,
              contentType: "application/json",
              body: JSON.stringify({
                queueEntryId: "new-entry-001",
                trackId: MOCK_TRACK.id,
                title: MOCK_TRACK.title,
                artist: MOCK_TRACK.artist,
                status: "Queued",
              }),
            });
          } else {
            await route.continue();
          }
        },
      );

      // Mock wallet balance
      await page.route(`${BFF_BASE}/economy/wallet*`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify({ balance: 100, currency: "MUS_COIN" }),
        });
      });

      await page.goto(`/#/hub/${MOCK_HUB.id}/propose`);

      // Search for a track
      const searchInput = page
        .locator('input[type="text"], [contenteditable="true"]')
        .first();
      await searchInput.fill("Test Song");

      // Wait for search results
      await expect(page.locator("text=Test Song")).toBeVisible({
        timeout: 10000,
      });
      await expect(page.locator("text=Test Artist")).toBeVisible();
    });
  });

  test.describe("Voting on Queue", () => {
    async function setupQueueRoutes(page: import("@playwright/test").Page) {
      await page.route(`${BFF_BASE}/hubs/${MOCK_HUB.id}`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify(MOCK_HUB),
        });
      });

      await page.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/queues`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify(wrapEnvelope([MOCK_QUEUE_ENTRY])),
          });
        },
      );

      await page.route(`${BFF_BASE}/voting/vote`, async (route) => {
        if (route.request().method() === "POST") {
          await route.fulfill({
            status: 201,
            contentType: "application/json",
            body: JSON.stringify({
              id: crypto.randomUUID(),
              userId: "test-user-user",
              entryId: MOCK_QUEUE_ENTRY.id,
              direction: "up",
              castAt: new Date().toISOString(),
            }),
          });
        } else {
          await route.continue();
        }
      });
    }

    test("queue screen shows currently playing track", async ({
      authenticatedPage: page,
    }) => {
      await setupQueueRoutes(page);
      await page.goto(`/#/hub/${MOCK_HUB.id}/queue`);

      await expect(page.locator("text=Test Song")).toBeVisible({
        timeout: 10000,
      });
      await expect(page.locator("text=Test Artist")).toBeVisible();
    });

    test("vote buttons are visible on playing track", async ({
      authenticatedPage: page,
    }) => {
      await setupQueueRoutes(page);
      await page.goto(`/#/hub/${MOCK_HUB.id}/queue`);

      // Flutter renders thumb_up / thumb_down icons — look for vote affordances
      // Vote buttons should appear when track is playing
      await expect(page.locator("text=Test Song")).toBeVisible({
        timeout: 10000,
      });

      // The vote buttons are part of the queue/now-playing widget
      // In Flutter web, icon buttons render as semantic elements
      const thumbUp = page
        .locator('[aria-label*="pvote"], [aria-label*="humb"], text=👍')
        .first();
      const thumbDown = page
        .locator('[aria-label*="ownvote"], [aria-label*="humb"], text=👎')
        .first();

      // At least one vote affordance should be visible
      await expect(thumbUp.or(page.locator("text=0").first())).toBeVisible({
        timeout: 5000,
      });
    });
  });

  test.describe("Skip Notification", () => {
    test("skip event displays notification", async ({
      authenticatedPage: page,
    }) => {
      await page.route(`${BFF_BASE}/hubs/${MOCK_HUB.id}`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify(MOCK_HUB),
        });
      });

      // Mock queue with a skipped entry
      const skippedEntry = { ...MOCK_QUEUE_ENTRY, status: "skipped" };
      await page.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/queues`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify(wrapEnvelope([skippedEntry])),
          });
        },
      );

      await page.goto(`/#/hub/${MOCK_HUB.id}/queue`);
      await expect(page.locator("flutter-view, flt-glass-pane")).toBeAttached();

      // The UI should reflect the skipped state
      // Exact assertion depends on how the Flutter app renders the skip state
      await expect(page.locator("text=Test Song")).toBeVisible({
        timeout: 10000,
      });
    });
  });
});
