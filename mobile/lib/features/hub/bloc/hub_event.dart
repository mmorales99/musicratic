import 'package:freezed_annotation/freezed_annotation.dart';

part 'hub_event.freezed.dart';

@freezed
class HubEvent with _$HubEvent {
  const factory HubEvent.started() = HubEventStarted;
  const factory HubEvent.loadHubs() = HubEventLoadHubs;
  const factory HubEvent.attachToHub({required String hubId}) =
      HubEventAttachToHub;
  const factory HubEvent.detachFromHub() = HubEventDetachFromHub;
}
