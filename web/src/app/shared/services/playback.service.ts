import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "./bff-api.service";
import { NowPlaying } from "@app/shared/models/playback.model";

@Injectable({ providedIn: "root" })
export class PlaybackService {
  private readonly api = inject(BffApiService);

  getNowPlaying(hubId: string): Observable<NowPlaying> {
    return this.api.get<NowPlaying>(
      `/hubs/${encodeURIComponent(hubId)}/now-playing`,
    );
  }

  skipTrack(hubId: string): Observable<void> {
    return this.api.post<void>(
      `/hubs/${encodeURIComponent(hubId)}/queue/skip`,
      {},
    );
  }
}
