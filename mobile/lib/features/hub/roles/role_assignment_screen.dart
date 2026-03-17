import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../shared/models/user_role.dart';
import '../members/member_models.dart';
import 'role_bloc.dart';
import 'role_event.dart';
import 'role_state.dart';

class RoleAssignmentScreen extends StatelessWidget {
  const RoleAssignmentScreen({super.key, required this.hubId});

  final String hubId;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Role Assignment')),
      body: BlocConsumer<RoleBloc, RoleState>(
        listener: (context, state) {
          state.whenOrNull(
            success: (message) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(content: Text(message)),
              );
            },
            error: (message) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text(message),
                  backgroundColor: Colors.red,
                ),
              );
            },
          );
        },
        builder: (context, state) {
          return state.when(
            initial: () => const Center(child: Text('Loading...')),
            loading: () => const Center(child: CircularProgressIndicator()),
            loaded: (members, tierLimits) => _RoleList(
              hubId: hubId,
              members: members,
              tierLimits: tierLimits,
            ),
            updating: () => const Center(child: CircularProgressIndicator()),
            success: (_) => const Center(child: CircularProgressIndicator()),
            error: (message) => Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text('Error: $message'),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () => context
                        .read<RoleBloc>()
                        .add(RoleEvent.load(hubId: hubId)),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}

class _RoleList extends StatelessWidget {
  const _RoleList({
    required this.hubId,
    required this.members,
    required this.tierLimits,
  });

  final String hubId;
  final List<HubMember> members;
  final Map<String, dynamic> tierLimits;

  @override
  Widget build(BuildContext context) {
    if (members.isEmpty) {
      return const Center(child: Text('No members'));
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        if (tierLimits.isNotEmpty)
          Padding(
            padding: const EdgeInsets.all(16),
            child: Card(
              child: Padding(
                padding: const EdgeInsets.all(12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Tier Limits',
                      style: Theme.of(context).textTheme.titleSmall,
                    ),
                    const SizedBox(height: 8),
                    ...tierLimits.entries.map(
                      (e) => Text('${e.key}: ${e.value}'),
                    ),
                  ],
                ),
              ),
            ),
          ),
        Expanded(
          child: ListView.builder(
            itemCount: members.length,
            itemBuilder: (context, index) {
              final member = members[index];
              return ListTile(
                leading: CircleAvatar(
                  backgroundImage: member.avatar != null
                      ? NetworkImage(member.avatar!)
                      : null,
                  child: member.avatar == null
                      ? Text(member.displayName[0].toUpperCase())
                      : null,
                ),
                title: Text(member.displayName),
                subtitle: Text(UserRole.fromString(member.role).label),
                trailing: const Icon(Icons.chevron_right),
                onTap: () => _showRoleSheet(context, member),
              );
            },
          ),
        ),
      ],
    );
  }

  void _showRoleSheet(BuildContext context, HubMember member) {
    final currentRole = UserRole.fromString(member.role);
    showModalBottomSheet<void>(
      context: context,
      builder: (ctx) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                member.displayName,
                style: Theme.of(ctx).textTheme.titleLarge,
              ),
              Text('Current role: ${currentRole.label}'),
              const SizedBox(height: 16),
              const Text('Change role:'),
              const SizedBox(height: 8),
              ...UserRole.values
                  .where((r) => r != UserRole.anonymous && r != currentRole)
                  .map((role) => ListTile(
                        title: Text(role.label),
                        leading: Icon(
                          role.level > currentRole.level
                              ? Icons.arrow_upward
                              : Icons.arrow_downward,
                          color: role.level > currentRole.level
                              ? Colors.green
                              : Colors.orange,
                        ),
                        onTap: () {
                          Navigator.of(ctx).pop();
                          final bloc = context.read<RoleBloc>();
                          if (role.level > currentRole.level) {
                            bloc.add(RoleEvent.promote(
                              hubId: hubId,
                              memberId: member.id,
                              newRole: role.name,
                            ));
                          } else {
                            bloc.add(RoleEvent.demote(
                              hubId: hubId,
                              memberId: member.id,
                              newRole: role.name,
                            ));
                          }
                        },
                      )),
            ],
          ),
        ),
      ),
    );
  }
}
