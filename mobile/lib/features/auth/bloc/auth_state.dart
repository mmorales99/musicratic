import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/auth_tokens.dart';
import '../models/auth_user.dart';

part 'auth_state.freezed.dart';

@freezed
class AuthState with _$AuthState {
  const factory AuthState.initial() = AuthInitial;
  const factory AuthState.authenticating() = AuthAuthenticating;
  const factory AuthState.authenticated({
    required AuthTokens tokens,
    required AuthUser user,
  }) = AuthAuthenticated;
  const factory AuthState.refreshing({
    required AuthTokens tokens,
    required AuthUser user,
  }) = AuthRefreshing;
  const factory AuthState.error({required String message}) = AuthError;
  const factory AuthState.loggingOut() = AuthLoggingOut;
}
