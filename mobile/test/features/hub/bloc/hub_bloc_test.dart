import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';

import 'package:musicratic_mobile/features/hub/bloc/hub_bloc.dart';
import 'package:musicratic_mobile/features/hub/bloc/hub_event.dart';
import 'package:musicratic_mobile/features/hub/bloc/hub_state.dart';
import 'package:musicratic_mobile/features/hub/repository/hub_repository.dart';

class MockHubRepository extends Mock implements HubRepository {}

void main() {
  late MockHubRepository mockRepository;

  setUp(() {
    mockRepository = MockHubRepository();
  });

  group('HubBloc', () {
    test('initial state is HubState.initial', () {
      final bloc = HubBloc(repository: mockRepository);
      expect(bloc.state, const HubState.initial());
      bloc.close();
    });

    blocTest<HubBloc, HubState>(
      'emits [loading, loaded] when loadHubs succeeds',
      setUp: () {
        when(() => mockRepository.getActiveHubs())
            .thenAnswer((_) async => ['hub-1', 'hub-2']);
      },
      build: () => HubBloc(repository: mockRepository),
      act: (bloc) => bloc.add(const HubEvent.loadHubs()),
      expect: () => [
        const HubState.loading(),
        const HubState.loaded(hubs: ['hub-1', 'hub-2']),
      ],
    );

    blocTest<HubBloc, HubState>(
      'emits [loading, error] when loadHubs fails',
      setUp: () {
        when(() => mockRepository.getActiveHubs())
            .thenThrow(Exception('Network error'));
      },
      build: () => HubBloc(repository: mockRepository),
      act: (bloc) => bloc.add(const HubEvent.loadHubs()),
      expect: () => [
        const HubState.loading(),
        const HubState.error(message: 'Exception: Network error'),
      ],
    );

    blocTest<HubBloc, HubState>(
      'emits [attached] when attachToHub succeeds',
      setUp: () {
        when(() => mockRepository.attachToHub(any()))
            .thenAnswer((_) async {});
      },
      build: () => HubBloc(repository: mockRepository),
      act: (bloc) =>
          bloc.add(const HubEvent.attachToHub(hubId: 'hub-123')),
      expect: () => [
        const HubState.attached(hubId: 'hub-123'),
      ],
    );

    blocTest<HubBloc, HubState>(
      'emits [loading, loaded] when detachFromHub succeeds then reloads',
      setUp: () {
        when(() => mockRepository.detachFromHub())
            .thenAnswer((_) async {});
        when(() => mockRepository.getActiveHubs())
            .thenAnswer((_) async => []);
      },
      build: () => HubBloc(repository: mockRepository),
      act: (bloc) => bloc.add(const HubEvent.detachFromHub()),
      expect: () => [
        const HubState.loading(),
        const HubState.loaded(hubs: []),
      ],
    );
  });
}
