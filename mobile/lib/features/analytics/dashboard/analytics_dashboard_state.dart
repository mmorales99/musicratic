import 'package:freezed_annotation/freezed_annotation.dart';

import 'dashboard_models.dart';

part 'analytics_dashboard_state.freezed.dart';

@freezed
class AnalyticsDashboardState with _$AnalyticsDashboardState {
  const factory AnalyticsDashboardState.loading() =
      AnalyticsDashboardStateLoading;
  const factory AnalyticsDashboardState.loaded({
    required DashboardData data,
  }) = AnalyticsDashboardStateLoaded;
  const factory AnalyticsDashboardState.error({required String message}) =
      AnalyticsDashboardStateError;
}
