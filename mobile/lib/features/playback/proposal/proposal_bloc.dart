import 'dart:async';

import 'package:flutter_bloc/flutter_bloc.dart';

import '../repositories/track_repository.dart';
import 'proposal_event.dart';
import 'proposal_repository.dart';
import 'proposal_state.dart';

class ProposalBloc extends Bloc<ProposalEvent, ProposalState> {
  ProposalBloc({
    required TrackSearchRepository searchRepository,
    required ProposalRepository proposalRepository,
  })  : _searchRepository = searchRepository,
        _proposalRepository = proposalRepository,
        super(const ProposalState.initial()) {
    on<ProposalEventSearchTracks>(_onSearchTracks);
    on<ProposalEventSelectTrack>(_onSelectTrack);
    on<ProposalEventConfirmProposal>(_onConfirmProposal);
    on<ProposalEventCancelProposal>(_onCancelProposal);
  }

  final TrackSearchRepository _searchRepository;
  final ProposalRepository _proposalRepository;
  Timer? _debounce;

  Future<void> _onSearchTracks(
    ProposalEventSearchTracks event,
    Emitter<ProposalState> emit,
  ) async {
    if (event.query.trim().isEmpty) {
      emit(const ProposalState.initial());
      return;
    }

    emit(ProposalState.searching(
      query: event.query,
      provider: event.provider,
    ));

    _debounce?.cancel();
    final completer = Completer<void>();
    _debounce = Timer(const Duration(milliseconds: 300), () {
      if (!isClosed) {
        _performSearch(event.query, event.provider, emit).then((_) {
          if (!completer.isCompleted) completer.complete();
        });
      } else {
        if (!completer.isCompleted) completer.complete();
      }
    });
    await completer.future;
  }

  Future<void> _performSearch(
    String query,
    String? provider,
    Emitter<ProposalState> emit,
  ) async {
    try {
      final tracks = await _searchRepository.searchTracks(
        query,
        provider: provider,
      );
      emit(ProposalState.results(
        query: query,
        provider: provider,
        tracks: tracks,
      ));
    } on Exception catch (e) {
      emit(ProposalState.error(
        message: e.toString(),
        previousState: state,
      ));
    }
  }

  Future<void> _onSelectTrack(
    ProposalEventSelectTrack event,
    Emitter<ProposalState> emit,
  ) async {
    final currentState = state;
    if (currentState is! ProposalResults) return;

    final track = currentState.tracks.firstWhere(
      (t) => t.id == event.trackId,
    );

    try {
      final results = await Future.wait([
        _proposalRepository.getTrackCost(track.id, track.provider),
        _proposalRepository.getCoinBalance(),
      ]);
      final cost = results[0] as int;
      final balance = results[1] as int;

      if (cost == 0) {
        // Free track — skip confirmation, go straight to proposing
        emit(ProposalState.proposing(track: track));
        return;
      }

      emit(ProposalState.selected(
        track: track,
        coinCost: cost,
        currentBalance: balance,
      ));
    } on Exception catch (e) {
      emit(ProposalState.error(
        message: e.toString(),
        previousState: currentState,
      ));
    }
  }

  Future<void> _onConfirmProposal(
    ProposalEventConfirmProposal event,
    Emitter<ProposalState> emit,
  ) async {
    final currentState = state;
    final TrackSearchResult? track;

    if (currentState is ProposalSelected) {
      track = currentState.track;
    } else if (currentState is ProposalProposing) {
      track = currentState.track;
    } else {
      return;
    }

    emit(ProposalState.proposing(track: track));

    try {
      await _proposalRepository.proposeTrack(
        event.hubId,
        event.trackId,
        event.providerId,
      );
      emit(const ProposalState.success(
        message: 'Track proposed successfully!',
      ));
    } on Exception catch (e) {
      emit(ProposalState.error(
        message: e.toString(),
        previousState: currentState,
      ));
    }
  }

  void _onCancelProposal(
    ProposalEventCancelProposal event,
    Emitter<ProposalState> emit,
  ) {
    emit(const ProposalState.initial());
  }

  @override
  Future<void> close() {
    _debounce?.cancel();
    return super.close();
  }
}
