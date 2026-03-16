import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';

import 'package:musicratic_mobile/features/hub/bloc/hub_join_bloc.dart';
import 'package:musicratic_mobile/features/hub/bloc/hub_join_event.dart';
import 'package:musicratic_mobile/features/hub/bloc/hub_join_state.dart';
import 'package:musicratic_mobile/features/hub/repository/hub_repository.dart';
import 'package:musicratic_mobile/shared/models/hub.dart';

class MockHubRepository extends Mock implements HubRepository {}

void main() {
  late MockHubRepository mockRepository;

  setUp(() {
    mockRepository = MockHubRepository();
  });

  const testHub = Hub(
    id: 'hub-123',
    name: 'Test Hub',
    hubType: 'bar',
    status: 'active',
    ownerId: 'owner-1',
  );

  group('HubJoinBloc', () {
    test('initial state is HubJoinState.initial', () {
      final bloc = HubJoinBloc(repository: mockRepository);
      expect(bloc.state, const HubJoinState.initial());
      bloc.close();
    });

    blocTest<HubJoinBloc, HubJoinState>(
      'emits [codeReady] when code entered',
      build: () => HubJoinBloc(repository: mockRepository),
      act: (bloc) =>
          bloc.add(const HubJoinEvent.codeEntered(code: 'CAFELUNA23')),
      expect: () => [
        const HubJoinState.codeReady(code: 'CAFELUNA23'),
      ],
    );

    blocTest<HubJoinBloc, HubJoinState>(
      'emits [initial] when empty code entered',
      build: () => HubJoinBloc(repository: mockRepository),
      act: (bloc) =>
          bloc.add(const HubJoinEvent.codeEntered(code: '')),
      expect: () => [
        const HubJoinState.initial(),
      ],
    );

    blocTest<HubJoinBloc, HubJoinState>(
      'emits [joining, joined] on successful QR scan with URL',
      setUp: () {
        when(() => mockRepository.attachByCode('CAFELUNA23'))
            .thenAnswer((_) async => testHub);
      },
      build: () => HubJoinBloc(repository: mockRepository),
      act: (bloc) => bloc.add(const HubJoinEvent.qrScanned(
        rawValue: 'https://musicratic.app/join/CAFELUNA23?sig=abc',
      )),
      expect: () => [
        const HubJoinState.joining(code: 'CAFELUNA23'),
        const HubJoinState.joined(hub: testHub),
      ],
    );

    blocTest<HubJoinBloc, HubJoinState>(
      'emits [joining, joined] on successful QR scan with plain code',
      setUp: () {
        when(() => mockRepository.attachByCode('CAFELUNA23'))
            .thenAnswer((_) async => testHub);
      },
      build: () => HubJoinBloc(repository: mockRepository),
      act: (bloc) => bloc.add(const HubJoinEvent.qrScanned(
        rawValue: 'CAFELUNA23',
      )),
      expect: () => [
        const HubJoinState.joining(code: 'CAFELUNA23'),
        const HubJoinState.joined(hub: testHub),
      ],
    );

    blocTest<HubJoinBloc, HubJoinState>(
      'emits [error] on invalid QR code',
      build: () => HubJoinBloc(repository: mockRepository),
      act: (bloc) => bloc.add(const HubJoinEvent.qrScanned(
        rawValue: 'not a valid code!@#',
      )),
      expect: () => [
        const HubJoinState.error(message: 'Invalid QR code'),
      ],
    );

    blocTest<HubJoinBloc, HubJoinState>(
      'emits [error] when joinRequested with no code',
      build: () => HubJoinBloc(repository: mockRepository),
      act: (bloc) =>
          bloc.add(const HubJoinEvent.joinRequested()),
      expect: () => [
        const HubJoinState.error(message: 'Please enter a hub code'),
      ],
    );

    blocTest<HubJoinBloc, HubJoinState>(
      'emits [codeReady, joining, joined] when code entered then join',
      setUp: () {
        when(() => mockRepository.attachByCode('TEST1234'))
            .thenAnswer((_) async => testHub);
      },
      build: () => HubJoinBloc(repository: mockRepository),
      act: (bloc) async {
        bloc.add(const HubJoinEvent.codeEntered(code: 'test1234'));
        await Future<void>.delayed(Duration.zero);
        bloc.add(const HubJoinEvent.joinRequested());
      },
      expect: () => [
        const HubJoinState.codeReady(code: 'TEST1234'),
        const HubJoinState.joining(code: 'TEST1234'),
        const HubJoinState.joined(hub: testHub),
      ],
    );

    blocTest<HubJoinBloc, HubJoinState>(
      'emits [joining, error] when join fails with 404',
      setUp: () {
        when(() => mockRepository.attachByCode('NOPE1234'))
            .thenThrow(Exception('404'));
      },
      build: () => HubJoinBloc(repository: mockRepository),
      act: (bloc) async {
        bloc.add(const HubJoinEvent.codeEntered(code: 'NOPE1234'));
        await Future<void>.delayed(Duration.zero);
        bloc.add(const HubJoinEvent.joinRequested());
      },
      expect: () => [
        const HubJoinState.codeReady(code: 'NOPE1234'),
        const HubJoinState.joining(code: 'NOPE1234'),
        const HubJoinState.error(message: 'Hub not found'),
      ],
    );

    blocTest<HubJoinBloc, HubJoinState>(
      'emits [initial] on reset',
      build: () => HubJoinBloc(repository: mockRepository),
      seed: () =>
          const HubJoinState.error(message: 'Something went wrong'),
      act: (bloc) => bloc.add(const HubJoinEvent.reset()),
      expect: () => [
        const HubJoinState.initial(),
      ],
    );
  });
}
