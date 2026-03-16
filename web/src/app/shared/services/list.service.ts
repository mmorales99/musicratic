import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "./bff-api.service";
import {
  HubList,
  CreateListRequest,
  UpdateListRequest,
  ListTrack,
  AddTrackRequest,
  BulkAddTracksRequest,
  PlayMode,
} from "@app/shared/models/hub.model";
import { ApiEnvelope } from "@app/shared/models/api-response.model";

@Injectable({ providedIn: "root" })
export class ListService {
  private readonly api = inject(BffApiService);

  private hubListsPath(hubId: string): string {
    return `/hubs/${encodeURIComponent(hubId)}/lists`;
  }

  private listPath(hubId: string, listId: string): string {
    return `${this.hubListsPath(hubId)}/${encodeURIComponent(listId)}`;
  }

  private tracksPath(hubId: string, listId: string): string {
    return `${this.listPath(hubId, listId)}/tracks`;
  }

  getLists(hubId: string): Observable<ApiEnvelope<HubList>> {
    return this.api.get<ApiEnvelope<HubList>>(this.hubListsPath(hubId));
  }

  createList(hubId: string, data: CreateListRequest): Observable<HubList> {
    return this.api.post<HubList>(this.hubListsPath(hubId), data);
  }

  updateList(
    hubId: string,
    listId: string,
    data: UpdateListRequest,
  ): Observable<HubList> {
    return this.api.put<HubList>(this.listPath(hubId, listId), data);
  }

  deleteList(hubId: string, listId: string): Observable<void> {
    return this.api.delete<void>(this.listPath(hubId, listId));
  }

  getTracks(hubId: string, listId: string): Observable<ApiEnvelope<ListTrack>> {
    return this.api.get<ApiEnvelope<ListTrack>>(this.tracksPath(hubId, listId));
  }

  addTrack(
    hubId: string,
    listId: string,
    trackData: AddTrackRequest,
  ): Observable<ListTrack> {
    return this.api.post<ListTrack>(this.tracksPath(hubId, listId), trackData);
  }

  removeTrack(
    hubId: string,
    listId: string,
    trackId: string,
  ): Observable<void> {
    return this.api.delete<void>(
      `${this.tracksPath(hubId, listId)}/${encodeURIComponent(trackId)}`,
    );
  }

  reorderTrack(
    hubId: string,
    listId: string,
    trackId: string,
    position: number,
  ): Observable<void> {
    return this.api.put<void>(
      `${this.tracksPath(hubId, listId)}/${encodeURIComponent(trackId)}/position`,
      { position },
    );
  }

  bulkAddTracks(
    hubId: string,
    listId: string,
    tracks: BulkAddTracksRequest,
  ): Observable<ApiEnvelope<ListTrack>> {
    return this.api.post<ApiEnvelope<ListTrack>>(
      `${this.tracksPath(hubId, listId)}/bulk`,
      tracks,
    );
  }

  setPlayMode(
    hubId: string,
    listId: string,
    mode: PlayMode,
  ): Observable<HubList> {
    return this.api.put<HubList>(`${this.listPath(hubId, listId)}/play-mode`, {
      playMode: mode,
    });
  }
}
