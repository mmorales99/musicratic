import 'package:freezed_annotation/freezed_annotation.dart';

part 'voting_state.freezed.dart';

@freezed
class VotingState with _$VotingState {
  const factory VotingState.idle() = VotingStateIdle;
  const factory VotingState.loading() = VotingStateLoading;
  const factory VotingState.windowOpen({
    required String queueEntryId,
    required DateTime expiresAt,
  }) = VotingStateWindowOpen;
  const factory VotingState.voteCast({
    required String queueEntryId,
    required String value,
  }) = VotingStateVoteCast;
  const factory VotingState.tallyLoaded({
    required int upvotes,
    required int downvotes,
  }) = VotingStateTallyLoaded;
  const factory VotingState.error({required String message}) =
      VotingStateError;
}
