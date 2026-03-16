import { Routes } from "@angular/router";
import { authGuard } from "./shared/guards/auth.guard";

export const routes: Routes = [
  { path: "", redirectTo: "hub", pathMatch: "full" },
  {
    path: "hub",
    loadChildren: () =>
      import("./features/hub/hub.routes").then((m) => m.HUB_ROUTES),
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
];
