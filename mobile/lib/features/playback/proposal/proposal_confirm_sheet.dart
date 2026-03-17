import 'package:flutter/material.dart';

import '../models/search_models.dart';

class ProposalConfirmSheet extends StatelessWidget {
  const ProposalConfirmSheet({
    super.key,
    required this.track,
    required this.coinCost,
    required this.currentBalance,
    required this.onConfirm,
    required this.onCancel,
    this.onBuyCoins,
  });

  final TrackSearchResult track;
  final int coinCost;
  final int currentBalance;
  final VoidCallback onConfirm;
  final VoidCallback onCancel;
  final VoidCallback? onBuyCoins;

  bool get _hasSufficientFunds => currentBalance >= coinCost;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: EdgeInsets.only(
        left: 24,
        right: 24,
        top: 24,
        bottom: MediaQuery.of(context).viewInsets.bottom + 24,
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            'Confirm Proposal',
            style: theme.textTheme.titleLarge,
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 20),
          _TrackInfoCard(track: track),
          const SizedBox(height: 20),
          _CostRow(
            label: 'Cost',
            value: '$coinCost coins',
            theme: theme,
          ),
          const SizedBox(height: 8),
          _CostRow(
            label: 'Your balance',
            value: '$currentBalance coins',
            theme: theme,
            valueColor: _hasSufficientFunds ? null : theme.colorScheme.error,
          ),
          if (!_hasSufficientFunds) ...[
            const SizedBox(height: 12),
            _InsufficientFundsWarning(onBuyCoins: onBuyCoins),
          ],
          const SizedBox(height: 24),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: onCancel,
                  child: const Text('Cancel'),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: FilledButton(
                  onPressed: _hasSufficientFunds ? onConfirm : null,
                  child: const Text('Propose'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _TrackInfoCard extends StatelessWidget {
  const _TrackInfoCard({required this.track});

  final TrackSearchResult track;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Row(
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: track.albumArt != null
                  ? Image.network(
                      track.albumArt!,
                      width: 56,
                      height: 56,
                      fit: BoxFit.cover,
                      errorBuilder: (_, __, ___) =>
                          const _AlbumArtPlaceholder(),
                    )
                  : const _AlbumArtPlaceholder(),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    track.title,
                    style: theme.textTheme.titleMedium,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 2),
                  Text(
                    track.artist,
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 2),
                  Text(
                    _formatDuration(track.durationMs),
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  String _formatDuration(int ms) {
    final duration = Duration(milliseconds: ms);
    final minutes = duration.inMinutes;
    final seconds = duration.inSeconds.remainder(60);
    return '$minutes:${seconds.toString().padLeft(2, '0')}';
  }
}

class _AlbumArtPlaceholder extends StatelessWidget {
  const _AlbumArtPlaceholder();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 56,
      height: 56,
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(8),
      ),
      child: const Icon(Icons.music_note, size: 28),
    );
  }
}

class _CostRow extends StatelessWidget {
  const _CostRow({
    required this.label,
    required this.value,
    required this.theme,
    this.valueColor,
  });

  final String label;
  final String value;
  final ThemeData theme;
  final Color? valueColor;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label, style: theme.textTheme.bodyLarge),
        Text(
          value,
          style: theme.textTheme.bodyLarge?.copyWith(
            fontWeight: FontWeight.bold,
            color: valueColor,
          ),
        ),
      ],
    );
  }
}

class _InsufficientFundsWarning extends StatelessWidget {
  const _InsufficientFundsWarning({this.onBuyCoins});

  final VoidCallback? onBuyCoins;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: theme.colorScheme.errorContainer,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        children: [
          Icon(
            Icons.warning_amber_rounded,
            color: theme.colorScheme.onErrorContainer,
            size: 20,
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              'Insufficient funds',
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onErrorContainer,
              ),
            ),
          ),
          if (onBuyCoins != null)
            TextButton(
              onPressed: onBuyCoins,
              child: const Text('Buy coins'),
            ),
        ],
      ),
    );
  }
}
