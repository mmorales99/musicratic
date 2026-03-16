import 'dart:async';
import 'dart:convert';

import 'package:web_socket_channel/web_socket_channel.dart';

import '../services/auth_service.dart';

typedef WebSocketMessageHandler = void Function(Map<String, dynamic> message);

class WebSocketService {
  WebSocketService({required AuthService authService})
      : _authService = authService;

  final AuthService _authService;
  WebSocketChannel? _channel;
  final StreamController<Map<String, dynamic>> _messageController =
      StreamController<Map<String, dynamic>>.broadcast();

  Stream<Map<String, dynamic>> get messages => _messageController.stream;

  bool get isConnected => _channel != null;

  Future<void> connect(String hubId) async {
    final token = await _authService.getAccessToken();
    // TODO: Configure WebSocket URL from environment
    final uri = Uri.parse(
      'wss://api.musicratic.app/mobile/ws?hubId=$hubId&token=$token',
    );

    _channel = WebSocketChannel.connect(uri);

    _channel!.stream.listen(
      (data) {
        final message = jsonDecode(data as String) as Map<String, dynamic>;
        _messageController.add(message);
      },
      onError: (Object error) {
        _messageController.addError(error);
      },
      onDone: () {
        _channel = null;
      },
    );
  }

  void send(Map<String, dynamic> message) {
    _channel?.sink.add(jsonEncode(message));
  }

  Future<void> disconnect() async {
    await _channel?.sink.close();
    _channel = null;
  }

  void dispose() {
    disconnect();
    _messageController.close();
  }
}
