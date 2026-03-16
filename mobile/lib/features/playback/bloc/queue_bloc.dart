import 'dart:async';

import 'package:flutter_bloc/flutter_bloc.dart';

import '../models/queue_models.dart';
import '../repositories/queue_repository.dart';
import 'queue_event.dart';
import 'queue_state.dart';

class QueueBloc extends Bloc<QueueEvent, QueueState> {
  QueueBloc({required QueueRepository repository})
      : _repository = repository,
        super(const QueueState.initial()) {
    on<QueueEventConnectToQueue>(_onConnect);
    on<QueueEventDisconnectFromQueue>(_onDisconnect);
    on<QueueEventQueueUpdated>(_onQueueUpdated);
    on<QueueEventNowPlayingChanged>(_onNowPlayingChanged);
    on<QueueEventTrackEnded>(_onTrackEnded);
    on<QueueEventTrackSkipped>(_onTrackSkipped);
    on<QueueEventRefresh>(_onRefresh);
    on<QueueEventConnectionError>(_onConnectionError);
  }

  final QueueRepository _repository;
  StreamSubscription<Map<String, dynamic>>? _wsSubscription;
  String? _currentHubId;

  Future<void> _onConnect(
    QueueEventConnectToQueue event,
    Emitter<QueueState> emit,
  ) async {
    emit(const QueueState.connecting());
    _currentHubId = event.hubId;

    try {
      final entries = await _repository.getQueue(event.hubId);
      emit(QueueState.loaded(entries: entries));

      final stream = _repository.connectToQueue(event.hubId);
      await _wsSubscription?.cancel();
      _wsSubscription = stream.listen(
        _handleWsMessage,
        onError: (Object error) {
          add(QueueEvent.connectionError(message: error.toString()));
        },
      );
    } on Exception catch (e) {
      emit(QueueState.error(message: e.toString()));
    }
  }

  void _handleWsMessage(Map<String, dynamic> message) {
    final type = message['type'] as String?;
    final data = message['data'] as Map<String, dynamic>?;

    switch (type) {
      case QueueWsEventType.queueUpdated:
        if (data != null) {
          final items = data['entries'] as List<dynamic>? ?? [];
          final entries = items
              .map((e) =>
                  QueueEntryDto.fromJson(e as Map<String, dynamic>))
              .toList();
          add(QueueEvent.queueUpdated(entries: entries));
        }
      case QueueWsEventType.nowPlaying:
        if (data != null) {
          final nowPlaying = NowPlayingDto.fromJson(data);
          add(QueueEvent.nowPlayingChanged(nowPlaying: nowPlaying));
        }
      case QueueWsEventType.trackEnded:
        add(const QueueEvent.trackEnded());
      case QueueWsEventType.trackSkipped:
        add(const QueueEvent.trackSkipped());
    }
  }

  Future<void> _onDisconnect(
    QueueEventDisconnectFromQueue event,
    Emitter<QueueState> emit,
  ) async {
    await _wsSubscription?.cancel();
    _wsSubscription = null;
    await _repository.disconnectFromQueue();
    _currentHubId = null;
    emit(const QueueState.initial());
  }

  void _onQueueUpdated(
    QueueEventQueueUpdated event,
    Emitter<QueueState> emit,
  ) {
    final current = state;
    final nowPlaying =
        current is QueueStateLoaded ? current.nowPlaying : null;
    emit(QueueState.loaded(
      entries: event.entries,
      nowPlaying: nowPlaying,
    ));
  }

  void _onNowPlayingChanged(
    QueueEventNowPlayingChanged event,
    Emitter<QueueState> emit,
  ) {
    final current = state;
    final entries =
        current is QueueStateLoaded ? current.entries : <QueueEntryDto>[];
    emit(QueueState.loaded(
      entries: entries,
      nowPlaying: event.nowPlaying,
    ));
  }

  void _onTrackEnded(
    QueueEventTrackEnded event,
    Emitter<QueueState> emit,
  ) {
    final current = state;
    if (current is QueueStateLoaded) {
      emit(QueueState.loaded(entries: current.entries));
    }
  }

  void _onTrackSkipped(
    QueueEventTrackSkipped event,
    Emitter<QueueState> emit,
  ) {
    final current = state;
    if (current is QueueStateLoaded) {
      emit(QueueState.loaded(entries: current.entries));
    }
  }

  Future<void> _onRefresh(
    QueueEventRefresh event,
    Emitter<QueueState> emit,
  ) async {
    try {
      final entries = await _repository.getQueue(event.hubId);
      final current = state;
      final nowPlaying =
          current is QueueStateLoaded ? current.nowPlaying : null;
      emit(QueueState.loaded(
        entries: entries,
        nowPlaying: nowPlaying,
      ));
    } on Exception catch (e) {
      emit(QueueState.error(message: e.toString()));
    }
  }

  void _onConnectionError(
    QueueEventConnectionError event,
    Emitter<QueueState> emit,
  ) {
    emit(QueueState.error(message: event.message));
    // Auto-reconnect after error
    if (_currentHubId != null) {
      Future<void>.delayed(
        const Duration(seconds: 3),
        () {
          if (!isClosed && _currentHubId != null) {
            add(QueueEvent.connectToQueue(hubId: _currentHubId!));
          }
        },
      );
    }
  }

  @override
  Future<void> close() async {
    await _wsSubscription?.cancel();
    await _repository.disconnectFromQueue();
    return super.close();
  }
}
