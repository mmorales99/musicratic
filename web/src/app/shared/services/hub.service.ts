import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "./bff-api.service";
import {
  Hub,
  HubSettings,
  HubAttachment,
  CreateHubRequest,
  UpdateHubRequest,
  SearchHubsParams,
  HubSearchResult,
} from "@app/shared/models/hub.model";
import { ApiEnvelope } from "@app/shared/models/api-response.model";

@Injectable({ providedIn: "root" })
export class HubService {
  private readonly api = inject(BffApiService);

  getActiveHubs(): Observable<ApiEnvelope<Hub>> {
    return this.api.get<ApiEnvelope<Hub>>("/hubs");
  }

  getHub(id: string): Observable<Hub> {
    return this.api.get<Hub>(`/hubs/${encodeURIComponent(id)}`);
  }

  createHub(data: CreateHubRequest): Observable<Hub> {
    return this.api.post<Hub>("/hubs", data);
  }

  updateHub(id: string, data: UpdateHubRequest): Observable<Hub> {
    return this.api.put<Hub>(`/hubs/${encodeURIComponent(id)}`, data);
  }

  deleteHub(id: string): Observable<void> {
    return this.api.delete<void>(`/hubs/${encodeURIComponent(id)}`);
  }

  getHubSettings(id: string): Observable<HubSettings> {
    return this.api.get<HubSettings>(
      `/hubs/${encodeURIComponent(id)}/settings`,
    );
  }

  updateHubSettings(
    id: string,
    data: Partial<HubSettings>,
  ): Observable<HubSettings> {
    return this.api.put<HubSettings>(
      `/hubs/${encodeURIComponent(id)}/settings`,
      data,
    );
  }

  activateHub(id: string): Observable<Hub> {
    return this.api.post<Hub>(`/hubs/${encodeURIComponent(id)}/activate`, {});
  }

  deactivateHub(id: string): Observable<Hub> {
    return this.api.post<Hub>(`/hubs/${encodeURIComponent(id)}/deactivate`, {});
  }

  pauseHub(id: string): Observable<Hub> {
    return this.api.post<Hub>(`/hubs/${encodeURIComponent(id)}/pause`, {});
  }

  resumeHub(id: string): Observable<Hub> {
    return this.api.post<Hub>(`/hubs/${encodeURIComponent(id)}/resume`, {});
  }

  attachToHub(code: string): Observable<HubAttachment> {
    return this.api.post<HubAttachment>("/hubs/attach", { code });
  }

  detachFromHub(): Observable<void> {
    return this.api.post<void>("/hubs/detach", {});
  }

  getMyAttachment(): Observable<HubAttachment> {
    return this.api.get<HubAttachment>("/hubs/my-attachment");
  }

  searchHubs(
    params: SearchHubsParams,
  ): Observable<ApiEnvelope<HubSearchResult>> {
    const query = new URLSearchParams();
    if (params.name) query.set("name", params.name);
    if (params.type) query.set("type", params.type);
    if (params.visibility) query.set("visibility", params.visibility);
    query.set("page", params.page.toString());
    query.set("pageSize", params.pageSize.toString());
    return this.api.get<ApiEnvelope<HubSearchResult>>(
      `/hubs/search?${query.toString()}`,
    );
  }
}
