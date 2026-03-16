import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/search_bloc.dart';
import '../bloc/search_event.dart';
import '../bloc/search_state.dart';
import '../models/search_models.dart';

class TrackSearchScreen extends StatelessWidget {
  const TrackSearchScreen({super.key, required this.hubId});

  final String hubId;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Search Tracks'),
      ),
      body: BlocConsumer<SearchBloc, SearchState>(
        listener: (context, state) {
          if (state.proposalSuccess != null) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.proposalSuccess!),
                backgroundColor: Colors.green,
              ),
            );
          }
          if (state.errorMessage != null && state.isProposing == false) {
            // Only show error snackbar for propose errors, not search errors
          }
        },
        builder: (context, state) {
          return Column(
            children: [
              _SearchBar(
                query: state.query,
                onChanged: (query) {
                  context.read<SearchBloc>().add(
                        SearchEvent.queryChanged(query: query),
                      );
                },
                onClear: () {
                  context.read<SearchBloc>().add(
                        const SearchEvent.clearSearch(),
                      );
                },
              ),
              _ProviderFilterChips(
                selectedProvider: state.provider,
                onChanged: (provider) {
                  context.read<SearchBloc>().add(
                        SearchEvent.providerFilterChanged(
                          provider: provider,
                        ),
                      );
                },
              ),
              Expanded(
                child: _buildBody(context, state),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildBody(BuildContext context, SearchState state) {
    if (state.query.isEmpty) {
      return const _SearchInitialView();
    }

    if (state.isLoading) {
      return const _SearchLoadingView();
    }

    if (state.errorMessage != null && !state.isProposing) {
      return _SearchErrorView(message: state.errorMessage!);
    }

    if (state.results.isEmpty) {
      return const _SearchEmptyView();
    }

    return _SearchResultsList(
      results: state.results,
      isProposing: state.isProposing,
      hubId: hubId,
    );
  }
}

class _SearchBar extends StatefulWidget {
  const _SearchBar({
    required this.query,
    required this.onChanged,
    required this.onClear,
  });

  final String query;
  final ValueChanged<String> onChanged;
  final VoidCallback onClear;

  @override
  State<_SearchBar> createState() => _SearchBarState();
}

class _SearchBarState extends State<_SearchBar> {
  late final TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.query);
  }

  @override
  void didUpdateWidget(covariant _SearchBar oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.query.isEmpty && _controller.text.isNotEmpty) {
      _controller.clear();
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
      child: TextField(
        controller: _controller,
        onChanged: widget.onChanged,
        decoration: InputDecoration(
          hintText: 'Search for songs, artists...',
          prefixIcon: const Icon(Icons.search),
          suffixIcon: _controller.text.isNotEmpty
              ? IconButton(
                  icon: const Icon(Icons.clear),
                  onPressed: () {
                    _controller.clear();
                    widget.onClear();
                  },
                )
              : null,
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          filled: true,
        ),
        textInputAction: TextInputAction.search,
      ),
    );
  }
}

class _ProviderFilterChips extends StatelessWidget {
  const _ProviderFilterChips({
    required this.selectedProvider,
    required this.onChanged,
  });

  final String? selectedProvider;
  final ValueChanged<String?> onChanged;

  static const _providers = [
    (null, 'All'),
    ('spotify', 'Spotify'),
    ('youtube_music', 'YouTube Music'),
  ];

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: Row(
          children: _providers.map((entry) {
            final (value, label) = entry;
            final isSelected = selectedProvider == value;
            return Padding(
              padding: const EdgeInsets.only(right: 8),
              child: FilterChip(
                label: Text(label),
                selected: isSelected,
                onSelected: (_) => onChanged(value),
              ),
            );
          }).toList(),
        ),
      ),
    );
  }
}

class _SearchResultsList extends StatelessWidget {
  const _SearchResultsList({
    required this.results,
    required this.isProposing,
    required this.hubId,
  });

  final List<TrackSearchResult> results;
  final bool isProposing;
  final String hubId;

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      itemCount: results.length,
      itemBuilder: (context, index) {
        final track = results[index];
        return _TrackResultTile(
          track: track,
          isProposing: isProposing,
          onPropose: () {
            context.read<SearchBloc>().add(
                  SearchEvent.proposeTrack(
                    hubId: hubId,
                    trackId: track.id,
                    source: track.provider,
                  ),
                );
          },
        );
      },
    );
  }
}

class _TrackResultTile extends StatelessWidget {
  const _TrackResultTile({
    required this.track,
    required this.isProposing,
    required this.onPropose,
  });

  final TrackSearchResult track;
  final bool isProposing;
  final VoidCallback onPropose;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final duration = Duration(milliseconds: track.durationMs);
    final minutes = duration.inMinutes;
    final seconds = duration.inSeconds.remainder(60);
    final durationText = '$minutes:${seconds.toString().padLeft(2, '0')}';

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
                    const _TrackArtPlaceholder(),
              )
            : const _TrackArtPlaceholder(),
      ),
      title: Text(
        track.title,
        maxLines: 1,
        overflow: TextOverflow.ellipsis,
      ),
      subtitle: Row(
        children: [
          Expanded(
            child: Text(
              track.artist,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ),
          Text(
            durationText,
            style: theme.textTheme.bodySmall?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
        ],
      ),
      trailing: isProposing
          ? const SizedBox(
              width: 24,
              height: 24,
              child: CircularProgressIndicator(strokeWidth: 2),
            )
          : IconButton(
              icon: const Icon(Icons.add_circle_outline),
              tooltip: 'Add to queue',
              onPressed: onPropose,
            ),
    );
  }
}

class _TrackArtPlaceholder extends StatelessWidget {
  const _TrackArtPlaceholder();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 48,
      height: 48,
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(6),
      ),
      child: Icon(
        Icons.music_note,
        color: Theme.of(context).colorScheme.onSurfaceVariant,
      ),
    );
  }
}

class _SearchInitialView extends StatelessWidget {
  const _SearchInitialView();

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            Icons.search,
            size: 64,
            color: Theme.of(context).colorScheme.onSurfaceVariant,
          ),
          const SizedBox(height: 16),
          Text(
            'Search for music',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          Text(
            'Find songs to add to the queue',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
          ),
        ],
      ),
    );
  }
}

class _SearchLoadingView extends StatelessWidget {
  const _SearchLoadingView();

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      itemCount: 8,
      itemBuilder: (context, index) {
        return const _ShimmerTile();
      },
    );
  }
}

class _ShimmerTile extends StatelessWidget {
  const _ShimmerTile();

  @override
  Widget build(BuildContext context) {
    final shimmerColor =
        Theme.of(context).colorScheme.surfaceContainerHighest;
    return ListTile(
      leading: Container(
        width: 48,
        height: 48,
        decoration: BoxDecoration(
          color: shimmerColor,
          borderRadius: BorderRadius.circular(6),
        ),
      ),
      title: Container(
        height: 14,
        width: double.infinity,
        decoration: BoxDecoration(
          color: shimmerColor,
          borderRadius: BorderRadius.circular(4),
        ),
      ),
      subtitle: Container(
        height: 12,
        width: 120,
        margin: const EdgeInsets.only(top: 4),
        decoration: BoxDecoration(
          color: shimmerColor,
          borderRadius: BorderRadius.circular(4),
        ),
      ),
    );
  }
}

class _SearchEmptyView extends StatelessWidget {
  const _SearchEmptyView();

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            Icons.music_off,
            size: 64,
            color: Theme.of(context).colorScheme.onSurfaceVariant,
          ),
          const SizedBox(height: 16),
          Text(
            'No results found',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          Text(
            'Try a different search term',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
          ),
        ],
      ),
    );
  }
}

class _SearchErrorView extends StatelessWidget {
  const _SearchErrorView({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            Icons.error_outline,
            size: 64,
            color: Theme.of(context).colorScheme.error,
          ),
          const SizedBox(height: 16),
          Text(
            'Search Error',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 32),
            child: Text(
              message,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodyMedium,
            ),
          ),
        ],
      ),
    );
  }
}
