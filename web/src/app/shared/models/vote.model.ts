export enum VoteValue {
  Upvote = 1,
  Downvote = -1,
}

export type VoteDirection = "up" | "down";

export interface Vote {
  userId: string;
  entryId: string;
  value: VoteValue;
  castAt: string;
}

export interface VoteTally {
  entryId: string;
  upvotes: number;
  downvotes: number;
  totalVoters: number;
  downvotePercentage: number;
}

export interface CastVoteRequest {
  value: VoteDirection;
}

export interface CastVoteResponse {
  entryId: string;
  value: VoteValue;
  castAt: string;
}

export interface WsVoteTallyPayload {
  entryId: string;
  upvotes: number;
  downvotes: number;
  totalVoters: number;
  downvotePercentage: number;
}

export type SkipReason = "owner_downvote" | "vote_threshold" | "manual_skip";

export interface WsSkipTriggeredPayload {
  entryId: string;
  trackTitle: string;
  reason: SkipReason;
  refundAmount: number | null;
}
