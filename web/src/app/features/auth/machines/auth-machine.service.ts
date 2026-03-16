import { Injectable, inject, signal, computed, OnDestroy } from "@angular/core";
import { createActor, Subscription as XStateSubscription } from "xstate";
import { AuthService } from "@app/shared/services/auth.service";
import { authMachine, AuthMachineContext } from "./auth.machine";
import { AuthUser } from "@app/shared/models/auth.model";

@Injectable({ providedIn: "root" })
export class AuthMachineService implements OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly actor = createActor(authMachine);
  private subscription: XStateSubscription | null = null;

  private readonly stateValue = signal<string>("idle");
  private readonly context = signal<AuthMachineContext>(
    this.actor.getSnapshot().context,
  );

  readonly state = this.stateValue.asReadonly();
  readonly user = computed<AuthUser | null>(() => this.context().user);
  readonly error = computed<string | null>(() => this.context().error);
  readonly isAuthenticated = computed<boolean>(
    () => this.stateValue() === "authenticated",
  );
  readonly isLoading = computed<boolean>(() =>
    ["authenticating", "refreshing", "loggingOut"].includes(this.stateValue()),
  );

  constructor() {
    this.subscription = this.actor.subscribe((snapshot) => {
      this.stateValue.set(snapshot.value as string);
      this.context.set(snapshot.context);
    });
    this.actor.start();
    this.restoreSession();
  }

  login(): void {
    this.actor.send({ type: "LOGIN" });
    this.authService.login();
  }

  async handleCallback(code: string, state: string): Promise<void> {
    this.actor.send({ type: "LOGIN" });
    try {
      await this.authService.handleCallback(code, state);
      const token = this.authService.getAccessToken();
      const user = this.authService.currentUser();
      if (token && user) {
        this.actor.send({
          type: "LOGIN_SUCCESS",
          accessToken: token,
          refreshToken: localStorage.getItem("musicratic_refresh_token") ?? "",
          expiresAt: Number(localStorage.getItem("musicratic_expires_at")),
          user,
        });
      }
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Authentication failed";
      this.actor.send({ type: "LOGIN_FAILURE", error: message });
      throw err;
    }
  }

  async refresh(): Promise<void> {
    this.actor.send({ type: "REFRESH" });
    const success = await this.authService.refreshToken();
    if (success) {
      const token = this.authService.getAccessToken();
      this.actor.send({
        type: "REFRESH_SUCCESS",
        accessToken: token ?? "",
        refreshToken: localStorage.getItem("musicratic_refresh_token") ?? "",
        expiresAt: Number(localStorage.getItem("musicratic_expires_at")),
      });
    } else {
      this.actor.send({
        type: "REFRESH_FAILURE",
        error: "Token refresh failed",
      });
    }
  }

  async logout(): Promise<void> {
    this.actor.send({ type: "LOGOUT" });
    await this.authService.logout();
    this.actor.send({ type: "LOGOUT_COMPLETE" });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    this.actor.stop();
  }

  private restoreSession(): void {
    if (this.authService.isAuthenticated()) {
      const token = this.authService.getAccessToken();
      const user = this.authService.currentUser();
      if (token && user) {
        this.actor.send({
          type: "RESTORE",
          accessToken: token,
          refreshToken: localStorage.getItem("musicratic_refresh_token") ?? "",
          expiresAt: Number(localStorage.getItem("musicratic_expires_at")),
          user,
        });
        this.authService.initialize();
      }
    }
  }
}
