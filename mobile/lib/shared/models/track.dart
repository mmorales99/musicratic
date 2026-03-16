import 'package:freezed_annotation/freezed_annotation.dart';

part 'track.freezed.dart';
part 'track.g.dart';

@freezed
class Track with _$Track {
  const factory Track({
    required String id,
    required String title,
    required String artist,
    String? album,
    @JsonKey(name: 'duration_ms') required int durationMs,
    @JsonKey(name: 'provider_uri') required String providerUri,
    required String provider,
    @JsonKey(name: 'artwork_url') String? artworkUrl,
  }) = _Track;

  factory Track.fromJson(Map<String, dynamic> json) => _$TrackFromJson(json);
}

@freezed
class QueueEntry with _$QueueEntry {
  const factory QueueEntry({
    required String id,
    required Track track,
    @JsonKey(name: 'proposed_by') String? proposedBy,
    @JsonKey(name: 'proposed_at') DateTime? proposedAt,
    required String status,
    @JsonKey(name: 'coins_spent') @Default(0) int coinsSpent,
    @Default(0) int upvotes,
    @Default(0) int downvotes,
    @JsonKey(name: 'is_from_list') @Default(false) bool isFromList,
  }) = _QueueEntry;

  factory QueueEntry.fromJson(Map<String, dynamic> json) =>
      _$QueueEntryFromJson(json);
}
