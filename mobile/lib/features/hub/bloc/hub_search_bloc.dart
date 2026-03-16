import 'dart:async';

import 'package:flutter_bloc/flutter_bloc.dart';

import '../models/search_hubs_params.dart';
import '../repository/hub_repository.dart';
import 'hub_search_event.dart';
import 'hub_search_state.dart';

class HubSearchBloc extends Bloc<HubSearchEvent, HubSearchState> {
  HubSearchBloc({required HubRepository repository})
      : _repository = repository,
        super(const HubSearchState()) {
    on<HubSearchChanged>(_onSearchChanged);
    on<HubSearchFilterChanged>(_onFilterChanged);
    on<HubSearchLoadMore>(_onLoadMore);
    on<HubSearchRefresh>(_onRefresh);
  }

  final HubRepository _repository;
  Timer? _debounce;

  static const _pageSize = 20;

  Future<void> _onSearchChanged(
    HubSearchChanged event,
    Emitter<HubSearchState> emit,
  ) async {
    emit(state.copyWith(query: event.query));
    _debounce?.cancel();
    final completer = Completer<void>();
    _debounce = Timer(const Duration(milliseconds: 400), () {
      if (!isClosed) {
        _performSearch(emit, resetPage: true).then((_) {
          if (!completer.isCompleted) completer.complete();
        });
      } else {
        if (!completer.isCompleted) completer.complete();
      }
    });
    await completer.future;
  }

  Future<void> _onFilterChanged(
    HubSearchFilterChanged event,
    Emitter<HubSearchState> emit,
  ) async {
    emit(state.copyWith(
      businessType: event.businessType,
      visibility: event.visibility,
    ));
    await _performSearch(emit, resetPage: true);
  }

  Future<void> _onLoadMore(
    HubSearchLoadMore event,
    Emitter<HubSearchState> emit,
  ) async {
    if (state.isLoadingMore || !state.hasMore) return;
    emit(state.copyWith(isLoadingMore: true));
    await _performSearch(emit, resetPage: false);
  }

  Future<void> _onRefresh(
    HubSearchRefresh event,
    Emitter<HubSearchState> emit,
  ) async {
    await _performSearch(emit, resetPage: true);
  }

  Future<void> _performSearch(
    Emitter<HubSearchState> emit, {
    required bool resetPage,
  }) async {
    final page = resetPage ? 1 : state.currentPage + 1;

    if (resetPage) {
      emit(state.copyWith(isLoading: true, errorMessage: null));
    }

    try {
      final params = SearchHubsParams(
        name: state.query.isEmpty ? null : state.query,
        businessType: state.businessType,
        visibility: state.visibility,
        page: page,
        pageSize: _pageSize,
      );

      final result = await _repository.searchHubs(params);

      final hubs = resetPage ? result.items : [...state.hubs, ...result.items];

      emit(state.copyWith(
        hubs: hubs,
        currentPage: page,
        hasMore: result.hasMoreItems,
        isLoading: false,
        isLoadingMore: false,
        errorMessage: null,
      ));
    } on Exception catch (e) {
      emit(state.copyWith(
        isLoading: false,
        isLoadingMore: false,
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
