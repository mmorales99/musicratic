import 'package:freezed_annotation/freezed_annotation.dart';

part 'report_models.freezed.dart';
part 'report_models.g.dart';

@freezed
class AnalyticsReport with _$AnalyticsReport {
  const factory AnalyticsReport({
    required String id,
    required String period,
    @JsonKey(name: 'start_date') required String startDate,
    @JsonKey(name: 'end_date') required String endDate,
    @JsonKey(name: 'total_plays') @Default(0) int totalPlays,
    @JsonKey(name: 'total_votes') @Default(0) int totalVotes,
    @JsonKey(name: 'unique_listeners') @Default(0) int uniqueListeners,
    @JsonKey(name: 'top_track') String? topTrack,
    @JsonKey(name: 'top_artist') String? topArtist,
    @JsonKey(name: 'upvote_rate') @Default(0.0) double upvoteRate,
    @JsonKey(name: 'peak_hour') String? peakHour,
    String? summary,
  }) = _AnalyticsReport;

  factory AnalyticsReport.fromJson(Map<String, dynamic> json) =>
      _$AnalyticsReportFromJson(json);
}
