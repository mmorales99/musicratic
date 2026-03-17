import 'package:freezed_annotation/freezed_annotation.dart';

part 'role_event.freezed.dart';

@freezed
class RoleEvent with _$RoleEvent {
  const factory RoleEvent.load({required String hubId}) = RoleEventLoad;
  const factory RoleEvent.promote({
    required String hubId,
    required String memberId,
    required String newRole,
  }) = RoleEventPromote;
  const factory RoleEvent.demote({
    required String hubId,
    required String memberId,
    required String newRole,
  }) = RoleEventDemote;
}
