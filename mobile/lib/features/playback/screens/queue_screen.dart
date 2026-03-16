import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/queue_bloc.dart';
import '../bloc/queue_event.dart';
import '../bloc/queue_state.dart';
import '../models/queue_models.dart';
import '../widgets/now_playing_widget.dart';

class QueueScreen extends StatelessWidget {
  const QueueScreen({super.key, required this.hubId});

  final String hubId;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Live Queue'),
      ),
      body: BlocConsumer<QueueBloc, QueueState>(
        listener: (context, state) {
          if (state is QueueStateError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Theme.of(context).colorScheme.error,
                action: SnackBarAction(
                  label: 'Retry',
                  onPressed: () {
                    context.read<QueueBloc>().add(
                          QueueEvent.connectToQueue(hubId: hubId),
                        );
                  },
                ),
              ),
            );
          }
        },
        builder: (context, state) {
          return state.when(
            initial: () => const Center(
              child: Text('Connecting to queue...'),
            ),
            connecting: () => const Center(
              child: CircularProgressIndicator(),
            ),
            loaded: (entries, nowPlaying) => _QueueLoadedView(
              hubId: hubId,
              entries: entries,
              nowPlaying: nowPlaying,
            ),
            error: (message) => _QueueErrorView(
              message: message,
              onRetry: () {
                context.read<QueueBloc>().add(
                      QueueEvent.connectToQueue(hubId: hubId),
                    );
              },
            ),
          );
        },
      ),
    );
  }
}

class _QueueLoadedView extends StatelessWidget {
  const _QueueLoadedView({
    required this.hubId,
    required this.entries,
    this.nowPlaying,
  });

  final String hubId;
  final List<QueueEntryDto> entries;
  final NowPlayingDto? nowPlaying;

  @override
  Widget build(BuildContext context) {
    if (entries.isEmpty && nowPlaying == null) {
      return const _QueueEmptyView();
    }

    return RefreshIndicator(
      onRefresh: () async {
        context.read<QueueBloc>().add(QueueEvent.refresh(hubId: hubId));
      },
      child: CustomScrollView(
        slivers: [
          if (nowPlaying != null)
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: NowPlayingWidget(
                  trackTitle: nowPlaying!.trackTitle,
                  trackArtist: nowPlaying!.trackArtist,
                  trackAlbumArt: nowPlaying!.trackAlbumArt,
                  albumName: nowPlaying!.albumName,
                  durationMs: nowPlaying!.durationMs,
                  elapsedMs: nowPlaying!.elapsedMs,
                  proposerName: nowPlaying!.proposerName,
                  startedAt: nowPlaying!.startedAt,
                  compact: false,
                ),
              ),
            ),
          if (entries.isNotEmpty)
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 8, 16, 4),
                child: Text(
                  'Up Next',
                  style: Theme.of(context).textTheme.titleMedium,
                ),
              ),
            ),
          SliverList(
            delegate: SliverChildBuilderDelegate(
              (context, index) => _QueueEntryTile(entry: entries[index]),
              childCount: entries.length,
            ),
          ),
        ],
      ),
    );
  }
}

class _QueueEntryTile extends StatelessWidget {
  const _QueueEntryTile({required this.entry});

  final QueueEntryDto entry;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final duration = Duration(milliseconds: entry.durationMs);
    final minutes = duration.inMinutes;
    final seconds = duration.inSeconds.remainder(60);
    final durationText = '$minutes:${seconds.toString().padLeft(2, '0')}';

    return ListTile(
      leading: _buildLeading(theme),
      title: Text(
        entry.trackTitle,
        maxLines: 1,
        overflow: TextOverflow.ellipsis,
      ),
      subtitle: Text(
        entry.trackArtist,
        maxLines: 1,
        overflow: TextOverflow.ellipsis,
      ),
      trailing: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            durationText,
            style: theme.textTheme.bodySmall,
          ),
          const SizedBox(width: 8),
          _StatusBadge(status: entry.status),
        ],
      ),
    );
  }

  Widget _buildLeading(ThemeData theme) {
    return SizedBox(
      width: 48,
      child: Row(
        children: [
          SizedBox(
            width: 20,
            child: Text(
              '${entry.position}',
              style: theme.textTheme.bodySmall?.copyWith(
                fontWeight: FontWeight.bold,
              ),
              textAlign: TextAlign.center,
            ),
          ),
          const SizedBox(width: 4),
          ClipRRect(
            borderRadius: BorderRadius.circular(4),
            child: entry.trackAlbumArt != null
                ? Image.network(
                    entry.trackAlbumArt!,
                    width: 24,
                    height: 24,
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => const _AlbumPlaceholder(),
                  )
                : const _AlbumPlaceholder(),
          ),
        ],
      ),
    );
  }
}

class _AlbumPlaceholder extends StatelessWidget {
  const _AlbumPlaceholder();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 24,
      height: 24,
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(4),
      ),
      child: Icon(
        Icons.music_note,
        size: 14,
        color: Theme.of(context).colorScheme.onSurfaceVariant,
      ),
    );
  }
}

class _StatusBadge extends StatelessWidget {
  const _StatusBadge({required this.status});

  final QueueEntryStatus status;

  @override
  Widget build(BuildContext context) {
    final (label, color) = switch (status) {
      QueueEntryStatus.pending => ('Pending', Colors.orange),
      QueueEntryStatus.playing => ('Playing', Colors.green),
      QueueEntryStatus.played => ('Played', Colors.grey),
      QueueEntryStatus.skipped => ('Skipped', Colors.red),
    };

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(4),
      ),
      child: Text(
        label,
        style: Theme.of(context).textTheme.labelSmall?.copyWith(
              color: color,
              fontWeight: FontWeight.w600,
            ),
      ),
    );
  }
}

class _QueueEmptyView extends StatelessWidget {
  const _QueueEmptyView();

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            Icons.queue_music,
            size: 64,
            color: Theme.of(context).colorScheme.onSurfaceVariant,
          ),
          const SizedBox(height: 16),
          Text(
            'Queue is empty',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          Text(
            'Propose a track to get the music started!',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
          ),
        ],
      ),
    );
  }
}

class _QueueErrorView extends StatelessWidget {
  const _QueueErrorView({
    required this.message,
    required this.onRetry,
  });

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
            size: 64,
            color: Theme.of(context).colorScheme.error,
          ),
          const SizedBox(height: 16),
          Text(
            'Connection Error',
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
          const SizedBox(height: 16),
          FilledButton.icon(
            onPressed: onRetry,
            icon: const Icon(Icons.refresh),
            label: const Text('Retry'),
          ),
        ],
      ),
    );
  }
}
