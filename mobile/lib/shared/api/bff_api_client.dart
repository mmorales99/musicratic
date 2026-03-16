import 'package:dio/dio.dart';

import '../services/auth_service.dart';

class BffApiClient {
  BffApiClient({required AuthService authService})
      : _authService = authService {
    _dio = Dio(
      BaseOptions(
        // TODO: Configure from environment
        baseUrl: 'https://api.musicratic.app/mobile',
        connectTimeout: const Duration(seconds: 10),
        receiveTimeout: const Duration(seconds: 10),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
      ),
    );

    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: _onRequest,
        onError: _onError,
      ),
    );
  }

  final AuthService _authService;
  late final Dio _dio;

  Future<void> _onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await _authService.getAccessToken();
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  Future<void> _onError(
    DioException error,
    ErrorInterceptorHandler handler,
  ) async {
    if (error.response?.statusCode == 401) {
      final refreshed = await _authService.refreshToken();
      if (refreshed) {
        final token = await _authService.getAccessToken();
        error.requestOptions.headers['Authorization'] = 'Bearer $token';
        final response = await _dio.fetch(error.requestOptions);
        return handler.resolve(response);
      }
    }
    handler.next(error);
  }

  Future<Response<dynamic>> get(
    String path, {
    Map<String, dynamic>? queryParameters,
  }) {
    return _dio.get(path, queryParameters: queryParameters);
  }

  Future<Response<dynamic>> post(
    String path, {
    Object? data,
  }) {
    return _dio.post(path, data: data);
  }

  Future<Response<dynamic>> put(
    String path, {
    Object? data,
  }) {
    return _dio.put(path, data: data);
  }

  Future<Response<dynamic>> delete(String path) {
    return _dio.delete(path);
  }
}
