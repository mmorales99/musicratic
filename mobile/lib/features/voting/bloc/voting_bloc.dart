import 'package:flutter_bloc/flutter_bloc.dart';

import '../repository/voting_repository.dart';
import 'voting_event.dart';
import 'voting_state.dart';

class VotingBloc extends Bloc<VotingEvent, VotingState> {
  VotingBloc({required VotingRepository repository})
      : _repository = repository,
        super(const VotingState.idle()) {
    on<VotingEventCastVote>(_onCastVote);
    on<VotingEventLoadTally>(_onLoadTally);
    on<VotingEventVoteWindowOpened>(_onVoteWindowOpened);
    on<VotingEventVoteWindowClosed>(_onVoteWindowClosed);
  }

  final VotingRepository _repository;

  Future<void> _onCastVote(
    VotingEventCastVote event,
    Emitter<VotingState> emit,
  ) async {
    try {
      await _repository.castVote(
        queueEntryId: event.queueEntryId,
        value: event.value,
      );
      emit(VotingState.voteCast(
        queueEntryId: event.queueEntryId,
        value: event.value,
      ));
    } on Exception catch (e) {
      emit(VotingState.error(message: e.toString()));
    }
  }

  Future<void> _onLoadTally(
    VotingEventLoadTally event,
    Emitter<VotingState> emit,
  ) async {
    emit(const VotingState.loading());
    try {
      final tally = await _repository.getTally(event.queueEntryId);
      emit(VotingState.tallyLoaded(
        upvotes: tally['upvotes'] as int,
        downvotes: tally['downvotes'] as int,
      ));
    } on Exception catch (e) {
      emit(VotingState.error(message: e.toString()));
    }
  }

  Future<void> _onVoteWindowOpened(
    VotingEventVoteWindowOpened event,
    Emitter<VotingState> emit,
  ) async {
    emit(VotingState.windowOpen(
      queueEntryId: event.queueEntryId,
      expiresAt: event.expiresAt,
    ));
  }

  Future<void> _onVoteWindowClosed(
    VotingEventVoteWindowClosed event,
    Emitter<VotingState> emit,
  ) async {
    emit(const VotingState.idle());
  }
}
