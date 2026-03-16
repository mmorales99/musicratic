import 'package:flutter_bloc/flutter_bloc.dart';

import '../repository/analytics_repository.dart';
import 'analytics_event.dart';
import 'analytics_state.dart';

class AnalyticsBloc extends Bloc<AnalyticsEvent, AnalyticsState> {
  AnalyticsBloc({required AnalyticsRepository repository})
      : _repository = repository,
        super(const AnalyticsState.initial()) {
    on<AnalyticsEventLoadWeeklyReport>(_onLoadWeeklyReport);
    on<AnalyticsEventLoadMonthlyReport>(_onLoadMonthlyReport);
    on<AnalyticsEventLoadTrackStats>(_onLoadTrackStats);
  }

  final AnalyticsRepository _repository;

  Future<void> _onLoadWeeklyReport(
    AnalyticsEventLoadWeeklyReport event,
    Emitter<AnalyticsState> emit,
  ) async {
    emit(const AnalyticsState.loading());
    try {
      final report = await _repository.getWeeklyReport(event.hubId);
      emit(AnalyticsState.reportLoaded(report: report));
    } on Exception catch (e) {
      emit(AnalyticsState.error(message: e.toString()));
    }
  }

  Future<void> _onLoadMonthlyReport(
    AnalyticsEventLoadMonthlyReport event,
    Emitter<AnalyticsState> emit,
  ) async {
    emit(const AnalyticsState.loading());
    try {
      final report = await _repository.getMonthlyReport(event.hubId);
      emit(AnalyticsState.reportLoaded(report: report));
    } on Exception catch (e) {
      emit(AnalyticsState.error(message: e.toString()));
    }
  }

  Future<void> _onLoadTrackStats(
    AnalyticsEventLoadTrackStats event,
    Emitter<AnalyticsState> emit,
  ) async {
    emit(const AnalyticsState.loading());
    try {
      final stats = await _repository.getTrackStats(event.trackId);
      emit(AnalyticsState.trackStatsLoaded(stats: stats));
    } on Exception catch (e) {
      emit(AnalyticsState.error(message: e.toString()));
    }
  }
}
