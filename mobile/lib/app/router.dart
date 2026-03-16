import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../features/analytics/screens/analytics_screen.dart';
import '../features/auth/bloc/auth_bloc.dart';
import '../features/auth/bloc/auth_state.dart';
import '../features/auth/screens/login_screen.dart';
import '../features/economy/screens/economy_screen.dart';
import '../features/hub/bloc/hub_detail_bloc.dart';
import '../features/hub/bloc/hub_detail_event.dart';
import '../features/hub/bloc/hub_join_bloc.dart';
import '../features/hub/bloc/hub_search_bloc.dart';
import '../features/hub/bloc/list_bloc.dart';
import '../features/hub/bloc/list_event.dart';
import '../features/hub/screens/hub_create_screen.dart';
import '../features/hub/screens/hub_detail_screen.dart';
import '../features/hub/screens/hub_join_screen.dart';
import '../features/hub/screens/hub_screen.dart';
import '../features/hub/screens/list_detail_screen.dart';
import '../features/playback/bloc/queue_bloc.dart';
import '../features/playback/bloc/queue_event.dart';
import '../features/playback/bloc/search_bloc.dart';
import '../features/playback/screens/playback_screen.dart';
import '../features/playback/screens/queue_screen.dart';
import '../features/playback/screens/track_search_screen.dart';
import '../features/profile/screens/profile_screen.dart';
import '../features/voting/screens/voting_screen.dart';
import 'injection.dart';

GoRouter createAppRouter({required AuthBloc authBloc}) {
  return GoRouter(
    initialLocation: '/login',
    refreshListenable: GoRouterRefreshStream(authBloc.stream),
    redirect: (context, state) {
      final authState = authBloc.state;
      final location = state.matchedLocation;

      final isAuthenticated =
          authState is AuthAuthenticated || authState is AuthRefreshing;

      if (!isAuthenticated && location != '/login') {
        return '/login';
      }

      if (isAuthenticated && location == '/login') {
        return '/hub';
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/login',
        name: 'login',
        builder: (context, state) => const LoginScreen(),
      ),
      GoRoute(
        path: '/hub',
        name: 'hub',
        builder: (context, state) => BlocProvider(
          create: (_) => getIt<HubSearchBloc>(),
          child: const HubScreen(),
        ),
        routes: [
          GoRoute(
            path: 'join',
            name: 'hub-join',
            builder: (context, state) => BlocProvider(
              create: (_) => getIt<HubJoinBloc>(),
              child: const HubJoinScreen(),
            ),
          ),
          GoRoute(
            path: 'create',
            name: 'hub-create',
            builder: (context, state) => const HubCreateScreen(),
          ),
          GoRoute(
            path: ':hubId',
            name: 'hub-detail',
            builder: (context, state) {
              final hubId = state.pathParameters['hubId']!;
              return BlocProvider(
                create: (_) => getIt<HubDetailBloc>()
                  ..add(HubDetailEvent.load(hubId: hubId)),
                child: HubDetailScreen(hubId: hubId),
              );
            },
            routes: [
              GoRoute(
                path: 'lists/:listId',
                name: 'list-detail',
                builder: (context, state) {
                  final listId = state.pathParameters['listId']!;
                  return BlocProvider(
                    create: (_) => getIt<ListBloc>()
                      ..add(ListEvent.load(listId: listId)),
                    child: ListDetailScreen(listId: listId),
                  );
                },
              ),
              GoRoute(
                path: 'queue',
                name: 'hub-queue',
                builder: (context, state) {
                  final hubId = state.pathParameters['hubId']!;
                  return BlocProvider(
                    create: (_) => getIt<QueueBloc>()
                      ..add(QueueEvent.connectToQueue(hubId: hubId)),
                    child: QueueScreen(hubId: hubId),
                  );
                },
              ),
              GoRoute(
                path: 'search',
                name: 'hub-track-search',
                builder: (context, state) {
                  final hubId = state.pathParameters['hubId']!;
                  return BlocProvider(
                    create: (_) => getIt<SearchBloc>(),
                    child: TrackSearchScreen(hubId: hubId),
                  );
                },
              ),
            ],
          ),
        ],
      ),
      GoRoute(
        path: '/playback',
        name: 'playback',
        builder: (context, state) => const PlaybackScreen(),
      ),
      GoRoute(
        path: '/voting',
        name: 'voting',
        builder: (context, state) => const VotingScreen(),
      ),
      GoRoute(
        path: '/economy',
        name: 'economy',
        builder: (context, state) => const EconomyScreen(),
      ),
      GoRoute(
        path: '/profile',
        name: 'profile',
        builder: (context, state) => const ProfileScreen(),
      ),
      GoRoute(
        path: '/analytics',
        name: 'analytics',
        builder: (context, state) => const AnalyticsScreen(),
      ),
    ],
  );
}

/// Converts a [Stream] into a [ChangeNotifier] for [GoRouter.refreshListenable].
class GoRouterRefreshStream extends ChangeNotifier {
  GoRouterRefreshStream(Stream<dynamic> stream) {
    _subscription = stream.listen((_) {
      notifyListeners();
    });
  }

  late final StreamSubscription<dynamic> _subscription;

  @override
  void dispose() {
    _subscription.cancel();
    super.dispose();
  }
}
