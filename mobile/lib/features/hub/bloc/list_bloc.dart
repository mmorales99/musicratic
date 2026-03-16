import 'package:flutter_bloc/flutter_bloc.dart';

import '../models/list_models.dart';
import '../repository/list_repository.dart';
import 'list_event.dart';
import 'list_state.dart';

class ListBloc extends Bloc<ListEvent, ListState> {
  ListBloc({required ListRepository repository})
      : _repository = repository,
        super(const ListState.initial()) {
    on<ListEventLoad>(_onLoad);
    on<ListEventAddTrack>(_onAddTrack);
    on<ListEventRemoveTrack>(_onRemoveTrack);
    on<ListEventReorderTracks>(_onReorderTracks);
    on<ListEventTogglePlayMode>(_onTogglePlayMode);
  }

  final ListRepository _repository;

  Future<void> _onLoad(
    ListEventLoad event,
    Emitter<ListState> emit,
  ) async {
    emit(const ListState.loading());
    try {
      final results = await Future.wait([
        _repository.getList(event.listId),
        _repository.getListTracks(event.listId),
      ]);
      emit(ListState.loaded(
        list: results[0] as HubList,
        tracks: results[1] as List<ListTrack>,
      ));
    } on Exception catch (e) {
      emit(ListState.error(message: e.toString()));
    }
  }

  Future<void> _onAddTrack(
    ListEventAddTrack event,
    Emitter<ListState> emit,
  ) async {
    final currentState = state;
    if (currentState is! ListStateLoaded) return;
    try {
      await _repository.addTrack(event.listId, trackId: event.trackId);
      add(ListEvent.load(listId: event.listId));
    } on Exception catch (e) {
      emit(ListState.error(message: e.toString()));
    }
  }

  Future<void> _onRemoveTrack(
    ListEventRemoveTrack event,
    Emitter<ListState> emit,
  ) async {
    final currentState = state;
    if (currentState is! ListStateLoaded) return;
    try {
      await _repository.removeTrack(event.listId, event.trackId);
      add(ListEvent.load(listId: event.listId));
    } on Exception catch (e) {
      emit(ListState.error(message: e.toString()));
    }
  }

  Future<void> _onReorderTracks(
    ListEventReorderTracks event,
    Emitter<ListState> emit,
  ) async {
    final currentState = state;
    if (currentState is! ListStateLoaded) return;
    try {
      await _repository.reorderTracks(event.listId, event.trackIds);
      add(ListEvent.load(listId: event.listId));
    } on Exception catch (e) {
      emit(ListState.error(message: e.toString()));
    }
  }

  Future<void> _onTogglePlayMode(
    ListEventTogglePlayMode event,
    Emitter<ListState> emit,
  ) async {
    final currentState = state;
    if (currentState is! ListStateLoaded) return;
    try {
      final newMode =
          currentState.list.playMode == 'ordered' ? 'shuffle' : 'ordered';
      await _repository.updateList(
        event.listId,
        {'play_mode': newMode},
      );
      add(ListEvent.load(listId: event.listId));
    } on Exception catch (e) {
      emit(ListState.error(message: e.toString()));
    }
  }
}
