import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { BffApiService } from "./bff-api.service";
import { WebSocketService } from "./websocket.service";
import { QueueEntry } from "@app/shared/models/track.model";
import {
  QueuePage,
  WsQueueUpdatedPayload,
  WsNowPlayingPayload,
  WsTrackEndedPayload,
  WsTrackSkippedPayload,
} from "@app/shared/models/playback.model";
import { ApiEnvelope } from "@app/shared/models/api-response.model";
import { WsMessage } from "./websocket.service";

@Injectable({ providedIn: "root" })
export class QueueService {
  private readonly api = inject(BffApiService);
  private readonly ws = inject(WebSocketService);

  getQueue(
    hubId: string,
    page: number = 1,
    pageSize: number = 50,
  ): Observable<QueuePage> {
    const params = `?page=${page}&pageSize=${pageSize}`;
    return this.api
      .get<
        ApiEnvelope<QueueEntry>
      >(`/hubs/${encodeURIComponent(hubId)}/queue${params}`)
      .pipe(
        map((envelope) => ({
          items: envelope.items,
          totalItemsInResponse: envelope.totalItemsInResponse,
          hasMoreItems: envelope.hasMoreItems,
        })),
      );
  }

  connectToHub(hubId: string): void {
    this.ws.connect(hubId);
  }

  disconnectFromHub(): void {
    this.ws.disconnect();
  }

  onQueueUpdated(): Observable<WsMessage<WsQueueUpdatedPayload>> {
    return this.ws.on<WsQueueUpdatedPayload>("queue.updated");
  }

  onNowPlaying(): Observable<WsMessage<WsNowPlayingPayload>> {
    return this.ws.on<WsNowPlayingPayload>("playback.now-playing");
  }

  onTrackEnded(): Observable<WsMessage<WsTrackEndedPayload>> {
    return this.ws.on<WsTrackEndedPayload>("playback.track-ended");
  }

  onTrackSkipped(): Observable<WsMessage<WsTrackSkippedPayload>> {
    return this.ws.on<WsTrackSkippedPayload>("playback.track-skipped");
  }
}
