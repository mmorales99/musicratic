import 'dart:async';

import 'package:flutter_bloc/flutter_bloc.dart';

import '../models/vote_models.dart';
import '../repository/voting_repository.dart';
import 'voting_event.dart';
import 'voting_state.dart';

class VotingBloc extends Bloc<VotingEvent, VotingState> {
  VotingBloc({required VotingRepository repository})
      : _repository = repository,
        super(const VotingState()) {
    on<VotingEventCastVote>(_onCastVote);
    on<VotingEventRemoveVote>(_onRemoveVote);
    on<VotingEventLoadTally>(_onLoadTally);
    on<VotingEventTallyUpdated>(_onTallyUpdated);
    on<VotingEventSkipTriggered>(_onSkipTriggered);
    on<VotingEventConnectToVoting>(_onConnect);
    on<VotingEventDisconnectFromVoting>(_onDisconnect);
  }

  final VotingRepository _repository;
  StreamSubscription<Map<String, dynamic>>? _wsSubscription;

  Future<void> _onConnect(
    VotingEventConnectToVoting event,
    Emitter<VotingState> emit,
  ) async {
    final stream = _repository.connectToVoting(event.hubId);
    await _wsSubscription?.cancel();
    _wsSubscription = stream.listen(
      _handleWsMessage,
      onError: (Object _) {
        // Connection errors are non-fatal for voting
      },
    );
  }

  void _handleWsMessage(Map<String, dynamic> message) {
    final type = message['type'] as String?;
    final data = message['data'] as Map<String, dynamic>?;

    switch (type) {
      case VoteWsEventType.tallyUpdated:
        if (data != null) {
          final entryId = data['entry_id'] as String;
          final tally = VoteTally.fromJson(data);
          add(VotingEvent.tallyUpdated(
            entryId: entryId,
            tally: tally,
          ));
        }
      case VoteWsEventType.skipTriggered:
        if (data != null) {
          final notification = SkipNotification.fromJson(data);
          add(VotingEvent.skipTriggered(notification: notification));
        }
    }
  }

  Future<void> _onCastVote(
    VotingEventCastVote event,
    Emitter<VotingState> emit,
  ) async {
    final entryData =
        state.entries[event.entryId] ?? const EntryVoteData();
    final previousVote = entryData.currentVote;

    // Toggle: tapping same direction removes the vote
    if (previousVote == event.direction) {
      add(VotingEvent.removeVote(
        hubId: event.hubId,
        entryId: event.entryId,
      ));
      return;
    }

    // Optimistic update
    final optimisticTally = _applyOptimisticVote(
      entryData.tally,
      previousVote,
      event.direction,
    );

    emit(state.copyWith(
      entries: {
        ...state.entries,
        event.entryId: entryData.copyWith(
          currentVote: event.direction,
          tally: optimisticTally,
          isVoting: true,
        ),
      },
      error: null,
    ));

    try {
      await _repository.castVote(
        hubId: event.hubId,
        entryId: event.entryId,
        direction: event.direction,
      );

      final current = state.entries[event.entryId];
      if (current != null) {
        emit(state.copyWith(
          entries: {
            ...state.entries,
            event.entryId: current.copyWith(isVoting: false),
          },
        ));
      }
    } on Exception catch (e) {
      // Rollback on error
      emit(state.copyWith(
        entries: {
          ...state.entries,
          event.entryId: entryData.copyWith(isVoting: false),
        },
        error: e.toString(),
      ));
    }
  }

  Future<void> _onRemoveVote(
    VotingEventRemoveVote event,
    Emitter<VotingState> emit,
  ) async {
    final entryData =
        state.entries[event.entryId] ?? const EntryVoteData();
    final previousVote = entryData.currentVote;

    if (previousVote == null) return;

    // Optimistic update
    final optimisticTally =
        _removeOptimisticVote(entryData.tally, previousVote);

    emit(state.copyWith(
      entries: {
        ...state.entries,
        event.entryId: entryData.copyWith(
          currentVote: null,
          tally: optimisticTally,
          isVoting: true,
        ),
      },
      error: null,
    ));

    try {
      await _repository.removeVote(
        hubId: event.hubId,
        entryId: event.entryId,
      );

      final current = state.entries[event.entryId];
      if (current != null) {
        emit(state.copyWith(
          entries: {
            ...state.entries,
            event.entryId: current.copyWith(isVoting: false),
          },
        ));
      }
    } on Exception catch (e) {
      // Rollback on error
      emit(state.copyWith(
        entries: {
          ...state.entries,
          event.entryId: entryData.copyWith(isVoting: false),
        },
        error: e.toString(),
      ));
    }
  }

  Future<void> _onLoadTally(
    VotingEventLoadTally event,
    Emitter<VotingState> emit,
  ) async {
    try {
      final tally = await _repository.getTally(
        hubId: event.hubId,
        entryId: event.entryId,
      );
      final entryData =
          state.entries[event.entryId] ?? const EntryVoteData();
      emit(state.copyWith(
        entries: {
          ...state.entries,
          event.entryId: entryData.copyWith(tally: tally),
        },
      ));
    } on Exception catch (e) {
      emit(state.copyWith(error: e.toString()));
    }
  }

  void _onTallyUpdated(
    VotingEventTallyUpdated event,
    Emitter<VotingState> emit,
  ) {
    final entryData =
        state.entries[event.entryId] ?? const EntryVoteData();
    emit(state.copyWith(
      entries: {
        ...state.entries,
        event.entryId: entryData.copyWith(tally: event.tally),
      },
    ));
  }

  void _onSkipTriggered(
    VotingEventSkipTriggered event,
    Emitter<VotingState> emit,
  ) {
    emit(state.copyWith(
      lastSkipNotification: event.notification,
    ));
  }

  Future<void> _onDisconnect(
    VotingEventDisconnectFromVoting event,
    Emitter<VotingState> emit,
  ) async {
    await _wsSubscription?.cancel();
    _wsSubscription = null;
    await _repository.disconnectFromVoting();
    emit(const VotingState());
  }

  VoteTally _applyOptimisticVote(
    VoteTally tally,
    VoteDirection? previous,
    VoteDirection next,
  ) {
    var up = tally.upCount;
    var down = tally.downCount;

    if (previous == VoteDirection.up) up--;
    if (previous == VoteDirection.down) down--;
    if (next == VoteDirection.up) up++;
    if (next == VoteDirection.down) down++;

    final total = up + down;
    final percentage = total > 0 ? (up / total) * 100 : 0.0;

    return VoteTally(
      upCount: up,
      downCount: down,
      total: total,
      percentage: percentage,
    );
  }

  VoteTally _removeOptimisticVote(
    VoteTally tally,
    VoteDirection previous,
  ) {
    var up = tally.upCount;
    var down = tally.downCount;

    if (previous == VoteDirection.up) up--;
    if (previous == VoteDirection.down) down--;

    final total = up + down;
    final percentage = total > 0 ? (up / total) * 100 : 0.0;

    return VoteTally(
      upCount: up,
      downCount: down,
      total: total,
      percentage: percentage,
    );
  }

  @override
  Future<void> close() async {
    await _wsSubscription?.cancel();
    await _repository.disconnectFromVoting();
    return super.close();
  }
}
