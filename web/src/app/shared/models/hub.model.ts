export type HubType = "venue" | "portable";

export type MoodType = "home_party" | "driving" | "chill" | "workout" | "study";

export type SubscriptionTier = "free_trial" | "monthly" | "annual" | "event";

export type MusicProvider = "spotify" | "youtube_music" | "local";

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
  mood: MoodType | null;
  ownerId: string;
  subscriptionTier: SubscriptionTier;
  subscriptionExpiresAt: string | null;
  isActive: boolean;
  qrUrl: string;
  directLink: string;
  createdAt: string;
  settings: HubSettings;
}

export interface HubAttachment {
  id: string;
  userId: string;
  hubId: string;
  attachedAt: string;
  expiresAt: string;
  isActive: boolean;
}
