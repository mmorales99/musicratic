import 'dart:async';

import '../models/user_role.dart';

/// Provides the current user's role within a hub context.
class RoleService {
  UserRole _currentRole = UserRole.anonymous;
  final _controller = StreamController<UserRole>.broadcast();

  UserRole get currentRole => _currentRole;
  Stream<UserRole> get roleStream => _controller.stream;

  void updateRole(UserRole role) {
    _currentRole = role;
    _controller.add(role);
  }

  bool hasPermission(UserRole required) {
    return _currentRole.hasAtLeast(required);
  }

  void dispose() {
    _controller.close();
  }
}
