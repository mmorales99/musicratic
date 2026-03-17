export type ReportType = "weekly" | "monthly";

export interface Report {
  id: string;
  hubId: string;
  type: ReportType;
  title: string;
  periodStart: string; // ISO 8601
  periodEnd: string;   // ISO 8601
  generatedAt: string; // ISO 8601
  summary: string;
}

export interface ReportTopTrack {
  trackId: string;
  title: string;
  artist: string;
  coverUrl: string | null;
  plays: number;
  upvotePercent: number;
}

export interface ReportActiveVoter {
  userId: string;
  displayName: string;
  avatarUrl: string | null;
  totalVotes: number;
}

export interface ReportRevenueSummary {
  totalRevenue: number;
  coinsPurchased: number;
  coinsSpent: number;
  subscriptionRevenue: number;
  currency: string;
}

export interface ReportDetail {
  id: string;
  hubId: string;
  type: ReportType;
  title: string;
  periodStart: string;
  periodEnd: string;
  generatedAt: string;
  summary: string;
  topTracks: ReportTopTrack[];
  mostActiveVoters: ReportActiveVoter[];
  revenueSummary: ReportRevenueSummary;
  suggestions: string[];
}
