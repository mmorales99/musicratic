import 'package:flutter_bloc/flutter_bloc.dart';

import 'reports_event.dart';
import 'reports_repository.dart';
import 'reports_state.dart';

class ReportsBloc extends Bloc<ReportsEvent, ReportsState> {
  ReportsBloc({required ReportsRepository repository})
      : _repository = repository,
        super(const ReportsState.loading()) {
    on<ReportsEventLoad>(_onLoad);
    on<ReportsEventTogglePeriod>(_onTogglePeriod);
  }

  final ReportsRepository _repository;

  Future<void> _onLoad(
    ReportsEventLoad event,
    Emitter<ReportsState> emit,
  ) async {
    emit(const ReportsState.loading());
    try {
      final reports = await _repository.getReports(
        event.hubId,
        period: event.period,
      );
      emit(ReportsState.loaded(reports: reports, period: event.period));
    } on Exception catch (e) {
      emit(ReportsState.error(message: e.toString()));
    }
  }

  Future<void> _onTogglePeriod(
    ReportsEventTogglePeriod event,
    Emitter<ReportsState> emit,
  ) async {
    emit(const ReportsState.loading());
    try {
      final reports = await _repository.getReports(
        event.hubId,
        period: event.period,
      );
      emit(ReportsState.loaded(reports: reports, period: event.period));
    } on Exception catch (e) {
      emit(ReportsState.error(message: e.toString()));
    }
  }
}
