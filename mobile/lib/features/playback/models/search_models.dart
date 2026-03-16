import 'package:freezed_annotation/freezed_annotation.dart';

part 'search_models.freezed.dart';
part 'search_models.g.dart';

@freezed
class TrackSearchResult with _$TrackSearchResult {
  const factory TrackSearchResult({
    required String id,
    required String title,
    required String artist,
    @JsonKey(name: 'album_art') String? albumArt,
    @JsonKey(name: 'duration_ms') required int durationMs,
    required String provider,
  }) = _TrackSearchResult;

  factory TrackSearchResult.fromJson(Map<String, dynamic> json) =>
      _$TrackSearchResultFromJson(json);
}

@freezed
class ProposeTrackRequest with _$ProposeTrackRequest {
  const factory ProposeTrackRequest({
    @JsonKey(name: 'track_id') required String trackId,
    required String source,
  }) = _ProposeTrackRequest;

  factory ProposeTrackRequest.fromJson(Map<String, dynamic> json) =>
      _$ProposeTrackRequestFromJson(json);
}
