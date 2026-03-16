import 'dart:async';

import 'package:flutter_bloc/flutter_bloc.dart';

import '../models/auth_tokens.dart';
import '../repository/auth_repository.dart';
import 'auth_event.dart';
import 'auth_state.dart';

class AuthBloc extends Bloc<AuthEvent, AuthState> {
  AuthBloc({required AuthRepository repository})
      : _repository = repository,
        super(const AuthState.initial()) {
    on<AuthSessionRestoreRequested>(_onSessionRestore);
    on<AuthLoginRequested>(_onLoginRequested);
    on<AuthCallbackReceived>(_onCallbackReceived);
    on<AuthTokenRefreshRequested>(_onTokenRefreshRequested);
    on<AuthLogoutRequested>(_onLogoutRequested);
  }

  final AuthRepository _repository;
  Timer? _refreshTimer;

  Future<void> _onSessionRestore(
    AuthSessionRestoreRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthState.authenticating());
    try {
      final tokens = await _repository.getStoredTokens();
      if (tokens == null) {
        emit(const AuthState.initial());
        return;
      }

      final user = await _repository.getStoredUser();
      if (user == null) {
        await _repository.clearTokens();
        emit(const AuthState.initial());
        return;
      }

      if (tokens.expiresAt.isBefore(DateTime.now())) {
        try {
          final newTokens = await _repository.refreshToken(
            refreshToken: tokens.refreshToken,
          );
          _scheduleTokenRefresh(newTokens);
          emit(AuthState.authenticated(tokens: newTokens, user: user));
        } on Exception {
          await _repository.clearTokens();
          emit(const AuthState.initial());
        }
      } else {
        _scheduleTokenRefresh(tokens);
        emit(AuthState.authenticated(tokens: tokens, user: user));
      }
    } on Exception catch (e) {
      emit(AuthState.error(message: e.toString()));
    }
  }

  Future<void> _onLoginRequested(
    AuthLoginRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthState.authenticating());
    try {
      await _repository.login();
      // Stays in authenticating until callback is received via deep link
    } on Exception catch (e) {
      emit(AuthState.error(message: e.toString()));
    }
  }

  Future<void> _onCallbackReceived(
    AuthCallbackReceived event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthState.authenticating());
    try {
      final response = await _repository.handleCallback(
        code: event.code,
        state: event.state,
      );
      _scheduleTokenRefresh(response.tokens);
      emit(AuthState.authenticated(
        tokens: response.tokens,
        user: response.user,
      ));
    } on Exception catch (e) {
      emit(AuthState.error(message: e.toString()));
    }
  }

  Future<void> _onTokenRefreshRequested(
    AuthTokenRefreshRequested event,
    Emitter<AuthState> emit,
  ) async {
    final currentState = state;
    if (currentState is! AuthAuthenticated) return;

    emit(AuthState.refreshing(
      tokens: currentState.tokens,
      user: currentState.user,
    ));

    try {
      final newTokens = await _repository.refreshToken(
        refreshToken: currentState.tokens.refreshToken,
      );
      _scheduleTokenRefresh(newTokens);
      emit(AuthState.authenticated(
        tokens: newTokens,
        user: currentState.user,
      ));
    } on Exception catch (e) {
      await _repository.clearTokens();
      _cancelRefreshTimer();
      emit(AuthState.error(message: e.toString()));
    }
  }

  Future<void> _onLogoutRequested(
    AuthLogoutRequested event,
    Emitter<AuthState> emit,
  ) async {
    final currentState = state;
    emit(const AuthState.loggingOut());
    _cancelRefreshTimer();

    try {
      if (currentState is AuthAuthenticated) {
        await _repository.logout(
          refreshToken: currentState.tokens.refreshToken,
        );
      } else {
        await _repository.clearTokens();
      }
    } on Exception {
      await _repository.clearTokens();
    }

    emit(const AuthState.initial());
  }

  void _scheduleTokenRefresh(AuthTokens tokens) {
    _cancelRefreshTimer();
    final expiresIn = tokens.expiresAt.difference(DateTime.now());
    // Refresh 1 minute before expiry
    final refreshIn = expiresIn - const Duration(minutes: 1);

    if (refreshIn.isNegative) {
      add(const AuthEvent.tokenRefreshRequested());
      return;
    }

    _refreshTimer = Timer(refreshIn, () {
      add(const AuthEvent.tokenRefreshRequested());
    });
  }

  void _cancelRefreshTimer() {
    _refreshTimer?.cancel();
    _refreshTimer = null;
  }

  @override
  Future<void> close() {
    _cancelRefreshTimer();
    return super.close();
  }
}
