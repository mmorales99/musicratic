import 'package:freezed_annotation/freezed_annotation.dart';

part 'analytics_state.freezed.dart';

@freezed
class AnalyticsState with _$AnalyticsState {
  const factory AnalyticsState.initial() = AnalyticsStateInitial;
  const factory AnalyticsState.loading() = AnalyticsStateLoading;
  const factory AnalyticsState.reportLoaded({
    required Map<String, dynamic> report,
  }) = AnalyticsStateReportLoaded;
  const factory AnalyticsState.trackStatsLoaded({
    required Map<String, dynamic> stats,
  }) = AnalyticsStateTrackStatsLoaded;
  const factory AnalyticsState.error({required String message}) =
      AnalyticsStateError;
}
