import { setup, assign } from "xstate";
import { User } from "@app/shared/models/user.model";

export interface AuthMachineContext {
  user: User | null;
  token: string | null;
  error: string | null;
}

export type AuthMachineEvent =
  | { type: "LOGIN" }
  | { type: "LOGIN_SUCCESS"; user: User; token: string }
  | { type: "LOGIN_FAILED"; error: string }
  | { type: "LOGOUT" }
  | { type: "TOKEN_REFRESHED"; token: string }
  | { type: "TOKEN_EXPIRED" }
  | { type: "RETRY" };

export const authMachine = setup({
  types: {
    context: {} as AuthMachineContext,
    events: {} as AuthMachineEvent,
  },
}).createMachine({
  id: "auth",
  initial: "unauthenticated",
  context: {
    user: null,
    token: null,
    error: null,
  },
  states: {
    unauthenticated: {
      on: {
        LOGIN: { target: "authenticating" },
      },
    },
    authenticating: {
      on: {
        LOGIN_SUCCESS: {
          target: "authenticated",
          actions: assign({
            user: ({ event }) => event.user,
            token: ({ event }) => event.token,
            error: () => null,
          }),
        },
        LOGIN_FAILED: {
          target: "error",
          actions: assign({
            error: ({ event }) => event.error,
          }),
        },
      },
    },
    authenticated: {
      on: {
        LOGOUT: {
          target: "unauthenticated",
          actions: assign({
            user: () => null,
            token: () => null,
          }),
        },
        TOKEN_REFRESHED: {
          actions: assign({
            token: ({ event }) => event.token,
          }),
        },
        TOKEN_EXPIRED: { target: "unauthenticated" },
      },
    },
    error: {
      on: {
        RETRY: { target: "authenticating" },
        LOGIN: { target: "authenticating" },
      },
    },
  },
});
