import 'package:freezed_annotation/freezed_annotation.dart';

part 'member_event.freezed.dart';

@freezed
class MemberEvent with _$MemberEvent {
  const factory MemberEvent.load({required String hubId}) = MemberEventLoad;
  const factory MemberEvent.search({
    required String hubId,
    required String query,
  }) = MemberEventSearch;
  const factory MemberEvent.removeMember({
    required String hubId,
    required String memberId,
  }) = MemberEventRemove;
}
