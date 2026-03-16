import 'dart:async';

import 'package:flutter/material.dart';

class NowPlayingWidget extends StatefulWidget {
  const NowPlayingWidget({
    super.key,
    required this.trackTitle,
    required this.trackArtist,
    this.trackAlbumArt,
    this.albumName,
    required this.durationMs,
    this.elapsedMs = 0,
    this.proposerName,
    this.startedAt,
    this.compact = false,
    this.onSkip,
  });

  final String trackTitle;
  final String trackArtist;
  final String? trackAlbumArt;
  final String? albumName;
  final int durationMs;
  final int elapsedMs;
  final String? proposerName;
  final DateTime? startedAt;
  final bool compact;
  final VoidCallback? onSkip;

  @override
  State<NowPlayingWidget> createState() => _NowPlayingWidgetState();
}

class _NowPlayingWidgetState extends State<NowPlayingWidget> {
  Timer? _progressTimer;
  late int _currentElapsedMs;

  @override
  void initState() {
    super.initState();
    _currentElapsedMs = _calculateElapsed();
    _startProgressTimer();
  }

  @override
  void didUpdateWidget(covariant NowPlayingWidget oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.trackTitle != widget.trackTitle ||
        oldWidget.startedAt != widget.startedAt) {
      _currentElapsedMs = _calculateElapsed();
    }
  }

  int _calculateElapsed() {
    if (widget.startedAt != null) {
      final elapsed =
          DateTime.now().difference(widget.startedAt!).inMilliseconds;
      return elapsed.clamp(0, widget.durationMs);
    }
    return widget.elapsedMs.clamp(0, widget.durationMs);
  }

  void _startProgressTimer() {
    _progressTimer?.cancel();
    _progressTimer = Timer.periodic(
      const Duration(seconds: 1),
      (_) {
        if (mounted) {
          setState(() {
            _currentElapsedMs =
                (_currentElapsedMs + 1000).clamp(0, widget.durationMs);
          });
        }
      },
    );
  }

  @override
  void dispose() {
    _progressTimer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return widget.compact ? _buildCompact(context) : _buildFull(context);
  }

  Widget _buildFull(BuildContext context) {
    final theme = Theme.of(context);
    final progress = widget.durationMs > 0
        ? _currentElapsedMs / widget.durationMs
        : 0.0;

    return Card(
      clipBehavior: Clip.antiAlias,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Album art
            ClipRRect(
              borderRadius: BorderRadius.circular(12),
              child: _buildAlbumArt(size: 200),
            ),
            const SizedBox(height: 16),
            // Track info
            Text(
              widget.trackTitle,
              style: theme.textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
              ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 4),
            Text(
              widget.trackArtist,
              style: theme.textTheme.bodyLarge?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              textAlign: TextAlign.center,
            ),
            if (widget.albumName != null) ...[
              const SizedBox(height: 2),
              Text(
                widget.albumName!,
                style: theme.textTheme.bodySmall?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                textAlign: TextAlign.center,
              ),
            ],
            const SizedBox(height: 16),
            // Progress bar
            LinearProgressIndicator(
              value: progress.clamp(0.0, 1.0),
              borderRadius: BorderRadius.circular(4),
            ),
            const SizedBox(height: 8),
            // Time display
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  _formatDuration(_currentElapsedMs),
                  style: theme.textTheme.bodySmall,
                ),
                Text(
                  _formatDuration(widget.durationMs),
                  style: theme.textTheme.bodySmall,
                ),
              ],
            ),
            const SizedBox(height: 12),
            // Bottom row: proposer + skip button
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                if (widget.proposerName != null)
                  Chip(
                    avatar: const Icon(Icons.person, size: 16),
                    label: Text(widget.proposerName!),
                    visualDensity: VisualDensity.compact,
                  )
                else
                  const SizedBox.shrink(),
                if (widget.onSkip != null)
                  IconButton.filled(
                    onPressed: widget.onSkip,
                    icon: const Icon(Icons.skip_next),
                    tooltip: 'Skip track',
                  ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCompact(BuildContext context) {
    final theme = Theme.of(context);
    final progress = widget.durationMs > 0
        ? _currentElapsedMs / widget.durationMs
        : 0.0;

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Row(
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: _buildAlbumArt(size: 56),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    widget.trackTitle,
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  Text(
                    widget.trackArtist,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  LinearProgressIndicator(
                    value: progress.clamp(0.0, 1.0),
                    borderRadius: BorderRadius.circular(2),
                  ),
                  const SizedBox(height: 2),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(
                        _formatDuration(_currentElapsedMs),
                        style: theme.textTheme.labelSmall,
                      ),
                      Text(
                        _formatDuration(widget.durationMs),
                        style: theme.textTheme.labelSmall,
                      ),
                    ],
                  ),
                ],
              ),
            ),
            if (widget.onSkip != null) ...[
              const SizedBox(width: 8),
              IconButton(
                onPressed: widget.onSkip,
                icon: const Icon(Icons.skip_next),
                tooltip: 'Skip track',
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildAlbumArt({required double size}) {
    if (widget.trackAlbumArt != null) {
      return Image.network(
        widget.trackAlbumArt!,
        width: size,
        height: size,
        fit: BoxFit.cover,
        errorBuilder: (_, __, ___) => _AlbumArtPlaceholder(size: size),
      );
    }
    return _AlbumArtPlaceholder(size: size);
  }

  static String _formatDuration(int ms) {
    final duration = Duration(milliseconds: ms);
    final minutes = duration.inMinutes;
    final seconds = duration.inSeconds.remainder(60);
    return '$minutes:${seconds.toString().padLeft(2, '0')}';
  }
}

class _AlbumArtPlaceholder extends StatelessWidget {
  const _AlbumArtPlaceholder({required this.size});

  final double size;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Icon(
        Icons.album,
        size: size * 0.5,
        color: Theme.of(context).colorScheme.onSurfaceVariant,
      ),
    );
  }
}
