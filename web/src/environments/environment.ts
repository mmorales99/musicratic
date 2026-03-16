export const environment = {
  production: false,
  bffBaseUrl: "http://localhost:5010/api/web",
  wsUrl: "ws://localhost:5010/ws",
  oidc: {
    authority: "http://localhost:9000/application/o/musicratic",
    clientId: "musicratic-web",
    callbackUrl: "http://localhost:4200/callback",
    logoutUrl: "http://localhost:4200/login",
    scopes: "openid profile email",
  },
};
