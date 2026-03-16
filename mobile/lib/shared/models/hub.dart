import 'package:freezed_annotation/freezed_annotation.dart';

part 'hub.freezed.dart';
part 'hub.g.dart';

@freezed
class Hub with _$Hub {
  const factory Hub({
    required String id,
    required String name,
    String? description,
    @JsonKey(name: 'hub_type') required String hubType,
    required String status,
    @JsonKey(name: 'hub_code') String? hubCode,
    @JsonKey(name: 'owner_id') required String ownerId,
    @Default('public') String visibility,
    @JsonKey(name: 'is_active') @Default(false) bool isActive,
    @JsonKey(name: 'is_paused') @Default(false) bool isPaused,
    @JsonKey(name: 'qr_url') String? qrUrl,
    @JsonKey(name: 'direct_link') String? directLink,
    @JsonKey(name: 'active_users_count') @Default(0) int activeUsersCount,
    @JsonKey(name: 'now_playing') String? nowPlaying,
    @JsonKey(name: 'genre_tags') List<String>? genreTags,
    @JsonKey(name: 'average_rating') double? averageRating,
    HubSettings? settings,
    @JsonKey(name: 'created_at') DateTime? createdAt,
  }) = _Hub;

  factory Hub.fromJson(Map<String, dynamic> json) => _$HubFromJson(json);
}

@freezed
class HubSettings with _$HubSettings {
  const factory HubSettings({
    @JsonKey(name: 'proposals_enabled') @Default(true) bool proposalsEnabled,
    @JsonKey(name: 'max_queue_depth') @Default(20) int maxQueueDepth,
    @JsonKey(name: 'vote_skip_threshold') @Default(0.65)
    double voteSkipThreshold,
    @JsonKey(name: 'voting_window_seconds') @Default(60)
    int votingWindowSeconds,
    @JsonKey(name: 'coin_cost_multiplier') @Default(1.0)
    double coinCostMultiplier,
    @JsonKey(name: 'min_vote_count') @Default(3) int minVoteCount,
    @JsonKey(name: 'proposal_cooldown_minutes') @Default(5)
    int proposalCooldownMinutes,
  }) = _HubSettings;

  factory HubSettings.fromJson(Map<String, dynamic> json) =>
      _$HubSettingsFromJson(json);
}
