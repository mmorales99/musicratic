import 'package:freezed_annotation/freezed_annotation.dart';

part 'analytics_event.freezed.dart';

@freezed
class AnalyticsEvent with _$AnalyticsEvent {
  const factory AnalyticsEvent.loadWeeklyReport({required String hubId}) =
      AnalyticsEventLoadWeeklyReport;
  const factory AnalyticsEvent.loadMonthlyReport({required String hubId}) =
      AnalyticsEventLoadMonthlyReport;
  const factory AnalyticsEvent.loadTrackStats({required String trackId}) =
      AnalyticsEventLoadTrackStats;
}
