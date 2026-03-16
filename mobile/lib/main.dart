import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'app/injection.dart';
import 'app/router.dart';
import 'app/theme.dart';
import 'features/hub/bloc/hub_bloc.dart';
import 'features/playback/bloc/playback_bloc.dart';
import 'features/voting/bloc/voting_bloc.dart';
import 'features/economy/bloc/economy_bloc.dart';
import 'features/profile/bloc/profile_bloc.dart';
import 'features/analytics/bloc/analytics_bloc.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  configureDependencies();
  runApp(const MusicraticApp());
}

class MusicraticApp extends StatelessWidget {
  const MusicraticApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
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
        routerConfig: appRouter,
        debugShowCheckedModeBanner: false,
      ),
    );
  }
}
