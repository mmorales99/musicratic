import { setup, assign } from "xstate";
import { HubList, ListTrack } from "@app/shared/models/hub.model";

export interface ListDetailContext {
  list: HubList | null;
  tracks: ListTrack[];
  error: string | null;
}

export type ListDetailEvent =
  | { type: "LOAD"; hubId: string; listId: string }
  | { type: "LOADED"; list: HubList; tracks: ListTrack[] }
  | { type: "LOAD_FAILED"; error: string }
  | { type: "TRACK_ADDED"; track: ListTrack }
  | { type: "TRACK_REMOVED"; trackId: string }
  | { type: "TRACKS_REORDERED"; tracks: ListTrack[] }
  | { type: "PLAY_MODE_CHANGED"; list: HubList }
  | { type: "ACTION_FAILED"; error: string }
  | { type: "CLEAR_ERROR" }
  | { type: "RETRY" };

export const listDetailMachine = setup({
  types: {
    context: {} as ListDetailContext,
    events: {} as ListDetailEvent,
  },
  actions: {
    setLoaded: assign({
      list: ({ event }) => (event.type === "LOADED" ? event.list : null),
      tracks: ({ event }) => (event.type === "LOADED" ? event.tracks : []),
      error: () => null,
    }),
    addTrack: assign({
      tracks: ({ context, event }) =>
        event.type === "TRACK_ADDED"
          ? [...context.tracks, event.track]
          : context.tracks,
    }),
    removeTrack: assign({
      tracks: ({ context, event }) =>
        event.type === "TRACK_REMOVED"
          ? context.tracks.filter((t) => t.id !== event.trackId)
          : context.tracks,
    }),
    setReordered: assign({
      tracks: ({ event }) =>
        event.type === "TRACKS_REORDERED" ? event.tracks : [],
    }),
    setPlayMode: assign({
      list: ({ event }) =>
        event.type === "PLAY_MODE_CHANGED" ? event.list : null,
    }),
    setError: assign({
      error: ({ event }) =>
        event.type === "LOAD_FAILED" || event.type === "ACTION_FAILED"
          ? event.error
          : null,
    }),
    clearError: assign({ error: () => null }),
  },
}).createMachine({
  id: "listDetail",
  initial: "idle",
  context: { list: null, tracks: [], error: null },
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
        TRACK_ADDED: { actions: "addTrack" },
        TRACK_REMOVED: { actions: "removeTrack" },
        TRACKS_REORDERED: { actions: "setReordered" },
        PLAY_MODE_CHANGED: { actions: "setPlayMode" },
        ACTION_FAILED: { actions: "setError" },
        CLEAR_ERROR: { actions: "clearError" },
      },
    },
    error: {
      on: { RETRY: { target: "loading" } },
    },
  },
});
