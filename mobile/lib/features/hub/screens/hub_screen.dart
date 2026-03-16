import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/hub_bloc.dart';
import '../bloc/hub_state.dart';

class HubScreen extends StatelessWidget {
  const HubScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Hub Discovery'),
      ),
      body: BlocBuilder<HubBloc, HubState>(
        builder: (context, state) {
          return state.when(
            initial: () => const Center(
              child: Text('Welcome to Musicratic'),
            ),
            loading: () => const Center(
              child: CircularProgressIndicator(),
            ),
            loaded: (hubs) => ListView.builder(
              itemCount: hubs.length,
              itemBuilder: (context, index) {
                return ListTile(
                  title: Text('Hub ${hubs[index]}'),
                );
              },
            ),
            attached: (hubId) => Center(
              child: Text('Attached to hub: $hubId'),
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
