import 'package:freezed_annotation/freezed_annotation.dart';

part 'playback_state.freezed.dart';

@freezed
class PlaybackState with _$PlaybackState {
  const factory PlaybackState.idle() = PlaybackStateIdle;
  const factory PlaybackState.loading() = PlaybackStateLoading;
  const factory PlaybackState.queueLoaded({
    required List<dynamic> entries,
  }) = PlaybackStateQueueLoaded;
  const factory PlaybackState.playing({required String trackId}) =
      PlaybackStatePlaying;
  const factory PlaybackState.error({required String message}) =
      PlaybackStateError;
}
