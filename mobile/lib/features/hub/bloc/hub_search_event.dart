import 'package:freezed_annotation/freezed_annotation.dart';

part 'hub_search_event.freezed.dart';

@freezed
class HubSearchEvent with _$HubSearchEvent {
  const factory HubSearchEvent.searchChanged({required String query}) =
      HubSearchChanged;
  const factory HubSearchEvent.filterChanged({
    String? businessType,
    String? visibility,
  }) = HubSearchFilterChanged;
  const factory HubSearchEvent.loadMore() = HubSearchLoadMore;
  const factory HubSearchEvent.refresh() = HubSearchRefresh;
}
