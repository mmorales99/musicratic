import { setup, assign } from "xstate";
import { Hub, HubSettings, HubList } from "@app/shared/models/hub.model";

export interface HubDetailContext {
  hub: Hub | null;
  settings: HubSettings | null;
  lists: HubList[];
  error: string | null;
}

export type HubDetailEvent =
  | { type: "LOAD"; hubId: string }
  | { type: "LOADED"; hub: Hub; settings: HubSettings; lists: HubList[] }
  | { type: "LOAD_FAILED"; error: string }
  | { type: "EDIT" }
  | { type: "CANCEL_EDIT" }
  | { type: "SAVE"; hub: Partial<Hub> }
  | { type: "SAVED"; hub: Hub }
  | { type: "SAVE_FAILED"; error: string }
  | { type: "SAVE_SETTINGS"; settings: Partial<HubSettings> }
  | { type: "SETTINGS_SAVED"; settings: HubSettings }
  | {
      type: "ACTION";
      action: "activate" | "deactivate" | "pause" | "resume" | "delete";
    }
  | { type: "ACTION_DONE"; hub: Hub }
  | { type: "DELETED" }
  | { type: "ACTION_FAILED"; error: string }
  | { type: "RETRY" };

export const hubDetailMachine = setup({
  types: {
    context: {} as HubDetailContext,
    events: {} as HubDetailEvent,
  },
  actions: {
    setLoaded: assign({
      hub: ({ event }) => (event.type === "LOADED" ? event.hub : null),
      settings: ({ event }) =>
        event.type === "LOADED" ? event.settings : null,
      lists: ({ event }) => (event.type === "LOADED" ? event.lists : []),
      error: () => null,
    }),
    setHub: assign({
      hub: ({ event }) =>
        event.type === "SAVED" || event.type === "ACTION_DONE"
          ? event.hub
          : null,
      error: () => null,
    }),
    setSettings: assign({
      settings: ({ event }) =>
        event.type === "SETTINGS_SAVED" ? event.settings : null,
      error: () => null,
    }),
    setError: assign({
      error: ({ event }) => {
        if (
          event.type === "LOAD_FAILED" ||
          event.type === "SAVE_FAILED" ||
          event.type === "ACTION_FAILED"
        ) {
          return event.error;
        }
        return null;
      },
    }),
  },
}).createMachine({
  id: "hubDetail",
  initial: "idle",
  context: {
    hub: null,
    settings: null,
    lists: [],
    error: null,
  },
  states: {
    idle: {
      on: { LOAD: { target: "loading" } },
    },
    loading: {
      on: {
        LOADED: { target: "loaded", actions: "setLoaded" },
        LOAD_FAILED: { target: "error", actions: "setError" },
      },
    },
    loaded: {
      on: {
        LOAD: { target: "loading" },
        EDIT: { target: "editing" },
        SAVE_SETTINGS: { target: "savingSettings" },
        ACTION: { target: "executingAction" },
      },
    },
    editing: {
      on: {
        CANCEL_EDIT: { target: "loaded" },
        SAVE: { target: "saving" },
      },
    },
    saving: {
      on: {
        SAVED: { target: "loaded", actions: "setHub" },
        SAVE_FAILED: { target: "editing", actions: "setError" },
      },
    },
    savingSettings: {
      on: {
        SETTINGS_SAVED: { target: "loaded", actions: "setSettings" },
        SAVE_FAILED: { target: "loaded", actions: "setError" },
      },
    },
    executingAction: {
      on: {
        ACTION_DONE: { target: "loaded", actions: "setHub" },
        DELETED: { target: "idle" },
        ACTION_FAILED: { target: "loaded", actions: "setError" },
      },
    },
    error: {
      on: { RETRY: { target: "loading" } },
    },
  },
});
