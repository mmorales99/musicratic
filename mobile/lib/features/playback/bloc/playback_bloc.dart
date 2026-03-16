import 'package:flutter_bloc/flutter_bloc.dart';

import '../repository/playback_repository.dart';
import 'playback_event.dart';
import 'playback_state.dart';

class PlaybackBloc extends Bloc<PlaybackEvent, PlaybackState> {
  PlaybackBloc({required PlaybackRepository repository})
      : _repository = repository,
        super(const PlaybackState.idle()) {
    on<PlaybackEventLoadQueue>(_onLoadQueue);
    on<PlaybackEventTrackStarted>(_onTrackStarted);
    on<PlaybackEventTrackEnded>(_onTrackEnded);
    on<PlaybackEventSkipRequested>(_onSkipRequested);
    on<PlaybackEventProposeTrack>(_onProposeTrack);
  }

  final PlaybackRepository _repository;

  Future<void> _onLoadQueue(
    PlaybackEventLoadQueue event,
    Emitter<PlaybackState> emit,
  ) async {
    emit(const PlaybackState.loading());
    try {
      final queue = await _repository.getQueue(event.hubId);
      emit(PlaybackState.queueLoaded(entries: queue));
    } on Exception catch (e) {
      emit(PlaybackState.error(message: e.toString()));
    }
  }

  Future<void> _onTrackStarted(
    PlaybackEventTrackStarted event,
    Emitter<PlaybackState> emit,
  ) async {
    emit(PlaybackState.playing(trackId: event.trackId));
  }

  Future<void> _onTrackEnded(
    PlaybackEventTrackEnded event,
    Emitter<PlaybackState> emit,
  ) async {
    emit(const PlaybackState.idle());
  }

  Future<void> _onSkipRequested(
    PlaybackEventSkipRequested event,
    Emitter<PlaybackState> emit,
  ) async {
    try {
      await _repository.skipTrack(event.trackId);
    } on Exception catch (e) {
      emit(PlaybackState.error(message: e.toString()));
    }
  }

  Future<void> _onProposeTrack(
    PlaybackEventProposeTrack event,
    Emitter<PlaybackState> emit,
  ) async {
    try {
      await _repository.proposeTrack(
        hubId: event.hubId,
        trackUri: event.trackUri,
      );
    } on Exception catch (e) {
      emit(PlaybackState.error(message: e.toString()));
    }
  }
}
