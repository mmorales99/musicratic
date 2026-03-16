import 'package:freezed_annotation/freezed_annotation.dart';

part 'voting_event.freezed.dart';

@freezed
class VotingEvent with _$VotingEvent {
  const factory VotingEvent.castVote({
    required String queueEntryId,
    required String value,
  }) = VotingEventCastVote;
  const factory VotingEvent.loadTally({
    required String queueEntryId,
  }) = VotingEventLoadTally;
  const factory VotingEvent.voteWindowOpened({
    required String queueEntryId,
    required DateTime expiresAt,
  }) = VotingEventVoteWindowOpened;
  const factory VotingEvent.voteWindowClosed() = VotingEventVoteWindowClosed;
}
