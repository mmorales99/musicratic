import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { BffApiService } from "./bff-api.service";
import {
  TrackSearchResult,
  TrackSearchResponse,
  ProposeTrackRequest,
  ProposeTrackResponse,
} from "@app/shared/models/playback.model";
import { MusicProvider } from "@app/shared/models/hub.model";

@Injectable({ providedIn: "root" })
export class TrackService {
  private readonly api = inject(BffApiService);

  searchTracks(
    query: string,
    provider?: MusicProvider,
    limit: number = 20,
  ): Observable<TrackSearchResponse> {
    const params = new URLSearchParams({ q: query, limit: limit.toString() });
    if (provider) {
      params.set("provider", provider);
    }
    return this.api.get<TrackSearchResponse>(
      `/tracks/search?${params.toString()}`,
    );
  }

  proposeTrack(
    hubId: string,
    request: ProposeTrackRequest,
  ): Observable<ProposeTrackResponse> {
    return this.api.post<ProposeTrackResponse>(
      `/hubs/${encodeURIComponent(hubId)}/queue/propose`,
      request,
    );
  }
}
