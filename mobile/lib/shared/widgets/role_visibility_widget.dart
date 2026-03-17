import 'package:flutter/material.dart';

import '../models/user_role.dart';
import '../services/role_service.dart';

/// Widget that shows/hides children based on the user's current role.
///
/// ```dart
/// RoleVisibility(
///   roleService: roleService,
///   requiredRole: UserRole.hubManager,
///   child: EditButton(),
/// )
/// ```
class RoleVisibility extends StatelessWidget {
  const RoleVisibility({
    super.key,
    required this.roleService,
    required this.requiredRole,
    required this.child,
    this.fallback,
  });

  final RoleService roleService;
  final UserRole requiredRole;
  final Widget child;
  final Widget? fallback;

  @override
  Widget build(BuildContext context) {
    return StreamBuilder<UserRole>(
      stream: roleService.roleStream,
      initialData: roleService.currentRole,
      builder: (context, snapshot) {
        final role = snapshot.data ?? UserRole.anonymous;
        if (role.hasAtLeast(requiredRole)) {
          return child;
        }
        return fallback ?? const SizedBox.shrink();
      },
    );
  }
}
