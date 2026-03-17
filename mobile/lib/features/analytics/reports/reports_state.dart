import 'package:freezed_annotation/freezed_annotation.dart';

import 'report_models.dart';

part 'reports_state.freezed.dart';

@freezed
class ReportsState with _$ReportsState {
  const factory ReportsState.loading() = ReportsStateLoading;
  const factory ReportsState.loaded({
    required List<AnalyticsReport> reports,
    required String period,
  }) = ReportsStateLoaded;
  const factory ReportsState.error({required String message}) =
      ReportsStateError;
}
