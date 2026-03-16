import { Routes } from "@angular/router";
import { LoginComponent } from "./login/login.component";
import { CallbackComponent } from "./callback/callback.component";

export const AUTH_ROUTES: Routes = [
  { path: "login", component: LoginComponent },
  { path: "callback", component: CallbackComponent },
];
