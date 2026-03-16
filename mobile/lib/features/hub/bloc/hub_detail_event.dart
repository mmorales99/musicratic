import 'package:freezed_annotation/freezed_annotation.dart';

part 'hub_detail_event.freezed.dart';

@freezed
class HubDetailEvent with _$HubDetailEvent {
  const factory HubDetailEvent.load({required String hubId}) =
      HubDetailEventLoad;
  const factory HubDetailEvent.refresh({required String hubId}) =
      HubDetailEventRefresh;
  const factory HubDetailEvent.activate({required String hubId}) =
      HubDetailEventActivate;
  const factory HubDetailEvent.deactivate({required String hubId}) =
      HubDetailEventDeactivate;
  const factory HubDetailEvent.pause({required String hubId}) =
      HubDetailEventPause;
  const factory HubDetailEvent.resume({required String hubId}) =
      HubDetailEventResume;
  const factory HubDetailEvent.delete({required String hubId}) =
      HubDetailEventDelete;
}
