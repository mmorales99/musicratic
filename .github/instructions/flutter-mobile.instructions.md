---
description: "Use when writing Flutter Dart code for Musicratic: screens, widgets, blocs, freezed models, repositories, or any mobile client code."
applyTo: "mobile/**/*.dart"
---

# Flutter Mobile Standards

## State Management

- `flutter_bloc` + `freezed` — no `setState()`, no `ChangeNotifier`.
- One Bloc per feature: `{feature}_bloc.dart`, `{feature}_event.dart`, `{feature}_state.dart`.
- States and events immutable via freezed.
- Mirror Angular XState machine logic — same states, same transitions.

## Architecture

- Feature-first: `features/{name}/bloc/`, `screens/`, `widgets/`, `repository/`, `models/`.
- Shared: `shared/api/`, `shared/widgets/`, `shared/services/`.
- Repository pattern for BFF API communication.

## BFF Communication

- JSON-RPC for commands, REST for CRUD, GraphQL for search.
- WebSocket for real-time updates.
- Mobile client talks ONLY to BFF.Mobile.

## Conventions

- File naming: `snake_case.dart` (e.g., `hub_discovery_screen.dart`).
- Class naming: `PascalCase`.
- Method naming: `camelCase`.
- Max ~300 lines per file.
- Strict analysis: `always_declare_return_types`, `avoid_dynamic_calls`, `prefer_const_constructors`.

## Testing

- `flutter_test` for unit + widget tests.
- Playwright for E2E.
- 100% coverage enforced.
