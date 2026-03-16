import { CanActivateFn, Router } from "@angular/router";
import { inject } from "@angular/core";

const TOKEN_KEY = "musicratic_access_token";

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token = localStorage.getItem(TOKEN_KEY);

  if (token) {
    return true;
  }

  return router.createUrlTree(["/hub"]);
};
