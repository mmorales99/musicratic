import 'package:freezed_annotation/freezed_annotation.dart';

part 'search_event.freezed.dart';

@freezed
class SearchEvent with _$SearchEvent {
  const factory SearchEvent.queryChanged({required String query}) =
      SearchEventQueryChanged;
  const factory SearchEvent.providerFilterChanged({String? provider}) =
      SearchEventProviderFilterChanged;
  const factory SearchEvent.proposeTrack({
    required String hubId,
    required String trackId,
    required String source,
  }) = SearchEventProposeTrack;
  const factory SearchEvent.clearSearch() = SearchEventClearSearch;
}
