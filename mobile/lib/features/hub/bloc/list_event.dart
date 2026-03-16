import 'package:freezed_annotation/freezed_annotation.dart';

part 'list_event.freezed.dart';

@freezed
class ListEvent with _$ListEvent {
  const factory ListEvent.load({required String listId}) = ListEventLoad;
  const factory ListEvent.addTrack({
    required String listId,
    required String trackId,
  }) = ListEventAddTrack;
  const factory ListEvent.removeTrack({
    required String listId,
    required String trackId,
  }) = ListEventRemoveTrack;
  const factory ListEvent.reorderTracks({
    required String listId,
    required List<String> trackIds,
  }) = ListEventReorderTracks;
  const factory ListEvent.togglePlayMode({required String listId}) =
      ListEventTogglePlayMode;
}
