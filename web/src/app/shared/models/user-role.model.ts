export enum UserRole {
  Anonymous = 1,
  Visitor = 2,
  User = 3,
  ListOwner = 4,
  HubManager = 5,
}

export const ROLE_HIERARCHY: ReadonlyMap<UserRole, number> = new Map([
  [UserRole.Anonymous, 1],
  [UserRole.Visitor, 2],
  [UserRole.User, 3],
  [UserRole.ListOwner, 4],
  [UserRole.HubManager, 5],
]);

export const ROLE_LABELS: ReadonlyMap<UserRole, string> = new Map([
  [UserRole.Anonymous, "Anonymous"],
  [UserRole.Visitor, "Visitor"],
  [UserRole.User, "User"],
  [UserRole.ListOwner, "List Owner"],
  [UserRole.HubManager, "Hub Manager"],
]);
