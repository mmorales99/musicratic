import { setup, assign } from "xstate";
import { VoteDirection } from "@app/shared/models/vote.model";

export interface VoteMachineContext {
  hubId: string | null;
  entryId: string | null;
  currentVote: VoteDirection | null;
  previousVote: VoteDirection | null;
  upCount: number;
  downCount: number;
  percentage: number;
  error: string | null;
}

export type VoteMachineEvent =
  | { type: "INIT"; hubId: string; entryId: string }
  | {
      type: "TALLY_LOADED";
      upvotes: number;
      downvotes: number;
      currentVote: VoteDirection | null;
    }
  | { type: "VOTE_UP" }
  | { type: "VOTE_DOWN" }
  | { type: "REMOVE_VOTE" }
  | { type: "VOTE_SUCCESS" }
  | { type: "VOTE_FAILED"; error: string }
  | {
      type: "TALLY_UPDATED";
      upvotes: number;
      downvotes: number;
    }
  | { type: "RESET" };

function calcPercentage(up: number, down: number): number {
  const total = up + down;
  return total === 0 ? 0 : Math.round((up / total) * 100);
}

function applyOptimisticVote(
  ctx: VoteMachineContext,
  newVote: VoteDirection | null,
): Partial<VoteMachineContext> {
  let upCount = ctx.upCount;
  let downCount = ctx.downCount;

  // Remove previous vote contribution
  if (ctx.currentVote === "up") upCount--;
  if (ctx.currentVote === "down") downCount--;

  // Add new vote contribution
  if (newVote === "up") upCount++;
  if (newVote === "down") downCount++;

  return {
    previousVote: ctx.currentVote,
    currentVote: newVote,
    upCount: Math.max(0, upCount),
    downCount: Math.max(0, downCount),
    percentage: calcPercentage(Math.max(0, upCount), Math.max(0, downCount)),
    error: null,
  };
}

export const voteMachine = setup({
  types: {
    context: {} as VoteMachineContext,
    events: {} as VoteMachineEvent,
  },
  actions: {
    setEntry: assign({
      hubId: ({ event }) => (event.type === "INIT" ? event.hubId : null),
      entryId: ({ event }) => (event.type === "INIT" ? event.entryId : null),
      error: () => null,
    }),
    setTally: assign({
      upCount: ({ event }) =>
        event.type === "TALLY_LOADED" ? event.upvotes : 0,
      downCount: ({ event }) =>
        event.type === "TALLY_LOADED" ? event.downvotes : 0,
      currentVote: ({ event }) =>
        event.type === "TALLY_LOADED" ? event.currentVote : null,
      previousVote: () => null,
      percentage: ({ event }) =>
        event.type === "TALLY_LOADED"
          ? calcPercentage(event.upvotes, event.downvotes)
          : 0,
      error: () => null,
    }),
    optimisticUp: assign(({ context }) => applyOptimisticVote(context, "up")),
    optimisticDown: assign(({ context }) =>
      applyOptimisticVote(context, "down"),
    ),
    optimisticRemove: assign(({ context }) =>
      applyOptimisticVote(context, null),
    ),
    rollback: assign({
      currentVote: ({ context }) => context.previousVote,
      upCount: ({ context }) => {
        let up = context.upCount;
        if (context.currentVote === "up") up--;
        if (context.previousVote === "up") up++;
        return Math.max(0, up);
      },
      downCount: ({ context }) => {
        let down = context.downCount;
        if (context.currentVote === "down") down--;
        if (context.previousVote === "down") down++;
        return Math.max(0, down);
      },
      percentage: ({ context }) => {
        let up = context.upCount;
        let down = context.downCount;
        if (context.currentVote === "up") up--;
        if (context.currentVote === "down") down--;
        if (context.previousVote === "up") up++;
        if (context.previousVote === "down") down++;
        return calcPercentage(Math.max(0, up), Math.max(0, down));
      },
      error: ({ event }) => (event.type === "VOTE_FAILED" ? event.error : null),
    }),
    confirmVote: assign({
      previousVote: () => null,
      error: () => null,
    }),
    updateTally: assign({
      upCount: ({ event }) =>
        event.type === "TALLY_UPDATED" ? event.upvotes : 0,
      downCount: ({ event }) =>
        event.type === "TALLY_UPDATED" ? event.downvotes : 0,
      percentage: ({ event }) =>
        event.type === "TALLY_UPDATED"
          ? calcPercentage(event.upvotes, event.downvotes)
          : 0,
    }),
    clearState: assign({
      hubId: () => null,
      entryId: () => null,
      currentVote: () => null,
      previousVote: () => null,
      upCount: () => 0,
      downCount: () => 0,
      percentage: () => 0,
      error: () => null,
    }),
  },
}).createMachine({
  id: "vote",
  initial: "idle",
  context: {
    hubId: null,
    entryId: null,
    currentVote: null,
    previousVote: null,
    upCount: 0,
    downCount: 0,
    percentage: 0,
    error: null,
  },
  states: {
    idle: {
      on: {
        INIT: { target: "loading", actions: "setEntry" },
      },
    },
    loading: {
      on: {
        TALLY_LOADED: { target: "ready", actions: "setTally" },
        VOTE_FAILED: { target: "error", actions: "rollback" },
        RESET: { target: "idle", actions: "clearState" },
      },
    },
    ready: {
      on: {
        VOTE_UP: { target: "voting", actions: "optimisticUp" },
        VOTE_DOWN: { target: "voting", actions: "optimisticDown" },
        REMOVE_VOTE: { target: "voting", actions: "optimisticRemove" },
        TALLY_UPDATED: { actions: "updateTally" },
        INIT: { target: "loading", actions: "setEntry" },
        RESET: { target: "idle", actions: "clearState" },
      },
    },
    voting: {
      on: {
        VOTE_SUCCESS: { target: "ready", actions: "confirmVote" },
        VOTE_FAILED: { target: "ready", actions: "rollback" },
        TALLY_UPDATED: { actions: "updateTally" },
        RESET: { target: "idle", actions: "clearState" },
      },
    },
    error: {
      on: {
        VOTE_UP: { target: "voting", actions: "optimisticUp" },
        VOTE_DOWN: { target: "voting", actions: "optimisticDown" },
        TALLY_UPDATED: { actions: "updateTally" },
        INIT: { target: "loading", actions: "setEntry" },
        RESET: { target: "idle", actions: "clearState" },
      },
    },
  },
});
