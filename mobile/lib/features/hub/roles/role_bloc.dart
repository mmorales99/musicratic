import 'package:flutter_bloc/flutter_bloc.dart';

import '../members/member_repository.dart';
import 'role_event.dart';
import 'role_repository.dart';
import 'role_state.dart';

class RoleBloc extends Bloc<RoleEvent, RoleState> {
  RoleBloc({
    required MemberRepository memberRepository,
    required RoleRepository roleRepository,
  })  : _memberRepository = memberRepository,
        _roleRepository = roleRepository,
        super(const RoleState.initial()) {
    on<RoleEventLoad>(_onLoad);
    on<RoleEventPromote>(_onPromote);
    on<RoleEventDemote>(_onDemote);
  }

  final MemberRepository _memberRepository;
  final RoleRepository _roleRepository;

  Future<void> _onLoad(
    RoleEventLoad event,
    Emitter<RoleState> emit,
  ) async {
    emit(const RoleState.loading());
    try {
      final members = await _memberRepository.getMembers(event.hubId);
      final limits = await _roleRepository.getTierLimits(event.hubId);
      emit(RoleState.loaded(members: members, tierLimits: limits));
    } on Exception catch (e) {
      emit(RoleState.error(message: e.toString()));
    }
  }

  Future<void> _onPromote(
    RoleEventPromote event,
    Emitter<RoleState> emit,
  ) async {
    emit(const RoleState.updating());
    try {
      await _roleRepository.promoteMember(
        event.hubId,
        event.memberId,
        event.newRole,
      );
      emit(RoleState.success(
        message: 'Member promoted to ${event.newRole}',
      ));
      add(RoleEvent.load(hubId: event.hubId));
    } on Exception catch (e) {
      emit(RoleState.error(message: e.toString()));
    }
  }

  Future<void> _onDemote(
    RoleEventDemote event,
    Emitter<RoleState> emit,
  ) async {
    emit(const RoleState.updating());
    try {
      await _roleRepository.demoteMember(
        event.hubId,
        event.memberId,
        event.newRole,
      );
      emit(RoleState.success(
        message: 'Member demoted to ${event.newRole}',
      ));
      add(RoleEvent.load(hubId: event.hubId));
    } on Exception catch (e) {
      emit(RoleState.error(message: e.toString()));
    }
  }
}
