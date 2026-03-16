import 'package:freezed_annotation/freezed_annotation.dart';

part 'vote.freezed.dart';
part 'vote.g.dart';

enum VoteValue {
  up,
  down,
}

@freezed
class Vote with _$Vote {
  const factory Vote({
    required String id,
    @JsonKey(name: 'queue_entry_id') required String queueEntryId,
    @JsonKey(name: 'user_id') required String userId,
    required VoteValue value,
    @JsonKey(name: 'cast_at') required DateTime castAt,
  }) = _Vote;

  factory Vote.fromJson(Map<String, dynamic> json) => _$VoteFromJson(json);
}
