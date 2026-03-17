import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/voting_bloc.dart';
import '../bloc/voting_event.dart';
import '../bloc/voting_state.dart';
import '../models/vote_models.dart';

class VoteButtonsWidget extends StatelessWidget {
  const VoteButtonsWidget({
    super.key,
    required this.hubId,
    required this.entryId,
    this.isAuthenticated = true,
    this.isTrackPlaying = true,
    this.compact = false,
  });

  final String hubId;
  final String entryId;
  final bool isAuthenticated;
  final bool isTrackPlaying;
  final bool compact;

  bool get _isEnabled => isAuthenticated && isTrackPlaying;

  @override
  Widget build(BuildContext context) {
    return BlocSelector<VotingBloc, VotingState, EntryVoteData>(
      selector: (state) =>
          state.entries[entryId] ?? const EntryVoteData(),
      builder: (context, voteData) {
        return Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            _VoteButton(
              direction: VoteDirection.up,
              count: voteData.tally.upCount,
              isActive: voteData.currentVote == VoteDirection.up,
              isLoading: voteData.isVoting,
              isEnabled: _isEnabled,
              compact: compact,
              onPressed: () => _onVote(context, VoteDirection.up),
            ),
            SizedBox(width: compact ? 8 : 16),
            _VoteButton(
              direction: VoteDirection.down,
              count: voteData.tally.downCount,
              isActive: voteData.currentVote == VoteDirection.down,
              isLoading: voteData.isVoting,
              isEnabled: _isEnabled,
              compact: compact,
              onPressed: () => _onVote(context, VoteDirection.down),
            ),
          ],
        );
      },
    );
  }

  void _onVote(BuildContext context, VoteDirection direction) {
    HapticFeedback.lightImpact();
    context.read<VotingBloc>().add(
          VotingEvent.castVote(
            hubId: hubId,
            entryId: entryId,
            direction: direction,
          ),
        );
  }
}

class _VoteButton extends StatefulWidget {
  const _VoteButton({
    required this.direction,
    required this.count,
    required this.isActive,
    required this.isLoading,
    required this.isEnabled,
    required this.compact,
    required this.onPressed,
  });

  final VoteDirection direction;
  final int count;
  final bool isActive;
  final bool isLoading;
  final bool isEnabled;
  final bool compact;
  final VoidCallback onPressed;

  @override
  State<_VoteButton> createState() => _VoteButtonState();
}

class _VoteButtonState extends State<_VoteButton>
    with SingleTickerProviderStateMixin {
  late final AnimationController _scaleController;
  late final Animation<double> _scaleAnimation;

  @override
  void initState() {
    super.initState();
    _scaleController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 150),
    );
    _scaleAnimation = Tween<double>(begin: 1.0, end: 0.85).animate(
      CurvedAnimation(
        parent: _scaleController,
        curve: Curves.easeInOut,
      ),
    );
  }

  @override
  void dispose() {
    _scaleController.dispose();
    super.dispose();
  }

  void _handleTap() {
    if (!widget.isEnabled || widget.isLoading) return;
    _scaleController.forward().then((_) => _scaleController.reverse());
    widget.onPressed();
  }

  @override
  Widget build(BuildContext context) {
    final isUp = widget.direction == VoteDirection.up;
    final activeColor = isUp ? Colors.green : Colors.red;
    final iconOutlined =
        isUp ? Icons.thumb_up_outlined : Icons.thumb_down_outlined;
    final iconFilled = isUp ? Icons.thumb_up : Icons.thumb_down;
    final size = widget.compact ? 18.0 : 24.0;

    return AnimatedBuilder(
      animation: _scaleAnimation,
      builder: (context, child) {
        return Transform.scale(
          scale: _scaleAnimation.value,
          child: child,
        );
      },
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          IconButton(
            onPressed: widget.isEnabled && !widget.isLoading
                ? _handleTap
                : null,
            icon: widget.isLoading
                ? SizedBox(
                    width: size,
                    height: size,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: activeColor,
                    ),
                  )
                : Icon(
                    widget.isActive ? iconFilled : iconOutlined,
                    color: widget.isActive
                        ? activeColor
                        : widget.isEnabled
                            ? null
                            : Theme.of(context).disabledColor,
                    size: size,
                  ),
            visualDensity: widget.compact
                ? VisualDensity.compact
                : VisualDensity.standard,
            tooltip: isUp ? 'Upvote' : 'Downvote',
          ),
          Text(
            '${widget.count}',
            style: Theme.of(context).textTheme.bodySmall?.copyWith(
                  color: widget.isActive ? activeColor : null,
                  fontWeight: widget.isActive
                      ? FontWeight.bold
                      : FontWeight.normal,
                ),
          ),
        ],
      ),
    );
  }
}
