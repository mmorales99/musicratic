import 'package:freezed_annotation/freezed_annotation.dart';

part 'queue_models.freezed.dart';
part 'queue_models.g.dart';

enum QueueEntryStatus {
  @JsonValue('pending')
  pending,
  @JsonValue('playing')
  playing,
  @JsonValue('played')
  played,
  @JsonValue('skipped')
  skipped,
}

enum QueueEntrySource {
  @JsonValue('proposal')
  proposal,
  @JsonValue('list')
  list,
  @JsonValue('auto')
  auto,
}

@freezed
class QueueEntryDto with _$QueueEntryDto {
  const factory QueueEntryDto({
    required String id,
    required int position,
    @JsonKey(name: 'track_title') required String trackTitle,
    @JsonKey(name: 'track_artist') required String trackArtist,
    @JsonKey(name: 'track_album_art') String? trackAlbumArt,
    @JsonKey(name: 'duration_ms') required int durationMs,
    @JsonKey(name: 'proposer_name') String? proposerName,
    required QueueEntryStatus status,
    required QueueEntrySource source,
    @Default(0) int upvotes,
    @Default(0) int downvotes,
  }) = _QueueEntryDto;

  factory QueueEntryDto.fromJson(Map<String, dynamic> json) =>
      _$QueueEntryDtoFromJson(json);
}

enum QueueConnectionStatus {
  disconnected,
  connecting,
  connected,
  error,
}

@freezed
class QueueData with _$QueueData {
  const factory QueueData({
    @Default([]) List<QueueEntryDto> entries,
    NowPlayingDto? nowPlaying,
    @Default(QueueConnectionStatus.disconnected)
    QueueConnectionStatus connectionStatus,
  }) = _QueueData;
}

@freezed
class NowPlayingDto with _$NowPlayingDto {
  const factory NowPlayingDto({
    @JsonKey(name: 'track_title') required String trackTitle,
    @JsonKey(name: 'track_artist') required String trackArtist,
    @JsonKey(name: 'track_album_art') String? trackAlbumArt,
    @JsonKey(name: 'album_name') String? albumName,
    @JsonKey(name: 'duration_ms') required int durationMs,
    @JsonKey(name: 'elapsed_ms') @Default(0) int elapsedMs,
    @JsonKey(name: 'proposer_name') String? proposerName,
    @JsonKey(name: 'started_at') DateTime? startedAt,
  }) = _NowPlayingDto;

  factory NowPlayingDto.fromJson(Map<String, dynamic> json) =>
      _$NowPlayingDtoFromJson(json);
}

/// WebSocket event types for queue updates.
abstract class QueueWsEventType {
  static const String nowPlaying = 'NOW_PLAYING';
  static const String queueUpdated = 'QUEUE_UPDATED';
  static const String trackEnded = 'TRACK_ENDED';
  static const String trackSkipped = 'TRACK_SKIPPED';
}
