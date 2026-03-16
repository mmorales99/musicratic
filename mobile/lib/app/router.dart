import 'package:go_router/go_router.dart';

import '../features/hub/screens/hub_screen.dart';
import '../features/playback/screens/playback_screen.dart';
import '../features/voting/screens/voting_screen.dart';
import '../features/economy/screens/economy_screen.dart';
import '../features/profile/screens/profile_screen.dart';
import '../features/analytics/screens/analytics_screen.dart';

final GoRouter appRouter = GoRouter(
  initialLocation: '/hub',
  routes: [
    GoRoute(
      path: '/hub',
      name: 'hub',
      builder: (context, state) => const HubScreen(),
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
