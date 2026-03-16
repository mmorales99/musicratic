import 'dart:async';

class AuthService {
  String? _accessToken;
  String? _refreshTokenValue;

  Future<String?> getAccessToken() async {
    return _accessToken;
  }

  Future<void> setTokens({
    required String accessToken,
    required String refreshToken,
  }) async {
    _accessToken = accessToken;
    _refreshTokenValue = refreshToken;
    // TODO: Persist tokens securely (flutter_secure_storage)
  }

  Future<bool> refreshToken() async {
    if (_refreshTokenValue == null) return false;

    try {
      // TODO: Call BFF.Mobile token refresh endpoint
      // final response = await dio.post('/auth/refresh', data: {
      //   'refresh_token': _refreshTokenValue,
      // });
      // _accessToken = response.data['access_token'];
      // _refreshTokenValue = response.data['refresh_token'];
      return false;
    } on Exception {
      await clearTokens();
      return false;
    }
  }

  Future<void> clearTokens() async {
    _accessToken = null;
    _refreshTokenValue = null;
    // TODO: Clear persisted tokens
  }

  bool get isAuthenticated => _accessToken != null;
}
