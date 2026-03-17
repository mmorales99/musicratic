import 'package:freezed_annotation/freezed_annotation.dart';

import 'member_models.dart';

part 'member_state.freezed.dart';

@freezed
class MemberState with _$MemberState {
  const factory MemberState.loading() = MemberStateLoading;
  const factory MemberState.loaded({
    required List<HubMember> members,
    @Default('') String searchQuery,
  }) = MemberStateLoaded;
  const factory MemberState.error({required String message}) =
      MemberStateError;
}
