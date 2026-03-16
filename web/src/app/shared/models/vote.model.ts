export enum VoteValue {
  Upvote = 1,
  Downvote = -1,
}

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
