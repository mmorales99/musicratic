import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/queue_models.dart';

part 'queue_state.freezed.dart';

@freezed
class QueueState with _$QueueState {
  const factory QueueState.initial() = QueueStateInitial;
  const factory QueueState.connecting() = QueueStateConnecting;
  const factory QueueState.loaded({
    required List<QueueEntryDto> entries,
    NowPlayingDto? nowPlaying,
  }) = QueueStateLoaded;
  const factory QueueState.error({required String message}) = QueueStateError;
}
