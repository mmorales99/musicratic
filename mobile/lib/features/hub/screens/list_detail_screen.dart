import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/list_bloc.dart';
import '../bloc/list_event.dart';
import '../bloc/list_state.dart';
import '../models/list_models.dart';

class ListDetailScreen extends StatelessWidget {
  const ListDetailScreen({super.key, required this.listId});

  final String listId;

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<ListBloc, ListState>(
      listener: (context, state) {
        if (state is ListStateError) {
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
          loaded: (list, tracks) => _LoadedView(
            listId: listId,
            list: list,
            tracks: tracks,
          ),
          error: (message) => Scaffold(
            appBar: AppBar(title: const Text('List')),
            body: Center(child: Text('Error: $message')),
          ),
        );
      },
    );
  }
}

class _LoadedView extends StatelessWidget {
  const _LoadedView({
    required this.listId,
    required this.list,
    required this.tracks,
  });

  final String listId;
  final HubList list;
  final List<ListTrack> tracks;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(list.name),
        actions: [
          IconButton(
            icon: Icon(
              list.playMode == 'shuffle' ? Icons.shuffle : Icons.repeat,
            ),
            tooltip: 'Play mode: ${list.playMode}',
            onPressed: () => context
                .read<ListBloc>()
                .add(ListEvent.togglePlayMode(listId: listId)),
          ),
        ],
      ),
      body: tracks.isEmpty
          ? const Center(child: Text('No tracks in this list'))
          : _TrackList(listId: listId, tracks: tracks),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _showAddTrackDialog(context),
        icon: const Icon(Icons.add),
        label: const Text('Add Track'),
      ),
    );
  }

  void _showAddTrackDialog(BuildContext context) {
    final controller = TextEditingController();
    showDialog<void>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Add Track'),
        content: TextField(
          controller: controller,
          decoration: const InputDecoration(
            labelText: 'Track ID',
            hintText: 'Enter track ID or search',
            border: OutlineInputBorder(),
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () {
              final trackId = controller.text.trim();
              if (trackId.isNotEmpty) {
                context.read<ListBloc>().add(
                      ListEvent.addTrack(listId: listId, trackId: trackId),
                    );
                Navigator.of(dialogContext).pop();
              }
            },
            child: const Text('Add'),
          ),
        ],
      ),
    );
  }
}

class _TrackList extends StatelessWidget {
  const _TrackList({required this.listId, required this.tracks});

  final String listId;
  final List<ListTrack> tracks;

  @override
  Widget build(BuildContext context) {
    return ReorderableListView.builder(
      padding: const EdgeInsets.only(bottom: 80),
      itemCount: tracks.length,
      onReorder: (oldIndex, newIndex) => _onReorder(context, oldIndex, newIndex),
      itemBuilder: (context, index) {
        final track = tracks[index];
        return Dismissible(
          key: ValueKey(track.id),
          direction: DismissDirection.endToStart,
          background: Container(
            alignment: Alignment.centerRight,
            padding: const EdgeInsets.only(right: 16),
            color: Theme.of(context).colorScheme.error,
            child: const Icon(Icons.delete, color: Colors.white),
          ),
          confirmDismiss: (_) => _confirmDelete(context, track),
          onDismissed: (_) {
            context.read<ListBloc>().add(
                  ListEvent.removeTrack(listId: listId, trackId: track.id),
                );
          },
          child: _TrackTile(track: track, index: index),
        );
      },
    );
  }

  void _onReorder(BuildContext context, int oldIndex, int newIndex) {
    final reordered = List<ListTrack>.from(tracks);
    if (newIndex > oldIndex) newIndex--;
    final item = reordered.removeAt(oldIndex);
    reordered.insert(newIndex, item);
    final trackIds = reordered.map((t) => t.id).toList();
    context
        .read<ListBloc>()
        .add(ListEvent.reorderTracks(listId: listId, trackIds: trackIds));
  }

  Future<bool> _confirmDelete(BuildContext context, ListTrack track) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Remove Track'),
        content: Text('Remove "${track.title}" from this list?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(
              backgroundColor: Theme.of(context).colorScheme.error,
            ),
            onPressed: () => Navigator.of(dialogContext).pop(true),
            child: const Text('Remove'),
          ),
        ],
      ),
    );
    return result ?? false;
  }
}

class _TrackTile extends StatelessWidget {
  const _TrackTile({required this.track, required this.index});

  final ListTrack track;
  final int index;

  @override
  Widget build(BuildContext context) {
    final duration = Duration(milliseconds: track.durationMs);
    final minutes = duration.inMinutes;
    final seconds = (duration.inSeconds % 60).toString().padLeft(2, '0');

    return ListTile(
      leading: track.albumArtUrl != null
          ? ClipRRect(
              borderRadius: BorderRadius.circular(4),
              child: Image.network(
                track.albumArtUrl!,
                width: 48,
                height: 48,
                fit: BoxFit.cover,
                errorBuilder: (_, __, ___) => const _PlaceholderArt(),
              ),
            )
          : const _PlaceholderArt(),
      title: Text(track.title, maxLines: 1, overflow: TextOverflow.ellipsis),
      subtitle: Text(track.artist, maxLines: 1, overflow: TextOverflow.ellipsis),
      trailing: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            '$minutes:$seconds',
            style: Theme.of(context).textTheme.bodySmall,
          ),
          const SizedBox(width: 8),
          const Icon(Icons.drag_handle),
        ],
      ),
    );
  }
}

class _PlaceholderArt extends StatelessWidget {
  const _PlaceholderArt();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 48,
      height: 48,
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(4),
      ),
      child: const Icon(Icons.music_note, size: 24),
    );
  }
}
