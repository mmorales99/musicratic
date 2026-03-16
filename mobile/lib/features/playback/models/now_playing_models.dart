import 'package:freezed_annotation/freezed_annotation.dart';

part 'now_playing_models.freezed.dart';
part 'now_playing_models.g.dart';

@freezed
class NowPlaying with _$NowPlaying {
  const factory NowPlaying({
    @JsonKey(name: 'track_title') required String trackTitle,
    @JsonKey(name: 'track_artist') required String trackArtist,
    @JsonKey(name: 'track_album_art') String? trackAlbumArt,
    @JsonKey(name: 'album_name') String? albumName,
    @JsonKey(name: 'duration_ms') required int durationMs,
    @JsonKey(name: 'elapsed_ms') @Default(0) int elapsedMs,
    @JsonKey(name: 'proposer_name') String? proposerName,
    @JsonKey(name: 'started_at') DateTime? startedAt,
  }) = _NowPlaying;

  factory NowPlaying.fromJson(Map<String, dynamic> json) =>
      _$NowPlayingFromJson(json);
}
