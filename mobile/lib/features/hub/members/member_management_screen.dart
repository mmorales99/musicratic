import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../shared/models/user_role.dart';
import 'member_bloc.dart';
import 'member_event.dart';
import 'member_models.dart';
import 'member_state.dart';

class MemberManagementScreen extends StatelessWidget {
  const MemberManagementScreen({super.key, required this.hubId});

  final String hubId;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Members')),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: TextField(
              decoration: const InputDecoration(
                hintText: 'Search members...',
                prefixIcon: Icon(Icons.search),
                border: OutlineInputBorder(),
              ),
              onChanged: (query) => context
                  .read<MemberBloc>()
                  .add(MemberEvent.search(hubId: hubId, query: query)),
            ),
          ),
          Expanded(
            child: BlocBuilder<MemberBloc, MemberState>(
              builder: (context, state) {
                return state.when(
                  loading: () =>
                      const Center(child: CircularProgressIndicator()),
                  loaded: (members, _) => _MemberList(
                    hubId: hubId,
                    members: members,
                  ),
                  error: (message) => Center(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text('Error: $message'),
                        const SizedBox(height: 16),
                        ElevatedButton(
                          onPressed: () => context
                              .read<MemberBloc>()
                              .add(MemberEvent.load(hubId: hubId)),
                          child: const Text('Retry'),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}

class _MemberList extends StatelessWidget {
  const _MemberList({required this.hubId, required this.members});

  final String hubId;
  final List<HubMember> members;

  @override
  Widget build(BuildContext context) {
    if (members.isEmpty) {
      return const Center(child: Text('No members found'));
    }
    return ListView.builder(
      itemCount: members.length,
      itemBuilder: (context, index) {
        final member = members[index];
        return Dismissible(
          key: Key(member.id),
          direction: DismissDirection.endToStart,
          background: Container(
            alignment: Alignment.centerRight,
            padding: const EdgeInsets.only(right: 16),
            color: Colors.red,
            child: const Icon(Icons.delete, color: Colors.white),
          ),
          confirmDismiss: (_) => _confirmRemove(context, member),
          onDismissed: (_) => context
              .read<MemberBloc>()
              .add(MemberEvent.removeMember(
                hubId: hubId,
                memberId: member.id,
              )),
          child: _MemberTile(member: member),
        );
      },
    );
  }

  Future<bool> _confirmRemove(BuildContext context, HubMember member) async {
    return await showDialog<bool>(
          context: context,
          builder: (ctx) => AlertDialog(
            title: const Text('Remove Member'),
            content: Text('Remove ${member.displayName} from this hub?'),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(ctx).pop(false),
                child: const Text('Cancel'),
              ),
              TextButton(
                onPressed: () => Navigator.of(ctx).pop(true),
                child: const Text('Remove', style: TextStyle(color: Colors.red)),
              ),
            ],
          ),
        ) ??
        false;
  }
}

class _MemberTile extends StatelessWidget {
  const _MemberTile({required this.member});

  final HubMember member;

  @override
  Widget build(BuildContext context) {
    final role = UserRole.fromString(member.role);
    return ListTile(
      leading: CircleAvatar(
        backgroundImage:
            member.avatar != null ? NetworkImage(member.avatar!) : null,
        child: member.avatar == null
            ? Text(member.displayName[0].toUpperCase())
            : null,
      ),
      title: Text(member.displayName),
      subtitle: Text(role.label),
      trailing: _RoleBadge(role: role),
    );
  }
}

class _RoleBadge extends StatelessWidget {
  const _RoleBadge({required this.role});

  final UserRole role;

  Color get _color {
    switch (role) {
      case UserRole.hubManager:
        return Colors.purple;
      case UserRole.listOwner:
        return Colors.blue;
      case UserRole.user:
        return Colors.green;
      case UserRole.visitor:
        return Colors.orange;
      case UserRole.anonymous:
        return Colors.grey;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: _color.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: _color),
      ),
      child: Text(
        role.label,
        style: TextStyle(
          color: _color,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }
}
