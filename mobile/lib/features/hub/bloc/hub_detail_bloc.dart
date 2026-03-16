import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../shared/models/hub.dart';
import '../models/list_models.dart';
import '../repository/hub_repository.dart';
import '../repository/list_repository.dart';
import 'hub_detail_event.dart';
import 'hub_detail_state.dart';

class HubDetailBloc extends Bloc<HubDetailEvent, HubDetailState> {
  HubDetailBloc({
    required HubRepository hubRepository,
    required ListRepository listRepository,
  })  : _hubRepository = hubRepository,
        _listRepository = listRepository,
        super(const HubDetailState.initial()) {
    on<HubDetailEventLoad>(_onLoad);
    on<HubDetailEventRefresh>(_onRefresh);
    on<HubDetailEventActivate>(_onActivate);
    on<HubDetailEventDeactivate>(_onDeactivate);
    on<HubDetailEventPause>(_onPause);
    on<HubDetailEventResume>(_onResume);
    on<HubDetailEventDelete>(_onDelete);
  }

  final HubRepository _hubRepository;
  final ListRepository _listRepository;

  Future<void> _onLoad(
    HubDetailEventLoad event,
    Emitter<HubDetailState> emit,
  ) async {
    emit(const HubDetailState.loading());
    try {
      final results = await Future.wait([
        _hubRepository.getHub(event.hubId),
        _hubRepository.getHubSettings(event.hubId),
        _listRepository.getLists(event.hubId),
      ]);
      emit(HubDetailState.loaded(
        hub: results[0] as Hub,
        settings: results[1] as HubSettings,
        lists: results[2] as List<HubList>,
      ));
    } on Exception catch (e) {
      emit(HubDetailState.error(message: e.toString()));
    }
  }

  Future<void> _onRefresh(
    HubDetailEventRefresh event,
    Emitter<HubDetailState> emit,
  ) async {
    try {
      final results = await Future.wait([
        _hubRepository.getHub(event.hubId),
        _hubRepository.getHubSettings(event.hubId),
        _listRepository.getLists(event.hubId),
      ]);
      emit(HubDetailState.loaded(
        hub: results[0] as Hub,
        settings: results[1] as HubSettings,
        lists: results[2] as List<HubList>,
      ));
    } on Exception catch (e) {
      emit(HubDetailState.error(message: e.toString()));
    }
  }

  Future<void> _onActivate(
    HubDetailEventActivate event,
    Emitter<HubDetailState> emit,
  ) async {
    final currentState = state;
    if (currentState is! HubDetailStateLoaded) return;
    emit(HubDetailState.acting(hub: currentState.hub, action: 'Activating'));
    try {
      await _hubRepository.activateHub(event.hubId);
      add(HubDetailEvent.refresh(hubId: event.hubId));
    } on Exception catch (e) {
      emit(HubDetailState.error(message: e.toString()));
    }
  }

  Future<void> _onDeactivate(
    HubDetailEventDeactivate event,
    Emitter<HubDetailState> emit,
  ) async {
    final currentState = state;
    if (currentState is! HubDetailStateLoaded) return;
    emit(HubDetailState.acting(
      hub: currentState.hub,
      action: 'Deactivating',
    ));
    try {
      await _hubRepository.deactivateHub(event.hubId);
      add(HubDetailEvent.refresh(hubId: event.hubId));
    } on Exception catch (e) {
      emit(HubDetailState.error(message: e.toString()));
    }
  }

  Future<void> _onPause(
    HubDetailEventPause event,
    Emitter<HubDetailState> emit,
  ) async {
    final currentState = state;
    if (currentState is! HubDetailStateLoaded) return;
    emit(HubDetailState.acting(hub: currentState.hub, action: 'Pausing'));
    try {
      await _hubRepository.pauseHub(event.hubId);
      add(HubDetailEvent.refresh(hubId: event.hubId));
    } on Exception catch (e) {
      emit(HubDetailState.error(message: e.toString()));
    }
  }

  Future<void> _onResume(
    HubDetailEventResume event,
    Emitter<HubDetailState> emit,
  ) async {
    final currentState = state;
    if (currentState is! HubDetailStateLoaded) return;
    emit(HubDetailState.acting(hub: currentState.hub, action: 'Resuming'));
    try {
      await _hubRepository.resumeHub(event.hubId);
      add(HubDetailEvent.refresh(hubId: event.hubId));
    } on Exception catch (e) {
      emit(HubDetailState.error(message: e.toString()));
    }
  }

  Future<void> _onDelete(
    HubDetailEventDelete event,
    Emitter<HubDetailState> emit,
  ) async {
    final currentState = state;
    if (currentState is! HubDetailStateLoaded) return;
    emit(HubDetailState.acting(hub: currentState.hub, action: 'Deleting'));
    try {
      await _hubRepository.deleteHub(event.hubId);
      emit(const HubDetailState.deleted());
    } on Exception catch (e) {
      emit(HubDetailState.error(message: e.toString()));
    }
  }
}
