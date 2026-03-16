import 'package:freezed_annotation/freezed_annotation.dart';

part 'playback_event.freezed.dart';

@freezed
class PlaybackEvent with _$PlaybackEvent {
  const factory PlaybackEvent.loadQueue({required String hubId}) =
      PlaybackEventLoadQueue;
  const factory PlaybackEvent.trackStarted({required String trackId}) =
      PlaybackEventTrackStarted;
  const factory PlaybackEvent.trackEnded({required String trackId}) =
      PlaybackEventTrackEnded;
  const factory PlaybackEvent.skipRequested({required String trackId}) =
      PlaybackEventSkipRequested;
  const factory PlaybackEvent.proposeTrack({
    required String hubId,
    required String trackUri,
  }) = PlaybackEventProposeTrack;
}
