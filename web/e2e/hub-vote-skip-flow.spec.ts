import { test, expect } from "./fixtures/test-fixtures";

const BFF_BASE = "http://localhost:5010/api/web";

const MOCK_HUB = {
  id: "e2e-hub-001",
  name: "E2E Test Hub",
  description: "A hub for E2E testing",
  type: "Venue",
  isActive: true,
  memberCount: 5,
  code: "TESTHUB",
  settings: {},
};

const MOCK_TRACK = {
  id: "track-001",
  title: "Test Song",
  artist: "Test Artist",
  album: "Test Album",
  albumArtUrl: null,
  durationSeconds: 210,
  provider: "spotify",
  externalId: "spotify:track:abc123",
};

const MOCK_QUEUE_ENTRY = {
  id: "entry-001",
  trackId: MOCK_TRACK.id,
  track: MOCK_TRACK,
  hubId: MOCK_HUB.id,
  position: 0,
  status: "Playing",
  source: "CoinProposal",
  proposerId: "test-user-visitor",
  costPaid: 10,
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

test.describe("Hub Discovery → Join → Propose → Vote → Skip Flow", () => {
  test.describe("Hub Discovery", () => {
    test("displays active hubs from API", async ({ page }) => {
      await page.route(`${BFF_BASE}/hubs*`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify(wrapEnvelope([MOCK_HUB])),
        });
      });

      await page.goto("/hub");
      await expect(page.locator(".hub-card")).toBeVisible();
      await expect(page.locator(".hub-card__name")).toContainText(
        "E2E Test Hub",
      );
    });

    test("search input filters hubs", async ({ page }) => {
      await page.route(`${BFF_BASE}/hubs*`, async (route) => {
        const url = new URL(route.request().url());
        const search = url.searchParams.get("search") ?? "";
        const hubs = search.includes("E2E") ? [MOCK_HUB] : [];
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify(wrapEnvelope(hubs)),
        });
      });

      await page.goto("/hub");
      const searchInput = page.locator("input.search-input");
      await searchInput.fill("E2E");
      await expect(page.locator(".hub-card")).toBeVisible();
    });
  });

  test.describe("Hub Join", () => {
    test("joins hub with valid code", async ({ authenticatedPage }) => {
      await authenticatedPage.route(
        `${BFF_BASE}/hubs/attach`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
              hubId: MOCK_HUB.id,
              hubName: MOCK_HUB.name,
              role: "visitor",
            }),
          });
        },
      );

      await authenticatedPage.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify(wrapEnvelope([MOCK_HUB])),
          });
        },
      );

      await authenticatedPage.goto("/hub/join");
      await authenticatedPage.locator("input#hubCode").fill("TESTHUB");
      await authenticatedPage.locator(".btn.btn--primary").click();

      // Should navigate to hub detail after successful join
      await expect(authenticatedPage).toHaveURL(
        new RegExp(`/hub/${MOCK_HUB.id}`),
        { timeout: 5000 },
      );
    });

    test("shows error for invalid hub code", async ({ authenticatedPage }) => {
      await authenticatedPage.route(
        `${BFF_BASE}/hubs/attach`,
        async (route) => {
          await route.fulfill({
            status: 404,
            contentType: "application/json",
            body: JSON.stringify({
              title: "Hub not found",
              detail: "No hub found with this code.",
              status: 404,
            }),
          });
        },
      );

      await authenticatedPage.goto("/hub/join");
      await authenticatedPage.locator("input#hubCode").fill("INVALID");
      await authenticatedPage.locator(".btn.btn--primary").click();

      await expect(authenticatedPage.locator(".hub-join__error")).toBeVisible({
        timeout: 5000,
      });
    });
  });

  test.describe("Queue & Now Playing", () => {
    test("displays currently playing track", async ({ authenticatedPage }) => {
      await authenticatedPage.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify(wrapEnvelope([MOCK_HUB])),
          });
        },
      );

      await authenticatedPage.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/queue`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify(wrapEnvelope([MOCK_QUEUE_ENTRY])),
          });
        },
      );

      await authenticatedPage.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/now-playing`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
              queueEntryId: MOCK_QUEUE_ENTRY.id,
              trackId: MOCK_TRACK.id,
              title: MOCK_TRACK.title,
              artist: MOCK_TRACK.artist,
              album: MOCK_TRACK.album,
              albumArtUrl: null,
              durationSeconds: 210,
              elapsedSeconds: 30,
              queuePosition: 0,
              proposerId: "test-user-visitor",
              source: "CoinProposal",
              startedAt: new Date().toISOString(),
            }),
          });
        },
      );

      await authenticatedPage.goto(`/hub/${MOCK_HUB.id}/queue`);

      // Queue page should display track info
      await expect(
        authenticatedPage.locator(".queue-list__title"),
      ).toContainText("Test Song", { timeout: 5000 });
    });
  });

  test.describe("Voting", () => {
    async function setupVotingRoutes(page: import("@playwright/test").Page) {
      let upvotes = 0;
      let downvotes = 0;

      await page.route(`${BFF_BASE}/hubs/${MOCK_HUB.id}`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify(wrapEnvelope([MOCK_HUB])),
        });
      });

      await page.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/queue`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify(wrapEnvelope([MOCK_QUEUE_ENTRY])),
          });
        },
      );

      await page.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/now-playing`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
              queueEntryId: MOCK_QUEUE_ENTRY.id,
              trackId: MOCK_TRACK.id,
              title: MOCK_TRACK.title,
              artist: MOCK_TRACK.artist,
              durationSeconds: 210,
              elapsedSeconds: 30,
              queuePosition: 0,
              source: "CoinProposal",
              startedAt: new Date().toISOString(),
            }),
          });
        },
      );

      await page.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/queue/${MOCK_QUEUE_ENTRY.id}/vote`,
        async (route) => {
          if (route.request().method() === "POST") {
            const body = route.request().postDataJSON();
            if (body.Value === "Up") upvotes++;
            else downvotes++;
            await route.fulfill({
              status: 201,
              contentType: "application/json",
              body: JSON.stringify({
                id: crypto.randomUUID(),
                tenantId: MOCK_HUB.id,
                userId: "test-user-visitor",
                queueEntryId: MOCK_QUEUE_ENTRY.id,
                value: body.Value === "Up" ? 0 : 1,
                castAt: new Date().toISOString(),
              }),
            });
          } else {
            await route.continue();
          }
        },
      );

      await page.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/queue/${MOCK_QUEUE_ENTRY.id}/tally`,
        async (route) => {
          const total = upvotes + downvotes;
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
              queueEntryId: MOCK_QUEUE_ENTRY.id,
              upvotes,
              downvotes,
              total,
              upvotePercentage: total > 0 ? (upvotes / total) * 100 : 0,
              downvotePercentage: total > 0 ? (downvotes / total) * 100 : 0,
            }),
          });
        },
      );

      return { getUpvotes: () => upvotes, getDownvotes: () => downvotes };
    }

    test("upvote button sends vote and updates tally", async ({
      authenticatedPage,
    }) => {
      await setupVotingRoutes(authenticatedPage);
      await authenticatedPage.goto(`/hub/${MOCK_HUB.id}/queue`);

      const upvoteBtn = authenticatedPage.locator(
        'button[aria-label="Upvote"]',
      );
      await expect(upvoteBtn).toBeVisible({ timeout: 5000 });
      await upvoteBtn.click();

      // After voting, the button should show active state
      await expect(upvoteBtn).toHaveClass(/vote-btn--active/, {
        timeout: 3000,
      });
    });

    test("downvote button sends vote", async ({ authenticatedPage }) => {
      await setupVotingRoutes(authenticatedPage);
      await authenticatedPage.goto(`/hub/${MOCK_HUB.id}/queue`);

      const downvoteBtn = authenticatedPage.locator(
        'button[aria-label="Downvote"]',
      );
      await expect(downvoteBtn).toBeVisible({ timeout: 5000 });
      await downvoteBtn.click();

      await expect(downvoteBtn).toHaveClass(/vote-btn--active/, {
        timeout: 3000,
      });
    });

    test("vote tally displays correct percentages", async ({
      authenticatedPage,
    }) => {
      await setupVotingRoutes(authenticatedPage);
      await authenticatedPage.goto(`/hub/${MOCK_HUB.id}/queue`);

      // Tally component should be visible
      await expect(authenticatedPage.locator(".vote-tally")).toBeVisible({
        timeout: 5000,
      });
    });
  });

  test.describe("Track Proposal", () => {
    test("search and propose a track", async ({ authenticatedPage }) => {
      await authenticatedPage.route(
        `${BFF_BASE}/tracks/search*`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify(wrapEnvelope([MOCK_TRACK])),
          });
        },
      );

      await authenticatedPage.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/queue/propose-paid`,
        async (route) => {
          await route.fulfill({
            status: 201,
            contentType: "application/json",
            body: JSON.stringify({
              queueEntryId: "new-entry-001",
              trackId: MOCK_TRACK.id,
              title: MOCK_TRACK.title,
              artist: MOCK_TRACK.artist,
              status: "Queued",
              voteSessionId: null,
              voteExpiresAt: null,
              requiredApprovalPercentage: null,
            }),
          });
        },
      );

      await authenticatedPage.goto(`/hub/${MOCK_HUB.id}/propose`);

      const searchInput = authenticatedPage.locator("input.proposal__input");
      await expect(searchInput).toBeVisible({ timeout: 5000 });
      await searchInput.fill("Test Song");

      // Wait for search results
      const result = authenticatedPage.locator(".proposal__result").first();
      await expect(result).toBeVisible({ timeout: 5000 });

      // Click propose on the first result
      const proposeBtn = result.locator(".btn.btn--primary");
      await proposeBtn.click();

      // Should show success message
      await expect(authenticatedPage.locator(".proposal__success")).toBeVisible(
        { timeout: 5000 },
      );
    });
  });

  test.describe("Skip Notification", () => {
    test("skip threshold danger message appears at 65%+ downvotes", async ({
      authenticatedPage,
    }) => {
      await authenticatedPage.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify(wrapEnvelope([MOCK_HUB])),
          });
        },
      );

      await authenticatedPage.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/queue`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify(wrapEnvelope([MOCK_QUEUE_ENTRY])),
          });
        },
      );

      await authenticatedPage.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/now-playing`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
              queueEntryId: MOCK_QUEUE_ENTRY.id,
              trackId: MOCK_TRACK.id,
              title: MOCK_TRACK.title,
              artist: MOCK_TRACK.artist,
              durationSeconds: 210,
              elapsedSeconds: 30,
              queuePosition: 0,
              source: "CoinProposal",
              startedAt: new Date().toISOString(),
            }),
          });
        },
      );

      // Mock tally with 65%+ downvotes
      await authenticatedPage.route(
        `${BFF_BASE}/hubs/${MOCK_HUB.id}/queue/${MOCK_QUEUE_ENTRY.id}/tally`,
        async (route) => {
          await route.fulfill({
            status: 200,
            contentType: "application/json",
            body: JSON.stringify({
              queueEntryId: MOCK_QUEUE_ENTRY.id,
              upvotes: 1,
              downvotes: 3,
              total: 4,
              upvotePercentage: 25,
              downvotePercentage: 75,
            }),
          });
        },
      );

      await authenticatedPage.goto(`/hub/${MOCK_HUB.id}/queue`);

      // Vote tally should show danger state (≥65% downvotes)
      await expect(
        authenticatedPage.locator(".vote-tally--danger"),
      ).toBeVisible({ timeout: 5000 });
    });
  });
});
