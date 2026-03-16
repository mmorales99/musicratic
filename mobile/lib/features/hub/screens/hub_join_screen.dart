import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../bloc/hub_join_bloc.dart';
import '../bloc/hub_join_event.dart';
import '../bloc/hub_join_state.dart';
import '../widgets/manual_code_entry_widget.dart';
import '../widgets/qr_scanner_widget.dart';

class HubJoinScreen extends StatelessWidget {
  const HubJoinScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<HubJoinBloc, HubJoinState>(
      listener: (context, state) {
        if (state is HubJoinJoined) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Joined ${state.hub.name}!'),
              backgroundColor: Colors.green,
            ),
          );
          context.go('/hub/${state.hub.id}');
        }
      },
      builder: (context, state) {
        return DefaultTabController(
          length: 2,
          child: Scaffold(
            appBar: AppBar(
              title: const Text('Join Hub'),
              bottom: const TabBar(
                tabs: [
                  Tab(icon: Icon(Icons.keyboard), text: 'Enter Code'),
                  Tab(icon: Icon(Icons.qr_code_scanner), text: 'Scan QR'),
                ],
              ),
            ),
            body: TabBarView(
              children: [
                _ManualTab(state: state),
                _ScannerTab(state: state),
              ],
            ),
          ),
        );
      },
    );
  }
}

class _ManualTab extends StatelessWidget {
  const _ManualTab({required this.state});

  final HubJoinState state;

  @override
  Widget build(BuildContext context) {
    final isLoading = state is HubJoinJoining;
    final errorMessage = state is HubJoinError
        ? (state as HubJoinError).message
        : null;

    return ManualCodeEntryWidget(
      onCodeChanged: (code) {
        context.read<HubJoinBloc>().add(HubJoinEvent.codeEntered(code: code));
      },
      onJoinPressed: () {
        context.read<HubJoinBloc>().add(const HubJoinEvent.joinRequested());
      },
      isLoading: isLoading,
      errorMessage: errorMessage,
    );
  }
}

class _ScannerTab extends StatelessWidget {
  const _ScannerTab({required this.state});

  final HubJoinState state;

  @override
  Widget build(BuildContext context) {
    if (state is HubJoinJoining) {
      return const Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            CircularProgressIndicator(),
            SizedBox(height: 16),
            Text('Joining hub...'),
          ],
        ),
      );
    }

    if (state is HubJoinError) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
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
                (state as HubJoinError).message,
                style: Theme.of(context).textTheme.bodyLarge,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 24),
              FilledButton(
                onPressed: () {
                  context
                      .read<HubJoinBloc>()
                      .add(const HubJoinEvent.reset());
                },
                child: const Text('Try Again'),
              ),
            ],
          ),
        ),
      );
    }

    return Padding(
      padding: const EdgeInsets.all(24),
      child: QrScannerWidget(
        onCodeScanned: (rawValue) {
          context
              .read<HubJoinBloc>()
              .add(HubJoinEvent.qrScanned(rawValue: rawValue));
        },
      ),
    );
  }
}
