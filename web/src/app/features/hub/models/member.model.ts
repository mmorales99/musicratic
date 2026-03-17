import { UserRole } from "@app/shared/models/user-role.model";

export interface Member {
  userId: string;
  hubId: string;
  displayName: string;
  email: string;
  avatarUrl: string | null;
  role: UserRole;
  roleName: string;
  joinedAt: string; // ISO 8601
}

export interface TierLimits {
  maxSubListOwners: number;
  currentSubListOwners: number;
  maxSubHubManagers: number;
  currentSubHubManagers: number;
}

export type RoleBadgeColor = "gray" | "blue" | "green" | "gold";

export const ROLE_BADGE_COLORS: ReadonlyMap<UserRole, RoleBadgeColor> = new Map([
  [UserRole.Visitor, "gray"],
  [UserRole.User, "gray"],
  [UserRole.ListOwner, "blue"],
  [UserRole.HubManager, "gold"],
]);
