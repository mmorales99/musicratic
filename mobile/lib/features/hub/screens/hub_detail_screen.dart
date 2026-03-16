import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../bloc/hub_detail_bloc.dart';
import '../bloc/hub_detail_event.dart';
import '../bloc/hub_detail_state.dart';
import '../../../shared/models/hub.dart';
import '../models/list_models.dart';

class HubDetailScreen extends StatelessWidget {
  const HubDetailScreen({super.key, required this.hubId});

  final String hubId;

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<HubDetailBloc, HubDetailState>(
      listener: (context, state) {
        if (state is HubDetailStateDeleted) {
          context.go('/hub');
        }
        if (state is HubDetailStateError) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(state.message),
              backgroundColor: Theme.of(context).colorScheme.error,
            ),
          );
        }
      },
      builder: (context, state) {
        return state.when(
          initial: () => const Scaffold(
            body: Center(child: CircularProgressIndicator()),
          ),
          loading: () => const Scaffold(
            body: Center(child: CircularProgressIndicator()),
          ),
          loaded: (hub, settings, lists) => _LoadedView(
            hubId: hubId,
            hub: hub,
            settings: settings,
            lists: lists,
          ),
          acting: (hub, action) => Scaffold(
            appBar: AppBar(title: Text(hub.name)),
            body: Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const CircularProgressIndicator(),
                  const SizedBox(height: 16),
                  Text('$action...'),
                ],
              ),
            ),
          ),
          deleted: () => const Scaffold(
            body: Center(child: Text('Hub deleted')),
          ),
          error: (message) => Scaffold(
            appBar: AppBar(title: const Text('Hub')),
            body: Center(child: Text('Error: $message')),
          ),
        );
      },
    );
  }
}

class _LoadedView extends StatelessWidget {
  const _LoadedView({
    required this.hubId,
    required this.hub,
    required this.settings,
    required this.lists,
  });

  final String hubId;
  final Hub hub;
  final HubSettings settings;
  final List<HubList> lists;

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 3,
      child: Scaffold(
        appBar: AppBar(
          title: Text(hub.name),
          actions: [_buildActionsMenu(context)],
          bottom: const TabBar(
            tabs: [
              Tab(text: 'Info'),
              Tab(text: 'Members'),
              Tab(text: 'Lists'),
            ],
          ),
        ),
        body: RefreshIndicator(
          onRefresh: () async {
            context
                .read<HubDetailBloc>()
                .add(HubDetailEvent.refresh(hubId: hubId));
          },
          child: TabBarView(
            children: [
              _InfoTab(hub: hub, settings: settings),
              const _MembersTab(),
              _ListsTab(hubId: hubId, lists: lists),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildActionsMenu(BuildContext context) {
    return PopupMenuButton<String>(
      onSelected: (action) => _handleAction(context, action),
      itemBuilder: (context) => [
        if (!hub.isActive)
          const PopupMenuItem(value: 'activate', child: Text('Activate')),
        if (hub.isActive && !hub.isPaused)
          const PopupMenuItem(value: 'pause', child: Text('Pause')),
        if (hub.isPaused)
          const PopupMenuItem(value: 'resume', child: Text('Resume')),
        if (hub.isActive)
          const PopupMenuItem(value: 'deactivate', child: Text('Deactivate')),
        const PopupMenuDivider(),
        const PopupMenuItem(
          value: 'delete',
          child: Text('Delete', style: TextStyle(color: Colors.red)),
        ),
      ],
    );
  }

  void _handleAction(BuildContext context, String action) {
    switch (action) {
      case 'activate':
        context
            .read<HubDetailBloc>()
            .add(HubDetailEvent.activate(hubId: hubId));
      case 'deactivate':
        context
            .read<HubDetailBloc>()
            .add(HubDetailEvent.deactivate(hubId: hubId));
      case 'pause':
        context
            .read<HubDetailBloc>()
            .add(HubDetailEvent.pause(hubId: hubId));
      case 'resume':
        context
            .read<HubDetailBloc>()
            .add(HubDetailEvent.resume(hubId: hubId));
      case 'delete':
        _showDeleteConfirmation(context);
    }
  }

  void _showDeleteConfirmation(BuildContext context) {
    showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Delete Hub'),
        content: Text('Are you sure you want to delete "${hub.name}"?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: const Text('Cancel'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(
              backgroundColor: Theme.of(context).colorScheme.error,
            ),
            onPressed: () {
              Navigator.of(dialogContext).pop();
              context
                  .read<HubDetailBloc>()
                  .add(HubDetailEvent.delete(hubId: hubId));
            },
            child: const Text('Delete'),
          ),
        ],
      ),
    );
  }
}

class _InfoTab extends StatelessWidget {
  const _InfoTab({required this.hub, required this.settings});

  final Hub hub;
  final HubSettings settings;

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        _buildStatusCard(context),
        const SizedBox(height: 12),
        _buildCodeCard(context),
        const SizedBox(height: 12),
        _buildSettingsCard(context),
      ],
    );
  }

  Widget _buildStatusCard(BuildContext context) {
    final statusText = hub.isPaused
        ? 'Paused'
        : hub.isActive
            ? 'Active'
            : 'Inactive';
    final statusColor = hub.isPaused
        ? Colors.orange
        : hub.isActive
            ? Colors.green
            : Colors.grey;

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.circle, color: statusColor, size: 12),
                const SizedBox(width: 8),
                Text(statusText,
                    style: Theme.of(context).textTheme.titleMedium),
                const Spacer(),
                Chip(label: Text(hub.hubType)),
              ],
            ),
            if (hub.description != null) ...[
              const SizedBox(height: 8),
              Text(hub.description!),
            ],
            const SizedBox(height: 8),
            Text(
              'Visibility: ${hub.visibility}',
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCodeCard(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Hub Code', style: Theme.of(context).textTheme.titleSmall),
            const SizedBox(height: 8),
            if (hub.hubCode != null)
              Row(
                children: [
                  Text(
                    hub.hubCode!,
                    style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                          fontFamily: 'monospace',
                          fontWeight: FontWeight.bold,
                        ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.copy, size: 20),
                    onPressed: () {
                      Clipboard.setData(ClipboardData(text: hub.hubCode!));
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Code copied')),
                      );
                    },
                  ),
                ],
              ),
            if (hub.qrUrl != null) ...[
              const SizedBox(height: 8),
              Center(
                child: Container(
                  width: 200,
                  height: 200,
                  decoration: BoxDecoration(
                    border: Border.all(color: Colors.grey.shade300),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: const Center(
                    child: Icon(Icons.qr_code_2, size: 120),
                  ),
                ),
              ),
            ],
            if (hub.directLink != null) ...[
              const SizedBox(height: 8),
              Row(
                children: [
                  Expanded(
                    child: Text(
                      hub.directLink!,
                      style: Theme.of(context).textTheme.bodySmall,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.share, size: 20),
                    onPressed: () {
                      Clipboard.setData(ClipboardData(text: hub.directLink!));
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Link copied')),
                      );
                    },
                  ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildSettingsCard(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Settings', style: Theme.of(context).textTheme.titleSmall),
            const SizedBox(height: 8),
            _settingRow('Skip threshold',
                '${(settings.voteSkipThreshold * 100).round()}%'),
            _settingRow('Voting window',
                '${settings.votingWindowSeconds}s'),
            _settingRow('Max queue depth',
                '${settings.maxQueueDepth}'),
            _settingRow('Min votes',
                '${settings.minVoteCount}'),
            _settingRow('Proposal cooldown',
                '${settings.proposalCooldownMinutes} min'),
            _settingRow('Proposals',
                settings.proposalsEnabled ? 'Enabled' : 'Disabled'),
          ],
        ),
      ),
    );
  }

  Widget _settingRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [Text(label), Text(value)],
      ),
    );
  }
}

class _MembersTab extends StatelessWidget {
  const _MembersTab();

  @override
  Widget build(BuildContext context) {
    return const Center(
      child: Text('Member management coming soon'),
    );
  }
}

class _ListsTab extends StatelessWidget {
  const _ListsTab({required this.hubId, required this.lists});

  final String hubId;
  final List<HubList> lists;

  @override
  Widget build(BuildContext context) {
    if (lists.isEmpty) {
      return const Center(child: Text('No lists yet'));
    }
    return ListView.builder(
      padding: const EdgeInsets.all(8),
      itemCount: lists.length,
      itemBuilder: (context, index) {
        final list = lists[index];
        return Card(
          child: ListTile(
            leading: const Icon(Icons.queue_music),
            title: Text(list.name),
            subtitle: Text(
              '${list.trackCount} tracks • ${list.playMode}',
            ),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.go('/hub/$hubId/lists/${list.id}'),
          ),
        );
      },
    );
  }
}
