import { setup, assign } from "xstate";
import { QueueEntry } from "@app/shared/models/track.model";
import { NowPlaying } from "@app/shared/models/playback.model";

export interface QueueMachineContext {
  hubId: string | null;
  queue: QueueEntry[];
  nowPlaying: NowPlaying | null;
  error: string | null;
}

export type QueueMachineEvent =
  | { type: "CONNECT"; hubId: string }
  | { type: "CONNECTED" }
  | { type: "CONNECTION_FAILED"; error: string }
  | { type: "DISCONNECT" }
  | { type: "QUEUE_LOADED"; queue: QueueEntry[] }
  | { type: "QUEUE_UPDATED"; queue: QueueEntry[] }
  | { type: "NOW_PLAYING"; nowPlaying: NowPlaying }
  | { type: "TRACK_ENDED"; entryId: string }
  | { type: "TRACK_SKIPPED"; entryId: string; reason: string }
  | { type: "RETRY" };

export const queueMachine = setup({
  types: {
    context: {} as QueueMachineContext,
    events: {} as QueueMachineEvent,
  },
  actions: {
    setHubId: assign({
      hubId: ({ event }) => (event.type === "CONNECT" ? event.hubId : null),
      error: () => null,
    }),
    setQueue: assign({
      queue: ({ event }) => {
        if (event.type === "QUEUE_LOADED" || event.type === "QUEUE_UPDATED") {
          return event.queue;
        }
        return [];
      },
    }),
    setNowPlaying: assign({
      nowPlaying: ({ event }) =>
        event.type === "NOW_PLAYING" ? event.nowPlaying : null,
    }),
    markTrackEnded: assign({
      queue: ({ context, event }) => {
        if (event.type !== "TRACK_ENDED") return context.queue;
        return context.queue.filter((e) => e.id !== event.entryId);
      },
      nowPlaying: () => null,
    }),
    markTrackSkipped: assign({
      queue: ({ context, event }) => {
        if (event.type !== "TRACK_SKIPPED") return context.queue;
        return context.queue.map((e) =>
          e.id === event.entryId ? { ...e, status: "skipped" as const } : e,
        );
      },
      nowPlaying: ({ context, event }) => {
        if (event.type !== "TRACK_SKIPPED") return context.nowPlaying;
        if (context.nowPlaying?.entry?.id === event.entryId) return null;
        return context.nowPlaying;
      },
    }),
    setError: assign({
      error: ({ event }) =>
        event.type === "CONNECTION_FAILED" ? event.error : null,
    }),
    clearState: assign({
      hubId: () => null,
      queue: () => [],
      nowPlaying: () => null,
      error: () => null,
    }),
  },
}).createMachine({
  id: "queue",
  initial: "disconnected",
  context: {
    hubId: null,
    queue: [],
    nowPlaying: null,
    error: null,
  },
  states: {
    disconnected: {
      on: {
        CONNECT: { target: "connecting", actions: "setHubId" },
      },
    },
    connecting: {
      on: {
        CONNECTED: { target: "connected" },
        CONNECTION_FAILED: { target: "error", actions: "setError" },
      },
    },
    connected: {
      on: {
        QUEUE_LOADED: { actions: "setQueue" },
        QUEUE_UPDATED: { actions: "setQueue" },
        NOW_PLAYING: { actions: "setNowPlaying" },
        TRACK_ENDED: { actions: "markTrackEnded" },
        TRACK_SKIPPED: { actions: "markTrackSkipped" },
        DISCONNECT: { target: "disconnected", actions: "clearState" },
      },
    },
    error: {
      on: {
        RETRY: { target: "connecting" },
        DISCONNECT: { target: "disconnected", actions: "clearState" },
      },
    },
  },
});
