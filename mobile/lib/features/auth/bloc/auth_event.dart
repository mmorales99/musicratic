import 'package:freezed_annotation/freezed_annotation.dart';

part 'auth_event.freezed.dart';

@freezed
class AuthEvent with _$AuthEvent {
  const factory AuthEvent.loginRequested() = AuthLoginRequested;
  const factory AuthEvent.callbackReceived({
    required String code,
    required String state,
  }) = AuthCallbackReceived;
  const factory AuthEvent.tokenRefreshRequested() = AuthTokenRefreshRequested;
  const factory AuthEvent.logoutRequested() = AuthLogoutRequested;
  const factory AuthEvent.sessionRestoreRequested() =
      AuthSessionRestoreRequested;
}
