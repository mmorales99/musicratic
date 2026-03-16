import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../../shared/models/hub.dart';
import '../bloc/hub_search_bloc.dart';
import '../bloc/hub_search_event.dart';
import '../bloc/hub_search_state.dart';
import '../widgets/hub_card.dart';
import '../widgets/hub_filter_chips.dart';

class HubScreen extends StatefulWidget {
  const HubScreen({super.key});

  @override
  State<HubScreen> createState() => _HubScreenState();
}

class _HubScreenState extends State<HubScreen> {
  final _searchController = TextEditingController();
  final _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    context.read<HubSearchBloc>().add(const HubSearchEvent.refresh());
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    _searchController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  void _onScroll() {
    if (!_scrollController.hasClients) return;
    final maxScroll = _scrollController.position.maxScrollExtent;
    final currentScroll = _scrollController.offset;
    if (currentScroll >= maxScroll - 200) {
      context.read<HubSearchBloc>().add(const HubSearchEvent.loadMore());
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Discover Hubs')),
      body: Column(
        children: [
          _SearchBar(controller: _searchController),
          BlocBuilder<HubSearchBloc, HubSearchState>(
            buildWhen: (prev, curr) =>
                prev.businessType != curr.businessType ||
                prev.visibility != curr.visibility,
            builder: (context, state) {
              return HubFilterChips(
                selectedBusinessType: state.businessType,
                selectedVisibility: state.visibility,
                onBusinessTypeChanged: (type) {
                  context.read<HubSearchBloc>().add(
                        HubSearchEvent.filterChanged(businessType: type),
                      );
                },
                onVisibilityChanged: (vis) {
                  context.read<HubSearchBloc>().add(
                        HubSearchEvent.filterChanged(visibility: vis),
                      );
                },
              );
            },
          ),
          const SizedBox(height: 8),
          Expanded(
            child: BlocBuilder<HubSearchBloc, HubSearchState>(
              builder: (context, state) {
                if (state.isLoading && state.hubs.isEmpty) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (state.errorMessage != null && state.hubs.isEmpty) {
                  return _ErrorView(
                    message: state.errorMessage!,
                    onRetry: () => context
                        .read<HubSearchBloc>()
                        .add(const HubSearchEvent.refresh()),
                  );
                }
                if (state.hubs.isEmpty) {
                  return const _EmptyView();
                }
                return _HubListView(
                  hubs: state.hubs,
                  scrollController: _scrollController,
                  isLoadingMore: state.isLoadingMore,
                  onRefresh: () async {
                    context
                        .read<HubSearchBloc>()
                        .add(const HubSearchEvent.refresh());
                  },
                );
              },
            ),
          ),
        ],
      ),
      floatingActionButton: _HubFab(),
    );
  }
}

class _SearchBar extends StatelessWidget {
  const _SearchBar({required this.controller});

  final TextEditingController controller;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
      child: TextField(
        controller: controller,
        decoration: InputDecoration(
          hintText: 'Search hubs...',
          prefixIcon: const Icon(Icons.search),
          suffixIcon: BlocBuilder<HubSearchBloc, HubSearchState>(
            buildWhen: (prev, curr) => prev.query != curr.query,
            builder: (context, state) {
              if (state.query.isEmpty) return const SizedBox.shrink();
              return IconButton(
                icon: const Icon(Icons.clear),
                onPressed: () {
                  controller.clear();
                  context.read<HubSearchBloc>().add(
                        const HubSearchEvent.searchChanged(query: ''),
                      );
                },
              );
            },
          ),
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          filled: true,
          fillColor: Theme.of(context).colorScheme.surfaceContainerHighest,
        ),
        onChanged: (value) {
          context
              .read<HubSearchBloc>()
              .add(HubSearchEvent.searchChanged(query: value));
        },
      ),
    );
  }
}

class _HubListView extends StatelessWidget {
  const _HubListView({
    required this.hubs,
    required this.scrollController,
    required this.isLoadingMore,
    required this.onRefresh,
  });

  final List<Hub> hubs;
  final ScrollController scrollController;
  final bool isLoadingMore;
  final Future<void> Function() onRefresh;

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: onRefresh,
      child: ListView.builder(
        controller: scrollController,
        padding: const EdgeInsets.only(bottom: 80),
        itemCount: hubs.length + (isLoadingMore ? 1 : 0),
        itemBuilder: (context, index) {
          if (index >= hubs.length) {
            return const Padding(
              padding: EdgeInsets.all(16),
              child: Center(child: CircularProgressIndicator()),
            );
          }
          final hub = hubs[index];
          return HubCard(
            hub: hub,
            onTap: () => context.go('/hub/${hub.id}'),
          );
        },
      ),
    );
  }
}

class _EmptyView extends StatelessWidget {
  const _EmptyView();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.music_off_outlined,
              size: 80,
              color: theme.colorScheme.outline,
            ),
            const SizedBox(height: 16),
            Text(
              'No hubs found',
              style: theme.textTheme.titleLarge,
            ),
            const SizedBox(height: 8),
            Text(
              'Try adjusting your search or filters,\nor create a new hub!',
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }
}

class _ErrorView extends StatelessWidget {
  const _ErrorView({required this.message, required this.onRetry});

  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            Icons.error_outline,
            size: 48,
            color: Theme.of(context).colorScheme.error,
          ),
          const SizedBox(height: 16),
          Text('Error: $message'),
          const SizedBox(height: 16),
          FilledButton(onPressed: onRetry, child: const Text('Retry')),
        ],
      ),
    );
  }
}

class _HubFab extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        FloatingActionButton.small(
          heroTag: 'join',
          onPressed: () => context.go('/hub/join'),
          child: const Icon(Icons.qr_code_scanner),
        ),
        const SizedBox(height: 8),
        FloatingActionButton.extended(
          heroTag: 'create',
          onPressed: () => context.go('/hub/create'),
          icon: const Icon(Icons.add),
          label: const Text('Create Hub'),
        ),
      ],
    );
  }
}
