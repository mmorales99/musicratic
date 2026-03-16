import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../../shared/services/auth_service.dart';
import '../models/auth_callback_response.dart';
import '../models/auth_tokens.dart';
import '../models/auth_user.dart';

abstract class AuthRepository {
  Future<void> login();
  Future<AuthCallbackResponse> handleCallback({
    required String code,
    required String state,
  });
  Future<AuthTokens> refreshToken({required String refreshToken});
  Future<void> logout({required String refreshToken});
  Future<AuthTokens?> getStoredTokens();
  Future<AuthUser?> getStoredUser();
  Future<void> clearTokens();
}

class AuthRepositoryImpl implements AuthRepository {
  AuthRepositoryImpl({required AuthService authService})
      : _authService = authService,
        _dio = Dio(
          BaseOptions(
            // TODO: Configure from environment variable MUSICRATIC_BFF_URL
            baseUrl: 'https://api.musicratic.app/mobile',
            connectTimeout: const Duration(seconds: 10),
            receiveTimeout: const Duration(seconds: 10),
            headers: {
              'Content-Type': 'application/json',
              'Accept': 'application/json',
            },
          ),
        ) {
    _authService.onRefreshToken = _handleTokenRefresh;
  }

  final AuthService _authService;
  final Dio _dio;

  static const _storage = FlutterSecureStorage(
    aOptions: AndroidOptions(encryptedSharedPreferences: true),
  );

  static const _userIdKey = 'user_id';
  static const _userEmailKey = 'user_email';
  static const _userDisplayNameKey = 'user_display_name';
  static const _userAvatarUrlKey = 'user_avatar_url';
  static const _oauthStateKey = 'oauth_state';

  // TODO: Configure from environment
  static const _authorizeUrl =
      'https://auth.musicratic.app/application/o/authorize/';
  static const _clientId = 'musicratic-mobile';
  static const _redirectUri = 'musicratic://callback';
  static const _scopes = 'openid profile email';

  Future<bool> _handleTokenRefresh() async {
    final refreshTokenValue = await _authService.getRefreshToken();
    if (refreshTokenValue == null) return false;
    try {
      await refreshToken(refreshToken: refreshTokenValue);
      return true;
    } on Exception {
      await _authService.clearTokens();
      return false;
    }
  }

  @override
  Future<void> login() async {
    final state = DateTime.now().millisecondsSinceEpoch.toString();
    await _storage.write(key: _oauthStateKey, value: state);

    final uri = Uri.parse(_authorizeUrl).replace(
      queryParameters: {
        'response_type': 'code',
        'client_id': _clientId,
        'redirect_uri': _redirectUri,
        'scope': _scopes,
        'state': state,
      },
    );

    if (!await launchUrl(uri, mode: LaunchMode.externalApplication)) {
      throw Exception('Could not launch authorization URL');
    }
  }

  @override
  Future<AuthCallbackResponse> handleCallback({
    required String code,
    required String state,
  }) async {
    final storedState = await _storage.read(key: _oauthStateKey);
    if (storedState != state) {
      throw Exception('Invalid OAuth state parameter');
    }
    await _storage.delete(key: _oauthStateKey);

    final response = await _dio.post<Map<String, dynamic>>(
      '/api/auth/callback',
      data: {
        'code': code,
        'state': state,
        'redirect_uri': _redirectUri,
      },
    );

    final callbackResponse = AuthCallbackResponse.fromJson(response.data!);
    await _storeAuthData(callbackResponse);
    return callbackResponse;
  }

  @override
  Future<AuthTokens> refreshToken({required String refreshToken}) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/auth/refresh',
      data: {'refresh_token': refreshToken},
    );

    final tokens = AuthTokens.fromJson(response.data!);
    await _authService.setTokens(
      accessToken: tokens.accessToken,
      refreshToken: tokens.refreshToken,
      expiresAt: tokens.expiresAt,
    );
    return tokens;
  }

  @override
  Future<void> logout({required String refreshToken}) async {
    try {
      await _dio.post<void>(
        '/api/auth/logout',
        data: {'refresh_token': refreshToken},
      );
    } on Exception {
      // Server-side logout may fail (e.g., token already expired).
      // We still clear local tokens regardless.
    }
    await clearTokens();
  }

  @override
  Future<AuthTokens?> getStoredTokens() async {
    final accessToken = await _authService.getAccessToken();
    final refreshTokenValue = await _authService.getRefreshToken();
    final expiresAt = await _authService.getExpiresAt();

    if (accessToken == null ||
        refreshTokenValue == null ||
        expiresAt == null) {
      return null;
    }

    return AuthTokens(
      accessToken: accessToken,
      refreshToken: refreshTokenValue,
      expiresAt: expiresAt,
    );
  }

  @override
  Future<AuthUser?> getStoredUser() async {
    final id = await _storage.read(key: _userIdKey);
    final email = await _storage.read(key: _userEmailKey);
    final displayName = await _storage.read(key: _userDisplayNameKey);
    final avatarUrl = await _storage.read(key: _userAvatarUrlKey);

    if (id == null || email == null || displayName == null) {
      return null;
    }

    return AuthUser(
      id: id,
      email: email,
      displayName: displayName,
      avatarUrl: avatarUrl,
    );
  }

  @override
  Future<void> clearTokens() async {
    await _authService.clearTokens();
    await _storage.delete(key: _userIdKey);
    await _storage.delete(key: _userEmailKey);
    await _storage.delete(key: _userDisplayNameKey);
    await _storage.delete(key: _userAvatarUrlKey);
    await _storage.delete(key: _oauthStateKey);
  }

  Future<void> _storeAuthData(AuthCallbackResponse response) async {
    await _authService.setTokens(
      accessToken: response.tokens.accessToken,
      refreshToken: response.tokens.refreshToken,
      expiresAt: response.tokens.expiresAt,
    );
    await _storage.write(key: _userIdKey, value: response.user.id);
    await _storage.write(key: _userEmailKey, value: response.user.email);
    await _storage.write(
      key: _userDisplayNameKey,
      value: response.user.displayName,
    );
    if (response.user.avatarUrl != null) {
      await _storage.write(
        key: _userAvatarUrlKey,
        value: response.user.avatarUrl,
      );
    }
  }
}
