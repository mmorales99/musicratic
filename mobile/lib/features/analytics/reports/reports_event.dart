import 'package:freezed_annotation/freezed_annotation.dart';

part 'reports_event.freezed.dart';

@freezed
class ReportsEvent with _$ReportsEvent {
  const factory ReportsEvent.load({
    required String hubId,
    @Default('weekly') String period,
  }) = ReportsEventLoad;
  const factory ReportsEvent.togglePeriod({
    required String hubId,
    required String period,
  }) = ReportsEventTogglePeriod;
}
