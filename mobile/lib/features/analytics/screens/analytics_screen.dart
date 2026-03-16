import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/analytics_bloc.dart';
import '../bloc/analytics_state.dart';

class AnalyticsScreen extends StatelessWidget {
  const AnalyticsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Analytics'),
      ),
      body: BlocBuilder<AnalyticsBloc, AnalyticsState>(
        builder: (context, state) {
          return state.when(
            initial: () => const Center(
              child: Text('Select a report to view'),
            ),
            loading: () => const Center(
              child: CircularProgressIndicator(),
            ),
            reportLoaded: (report) => Center(
              child: Text('Report: ${report.keys.join(', ')}'),
            ),
            trackStatsLoaded: (stats) => Center(
              child: Text('Stats: ${stats.keys.join(', ')}'),
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
