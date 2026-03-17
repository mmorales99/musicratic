import 'package:flutter_bloc/flutter_bloc.dart';

import 'member_event.dart';
import 'member_repository.dart';
import 'member_state.dart';

class MemberBloc extends Bloc<MemberEvent, MemberState> {
  MemberBloc({required MemberRepository repository})
      : _repository = repository,
        super(const MemberState.loading()) {
    on<MemberEventLoad>(_onLoad);
    on<MemberEventSearch>(_onSearch);
    on<MemberEventRemove>(_onRemove);
  }

  final MemberRepository _repository;

  Future<void> _onLoad(
    MemberEventLoad event,
    Emitter<MemberState> emit,
  ) async {
    emit(const MemberState.loading());
    try {
      final members = await _repository.getMembers(event.hubId);
      emit(MemberState.loaded(members: members));
    } on Exception catch (e) {
      emit(MemberState.error(message: e.toString()));
    }
  }

  Future<void> _onSearch(
    MemberEventSearch event,
    Emitter<MemberState> emit,
  ) async {
    try {
      final members = await _repository.getMembers(
        event.hubId,
        search: event.query,
      );
      emit(MemberState.loaded(members: members, searchQuery: event.query));
    } on Exception catch (e) {
      emit(MemberState.error(message: e.toString()));
    }
  }

  Future<void> _onRemove(
    MemberEventRemove event,
    Emitter<MemberState> emit,
  ) async {
    try {
      await _repository.removeMember(event.hubId, event.memberId);
      add(MemberEvent.load(hubId: event.hubId));
    } on Exception catch (e) {
      emit(MemberState.error(message: e.toString()));
    }
  }
}
