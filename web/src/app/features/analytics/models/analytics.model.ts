export interface DateRange {
  startDate: string; // ISO 8601
  endDate: string;   // ISO 8601
}

export interface HubAnalytics {
  hubId: string;
  dateRange: DateRange;
  totalPlays: number;
  totalVotes: number;
  averageUpvotePercent: number;
  activeListeners: number;
  topTracks: TrackStat[];
  voteDistribution: VoteDistribution[];
  playCountByDay: DailyPlayCount[];
}

export interface TrackStat {
  trackId: string;
  title: string;
  artist: string;
  coverUrl: string | null;
  plays: number;
  upvotes: number;
  downvotes: number;
  upvotePercent: number;
}

export interface VoteDistribution {
  date: string; // ISO 8601 date
  upvotes: number;
  downvotes: number;
  totalVotes: number;
}

export interface DailyPlayCount {
  date: string; // ISO 8601 date
  count: number;
}

export interface AnalyticsStatsCard {
  label: string;
  value: string | number;
  icon: string;
  trend?: number; // percentage change
}
