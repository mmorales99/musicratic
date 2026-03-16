import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/queue_models.dart';

part 'queue_event.freezed.dart';

@freezed
class QueueEvent with _$QueueEvent {
  const factory QueueEvent.connectToQueue({required String hubId}) =
      QueueEventConnectToQueue;
  const factory QueueEvent.disconnectFromQueue() =
      QueueEventDisconnectFromQueue;
  const factory QueueEvent.queueUpdated({
    required List<QueueEntryDto> entries,
  }) = QueueEventQueueUpdated;
  const factory QueueEvent.nowPlayingChanged({
    required NowPlayingDto nowPlaying,
  }) = QueueEventNowPlayingChanged;
  const factory QueueEvent.trackEnded() = QueueEventTrackEnded;
  const factory QueueEvent.trackSkipped() = QueueEventTrackSkipped;
  const factory QueueEvent.refresh({required String hubId}) =
      QueueEventRefresh;
  const factory QueueEvent.connectionError({required String message}) =
      QueueEventConnectionError;
}
