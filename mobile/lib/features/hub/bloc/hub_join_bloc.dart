import 'package:flutter_bloc/flutter_bloc.dart';

import '../repository/hub_repository.dart';
import 'hub_join_event.dart';
import 'hub_join_state.dart';

class HubJoinBloc extends Bloc<HubJoinEvent, HubJoinState> {
  HubJoinBloc({required HubRepository repository})
      : _repository = repository,
        super(const HubJoinState.initial()) {
    on<HubJoinCodeEntered>(_onCodeEntered);
    on<HubJoinQrScanned>(_onQrScanned);
    on<HubJoinRequested>(_onJoinRequested);
    on<HubJoinReset>(_onReset);
  }

  final HubRepository _repository;
  String _currentCode = '';

  Future<void> _onCodeEntered(
    HubJoinCodeEntered event,
    Emitter<HubJoinState> emit,
  ) async {
    _currentCode = event.code.trim().toUpperCase();
    if (_currentCode.isNotEmpty) {
      emit(HubJoinState.codeReady(code: _currentCode));
    } else {
      emit(const HubJoinState.initial());
    }
  }

  Future<void> _onQrScanned(
    HubJoinQrScanned event,
    Emitter<HubJoinState> emit,
  ) async {
    final code = _extractCodeFromQr(event.rawValue);
    if (code == null) {
      emit(const HubJoinState.error(message: 'Invalid QR code'));
      return;
    }
    _currentCode = code;
    emit(HubJoinState.joining(code: code));
    await _performJoin(emit);
  }

  Future<void> _onJoinRequested(
    HubJoinRequested event,
    Emitter<HubJoinState> emit,
  ) async {
    if (_currentCode.isEmpty) {
      emit(const HubJoinState.error(message: 'Please enter a hub code'));
      return;
    }
    emit(HubJoinState.joining(code: _currentCode));
    await _performJoin(emit);
  }

  Future<void> _onReset(
    HubJoinReset event,
    Emitter<HubJoinState> emit,
  ) async {
    _currentCode = '';
    emit(const HubJoinState.initial());
  }

  Future<void> _performJoin(Emitter<HubJoinState> emit) async {
    try {
      final hub = await _repository.attachByCode(_currentCode);
      emit(HubJoinState.joined(hub: hub));
    } on Exception catch (e) {
      emit(HubJoinState.error(message: _parseError(e)));
    }
  }

  static String? _extractCodeFromQr(String rawValue) {
    // Try URL format: https://musicratic.app/join/{code}?sig=...
    final uri = Uri.tryParse(rawValue);
    if (uri != null && uri.pathSegments.length >= 2) {
      final joinIndex = uri.pathSegments.indexOf('join');
      if (joinIndex >= 0 && joinIndex + 1 < uri.pathSegments.length) {
        return uri.pathSegments[joinIndex + 1].toUpperCase();
      }
    }
    // Try plain code (alphanumeric, 6-20 chars)
    final plain = rawValue.trim().toUpperCase();
    if (RegExp(r'^[A-Z0-9]{6,20}$').hasMatch(plain)) {
      return plain;
    }
    return null;
  }

  static String _parseError(Exception e) {
    final message = e.toString();
    if (message.contains('404')) return 'Hub not found';
    if (message.contains('409')) return 'Hub is not active';
    if (message.contains('403')) return 'Access denied';
    return 'Failed to join hub. Please try again.';
  }
}
