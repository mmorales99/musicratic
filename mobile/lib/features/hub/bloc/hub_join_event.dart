import 'package:freezed_annotation/freezed_annotation.dart';

part 'hub_join_event.freezed.dart';

@freezed
class HubJoinEvent with _$HubJoinEvent {
  const factory HubJoinEvent.codeEntered({required String code}) =
      HubJoinCodeEntered;
  const factory HubJoinEvent.qrScanned({required String rawValue}) =
      HubJoinQrScanned;
  const factory HubJoinEvent.joinRequested() = HubJoinRequested;
  const factory HubJoinEvent.reset() = HubJoinReset;
}
