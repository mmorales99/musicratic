import { setup, assign } from "xstate";
import { TrackSearchResult } from "@app/shared/models/playback.model";
import { TrackPrice, Wallet } from "@app/shared/models/economy.model";

export interface ProposalMachineContext {
  hubId: string | null;
  query: string;
  searchResults: TrackSearchResult[];
  selectedTrack: TrackSearchResult | null;
  trackPrice: TrackPrice | null;
  wallet: Wallet | null;
  entryId: string | null;
  error: string | null;
}

export type ProposalMachineEvent =
  | { type: "SET_HUB"; hubId: string }
  | { type: "SEARCH"; query: string }
  | { type: "SEARCH_SUCCESS"; results: TrackSearchResult[] }
  | { type: "SEARCH_FAILED"; error: string }
  | { type: "SELECT_TRACK"; track: TrackSearchResult }
  | { type: "PRICE_LOADED"; price: TrackPrice; wallet: Wallet }
  | { type: "PRICE_FAILED"; error: string }
  | { type: "CONFIRM" }
  | { type: "PROPOSE" }
  | { type: "PROPOSE_SUCCESS"; entryId: string }
  | { type: "PROPOSE_FAILED"; error: string }
  | { type: "CANCEL" }
  | { type: "RESET" };

export const proposalMachine = setup({
  types: {
    context: {} as ProposalMachineContext,
    events: {} as ProposalMachineEvent,
  },
  guards: {
    hasEnoughCoins: ({ context }) => {
      if (!context.wallet || !context.trackPrice) return false;
      return context.wallet.balance >= context.trackPrice.finalCost;
    },
    isFreeProposal: ({ context }) => {
      return !context.trackPrice || context.trackPrice.finalCost === 0;
    },
  },
  actions: {
    setHub: assign({
      hubId: ({ event }) => (event.type === "SET_HUB" ? event.hubId : null),
    }),
    setQuery: assign({
      query: ({ event }) => (event.type === "SEARCH" ? event.query : ""),
    }),
    setSearchResults: assign({
      searchResults: ({ event }) =>
        event.type === "SEARCH_SUCCESS" ? event.results : [],
      error: () => null,
    }),
    setSelectedTrack: assign({
      selectedTrack: ({ event }) =>
        event.type === "SELECT_TRACK" ? event.track : null,
    }),
    setPriceAndWallet: assign({
      trackPrice: ({ event }) =>
        event.type === "PRICE_LOADED" ? event.price : null,
      wallet: ({ event }) =>
        event.type === "PRICE_LOADED" ? event.wallet : null,
      error: () => null,
    }),
    setProposalSuccess: assign({
      entryId: ({ event }) =>
        event.type === "PROPOSE_SUCCESS" ? event.entryId : null,
      error: () => null,
    }),
    setError: assign({
      error: ({ event }) => {
        if (
          event.type === "SEARCH_FAILED" ||
          event.type === "PRICE_FAILED" ||
          event.type === "PROPOSE_FAILED"
        ) {
          return event.error;
        }
        return null;
      },
    }),
    clearSelection: assign({
      selectedTrack: () => null,
      trackPrice: () => null,
      wallet: () => null,
    }),
    resetAll: assign({
      query: () => "",
      searchResults: () => [],
      selectedTrack: () => null,
      trackPrice: () => null,
      wallet: () => null,
      entryId: () => null,
      error: () => null,
    }),
  },
}).createMachine({
  id: "proposal",
  initial: "idle",
  context: {
    hubId: null,
    query: "",
    searchResults: [],
    selectedTrack: null,
    trackPrice: null,
    wallet: null,
    entryId: null,
    error: null,
  },
  on: {
    SET_HUB: { actions: "setHub" },
    RESET: { target: ".idle", actions: "resetAll" },
  },
  states: {
    idle: {
      on: {
        SEARCH: { target: "searching", actions: "setQuery" },
      },
    },
    searching: {
      on: {
        SEARCH_SUCCESS: {
          target: "results",
          actions: "setSearchResults",
        },
        SEARCH_FAILED: {
          target: "error",
          actions: "setError",
        },
        SEARCH: { target: "searching", actions: "setQuery" },
      },
    },
    results: {
      on: {
        SEARCH: { target: "searching", actions: "setQuery" },
        SELECT_TRACK: { target: "selected", actions: "setSelectedTrack" },
      },
    },
    selected: {
      on: {
        PRICE_LOADED: [
          {
            guard: "isFreeProposal",
            target: "proposing",
            actions: "setPriceAndWallet",
          },
          {
            target: "confirming",
            actions: "setPriceAndWallet",
          },
        ],
        PRICE_FAILED: {
          target: "error",
          actions: "setError",
        },
        CANCEL: { target: "results", actions: "clearSelection" },
      },
    },
    confirming: {
      on: {
        CONFIRM: [
          {
            guard: "hasEnoughCoins",
            target: "proposing",
          },
        ],
        CANCEL: { target: "results", actions: "clearSelection" },
      },
    },
    proposing: {
      on: {
        PROPOSE_SUCCESS: {
          target: "success",
          actions: "setProposalSuccess",
        },
        PROPOSE_FAILED: {
          target: "error",
          actions: "setError",
        },
      },
    },
    success: {
      on: {
        RESET: { target: "idle", actions: "resetAll" },
        SEARCH: { target: "searching", actions: ["resetAll", "setQuery"] },
      },
    },
    error: {
      on: {
        SEARCH: { target: "searching", actions: ["resetAll", "setQuery"] },
        CANCEL: { target: "results", actions: "clearSelection" },
        RESET: { target: "idle", actions: "resetAll" },
      },
    },
  },
});
