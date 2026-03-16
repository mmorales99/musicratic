import 'package:freezed_annotation/freezed_annotation.dart';

part 'list_models.freezed.dart';
part 'list_models.g.dart';

@freezed
class HubList with _$HubList {
  const factory HubList({
    required String id,
    @JsonKey(name: 'hub_id') required String hubId,
    required String name,
    @JsonKey(name: 'play_mode') @Default('ordered') String playMode,
    @JsonKey(name: 'track_count') @Default(0) int trackCount,
  }) = _HubList;

  factory HubList.fromJson(Map<String, dynamic> json) =>
      _$HubListFromJson(json);
}

@freezed
class ListTrack with _$ListTrack {
  const factory ListTrack({
    required String id,
    @JsonKey(name: 'track_id') required String trackId,
    required int position,
    required String title,
    required String artist,
    @JsonKey(name: 'duration_ms') required int durationMs,
    @JsonKey(name: 'album_art_url') String? albumArtUrl,
  }) = _ListTrack;

  factory ListTrack.fromJson(Map<String, dynamic> json) =>
      _$ListTrackFromJson(json);
}

@freezed
class CreateListRequest with _$CreateListRequest {
  const factory CreateListRequest({
    required String name,
    @JsonKey(name: 'play_mode') @Default('ordered') String playMode,
  }) = _CreateListRequest;

  factory CreateListRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateListRequestFromJson(json);
}
