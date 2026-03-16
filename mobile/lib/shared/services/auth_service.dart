import 'dart:async';

import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthService {
  static const _storage = FlutterSecureStorage(
    aOptions: AndroidOptions(encryptedSharedPreferences: true),
  );

  static const _accessTokenKey = 'access_token';
  static const _refreshTokenKey = 'refresh_token';
  static const _expiresAtKey = 'expires_at';

  String? _cachedAccessToken;
  String? _cachedRefreshToken;

  /// Callback set by AuthRepository for handling token refresh via BFF.
  Future<bool> Function()? onRefreshToken;

  Future<String?> getAccessToken() async {
    _cachedAccessToken ??= await _storage.read(key: _accessTokenKey);
    return _cachedAccessToken;
  }

  Future<String?> getRefreshToken() async {
    _cachedRefreshToken ??= await _storage.read(key: _refreshTokenKey);
    return _cachedRefreshToken;
  }

  Future<DateTime?> getExpiresAt() async {
    final value = await _storage.read(key: _expiresAtKey);
    if (value == null) return null;
    return DateTime.tryParse(value);
  }

  Future<void> setTokens({
    required String accessToken,
    required String refreshToken,
    required DateTime expiresAt,
  }) async {
    _cachedAccessToken = accessToken;
    _cachedRefreshToken = refreshToken;
    await _storage.write(key: _accessTokenKey, value: accessToken);
    await _storage.write(key: _refreshTokenKey, value: refreshToken);
    await _storage.write(
      key: _expiresAtKey,
      value: expiresAt.toIso8601String(),
    );
  }

  Future<bool> refreshToken() async {
    if (onRefreshToken != null) {
      return onRefreshToken!();
    }
    return false;
  }

  Future<void> clearTokens() async {
    _cachedAccessToken = null;
    _cachedRefreshToken = null;
    await _storage.delete(key: _accessTokenKey);
    await _storage.delete(key: _refreshTokenKey);
    await _storage.delete(key: _expiresAtKey);
  }

  bool get isAuthenticated => _cachedAccessToken != null;
}
