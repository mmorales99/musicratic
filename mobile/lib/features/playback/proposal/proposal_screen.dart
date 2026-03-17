import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../models/search_models.dart';
import 'proposal_bloc.dart';
import 'proposal_confirm_sheet.dart';
import 'proposal_event.dart';
import 'proposal_state.dart';

class ProposalScreen extends StatelessWidget {
  const ProposalScreen({super.key, required this.hubId});

  final String hubId;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Propose a Track')),
      body: BlocConsumer<ProposalBloc, ProposalState>(
        listener: _handleStateChange,
        builder: (context, state) {
          return Column(
            children: [
              _SearchInput(hubId: hubId),
              _ProviderTabs(hubId: hubId, state: state),
              Expanded(child: _buildContent(context, state)),
            ],
          );
        },
      ),
    );
  }

  void _handleStateChange(BuildContext context, ProposalState state) {
    state.maybeWhen(
      selected: (track, coinCost, currentBalance) {
        _showConfirmSheet(context, track, coinCost, currentBalance);
      },
      proposing: (_) {},
      success: (message) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(message),
            backgroundColor: Colors.green,
          ),
        );
        context.pop();
      },
      error: (message, _) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(message),
            backgroundColor: Theme.of(context).colorScheme.error,
          ),
        );
      },
      orElse: () {},
    );
  }

  void _showConfirmSheet(
    BuildContext context,
    TrackSearchResult track,
    int coinCost,
    int currentBalance,
  ) {
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      builder: (_) => ProposalConfirmSheet(
        track: track,
        coinCost: coinCost,
        currentBalance: currentBalance,
        onConfirm: () {
          Navigator.of(context).pop();
          context.read<ProposalBloc>().add(
                ProposalEvent.confirmProposal(
                  hubId: hubId,
                  trackId: track.id,
                  providerId: track.provider,
                ),
              );
        },
        onCancel: () {
          Navigator.of(context).pop();
          context.read<ProposalBloc>().add(
                const ProposalEvent.cancelProposal(),
              );
        },
        onBuyCoins: () {
          Navigator.of(context).pop();
          context.pushNamed('economy-purchase');
        },
      ),
    );
  }

  Widget _buildContent(BuildContext context, ProposalState state) {
    return state.when(
      initial: () => const _EmptyView(),
      searching: (_, __) => const _LoadingView(),
      results: (_, __, tracks) => _ResultsList(
        tracks: tracks,
        hubId: hubId,
      ),
      selected: (_, __, ___) => const SizedBox.shrink(),
      proposing: (_) => const _ProposingView(),
      success: (_) => const SizedBox.shrink(),
      error: (message, previousState) => _ErrorView(message: message),
    );
  }
}

class _SearchInput extends StatefulWidget {
  const _SearchInput({required this.hubId});

  final String hubId;

  @override
  State<_SearchInput> createState() => _SearchInputState();
}

class _SearchInputState extends State<_SearchInput> {
  late final TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: TextField(
        controller: _controller,
        decoration: InputDecoration(
          hintText: 'Search for a track...',
          prefixIcon: const Icon(Icons.search),
          suffixIcon: _controller.text.isNotEmpty
              ? IconButton(
                  icon: const Icon(Icons.clear),
                  onPressed: () {
                    _controller.clear();
                    context.read<ProposalBloc>().add(
                          const ProposalEvent.searchTracks(query: ''),
                        );
                  },
                )
              : null,
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
        onChanged: (query) {
          context.read<ProposalBloc>().add(
                ProposalEvent.searchTracks(query: query),
              );
        },
      ),
    );
  }
}

class _ProviderTabs extends StatelessWidget {
  const _ProviderTabs({required this.hubId, required this.state});

  final String hubId;
  final ProposalState state;

  String? get _currentProvider => state.maybeWhen(
        searching: (_, provider) => provider,
        results: (_, provider, __) => provider,
        orElse: () => null,
      );

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Row(
        children: [
          _ProviderChip(
            label: 'All',
            isSelected: _currentProvider == null,
            onSelected: () => _setProvider(context, null),
          ),
          const SizedBox(width: 8),
          _ProviderChip(
            label: 'Spotify',
            isSelected: _currentProvider == 'spotify',
            onSelected: () => _setProvider(context, 'spotify'),
          ),
          const SizedBox(width: 8),
          _ProviderChip(
            label: 'YouTube',
            isSelected: _currentProvider == 'youtube',
            onSelected: () => _setProvider(context, 'youtube'),
          ),
        ],
      ),
    );
  }

  void _setProvider(BuildContext context, String? provider) {
    final query = state.maybeWhen(
      searching: (q, _) => q,
      results: (q, _, __) => q,
      orElse: () => '',
    );
    if (query.isNotEmpty) {
      context.read<ProposalBloc>().add(
            ProposalEvent.searchTracks(
              query: query,
              provider: provider,
            ),
          );
    }
  }
}

class _ProviderChip extends StatelessWidget {
  const _ProviderChip({
    required this.label,
    required this.isSelected,
    required this.onSelected,
  });

  final String label;
  final bool isSelected;
  final VoidCallback onSelected;

  @override
  Widget build(BuildContext context) {
    return FilterChip(
      label: Text(label),
      selected: isSelected,
      onSelected: (_) => onSelected(),
    );
  }
}

class _ResultsList extends StatelessWidget {
  const _ResultsList({required this.tracks, required this.hubId});

  final List<TrackSearchResult> tracks;
  final String hubId;

  @override
  Widget build(BuildContext context) {
    if (tracks.isEmpty) {
      return const Center(
        child: Text('No tracks found. Try a different search.'),
      );
    }

    return ListView.builder(
      padding: const EdgeInsets.symmetric(vertical: 8),
      itemCount: tracks.length,
      itemBuilder: (context, index) => _TrackCard(
        track: tracks[index],
        onPropose: () {
          context.read<ProposalBloc>().add(
                ProposalEvent.selectTrack(trackId: tracks[index].id),
              );
        },
      ),
    );
  }
}

class _TrackCard extends StatelessWidget {
  const _TrackCard({required this.track, required this.onPropose});

  final TrackSearchResult track;
  final VoidCallback onPropose;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ListTile(
      leading: ClipRRect(
        borderRadius: BorderRadius.circular(6),
        child: track.albumArt != null
            ? Image.network(
                track.albumArt!,
                width: 48,
                height: 48,
                fit: BoxFit.cover,
                errorBuilder: (_, __, ___) =>
                    const _Placeholder(),
              )
            : const _Placeholder(),
      ),
      title: Text(
        track.title,
        maxLines: 1,
        overflow: TextOverflow.ellipsis,
      ),
      subtitle: Text(
        '${track.artist} · ${_formatDuration(track.durationMs)}',
        style: theme.textTheme.bodySmall,
        maxLines: 1,
        overflow: TextOverflow.ellipsis,
      ),
      trailing: FilledButton.tonal(
        onPressed: onPropose,
        child: const Text('Propose'),
      ),
    );
  }

  String _formatDuration(int ms) {
    final duration = Duration(milliseconds: ms);
    final minutes = duration.inMinutes;
    final seconds = duration.inSeconds.remainder(60);
    return '$minutes:${seconds.toString().padLeft(2, '0')}';
  }
}

class _Placeholder extends StatelessWidget {
  const _Placeholder();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 48,
      height: 48,
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(6),
      ),
      child: const Icon(Icons.music_note, size: 24),
    );
  }
}

class _EmptyView extends StatelessWidget {
  const _EmptyView();

  @override
  Widget build(BuildContext context) {
    return const Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.search, size: 64, color: Colors.grey),
          SizedBox(height: 16),
          Text('Search for a track to propose'),
        ],
      ),
    );
  }
}

class _LoadingView extends StatelessWidget {
  const _LoadingView();

  @override
  Widget build(BuildContext context) {
    return const Center(child: CircularProgressIndicator());
  }
}

class _ProposingView extends StatelessWidget {
  const _ProposingView();

  @override
  Widget build(BuildContext context) {
    return const Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          CircularProgressIndicator(),
          SizedBox(height: 16),
          Text('Proposing track...'),
        ],
      ),
    );
  }
}

class _ErrorView extends StatelessWidget {
  const _ErrorView({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.error_outline,
              size: 48,
              color: Theme.of(context).colorScheme.error,
            ),
            const SizedBox(height: 16),
            Text(
              message,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}
