import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/playback_bloc.dart';
import '../bloc/playback_state.dart';

class PlaybackScreen extends StatelessWidget {
  const PlaybackScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Now Playing'),
      ),
      body: BlocBuilder<PlaybackBloc, PlaybackState>(
        builder: (context, state) {
          return state.when(
            idle: () => const Center(
              child: Text('No track playing'),
            ),
            loading: () => const Center(
              child: CircularProgressIndicator(),
            ),
            queueLoaded: (entries) => ListView.builder(
              itemCount: entries.length,
              itemBuilder: (context, index) {
                return ListTile(
                  title: Text('Track ${entries[index]}'),
                );
              },
            ),
            playing: (trackId) => Center(
              child: Text('Now playing: $trackId'),
            ),
            error: (message) => Center(
              child: Text('Error: $message'),
            ),
          );
        },
      ),
    );
  }
}
