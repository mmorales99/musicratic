import { MusicProvider } from "./hub.model";

export type QueueEntryStatus =
  | "pending"
  | "playing"
  | "played"
  | "skipped"
  | "removed";

export type QueueEntrySource = "list" | "proposal";

export interface Track {
  id: string;
  provider: MusicProvider;
  externalId: string;
  title: string;
  artist: string;
  album: string | null;
  durationSeconds: number;
  coverUrl: string | null;
  hotnessScore: number;
}

export interface QueueEntry {
  id: string;
  trackId: string;
  track: Track;
  position: number;
  status: QueueEntryStatus;
  source: QueueEntrySource;
  proposedByUserId: string | null;
  paid: boolean;
  addedAt: string;
}

export interface TrackStats {
  trackId: string;
  hubId: string;
  upvotes: number;
  downvotes: number;
  plays: number;
  skips: number;
}
