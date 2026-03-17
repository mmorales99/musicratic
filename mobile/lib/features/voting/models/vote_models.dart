import 'package:freezed_annotation/freezed_annotation.dart';

part 'vote_models.freezed.dart';
part 'vote_models.g.dart';

enum VoteDirection {
  @JsonValue('up')
  up,
  @JsonValue('down')
  down,
}

@freezed
class VoteTally with _$VoteTally {
  const factory VoteTally({
    @JsonKey(name: 'up_count') @Default(0) int upCount,
    @JsonKey(name: 'down_count') @Default(0) int downCount,
    @Default(0) int total,
    @Default(0.0) double percentage,
  }) = _VoteTally;

  factory VoteTally.fromJson(Map<String, dynamic> json) =>
      _$VoteTallyFromJson(json);
}

@freezed
class EntryVoteData with _$EntryVoteData {
  const factory EntryVoteData({
    VoteDirection? currentVote,
    @Default(VoteTally()) VoteTally tally,
    @Default(false) bool isVoting,
  }) = _EntryVoteData;
}

enum SkipReason {
  @JsonValue('community')
  community,
  @JsonValue('owner')
  owner,
  @JsonValue('auto_advance')
  autoAdvance,
}

@freezed
class SkipNotification with _$SkipNotification {
  const factory SkipNotification({
    required SkipReason reason,
    @JsonKey(name: 'track_title') required String trackTitle,
    @JsonKey(name: 'refund_amount') int? refundAmount,
  }) = _SkipNotification;

  factory SkipNotification.fromJson(Map<String, dynamic> json) =>
      _$SkipNotificationFromJson(json);
}

/// WebSocket event types for voting updates.
abstract class VoteWsEventType {
  static const String tallyUpdated = 'VOTE_TALLY_UPDATED';
  static const String skipTriggered = 'SKIP_TRIGGERED';
}
