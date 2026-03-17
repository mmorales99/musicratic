/// Accumulative user roles with hierarchy.
/// anonymous → visitor → user → list_owner → hub_manager
enum UserRole {
  anonymous(0, 'Anonymous'),
  visitor(1, 'Visitor'),
  user(2, 'User'),
  listOwner(3, 'List Owner'),
  hubManager(4, 'Hub Manager');

  const UserRole(this.level, this.label);

  final int level;
  final String label;

  bool hasAtLeast(UserRole required) => level >= required.level;

  static UserRole fromString(String value) {
    switch (value.toLowerCase()) {
      case 'hub_manager':
        return UserRole.hubManager;
      case 'list_owner':
        return UserRole.listOwner;
      case 'user':
        return UserRole.user;
      case 'visitor':
        return UserRole.visitor;
      default:
        return UserRole.anonymous;
    }
  }
}
