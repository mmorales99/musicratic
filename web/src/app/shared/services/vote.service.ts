import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "./bff-api.service";
import { WebSocketService, WsMessage } from "./websocket.service";
import {
  VoteTally,
  VoteDirection,
  CastVoteResponse,
  WsVoteTallyPayload,
  WsSkipTriggeredPayload,
} from "@app/shared/models/vote.model";

@Injectable({ providedIn: "root" })
export class VoteService {
  private readonly api = inject(BffApiService);
  private readonly ws = inject(WebSocketService);

  castVote(
    hubId: string,
    entryId: string,
    value: VoteDirection,
  ): Observable<CastVoteResponse> {
    return this.api.post<CastVoteResponse>(
      `/hubs/${encodeURIComponent(hubId)}/queue/${encodeURIComponent(entryId)}/vote`,
      { value },
    );
  }

  removeVote(hubId: string, entryId: string): Observable<void> {
    return this.api.delete<void>(
      `/hubs/${encodeURIComponent(hubId)}/queue/${encodeURIComponent(entryId)}/vote`,
    );
  }

  getTally(hubId: string, entryId: string): Observable<VoteTally> {
    return this.api.get<VoteTally>(
      `/hubs/${encodeURIComponent(hubId)}/queue/${encodeURIComponent(entryId)}/tally`,
    );
  }

  onTallyUpdated(): Observable<WsMessage<WsVoteTallyPayload>> {
    return this.ws.on<WsVoteTallyPayload>("vote.tally");
  }

  onSkipTriggered(): Observable<WsMessage<WsSkipTriggeredPayload>> {
    return this.ws.on<WsSkipTriggeredPayload>("playback.skip-triggered");
  }
}
