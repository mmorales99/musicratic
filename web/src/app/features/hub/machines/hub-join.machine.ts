import { setup, assign } from "xstate";
import { HubAttachment } from "@app/shared/models/hub.model";

export interface HubJoinContext {
  code: string;
  attachment: HubAttachment | null;
  error: string | null;
}

export type HubJoinEvent =
  | { type: "SET_CODE"; code: string }
  | { type: "SUBMIT" }
  | { type: "ATTACHED"; attachment: HubAttachment }
  | { type: "ATTACH_FAILED"; error: string }
  | { type: "RETRY" }
  | { type: "RESET" };

export const hubJoinMachine = setup({
  types: {
    context: {} as HubJoinContext,
    events: {} as HubJoinEvent,
  },
  actions: {
    setCode: assign({
      code: ({ event }) => (event.type === "SET_CODE" ? event.code : ""),
    }),
    setAttached: assign({
      attachment: ({ event }) =>
        event.type === "ATTACHED" ? event.attachment : null,
      error: () => null,
    }),
    setError: assign({
      error: ({ event }) =>
        event.type === "ATTACH_FAILED" ? event.error : null,
    }),
    clearError: assign({ error: () => null }),
  },
  guards: {
    hasCode: ({ context }) => context.code.trim().length > 0,
  },
}).createMachine({
  id: "hubJoin",
  initial: "idle",
  context: {
    code: "",
    attachment: null,
    error: null,
  },
  states: {
    idle: {
      on: {
        SET_CODE: { actions: ["setCode", "clearError"] },
        SUBMIT: { target: "attaching", guard: "hasCode" },
      },
    },
    attaching: {
      on: {
        ATTACHED: { target: "attached", actions: "setAttached" },
        ATTACH_FAILED: { target: "error", actions: "setError" },
      },
    },
    attached: {
      type: "final",
    },
    error: {
      on: {
        SET_CODE: { target: "idle", actions: ["setCode", "clearError"] },
        RETRY: { target: "attaching" },
        RESET: {
          target: "idle",
          actions: assign({ code: () => "", error: () => null }),
        },
      },
    },
  },
});
