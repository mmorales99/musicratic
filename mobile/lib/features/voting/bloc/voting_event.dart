import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/vote_models.dart';

part 'voting_event.freezed.dart';

@freezed
class VotingEvent with _$VotingEvent {
  const factory VotingEvent.castVote({
    required String hubId,
    required String entryId,
    required VoteDirection direction,
  }) = VotingEventCastVote;

  const factory VotingEvent.removeVote({
    required String hubId,
    required String entryId,
  }) = VotingEventRemoveVote;

  const factory VotingEvent.loadTally({
    required String hubId,
    required String entryId,
  }) = VotingEventLoadTally;

  const factory VotingEvent.tallyUpdated({
    required String entryId,
    required VoteTally tally,
  }) = VotingEventTallyUpdated;

  const factory VotingEvent.skipTriggered({
    required SkipNotification notification,
  }) = VotingEventSkipTriggered;

  const factory VotingEvent.connectToVoting({
    required String hubId,
  }) = VotingEventConnectToVoting;

  const factory VotingEvent.disconnectFromVoting() =
      VotingEventDisconnectFromVoting;
}
