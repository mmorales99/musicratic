import 'package:flutter_bloc/flutter_bloc.dart';

import '../repository/hub_repository.dart';
import 'hub_event.dart';
import 'hub_state.dart';

class HubBloc extends Bloc<HubEvent, HubState> {
  HubBloc({required HubRepository repository})
      : _repository = repository,
        super(const HubState.initial()) {
    on<HubEventStarted>(_onStarted);
    on<HubEventLoadHubs>(_onLoadHubs);
    on<HubEventAttachToHub>(_onAttachToHub);
    on<HubEventDetachFromHub>(_onDetachFromHub);
  }

  final HubRepository _repository;

  Future<void> _onStarted(
    HubEventStarted event,
    Emitter<HubState> emit,
  ) async {
    add(const HubEvent.loadHubs());
  }

  Future<void> _onLoadHubs(
    HubEventLoadHubs event,
    Emitter<HubState> emit,
  ) async {
    emit(const HubState.loading());
    try {
      final hubs = await _repository.getActiveHubs();
      emit(HubState.loaded(hubs: hubs));
    } on Exception catch (e) {
      emit(HubState.error(message: e.toString()));
    }
  }

  Future<void> _onAttachToHub(
    HubEventAttachToHub event,
    Emitter<HubState> emit,
  ) async {
    try {
      await _repository.attachToHub(event.hubId);
      emit(HubState.attached(hubId: event.hubId));
    } on Exception catch (e) {
      emit(HubState.error(message: e.toString()));
    }
  }

  Future<void> _onDetachFromHub(
    HubEventDetachFromHub event,
    Emitter<HubState> emit,
  ) async {
    try {
      await _repository.detachFromHub();
      add(const HubEvent.loadHubs());
    } on Exception catch (e) {
      emit(HubState.error(message: e.toString()));
    }
  }
}
