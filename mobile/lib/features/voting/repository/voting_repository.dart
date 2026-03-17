import 'dart:async';
import 'dart:convert';

import 'package:web_socket_channel/web_socket_channel.dart';

import '../../../shared/api/bff_api_client.dart';
import '../../../shared/services/auth_service.dart';
import '../models/vote_models.dart';

abstract class VotingRepository {
  Future<void> castVote({
    required String hubId,
    required String entryId,
    required VoteDirection direction,
  });
  Future<void> removeVote({
    required String hubId,
    required String entryId,
  });
  Future<VoteTally> getTally({
    required String hubId,
    required String entryId,
  });
  Stream<Map<String, dynamic>> connectToVoting(String hubId);
  Future<void> disconnectFromVoting();
}

class VotingRepositoryImpl implements VotingRepository {
  VotingRepositoryImpl({
    required BffApiClient apiClient,
    required AuthService authService,
  })  : _apiClient = apiClient,
        _authService = authService;

  final BffApiClient _apiClient;
  final AuthService _authService;
  WebSocketChannel? _channel;
  StreamController<Map<String, dynamic>>? _messageController;

  @override
  Future<void> castVote({
    required String hubId,
    required String entryId,
    required VoteDirection direction,
  }) async {
    await _apiClient.post(
      '/hubs/$hubId/queue/$entryId/vote',
      data: {'direction': direction.name},
    );
  }

  @override
  Future<void> removeVote({
    required String hubId,
    required String entryId,
  }) async {
    await _apiClient.delete('/hubs/$hubId/queue/$entryId/vote');
  }

  @override
  Future<VoteTally> getTally({
    required String hubId,
    required String entryId,
  }) async {
    final response = await _apiClient.get(
      '/hubs/$hubId/queue/$entryId/tally',
    );
    return VoteTally.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Stream<Map<String, dynamic>> connectToVoting(String hubId) {
    _messageController?.close();
    _messageController =
        StreamController<Map<String, dynamic>>.broadcast();

    _connect(hubId);

    return _messageController!.stream;
  }

  Future<void> _connect(String hubId) async {
    final token = await _authService.getAccessToken();
    final uri = Uri.parse(
      'wss://api.musicratic.app/mobile/ws/hubs/$hubId/voting'
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
  Future<void> disconnectFromVoting() async {
    await _channel?.sink.close();
    _channel = null;
    await _messageController?.close();
    _messageController = null;
  }
}
