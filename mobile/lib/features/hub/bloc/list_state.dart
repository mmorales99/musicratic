import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/list_models.dart';

part 'list_state.freezed.dart';

@freezed
class ListState with _$ListState {
  const factory ListState.initial() = ListStateInitial;
  const factory ListState.loading() = ListStateLoading;
  const factory ListState.loaded({
    required HubList list,
    required List<ListTrack> tracks,
  }) = ListStateLoaded;
  const factory ListState.error({required String message}) = ListStateError;
}
