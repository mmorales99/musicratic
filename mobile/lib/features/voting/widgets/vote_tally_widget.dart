import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/voting_bloc.dart';
import '../bloc/voting_state.dart';
import '../models/vote_models.dart';

class VoteTallyWidget extends StatelessWidget {
  const VoteTallyWidget({
    super.key,
    required this.entryId,
    this.compact = false,
  });

  final String entryId;
  final bool compact;

  static const double _skipThreshold = 65.0;

  @override
  Widget build(BuildContext context) {
    return BlocSelector<VotingBloc, VotingState, EntryVoteData>(
      selector: (state) =>
          state.entries[entryId] ?? const EntryVoteData(),
      builder: (context, voteData) {
        final tally = voteData.tally;
        if (tally.total == 0) return const SizedBox.shrink();

        final upPercent = tally.percentage;
        final downPercent = 100.0 - upPercent;
        final isNearSkip = downPercent >= _skipThreshold;

        return Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            _buildPercentageRow(context, upPercent, isNearSkip),
            SizedBox(height: compact ? 4 : 6),
            _buildBar(context, tally),
            SizedBox(height: compact ? 2 : 4),
            _buildTotalVotes(context, tally.total),
          ],
        );
      },
    );
  }

  Widget _buildPercentageRow(
    BuildContext context,
    double upPercent,
    bool isNearSkip,
  ) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          '${upPercent.round()}% 👍',
          style: compact
              ? Theme.of(context).textTheme.labelSmall?.copyWith(
                    fontWeight: FontWeight.w600,
                  )
              : Theme.of(context).textTheme.bodySmall?.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
        ),
        if (isNearSkip)
          Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                Icons.warning_amber_rounded,
                size: compact ? 14 : 16,
                color: Colors.orange,
              ),
              const SizedBox(width: 4),
              Text(
                'Near skip',
                style: Theme.of(context).textTheme.labelSmall?.copyWith(
                      color: Colors.orange,
                      fontWeight: FontWeight.w600,
                    ),
              ),
            ],
          ),
      ],
    );
  }

  Widget _buildBar(BuildContext context, VoteTally tally) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(compact ? 3 : 4),
      child: SizedBox(
        height: compact ? 6 : 10,
        child: Row(
          children: [
            if (tally.upCount > 0)
              Expanded(
                flex: tally.upCount,
                child: AnimatedContainer(
                  duration: const Duration(milliseconds: 400),
                  curve: Curves.easeInOut,
                  color: Colors.green,
                ),
              ),
            if (tally.downCount > 0)
              Expanded(
                flex: tally.downCount,
                child: AnimatedContainer(
                  duration: const Duration(milliseconds: 400),
                  curve: Curves.easeInOut,
                  color: Colors.red,
                ),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildTotalVotes(BuildContext context, int total) {
    return Text(
      '$total vote${total == 1 ? '' : 's'}',
      style: Theme.of(context).textTheme.labelSmall?.copyWith(
            color: Theme.of(context).colorScheme.onSurfaceVariant,
          ),
      textAlign: TextAlign.center,
    );
  }
}
