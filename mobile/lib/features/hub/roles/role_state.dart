import 'package:freezed_annotation/freezed_annotation.dart';

import '../members/member_models.dart';

part 'role_state.freezed.dart';

@freezed
class RoleState with _$RoleState {
  const factory RoleState.initial() = RoleStateInitial;
  const factory RoleState.loading() = RoleStateLoading;
  const factory RoleState.loaded({
    required List<HubMember> members,
    required Map<String, dynamic> tierLimits,
  }) = RoleStateLoaded;
  const factory RoleState.updating() = RoleStateUpdating;
  const factory RoleState.success({required String message}) =
      RoleStateSuccess;
  const factory RoleState.error({required String message}) = RoleStateError;
}
