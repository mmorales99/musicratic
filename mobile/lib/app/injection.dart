import 'package:get_it/get_it.dart';

import '../features/hub/bloc/hub_bloc.dart';
import '../features/hub/repository/hub_repository.dart';
import '../features/playback/bloc/playback_bloc.dart';
import '../features/playback/repository/playback_repository.dart';
import '../features/voting/bloc/voting_bloc.dart';
import '../features/voting/repository/voting_repository.dart';
import '../features/economy/bloc/economy_bloc.dart';
import '../features/economy/repository/economy_repository.dart';
import '../features/profile/bloc/profile_bloc.dart';
import '../features/profile/repository/profile_repository.dart';
import '../features/analytics/bloc/analytics_bloc.dart';
import '../features/analytics/repository/analytics_repository.dart';
import '../shared/api/bff_api_client.dart';
import '../shared/api/websocket_service.dart';
import '../shared/services/auth_service.dart';

final GetIt getIt = GetIt.instance;

void configureDependencies() {
  // Core services
  getIt.registerLazySingleton<AuthService>(() => AuthService());
  getIt.registerLazySingleton<BffApiClient>(
    () => BffApiClient(authService: getIt<AuthService>()),
  );
  getIt.registerLazySingleton<WebSocketService>(
    () => WebSocketService(authService: getIt<AuthService>()),
  );

  // Repositories
  getIt.registerLazySingleton<HubRepository>(
    () => HubRepositoryImpl(apiClient: getIt<BffApiClient>()),
  );
  getIt.registerLazySingleton<PlaybackRepository>(
    () => PlaybackRepositoryImpl(apiClient: getIt<BffApiClient>()),
  );
  getIt.registerLazySingleton<VotingRepository>(
    () => VotingRepositoryImpl(apiClient: getIt<BffApiClient>()),
  );
  getIt.registerLazySingleton<EconomyRepository>(
    () => EconomyRepositoryImpl(apiClient: getIt<BffApiClient>()),
  );
  getIt.registerLazySingleton<ProfileRepository>(
    () => ProfileRepositoryImpl(apiClient: getIt<BffApiClient>()),
  );
  getIt.registerLazySingleton<AnalyticsRepository>(
    () => AnalyticsRepositoryImpl(apiClient: getIt<BffApiClient>()),
  );

  // Blocs
  getIt.registerFactory<HubBloc>(
    () => HubBloc(repository: getIt<HubRepository>()),
  );
  getIt.registerFactory<PlaybackBloc>(
    () => PlaybackBloc(repository: getIt<PlaybackRepository>()),
  );
  getIt.registerFactory<VotingBloc>(
    () => VotingBloc(repository: getIt<VotingRepository>()),
  );
  getIt.registerFactory<EconomyBloc>(
    () => EconomyBloc(repository: getIt<EconomyRepository>()),
  );
  getIt.registerFactory<ProfileBloc>(
    () => ProfileBloc(repository: getIt<ProfileRepository>()),
  );
  getIt.registerFactory<AnalyticsBloc>(
    () => AnalyticsBloc(repository: getIt<AnalyticsRepository>()),
  );
}
