import 'package:flutter_bloc/flutter_bloc.dart';

import 'analytics_dashboard_event.dart';
import 'analytics_dashboard_repository.dart';
import 'analytics_dashboard_state.dart';

class AnalyticsDashboardBloc
    extends Bloc<AnalyticsDashboardEvent, AnalyticsDashboardState> {
  AnalyticsDashboardBloc({required DashboardRepository repository})
      : _repository = repository,
        super(const AnalyticsDashboardState.loading()) {
    on<AnalyticsDashboardEventLoad>(_onLoad);
    on<AnalyticsDashboardEventRefresh>(_onRefresh);
  }

  final DashboardRepository _repository;

  Future<void> _onLoad(
    AnalyticsDashboardEventLoad event,
    Emitter<AnalyticsDashboardState> emit,
  ) async {
    emit(const AnalyticsDashboardState.loading());
    try {
      final data = await _repository.getDashboard(event.hubId);
      emit(AnalyticsDashboardState.loaded(data: data));
    } on Exception catch (e) {
      emit(AnalyticsDashboardState.error(message: e.toString()));
    }
  }

  Future<void> _onRefresh(
    AnalyticsDashboardEventRefresh event,
    Emitter<AnalyticsDashboardState> emit,
  ) async {
    try {
      final data = await _repository.getDashboard(event.hubId);
      emit(AnalyticsDashboardState.loaded(data: data));
    } on Exception catch (e) {
      emit(AnalyticsDashboardState.error(message: e.toString()));
    }
  }
}
