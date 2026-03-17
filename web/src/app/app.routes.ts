import { Routes } from "@angular/router";
import { authGuard } from "./shared/guards/auth.guard";

export const routes: Routes = [
  { path: "", redirectTo: "hub", pathMatch: "full" },
  {
    path: "join/:code",
    loadComponent: () =>
      import("./features/hub/join/hub-join.component").then(
        (m) => m.HubJoinComponent,
      ),
  },
  {
    path: "login",
    loadComponent: () =>
      import("./features/auth/login/login.component").then(
        (m) => m.LoginComponent,
      ),
  },
  {
    path: "callback",
    loadComponent: () =>
      import("./features/auth/callback/callback.component").then(
        (m) => m.CallbackComponent,
      ),
  },
  {
    path: "hub",
    loadChildren: () =>
      import("./features/hub/hub.routes").then((m) => m.HUB_ROUTES),
  },
  {
    path: "hub/:hubId/queue",
    loadComponent: () =>
      import("./features/playback/queue/queue.component").then(
        (m) => m.QueueComponent,
      ),
  },
  {
    path: "hub/:hubId/search",
    loadComponent: () =>
      import("./features/playback/search/track-search.component").then(
        (m) => m.TrackSearchComponent,
      ),
  },
  {
    path: "hub/:hubId/propose",
    loadComponent: () =>
      import(
        "./features/playback/components/track-proposal/track-proposal.component"
      ).then((m) => m.TrackProposalComponent),
    canActivate: [authGuard],
  },
  {
    path: "playback",
    loadChildren: () =>
      import("./features/playback/playback.routes").then(
        (m) => m.PLAYBACK_ROUTES,
      ),
    canActivate: [authGuard],
  },
  {
    path: "voting",
    loadChildren: () =>
      import("./features/voting/voting.routes").then((m) => m.VOTING_ROUTES),
    canActivate: [authGuard],
  },
  {
    path: "economy",
    loadChildren: () =>
      import("./features/economy/economy.routes").then((m) => m.ECONOMY_ROUTES),
    canActivate: [authGuard],
  },
  {
    path: "wallet",
    loadChildren: () =>
      import("./features/economy/economy.routes").then((m) => m.ECONOMY_ROUTES),
    canActivate: [authGuard],
  },
  {
    path: "hub/:hubId/subscription",
    loadComponent: () =>
      import(
        "./features/economy/subscription/subscription.component"
      ).then((m) => m.SubscriptionComponent),
    canActivate: [authGuard],
  },
  {
    path: "profile",
    loadChildren: () =>
      import("./features/profile/profile.routes").then((m) => m.PROFILE_ROUTES),
    canActivate: [authGuard],
  },
  {
    path: "analytics",
    loadChildren: () =>
      import("./features/analytics/analytics.routes").then(
        (m) => m.ANALYTICS_ROUTES,
      ),
    canActivate: [authGuard],
  },
  { path: "**", redirectTo: "hub" },
];
