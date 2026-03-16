import { setup, assign } from "xstate";
import { AuthUser } from "@app/shared/models/auth.model";

export interface AuthMachineContext {
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: number | null;
  user: AuthUser | null;
  error: string | null;
}

export type AuthMachineEvent =
  | { type: "LOGIN" }
  | {
      type: "LOGIN_SUCCESS";
      accessToken: string;
      refreshToken: string;
      expiresAt: number;
      user: AuthUser;
    }
  | { type: "LOGIN_FAILURE"; error: string }
  | { type: "REFRESH" }
  | {
      type: "REFRESH_SUCCESS";
      accessToken: string;
      refreshToken: string;
      expiresAt: number;
    }
  | { type: "REFRESH_FAILURE"; error: string }
  | { type: "LOGOUT" }
  | { type: "LOGOUT_COMPLETE" }
  | {
      type: "RESTORE";
      accessToken: string;
      refreshToken: string;
      expiresAt: number;
      user: AuthUser;
    };

export const authMachine = setup({
  types: {
    context: {} as AuthMachineContext,
    events: {} as AuthMachineEvent,
  },
  actions: {
    setAuthData: assign({
      accessToken: ({ event }) => {
        if (event.type === "LOGIN_SUCCESS" || event.type === "RESTORE")
          return event.accessToken;
        return null;
      },
      refreshToken: ({ event }) => {
        if (event.type === "LOGIN_SUCCESS" || event.type === "RESTORE")
          return event.refreshToken;
        return null;
      },
      expiresAt: ({ event }) => {
        if (event.type === "LOGIN_SUCCESS" || event.type === "RESTORE")
          return event.expiresAt;
        return null;
      },
      user: ({ event }) => {
        if (event.type === "LOGIN_SUCCESS" || event.type === "RESTORE")
          return event.user;
        return null;
      },
      error: () => null,
    }),
    setRefreshedTokens: assign({
      accessToken: ({ event }) => {
        if (event.type === "REFRESH_SUCCESS") return event.accessToken;
        return null;
      },
      refreshToken: ({ event }) => {
        if (event.type === "REFRESH_SUCCESS") return event.refreshToken;
        return null;
      },
      expiresAt: ({ event }) => {
        if (event.type === "REFRESH_SUCCESS") return event.expiresAt;
        return null;
      },
      error: () => null,
    }),
    setError: assign({
      error: ({ event }) => {
        if (event.type === "LOGIN_FAILURE" || event.type === "REFRESH_FAILURE")
          return event.error;
        return null;
      },
    }),
    clearAuth: assign({
      accessToken: () => null,
      refreshToken: () => null,
      expiresAt: () => null,
      user: () => null,
      error: () => null,
    }),
  },
}).createMachine({
  id: "auth",
  initial: "idle",
  context: {
    accessToken: null,
    refreshToken: null,
    expiresAt: null,
    user: null,
    error: null,
  },
  states: {
    idle: {
      on: {
        LOGIN: { target: "authenticating" },
        RESTORE: {
          target: "authenticated",
          actions: "setAuthData",
        },
      },
    },
    authenticating: {
      on: {
        LOGIN_SUCCESS: {
          target: "authenticated",
          actions: "setAuthData",
        },
        LOGIN_FAILURE: {
          target: "error",
          actions: "setError",
        },
      },
    },
    authenticated: {
      on: {
        REFRESH: { target: "refreshing" },
        LOGOUT: { target: "loggingOut" },
      },
    },
    refreshing: {
      on: {
        REFRESH_SUCCESS: {
          target: "authenticated",
          actions: "setRefreshedTokens",
        },
        REFRESH_FAILURE: {
          target: "error",
          actions: "setError",
        },
      },
    },
    error: {
      on: {
        LOGIN: { target: "authenticating" },
        LOGOUT: { target: "loggingOut" },
      },
    },
    loggingOut: {
      on: {
        LOGOUT_COMPLETE: {
          target: "idle",
          actions: "clearAuth",
        },
      },
    },
  },
});
