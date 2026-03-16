import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'app/injection.dart';
import 'app/router.dart';
import 'app/theme.dart';
import 'features/auth/bloc/auth_bloc.dart';
import 'features/auth/bloc/auth_event.dart';
import 'features/hub/bloc/hub_bloc.dart';
import 'features/playback/bloc/playback_bloc.dart';
import 'features/voting/bloc/voting_bloc.dart';
import 'features/economy/bloc/economy_bloc.dart';
import 'features/profile/bloc/profile_bloc.dart';
import 'features/analytics/bloc/analytics_bloc.dart';
import 'shared/services/deep_link_service.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  configureDependencies();
  runApp(const MusicraticApp());
}

class MusicraticApp extends StatefulWidget {
  const MusicraticApp({super.key});

  @override
  State<MusicraticApp> createState() => _MusicraticAppState();
}

class _MusicraticAppState extends State<MusicraticApp> {
  late final AuthBloc _authBloc = getIt<AuthBloc>();
  late final _router = createAppRouter(authBloc: _authBloc);
  late final DeepLinkService _deepLinkService = getIt<DeepLinkService>();
  StreamSubscription<({String code, String state})>? _deepLinkSubscription;

  @override
  void initState() {
    super.initState();
    _authBloc.add(const AuthEvent.sessionRestoreRequested());
    _initializeDeepLinks();
  }

  Future<void> _initializeDeepLinks() async {
    // Subscribe before initialize to catch initial deep links
    _deepLinkSubscription =
        _deepLinkService.authCallbacks.listen((callback) {
      _authBloc.add(AuthEvent.callbackReceived(
        code: callback.code,
        state: callback.state,
      ));
    });
    await _deepLinkService.initialize();
  }

  @override
  void dispose() {
    _deepLinkSubscription?.cancel();
    _deepLinkService.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider<AuthBloc>.value(value: _authBloc),
        BlocProvider<HubBloc>(
          create: (_) => getIt<HubBloc>(),
        ),
        BlocProvider<PlaybackBloc>(
          create: (_) => getIt<PlaybackBloc>(),
        ),
        BlocProvider<VotingBloc>(
          create: (_) => getIt<VotingBloc>(),
        ),
        BlocProvider<EconomyBloc>(
          create: (_) => getIt<EconomyBloc>(),
        ),
        BlocProvider<ProfileBloc>(
          create: (_) => getIt<ProfileBloc>(),
        ),
        BlocProvider<AnalyticsBloc>(
          create: (_) => getIt<AnalyticsBloc>(),
        ),
      ],
      child: MaterialApp.router(
        title: 'Musicratic',
        theme: musicraticTheme,
        darkTheme: musicraticDarkTheme,
        themeMode: ThemeMode.system,
        routerConfig: _router,
        debugShowCheckedModeBanner: false,
      ),
    );
  }
}
