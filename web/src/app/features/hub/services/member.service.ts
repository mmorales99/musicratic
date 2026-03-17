import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "@app/shared/services/bff-api.service";
import { Member, TierLimits } from "../models/member.model";

@Injectable({ providedIn: "root" })
export class MemberService {
  private readonly api = inject(BffApiService);

  getMembers(hubId: string): Observable<Member[]> {
    return this.api.get<Member[]>(
      `/hubs/${encodeURIComponent(hubId)}/members`,
    );
  }

  removeMember(hubId: string, userId: string): Observable<void> {
    return this.api.delete<void>(
      `/hubs/${encodeURIComponent(hubId)}/members/${encodeURIComponent(userId)}`,
    );
  }

  promoteMember(
    hubId: string,
    userId: string,
    newRole: string,
  ): Observable<void> {
    return this.api.put<void>(
      `/hubs/${encodeURIComponent(hubId)}/members/${encodeURIComponent(userId)}/promote`,
      { role: newRole },
    );
  }

  demoteMember(
    hubId: string,
    userId: string,
    newRole: string,
  ): Observable<void> {
    return this.api.put<void>(
      `/hubs/${encodeURIComponent(hubId)}/members/${encodeURIComponent(userId)}/demote`,
      { role: newRole },
    );
  }

  getTierLimits(hubId: string): Observable<TierLimits> {
    return this.api.get<TierLimits>(
      `/hubs/${encodeURIComponent(hubId)}/tier-limits`,
    );
  }
}
