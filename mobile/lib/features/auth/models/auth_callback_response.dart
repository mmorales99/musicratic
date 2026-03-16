import 'package:freezed_annotation/freezed_annotation.dart';

import 'auth_tokens.dart';
import 'auth_user.dart';

part 'auth_callback_response.freezed.dart';
part 'auth_callback_response.g.dart';

@freezed
class AuthCallbackResponse with _$AuthCallbackResponse {
  const factory AuthCallbackResponse({
    required AuthTokens tokens,
    required AuthUser user,
  }) = _AuthCallbackResponse;

  factory AuthCallbackResponse.fromJson(Map<String, dynamic> json) =>
      _$AuthCallbackResponseFromJson(json);
}
