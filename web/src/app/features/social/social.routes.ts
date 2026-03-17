import { Routes } from "@angular/router";
import { authGuard } from "@app/shared/guards/auth.guard";

export const SOCIAL_ROUTES: Routes = [
  {
    path: "profile/me",
    loadComponent: () =>
      import(
        "./components/user-profile/user-profile.component"
      ).then((m) => m.UserProfileComponent),
    canActivate: [authGuard],
  },
  {
    path: "profile/:userId",
    loadComponent: () =>
      import(
        "./components/user-profile/user-profile.component"
      ).then((m) => m.UserProfileComponent),
  },
  {
    path: "hub/:hubId/reviews",
    loadComponent: () =>
      import(
        "./components/hub-reviews-page/hub-reviews-page.component"
      ).then((m) => m.HubReviewsPageComponent),
  },
];
