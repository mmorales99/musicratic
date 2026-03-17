import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'report_models.dart';
import 'reports_bloc.dart';
import 'reports_event.dart';
import 'reports_state.dart';

class ReportsScreen extends StatelessWidget {
  const ReportsScreen({super.key, required this.hubId});

  final String hubId;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Reports')),
      body: BlocBuilder<ReportsBloc, ReportsState>(
        builder: (context, state) {
          return state.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            loaded: (reports, period) =>
                _ReportsContent(hubId: hubId, reports: reports, period: period),
            error: (message) => Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text('Error: $message'),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () => context
                        .read<ReportsBloc>()
                        .add(ReportsEvent.load(hubId: hubId)),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}

class _ReportsContent extends StatelessWidget {
  const _ReportsContent({
    required this.hubId,
    required this.reports,
    required this.period,
  });

  final String hubId;
  final List<AnalyticsReport> reports;
  final String period;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Padding(
          padding: const EdgeInsets.all(16),
          child: SegmentedButton<String>(
            segments: const [
              ButtonSegment(value: 'weekly', label: Text('Weekly')),
              ButtonSegment(value: 'monthly', label: Text('Monthly')),
            ],
            selected: {period},
            onSelectionChanged: (selected) {
              context.read<ReportsBloc>().add(
                    ReportsEvent.togglePeriod(
                      hubId: hubId,
                      period: selected.first,
                    ),
                  );
            },
          ),
        ),
        Expanded(
          child: reports.isEmpty
              ? const Center(child: Text('No reports available'))
              : ListView.builder(
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  itemCount: reports.length,
                  itemBuilder: (context, index) =>
                      _ReportCard(report: reports[index]),
                ),
        ),
      ],
    );
  }
}

class _ReportCard extends StatelessWidget {
  const _ReportCard({required this.report});

  final AnalyticsReport report;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: ExpansionTile(
        title: Text(
          '${report.startDate} – ${report.endDate}',
          style: theme.textTheme.titleSmall,
        ),
        subtitle: Text('${report.totalPlays} plays · ${report.totalVotes} votes'),
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _DetailRow('Unique Listeners', '${report.uniqueListeners}'),
                _DetailRow(
                  'Upvote Rate',
                  '${report.upvoteRate.toStringAsFixed(1)}%',
                ),
                if (report.topTrack != null)
                  _DetailRow('Top Track', report.topTrack!),
                if (report.topArtist != null)
                  _DetailRow('Top Artist', report.topArtist!),
                if (report.peakHour != null)
                  _DetailRow('Peak Hour', report.peakHour!),
                if (report.summary != null) ...[
                  const SizedBox(height: 8),
                  Text(
                    report.summary!,
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  const _DetailRow(this.label, this.value);

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: Theme.of(context).textTheme.bodySmall),
          Text(value, style: Theme.of(context).textTheme.bodyMedium),
        ],
      ),
    );
  }
}
