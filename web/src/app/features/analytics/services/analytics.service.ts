import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "@app/shared/services/bff-api.service";
import {
  DateRange,
  HubAnalytics,
  TrackStat,
  VoteDistribution,
} from "../models/analytics.model";

@Injectable({ providedIn: "root" })
export class AnalyticsService {
  private readonly api = inject(BffApiService);

  getHubAnalytics(
    hubId: string,
    dateRange: DateRange,
  ): Observable<HubAnalytics> {
    const params = new URLSearchParams({
      startDate: dateRange.startDate,
      endDate: dateRange.endDate,
    });
    return this.api.get<HubAnalytics>(
      `/analytics/hubs/${encodeURIComponent(hubId)}/dashboard?${params.toString()}`,
    );
  }

  getTopTracks(hubId: string, limit: number = 10): Observable<TrackStat[]> {
    const params = new URLSearchParams({ limit: limit.toString() });
    return this.api.get<TrackStat[]>(
      `/analytics/hubs/${encodeURIComponent(hubId)}/top-tracks?${params.toString()}`,
    );
  }

  getVoteDistribution(
    hubId: string,
    dateRange: DateRange,
  ): Observable<VoteDistribution[]> {
    const params = new URLSearchParams({
      startDate: dateRange.startDate,
      endDate: dateRange.endDate,
    });
    return this.api.get<VoteDistribution[]>(
      `/analytics/hubs/${encodeURIComponent(hubId)}/vote-distribution?${params.toString()}`,
    );
  }
}
