import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';

import 'package:musicratic_mobile/features/hub/bloc/hub_search_bloc.dart';
import 'package:musicratic_mobile/features/hub/bloc/hub_search_event.dart';
import 'package:musicratic_mobile/features/hub/bloc/hub_search_state.dart';
import 'package:musicratic_mobile/features/hub/models/search_hubs_params.dart';
import 'package:musicratic_mobile/features/hub/repository/hub_repository.dart';
import 'package:musicratic_mobile/shared/models/hub.dart';
import 'package:musicratic_mobile/shared/models/paged_result.dart';

class MockHubRepository extends Mock implements HubRepository {}

void main() {
  late MockHubRepository mockRepository;

  setUp(() {
    mockRepository = MockHubRepository();
    registerFallbackValue(const SearchHubsParams());
  });

  const testHubs = [
    Hub(
      id: 'hub-1',
      name: 'Cafe Luna',
      hubType: 'cafe',
      status: 'active',
      ownerId: 'owner-1',
      isActive: true,
      activeUsersCount: 5,
    ),
    Hub(
      id: 'hub-2',
      name: 'Bar Noir',
      hubType: 'bar',
      status: 'active',
      ownerId: 'owner-2',
      isActive: true,
      activeUsersCount: 12,
    ),
  ];

  PagedResult<Hub> pagedResult({
    List<Hub> items = const [],
    bool hasMore = false,
  }) {
    return PagedResult<Hub>(
      items: items,
      totalItemsInResponse: items.length,
      hasMoreItems: hasMore,
    );
  }

  group('HubSearchBloc', () {
    test('initial state is empty HubSearchState', () {
      final bloc = HubSearchBloc(repository: mockRepository);
      expect(bloc.state, const HubSearchState());
      bloc.close();
    });

    blocTest<HubSearchBloc, HubSearchState>(
      'emits [loading, loaded] on refresh',
      setUp: () {
        when(() => mockRepository.searchHubs(any()))
            .thenAnswer((_) async => pagedResult(items: testHubs));
      },
      build: () => HubSearchBloc(repository: mockRepository),
      act: (bloc) => bloc.add(const HubSearchEvent.refresh()),
      expect: () => [
        const HubSearchState(isLoading: true),
        HubSearchState(hubs: testHubs),
      ],
    );

    blocTest<HubSearchBloc, HubSearchState>(
      'emits error state on refresh failure',
      setUp: () {
        when(() => mockRepository.searchHubs(any()))
            .thenThrow(Exception('Network error'));
      },
      build: () => HubSearchBloc(repository: mockRepository),
      act: (bloc) => bloc.add(const HubSearchEvent.refresh()),
      expect: () => [
        const HubSearchState(isLoading: true),
        const HubSearchState(
          errorMessage: 'Exception: Network error',
        ),
      ],
    );

    blocTest<HubSearchBloc, HubSearchState>(
      'emits updated filter on filterChanged and reloads',
      setUp: () {
        when(() => mockRepository.searchHubs(any()))
            .thenAnswer((_) async => pagedResult(items: [testHubs[0]]));
      },
      build: () => HubSearchBloc(repository: mockRepository),
      act: (bloc) => bloc.add(
        const HubSearchEvent.filterChanged(businessType: 'cafe'),
      ),
      expect: () => [
        const HubSearchState(businessType: 'cafe'),
        const HubSearchState(businessType: 'cafe', isLoading: true),
        HubSearchState(businessType: 'cafe', hubs: [testHubs[0]]),
      ],
    );

    blocTest<HubSearchBloc, HubSearchState>(
      'appends hubs on loadMore',
      setUp: () {
        when(() => mockRepository.searchHubs(any()))
            .thenAnswer((_) async => pagedResult(items: [testHubs[1]]));
      },
      seed: () => HubSearchState(
        hubs: [testHubs[0]],
        currentPage: 1,
        hasMore: true,
      ),
      build: () => HubSearchBloc(repository: mockRepository),
      act: (bloc) => bloc.add(const HubSearchEvent.loadMore()),
      expect: () => [
        HubSearchState(
          hubs: [testHubs[0]],
          currentPage: 1,
          hasMore: true,
          isLoadingMore: true,
        ),
        HubSearchState(
          hubs: testHubs,
          currentPage: 2,
        ),
      ],
    );

    blocTest<HubSearchBloc, HubSearchState>(
      'does not load more when hasMore is false',
      seed: () => HubSearchState(
        hubs: testHubs,
        currentPage: 1,
        hasMore: false,
      ),
      build: () => HubSearchBloc(repository: mockRepository),
      act: (bloc) => bloc.add(const HubSearchEvent.loadMore()),
      expect: () => <HubSearchState>[],
      verify: (_) {
        verifyNever(() => mockRepository.searchHubs(any()));
      },
    );

    blocTest<HubSearchBloc, HubSearchState>(
      'debounces search input',
      setUp: () {
        when(() => mockRepository.searchHubs(any()))
            .thenAnswer((_) async => pagedResult(items: testHubs));
      },
      build: () => HubSearchBloc(repository: mockRepository),
      act: (bloc) async {
        bloc.add(const HubSearchEvent.searchChanged(query: 'ca'));
        bloc.add(const HubSearchEvent.searchChanged(query: 'caf'));
        bloc.add(const HubSearchEvent.searchChanged(query: 'cafe'));
        await Future<void>.delayed(const Duration(milliseconds: 500));
      },
      wait: const Duration(milliseconds: 600),
      verify: (_) {
        // Should only call searchHubs once due to debounce
        verify(() => mockRepository.searchHubs(any())).called(1);
      },
    );
  });
}
