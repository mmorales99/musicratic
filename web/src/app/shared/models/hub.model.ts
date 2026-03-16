export type HubType = "venue" | "portable";

export type HubBusinessType =
  | "bar"
  | "restaurant"
  | "gym"
  | "store"
  | "office"
  | "custom";

export type MoodType = "home_party" | "driving" | "chill" | "workout" | "study";

export type SubscriptionTier = "free_trial" | "monthly" | "annual" | "event";

export type MusicProvider = "spotify" | "youtube_music" | "local";

export type HubVisibility = "public" | "private";

export type PlayMode = "ordered" | "weighted_shuffle";

export interface HubSettings {
  allowProposals: boolean;
  autoSkipThreshold: number;
  votingWindowSeconds: number;
  maxQueueSize: number;
  allowedProviders: MusicProvider[];
  enableLocalStorage: boolean;
  adsEnabled: boolean;
}

export interface Hub {
  id: string;
  tenantId: string;
  name: string;
  code: string;
  type: HubType;
  businessType: HubBusinessType;
  visibility: HubVisibility;
  mood: MoodType | null;
  ownerId: string;
  subscriptionTier: SubscriptionTier;
  subscriptionExpiresAt: string | null;
  isActive: boolean;
  isPaused: boolean;
  qrUrl: string;
  directLink: string;
  createdAt: string;
  settings: HubSettings;
}

export interface CreateHubRequest {
  name: string;
  businessType: HubBusinessType;
  musicProviders: MusicProvider[];
  visibility: HubVisibility;
}

export interface UpdateHubRequest {
  name?: string;
  businessType?: HubBusinessType;
  visibility?: HubVisibility;
}

export interface HubAttachment {
  id: string;
  userId: string;
  hubId: string;
  attachedAt: string;
  expiresAt: string;
  isActive: boolean;
}

export interface HubList {
  id: string;
  hubId: string;
  name: string;
  playMode: PlayMode;
  trackCount: number;
  createdAt: string;
}

export interface CreateListRequest {
  name: string;
  playMode: PlayMode;
}

export interface UpdateListRequest {
  name?: string;
  playMode?: PlayMode;
}

export interface ListTrack {
  id: string;
  trackId: string;
  title: string;
  artist: string;
  album: string | null;
  provider: MusicProvider;
  durationSeconds: number;
  coverUrl: string | null;
  position: number;
}

export interface AddTrackRequest {
  provider: MusicProvider;
  externalId: string;
  title: string;
  artist: string;
  album?: string;
  durationSeconds: number;
  coverUrl?: string;
}

export interface BulkAddTracksRequest {
  tracks: AddTrackRequest[];
}

export interface AttachToHubRequest {
  code: string;
}

export interface SearchHubsParams {
  name?: string;
  type?: HubBusinessType;
  visibility?: HubVisibility;
  page: number;
  pageSize: number;
}

export interface HubSearchResult {
  id: string;
  name: string;
  code: string;
  businessType: HubBusinessType;
  visibility: HubVisibility;
  isActive: boolean;
  memberCount: number;
  rating: number | null;
}
