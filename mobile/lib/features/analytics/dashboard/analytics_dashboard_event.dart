import 'package:freezed_annotation/freezed_annotation.dart';

part 'analytics_dashboard_event.freezed.dart';

@freezed
class AnalyticsDashboardEvent with _$AnalyticsDashboardEvent {
  const factory AnalyticsDashboardEvent.load({required String hubId}) =
      AnalyticsDashboardEventLoad;
  const factory AnalyticsDashboardEvent.refresh({required String hubId}) =
      AnalyticsDashboardEventRefresh;
}
