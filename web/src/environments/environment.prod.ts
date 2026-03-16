export const environment = {
  production: true,
  bffBaseUrl: "/api/web",
  wsUrl: "/ws",
  oidc: {
    authority: "https://auth.musicratic.com/application/o/musicratic",
    clientId: "musicratic-web",
    callbackUrl: "https://musicratic.com/callback",
    logoutUrl: "https://musicratic.com/login",
    scopes: "openid profile email",
  },
};
