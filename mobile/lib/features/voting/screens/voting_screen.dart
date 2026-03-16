import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/voting_bloc.dart';
import '../bloc/voting_state.dart';

class VotingScreen extends StatelessWidget {
  const VotingScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Voting'),
      ),
      body: BlocBuilder<VotingBloc, VotingState>(
        builder: (context, state) {
          return state.when(
            idle: () => const Center(
              child: Text('No active voting session'),
            ),
            loading: () => const Center(
              child: CircularProgressIndicator(),
            ),
            windowOpen: (queueEntryId, expiresAt) => Center(
              child: Text('Vote now! Expires: $expiresAt'),
            ),
            voteCast: (queueEntryId, value) => Center(
              child: Text('You voted: $value'),
            ),
            tallyLoaded: (upvotes, downvotes) => Center(
              child: Text('Up: $upvotes / Down: $downvotes'),
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
