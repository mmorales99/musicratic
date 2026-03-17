import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "@app/shared/services/bff-api.service";
import { ProposeTrackResponse } from "@app/shared/models/playback.model";
import { MusicProvider } from "@app/shared/models/hub.model";

export interface ProposeTrackPayload {
  trackId: string;
  providerId: string;
}

@Injectable({ providedIn: "root" })
export class ProposalService {
  private readonly api = inject(BffApiService);

  proposeTrack(
    hubId: string,
    trackId: string,
    providerId: MusicProvider,
  ): Observable<ProposeTrackResponse> {
    const payload: ProposeTrackPayload = { trackId, providerId };
    return this.api.post<ProposeTrackResponse>(
      `/hubs/${encodeURIComponent(hubId)}/queue/propose`,
      payload,
    );
  }
}
