import 'package:freezed_annotation/freezed_annotation.dart';

part 'hub_state.freezed.dart';

@freezed
class HubState with _$HubState {
  const factory HubState.initial() = HubStateInitial;
  const factory HubState.loading() = HubStateLoading;
  const factory HubState.loaded({required List<dynamic> hubs}) =
      HubStateLoaded;
  const factory HubState.attached({required String hubId}) = HubStateAttached;
  const factory HubState.error({required String message}) = HubStateError;
}
