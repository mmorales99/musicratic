/**
 * Re-export hub machines from the hub feature module.
 * The canonical machines live in features/hub/machines/.
 */
export {
  hubDetailMachine,
  type HubDetailContext,
  type HubDetailEvent,
} from "@app/features/hub/machines/hub-detail.machine";

// Legacy hub discovery machine — kept for backward compatibility
import { setup, assign } from "xstate";
import { Hub } from "@app/shared/models/hub.model";

export interface HubMachineContext {
  hubs: Hub[];
  activeHub: Hub | null;
  error: string | null;
}

export type HubMachineEvent =
  | { type: "LOAD_HUBS" }
  | { type: "HUBS_LOADED"; hubs: Hub[] }
  | { type: "LOAD_FAILED"; error: string }
  | { type: "SELECT_HUB"; hub: Hub }
  | { type: "ATTACH_HUB"; hubId: string }
  | { type: "ATTACHED"; hub: Hub }
  | { type: "DETACH" }
  | { type: "RETRY" };

export const hubMachine = setup({
  types: {
    context: {} as HubMachineContext,
    events: {} as HubMachineEvent,
  },
}).createMachine({
  id: "hub",
  initial: "idle",
  context: {
    hubs: [],
    activeHub: null,
    error: null,
  },
  states: {
    idle: {
      on: {
        LOAD_HUBS: { target: "loading" },
      },
    },
    loading: {
      on: {
        HUBS_LOADED: {
          target: "loaded",
          actions: assign({
            hubs: ({ event }) => event.hubs,
            error: () => null,
          }),
        },
        LOAD_FAILED: {
          target: "error",
          actions: assign({
            error: ({ event }) => event.error,
          }),
        },
      },
    },
    loaded: {
      on: {
        LOAD_HUBS: { target: "loading" },
        SELECT_HUB: {
          actions: assign({
            activeHub: ({ event }) => event.hub,
          }),
        },
        ATTACH_HUB: { target: "attaching" },
      },
    },
    attaching: {
      on: {
        ATTACHED: {
          target: "attached",
          actions: assign({
            activeHub: ({ event }) => event.hub,
            error: () => null,
          }),
        },
        LOAD_FAILED: {
          target: "error",
          actions: assign({
            error: ({ event }) => event.error,
          }),
        },
      },
    },
    attached: {
      on: {
        DETACH: {
          target: "loaded",
          actions: assign({
            activeHub: () => null,
          }),
        },
        LOAD_HUBS: { target: "loading" },
      },
    },
    error: {
      on: {
        RETRY: { target: "loading" },
      },
    },
  },
});
