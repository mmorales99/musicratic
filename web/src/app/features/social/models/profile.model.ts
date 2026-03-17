export interface UserProfile {
  id: string;
  displayName: string;
  avatarUrl: string | null;
  bio: string;
  favoriteGenres: string[];
  stats: ProfileStats;
  memberSince: string;
}

export interface ProfileStats {
  totalProposals: number;
  totalUpvotes: number;
  hubsVisited: number;
  proposerScore: number;
  activeVoterBadge: boolean;
  topContributorBadge: boolean;
}

export interface UpdateProfileRequest {
  displayName?: string;
  bio?: string;
  favoriteGenres?: string[];
}
