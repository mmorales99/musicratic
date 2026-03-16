import 'dart:async';

import 'package:app_links/app_links.dart';

class DeepLinkService {
  DeepLinkService() : _appLinks = AppLinks();

  final AppLinks _appLinks;
  StreamSubscription<Uri>? _linkSubscription;

  final StreamController<({String code, String state})>
      _authCallbackController =
      StreamController<({String code, String state})>.broadcast();

  Stream<({String code, String state})> get authCallbacks =>
      _authCallbackController.stream;

  Future<void> initialize() async {
    final initialUri = await _appLinks.getInitialLink();
    if (initialUri != null) {
      _handleUri(initialUri);
    }

    _linkSubscription = _appLinks.uriLinkStream.listen(_handleUri);
  }

  void _handleUri(Uri uri) {
    if (uri.scheme == 'musicratic' && uri.host == 'callback') {
      final code = uri.queryParameters['code'];
      final state = uri.queryParameters['state'];
      if (code != null && state != null) {
        _authCallbackController.add((code: code, state: state));
      }
    }
  }

  void dispose() {
    _linkSubscription?.cancel();
    _authCallbackController.close();
  }
}
