import 'package:freezed_annotation/freezed_annotation.dart';

part 'dashboard_models.freezed.dart';
part 'dashboard_models.g.dart';

@freezed
class DashboardData with _$DashboardData {
  const factory DashboardData({
    @JsonKey(name: 'total_plays') @Default(0) int totalPlays,
    @JsonKey(name: 'total_votes') @Default(0) int totalVotes,
    @JsonKey(name: 'upvote_percentage') @Default(0.0) double upvotePercentage,
    @JsonKey(name: 'active_listeners') @Default(0) int activeListeners,
    @JsonKey(name: 'top_tracks') @Default([]) List<TrackStat> topTracks,
    @JsonKey(name: 'daily_plays') @Default([]) List<DailyStat> dailyPlays,
    @JsonKey(name: 'daily_votes') @Default([]) List<DailyStat> dailyVotes,
  }) = _DashboardData;

  factory DashboardData.fromJson(Map<String, dynamic> json) =>
      _$DashboardDataFromJson(json);
}

@freezed
class TrackStat with _$TrackStat {
  const factory TrackStat({
    @JsonKey(name: 'track_name') required String trackName,
    @JsonKey(name: 'artist_name') required String artistName,
    @JsonKey(name: 'play_count') @Default(0) int playCount,
    @JsonKey(name: 'vote_count') @Default(0) int voteCount,
  }) = _TrackStat;

  factory TrackStat.fromJson(Map<String, dynamic> json) =>
      _$TrackStatFromJson(json);
}

@freezed
class DailyStat with _$DailyStat {
  const factory DailyStat({
    required String date,
    required int count,
  }) = _DailyStat;

  factory DailyStat.fromJson(Map<String, dynamic> json) =>
      _$DailyStatFromJson(json);
}
