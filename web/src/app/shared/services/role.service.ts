import { Injectable, inject, signal, computed } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "./bff-api.service";
import { AuthService } from "./auth.service";
import {
  UserRole,
  ROLE_HIERARCHY,
} from "@app/shared/models/user-role.model";

@Injectable({ providedIn: "root" })
export class RoleService {
  private readonly api = inject(BffApiService);
  private readonly auth = inject(AuthService);

  readonly currentRole = signal<UserRole>(UserRole.Anonymous);

  readonly isAuthenticated = computed(
    () => this.currentRole() >= UserRole.Visitor,
  );

  hasRole(requiredRole: UserRole): boolean {
    const currentLevel = ROLE_HIERARCHY.get(this.currentRole()) ?? 0;
    const requiredLevel = ROLE_HIERARCHY.get(requiredRole) ?? 99;
    return currentLevel >= requiredLevel;
  }

  fetchRole(hubId: string): void {
    this.api
      .get<{ role: string }>(
        `/hubs/${encodeURIComponent(hubId)}/my-role`,
      )
      .subscribe({
        next: (response) => {
          this.currentRole.set(this.parseRole(response.role));
        },
        error: () => {
          const fallback = this.auth.isAuthenticated()
            ? UserRole.Visitor
            : UserRole.Anonymous;
          this.currentRole.set(fallback);
        },
      });
  }

  resetRole(): void {
    this.currentRole.set(UserRole.Anonymous);
  }

  private parseRole(role: string): UserRole {
    const mapping: Record<string, UserRole> = {
      anonymous: UserRole.Anonymous,
      visitor: UserRole.Visitor,
      user: UserRole.User,
      list_owner: UserRole.ListOwner,
      hub_manager: UserRole.HubManager,
    };
    return mapping[role.toLowerCase()] ?? UserRole.Anonymous;
  }
}
