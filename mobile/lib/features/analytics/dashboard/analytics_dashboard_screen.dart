import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'analytics_dashboard_bloc.dart';
import 'analytics_dashboard_state.dart';
import 'dashboard_models.dart';

class AnalyticsDashboardScreen extends StatelessWidget {
  const AnalyticsDashboardScreen({super.key, required this.hubId});

  final String hubId;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Analytics Dashboard')),
      body: BlocBuilder<AnalyticsDashboardBloc, AnalyticsDashboardState>(
        builder: (context, state) {
          return state.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            loaded: (data) => _DashboardContent(data: data),
            error: (message) => Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text('Error: $message'),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () => context
                        .read<AnalyticsDashboardBloc>()
                        .add(AnalyticsDashboardEvent.load(hubId: hubId)),
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

class _DashboardContent extends StatelessWidget {
  const _DashboardContent({required this.data});

  final DashboardData data;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return RefreshIndicator(
      onRefresh: () async {},
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          _buildStatsRow(theme),
          const SizedBox(height: 24),
          Text('Daily Plays', style: theme.textTheme.titleMedium),
          const SizedBox(height: 8),
          _buildBarChart(context, data.dailyPlays, Colors.blue),
          const SizedBox(height: 24),
          Text('Daily Votes', style: theme.textTheme.titleMedium),
          const SizedBox(height: 8),
          _buildBarChart(context, data.dailyVotes, Colors.orange),
          const SizedBox(height: 24),
          Text('Top Tracks', style: theme.textTheme.titleMedium),
          const SizedBox(height: 8),
          ...data.topTracks.map((t) => _buildTrackTile(t)),
        ],
      ),
    );
  }

  Widget _buildStatsRow(ThemeData theme) {
    return Wrap(
      spacing: 12,
      runSpacing: 12,
      children: [
        _StatCard(
          label: 'Total Plays',
          value: data.totalPlays.toString(),
          icon: Icons.play_circle_outline,
          color: Colors.blue,
        ),
        _StatCard(
          label: 'Total Votes',
          value: data.totalVotes.toString(),
          icon: Icons.how_to_vote,
          color: Colors.orange,
        ),
        _StatCard(
          label: 'Upvote %',
          value: '${data.upvotePercentage.toStringAsFixed(1)}%',
          icon: Icons.thumb_up_outlined,
          color: Colors.green,
        ),
        _StatCard(
          label: 'Listeners',
          value: data.activeListeners.toString(),
          icon: Icons.headphones,
          color: Colors.purple,
        ),
      ],
    );
  }

  Widget _buildBarChart(
    BuildContext context,
    List<DailyStat> stats,
    Color color,
  ) {
    if (stats.isEmpty) {
      return const SizedBox(
        height: 120,
        child: Center(child: Text('No data available')),
      );
    }
    final maxCount =
        stats.map((s) => s.count).reduce((a, b) => a > b ? a : b);
    return SizedBox(
      height: 120,
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.end,
        children: stats.map((stat) {
          final ratio = maxCount > 0 ? stat.count / maxCount : 0.0;
          return Expanded(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 2),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  Text(
                    stat.count.toString(),
                    style: const TextStyle(fontSize: 10),
                  ),
                  const SizedBox(height: 4),
                  Container(
                    height: 80 * ratio,
                    decoration: BoxDecoration(
                      color: color.withValues(alpha: 0.8),
                      borderRadius: BorderRadius.circular(4),
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    stat.date.length >= 5
                        ? stat.date.substring(5)
                        : stat.date,
                    style: const TextStyle(fontSize: 9),
                  ),
                ],
              ),
            ),
          );
        }).toList(),
      ),
    );
  }

  Widget _buildTrackTile(TrackStat track) {
    return ListTile(
      leading: const Icon(Icons.music_note),
      title: Text(track.trackName),
      subtitle: Text(track.artistName),
      trailing: Text('${track.playCount} plays'),
    );
  }
}

class _StatCard extends StatelessWidget {
  const _StatCard({
    required this.label,
    required this.value,
    required this.icon,
    required this.color,
  });

  final String label;
  final String value;
  final IconData icon;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: (MediaQuery.of(context).size.width - 44) / 2,
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Icon(icon, color: color, size: 28),
              const SizedBox(height: 8),
              Text(
                value,
                style: Theme.of(context)
                    .textTheme
                    .headlineSmall
                    ?.copyWith(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 4),
              Text(label, style: Theme.of(context).textTheme.bodySmall),
            ],
          ),
        ),
      ),
    );
  }
}
