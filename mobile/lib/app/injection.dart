import 'package:get_it/get_it.dart';

import '../features/auth/bloc/auth_bloc.dart';
import '../features/auth/repository/auth_repository.dart';
import '../features/hub/bloc/hub_bloc.dart';
import '../features/hub/bloc/hub_detail_bloc.dart';
import '../features/hub/bloc/hub_join_bloc.dart';
import '../features/hub/bloc/hub_search_bloc.dart';
import '../features/hub/bloc/list_bloc.dart';
import '../features/hub/repository/hub_repository.dart';
import '../features/hub/repository/list_repository.dart';
import '../features/playback/bloc/playback_bloc.dart';
import '../features/playback/bloc/queue_bloc.dart';
import '../features/playback/bloc/search_bloc.dart';
import '../features/playback/repository/playback_repository.dart';
import '../features/playback/repositories/queue_repository.dart';
import '../features/playback/repositories/track_repository.dart';
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
import '../shared/services/deep_link_service.dart';

final GetIt getIt = GetIt.instance;

void configureDependencies() {
  // Core services
  getIt.registerLazySingleton<AuthService>(() => AuthService());
  getIt.registerLazySingleton<DeepLinkService>(() => DeepLinkService());
  getIt.registerLazySingleton<BffApiClient>(
    () => BffApiClient(authService: getIt<AuthService>()),
  );
  getIt.registerLazySingleton<WebSocketService>(
    () => WebSocketService(authService: getIt<AuthService>()),
  );

  // Repositories
  getIt.registerLazySingleton<AuthRepository>(
    () => AuthRepositoryImpl(authService: getIt<AuthService>()),
  );
  getIt.registerLazySingleton<HubRepository>(
    () => HubRepositoryImpl(apiClient: getIt<BffApiClient>()),
  );
  getIt.registerLazySingleton<ListRepository>(
    () => ListRepositoryImpl(apiClient: getIt<BffApiClient>()),
  );
  getIt.registerLazySingleton<PlaybackRepository>(
    () => PlaybackRepositoryImpl(apiClient: getIt<BffApiClient>()),
  );
  getIt.registerLazySingleton<QueueRepository>(
    () => QueueRepositoryImpl(
      apiClient: getIt<BffApiClient>(),
      authService: getIt<AuthService>(),
    ),
  );
  getIt.registerLazySingleton<NowPlayingRepository>(
    () => NowPlayingRepositoryImpl(apiClient: getIt<BffApiClient>()),
  );
  getIt.registerLazySingleton<TrackSearchRepository>(
    () => TrackSearchRepositoryImpl(apiClient: getIt<BffApiClient>()),
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

  // Blocs — AuthBloc is singleton (app-wide auth state)
  getIt.registerLazySingleton<AuthBloc>(
    () => AuthBloc(repository: getIt<AuthRepository>()),
  );
  getIt.registerFactory<HubBloc>(
    () => HubBloc(repository: getIt<HubRepository>()),
  );
  getIt.registerFactory<HubJoinBloc>(
    () => HubJoinBloc(repository: getIt<HubRepository>()),
  );
  getIt.registerFactory<HubSearchBloc>(
    () => HubSearchBloc(repository: getIt<HubRepository>()),
  );
  getIt.registerFactory<HubDetailBloc>(
    () => HubDetailBloc(
      hubRepository: getIt<HubRepository>(),
      listRepository: getIt<ListRepository>(),
    ),
  );
  getIt.registerFactory<ListBloc>(
    () => ListBloc(repository: getIt<ListRepository>()),
  );
  getIt.registerFactory<PlaybackBloc>(
    () => PlaybackBloc(repository: getIt<PlaybackRepository>()),
  );
  getIt.registerFactory<QueueBloc>(
    () => QueueBloc(repository: getIt<QueueRepository>()),
  );
  getIt.registerFactory<SearchBloc>(
    () => SearchBloc(repository: getIt<TrackSearchRepository>()),
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
