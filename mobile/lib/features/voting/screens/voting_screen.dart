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
          if (state.entries.isEmpty) {
            return const Center(
              child: Text('No active voting session'),
            );
          }

          return ListView.builder(
            itemCount: state.entries.length,
            itemBuilder: (context, index) {
              final entryId = state.entries.keys.elementAt(index);
              final data = state.entries[entryId]!;
              return ListTile(
                title: Text('Entry: $entryId'),
                subtitle: Text(
                  'Up: ${data.tally.upCount} / Down: ${data.tally.downCount}',
                ),
              );
            },
          );
        },
      ),
    );
  }
}
