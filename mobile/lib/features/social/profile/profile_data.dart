import 'package:freezed_annotation/freezed_annotation.dart';

part 'profile_data.freezed.dart';
part 'profile_data.g.dart';

@freezed
class ProfileData with _$ProfileData {
  const factory ProfileData({
    required String id,
    @JsonKey(name: 'display_name') required String displayName,
    String? avatar,
    String? bio,
    @JsonKey(name: 'favorite_genres') @Default([]) List<String> favoriteGenres,
    @JsonKey(name: 'total_tracks_proposed') @Default(0) int totalTracksProposed,
    @JsonKey(name: 'total_upvotes_received')
    @Default(0)
    int totalUpvotesReceived,
    @JsonKey(name: 'hubs_visited_count') @Default(0) int hubsVisitedCount,
    @JsonKey(name: 'member_since') DateTime? memberSince,
  }) = _ProfileData;

  factory ProfileData.fromJson(Map<String, dynamic> json) =>
      _$ProfileDataFromJson(json);
}
