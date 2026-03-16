import 'dart:async';

import 'package:flutter_bloc/flutter_bloc.dart';

import '../models/search_models.dart';
import '../repositories/track_repository.dart';
import 'search_event.dart';
import 'search_state.dart';

class SearchBloc extends Bloc<SearchEvent, SearchState> {
  SearchBloc({required TrackSearchRepository repository})
      : _repository = repository,
        super(const SearchState()) {
    on<SearchEventQueryChanged>(_onQueryChanged);
    on<SearchEventProviderFilterChanged>(_onProviderFilterChanged);
    on<SearchEventProposeTrack>(_onProposeTrack);
    on<SearchEventClearSearch>(_onClearSearch);
  }

  final TrackSearchRepository _repository;
  Timer? _debounce;

  Future<void> _onQueryChanged(
    SearchEventQueryChanged event,
    Emitter<SearchState> emit,
  ) async {
    emit(state.copyWith(query: event.query, errorMessage: null));

    if (event.query.trim().isEmpty) {
      emit(state.copyWith(results: [], isLoading: false));
      return;
    }

    _debounce?.cancel();
    final completer = Completer<void>();
    _debounce = Timer(const Duration(milliseconds: 300), () {
      if (!isClosed) {
        _performSearch(emit).then((_) {
          if (!completer.isCompleted) completer.complete();
        });
      } else {
        if (!completer.isCompleted) completer.complete();
      }
    });
    await completer.future;
  }

  Future<void> _onProviderFilterChanged(
    SearchEventProviderFilterChanged event,
    Emitter<SearchState> emit,
  ) async {
    emit(state.copyWith(provider: event.provider));
    if (state.query.trim().isNotEmpty) {
      await _performSearch(emit);
    }
  }

  Future<void> _onProposeTrack(
    SearchEventProposeTrack event,
    Emitter<SearchState> emit,
  ) async {
    emit(state.copyWith(
      isProposing: true,
      proposalSuccess: null,
      errorMessage: null,
    ));
    try {
      await _repository.proposeTrack(
        event.hubId,
        ProposeTrackRequest(
          trackId: event.trackId,
          source: event.source,
        ),
      );
      emit(state.copyWith(
        isProposing: false,
        proposalSuccess: 'Track added to queue!',
      ));
    } on Exception catch (e) {
      emit(state.copyWith(
        isProposing: false,
        errorMessage: e.toString(),
      ));
    }
  }

  void _onClearSearch(
    SearchEventClearSearch event,
    Emitter<SearchState> emit,
  ) {
    _debounce?.cancel();
    emit(const SearchState());
  }

  Future<void> _performSearch(Emitter<SearchState> emit) async {
    emit(state.copyWith(isLoading: true, errorMessage: null));
    try {
      final results = await _repository.searchTracks(
        state.query,
        provider: state.provider,
      );
      emit(state.copyWith(results: results, isLoading: false));
    } on Exception catch (e) {
      emit(state.copyWith(
        isLoading: false,
        errorMessage: e.toString(),
      ));
    }
  }

  @override
  Future<void> close() {
    _debounce?.cancel();
    return super.close();
  }
}
