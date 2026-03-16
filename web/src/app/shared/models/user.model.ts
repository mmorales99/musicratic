export enum UserRole {
  Anonymous = 1,
  Visitor = 2,
  User = 3,
  ListOwner = 4,
  HubManager = 5,
}

export interface User {
  id: string;
  authentikSub: string;
  displayName: string;
  email: string;
  avatarUrl: string | null;
  createdAt: string;
  walletBalance: number;
  currentAttachment: {
    hubId: string;
    attachedAt: string;
    expiresAt: string;
  } | null;
}
