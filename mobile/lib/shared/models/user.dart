import 'package:freezed_annotation/freezed_annotation.dart';

part 'user.freezed.dart';
part 'user.g.dart';

@freezed
class User with _$User {
  const factory User({
    required String id,
    @JsonKey(name: 'display_name') required String displayName,
    String? avatar,
    String? bio,
    @JsonKey(name: 'favorite_genres') List<String>? favoriteGenres,
    @JsonKey(name: 'total_tracks_proposed') @Default(0) int totalTracksProposed,
    @JsonKey(name: 'total_upvotes_received')
    @Default(0)
    int totalUpvotesReceived,
    @JsonKey(name: 'hubs_visited_count') @Default(0) int hubsVisitedCount,
    @JsonKey(name: 'member_since') DateTime? memberSince,
    @JsonKey(name: 'current_hub_id') String? currentHubId,
  }) = _User;

  factory User.fromJson(Map<String, dynamic> json) => _$UserFromJson(json);
}
