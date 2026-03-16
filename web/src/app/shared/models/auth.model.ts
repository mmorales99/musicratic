export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
}

export interface AuthCallbackResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: AuthUser;
}

export interface AuthRefreshResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface AuthUser {
  id: string;
  displayName: string;
  email: string;
  avatarUrl: string | null;
}

export interface OidcConfig {
  authority: string;
  clientId: string;
  callbackUrl: string;
  logoutUrl: string;
  scopes: string;
}
