import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../shared/models/hub.dart';

part 'hub_join_state.freezed.dart';

@freezed
class HubJoinState with _$HubJoinState {
  const factory HubJoinState.initial() = HubJoinInitial;
  const factory HubJoinState.codeReady({required String code}) =
      HubJoinCodeReady;
  const factory HubJoinState.joining({required String code}) = HubJoinJoining;
  const factory HubJoinState.joined({required Hub hub}) = HubJoinJoined;
  const factory HubJoinState.error({required String message}) = HubJoinError;
}
