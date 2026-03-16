import { MusicProvider } from "./hub.model";
import { Track, QueueEntry, QueueEntryStatus } from "./track.model";

export interface NowPlaying {
  hubId: string;
  entry: QueueEntry | null;
  startedAt: string | null;
  elapsedSeconds: number;
  isPlaying: boolean;
}

export interface QueuePage {
  items: QueueEntry[];
  totalItemsInResponse: number;
  hasMoreItems: boolean;
}

export interface TrackSearchResult {
  id: string;
  provider: MusicProvider;
  externalId: string;
  title: string;
  artist: string;
  album: string | null;
  durationSeconds: number;
  coverUrl: string | null;
}

export interface TrackSearchResponse {
  results: TrackSearchResult[];
  total: number;
  query: string;
}

export interface ProposeTrackRequest {
  trackId: string;
  source: MusicProvider;
  externalId: string;
  title: string;
  artist: string;
  album?: string;
  durationSeconds: number;
  coverUrl?: string;
}

export interface ProposeTrackResponse {
  entryId: string;
  position: number;
  status: QueueEntryStatus;
}

/** WebSocket payload types for playback events */
export interface WsNowPlayingPayload {
  entry: QueueEntry;
  startedAt: string;
}

export interface WsQueueUpdatedPayload {
  queue: QueueEntry[];
}

export interface WsTrackEndedPayload {
  entryId: string;
  trackId: string;
}

export interface WsTrackSkippedPayload {
  entryId: string;
  trackId: string;
  reason: "owner_downvote" | "vote_threshold" | "manual_skip";
}
