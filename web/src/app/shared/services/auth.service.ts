import { Injectable, inject, signal, OnDestroy } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { firstValueFrom } from "rxjs";
import { environment } from "@env/environment";
import {
  AuthCallbackResponse,
  AuthRefreshResponse,
  AuthTokens,
  AuthUser,
} from "@app/shared/models/auth.model";

const STORAGE_KEYS = {
  accessToken: "musicratic_access_token",
  refreshToken: "musicratic_refresh_token",
  expiresAt: "musicratic_expires_at",
  user: "musicratic_user",
} as const;

const REFRESH_BUFFER_MS = 60_000;

@Injectable({ providedIn: "root" })
export class AuthService implements OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private refreshTimerId: ReturnType<typeof setTimeout> | null = null;

  readonly authenticated = signal<boolean>(this.hasValidToken());
  readonly currentUser = signal<AuthUser | null>(this.loadStoredUser());

  login(): void {
    const { oidc } = environment;
    const state = this.generateState();
    sessionStorage.setItem("musicratic_oauth_state", state);

    const params = new URLSearchParams({
      response_type: "code",
      client_id: oidc.clientId,
      redirect_uri: oidc.callbackUrl,
      scope: oidc.scopes,
      state,
    });

    window.location.href = `${oidc.authority}/authorize?${params.toString()}`;
  }

  async handleCallback(code: string, state: string): Promise<void> {
    const savedState = sessionStorage.getItem("musicratic_oauth_state");
    sessionStorage.removeItem("musicratic_oauth_state");

    if (!savedState || savedState !== state) {
      throw new Error("Invalid OAuth state parameter");
    }

    const response = await firstValueFrom(
      this.http.post<AuthCallbackResponse>(
        `${environment.bffBaseUrl}/auth/callback`,
        { code, redirectUri: environment.oidc.callbackUrl },
      ),
    );

    this.storeTokens({
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiresAt: Date.now() + response.expiresIn * 1000,
    });
    this.storeUser(response.user);
    this.authenticated.set(true);
    this.currentUser.set(response.user);
    this.scheduleRefresh(response.expiresIn * 1000);
  }

  async refreshToken(): Promise<boolean> {
    const storedRefreshToken = localStorage.getItem(STORAGE_KEYS.refreshToken);
    if (!storedRefreshToken) {
      this.clearState();
      return false;
    }

    try {
      const response = await firstValueFrom(
        this.http.post<AuthRefreshResponse>(
          `${environment.bffBaseUrl}/auth/refresh`,
          { refreshToken: storedRefreshToken },
        ),
      );

      this.storeTokens({
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        expiresAt: Date.now() + response.expiresIn * 1000,
      });
      this.authenticated.set(true);
      this.scheduleRefresh(response.expiresIn * 1000);
      return true;
    } catch {
      this.clearState();
      return false;
    }
  }

  async logout(): Promise<void> {
    try {
      await firstValueFrom(
        this.http.post(`${environment.bffBaseUrl}/auth/logout`, {}),
      );
    } finally {
      this.clearState();
      this.router.navigate(["/login"]);
    }
  }

  getAccessToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.accessToken);
  }

  isAuthenticated(): boolean {
    return this.authenticated();
  }

  initialize(): void {
    if (this.hasValidToken()) {
      const expiresAt = Number(localStorage.getItem(STORAGE_KEYS.expiresAt));
      const remaining = expiresAt - Date.now();
      this.scheduleRefresh(remaining);
    }
  }

  ngOnDestroy(): void {
    this.cancelRefreshTimer();
  }

  private storeTokens(tokens: AuthTokens): void {
    localStorage.setItem(STORAGE_KEYS.accessToken, tokens.accessToken);
    localStorage.setItem(STORAGE_KEYS.refreshToken, tokens.refreshToken);
    localStorage.setItem(STORAGE_KEYS.expiresAt, String(tokens.expiresAt));
  }

  private storeUser(user: AuthUser): void {
    localStorage.setItem(STORAGE_KEYS.user, JSON.stringify(user));
  }

  private loadStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(STORAGE_KEYS.user);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as AuthUser;
    } catch {
      return null;
    }
  }

  private hasValidToken(): boolean {
    const token = localStorage.getItem(STORAGE_KEYS.accessToken);
    const expiresAt = Number(localStorage.getItem(STORAGE_KEYS.expiresAt));
    return !!token && expiresAt > Date.now();
  }

  private clearState(): void {
    this.cancelRefreshTimer();
    localStorage.removeItem(STORAGE_KEYS.accessToken);
    localStorage.removeItem(STORAGE_KEYS.refreshToken);
    localStorage.removeItem(STORAGE_KEYS.expiresAt);
    localStorage.removeItem(STORAGE_KEYS.user);
    this.authenticated.set(false);
    this.currentUser.set(null);
  }

  private scheduleRefresh(expiresInMs: number): void {
    this.cancelRefreshTimer();
    const delay = Math.max(expiresInMs - REFRESH_BUFFER_MS, 0);
    this.refreshTimerId = setTimeout(() => {
      void this.refreshToken();
    }, delay);
  }

  private cancelRefreshTimer(): void {
    if (this.refreshTimerId !== null) {
      clearTimeout(this.refreshTimerId);
      this.refreshTimerId = null;
    }
  }

  private generateState(): string {
    const array = new Uint8Array(32);
    crypto.getRandomValues(array);
    return Array.from(array, (b) => b.toString(16).padStart(2, "0")).join("");
  }
}
