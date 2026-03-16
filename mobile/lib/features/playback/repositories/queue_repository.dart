import 'dart:async';
import 'dart:convert';

import 'package:web_socket_channel/web_socket_channel.dart';

import '../../../shared/api/bff_api_client.dart';
import '../../../shared/services/auth_service.dart';
import '../models/queue_models.dart';

abstract class QueueRepository {
  Future<List<QueueEntryDto>> getQueue(
    String hubId, {
    int page,
    int pageSize,
  });
  Stream<Map<String, dynamic>> connectToQueue(String hubId);
  Future<void> disconnectFromQueue();
  bool get isConnected;
}

class QueueRepositoryImpl implements QueueRepository {
  QueueRepositoryImpl({
    required BffApiClient apiClient,
    required AuthService authService,
  })  : _apiClient = apiClient,
        _authService = authService;

  final BffApiClient _apiClient;
  final AuthService _authService;
  WebSocketChannel? _channel;
  StreamController<Map<String, dynamic>>? _messageController;

  @override
  bool get isConnected => _channel != null;

  @override
  Future<List<QueueEntryDto>> getQueue(
    String hubId, {
    int page = 1,
    int pageSize = 50,
  }) async {
    final response = await _apiClient.get(
      '/hubs/$hubId/queue',
      queryParameters: {'page': page, 'pageSize': pageSize},
    );
    final data = response.data as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>;
    return items
        .map((e) => QueueEntryDto.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Stream<Map<String, dynamic>> connectToQueue(String hubId) {
    _messageController?.close();
    _messageController =
        StreamController<Map<String, dynamic>>.broadcast();

    _connect(hubId);

    return _messageController!.stream;
  }

  Future<void> _connect(String hubId) async {
    final token = await _authService.getAccessToken();
    final uri = Uri.parse(
      'wss://api.musicratic.app/mobile/ws/hubs/$hubId/queue'
      '?token=$token',
    );

    _channel = WebSocketChannel.connect(uri);

    _channel!.stream.listen(
      (data) {
        final message =
            jsonDecode(data as String) as Map<String, dynamic>;
        _messageController?.add(message);
      },
      onError: (Object error) {
        _messageController?.addError(error);
      },
      onDone: () {
        _channel = null;
      },
    );
  }

  @override
  Future<void> disconnectFromQueue() async {
    await _channel?.sink.close();
    _channel = null;
    await _messageController?.close();
    _messageController = null;
  }
}
