import 'package:freezed_annotation/freezed_annotation.dart';

part 'paged_result.freezed.dart';
part 'paged_result.g.dart';

@Freezed(genericArgumentFactories: true)
class PagedResult<T> with _$PagedResult<T> {
  const factory PagedResult({
    @Default(true) bool success,
    @JsonKey(name: 'total_items_in_response') @Default(0)
    int totalItemsInResponse,
    @JsonKey(name: 'has_more_items') @Default(false) bool hasMoreItems,
    required List<T> items,
  }) = _PagedResult<T>;

  factory PagedResult.fromJson(
    Map<String, dynamic> json,
    T Function(Object?) fromJsonT,
  ) =>
      _$PagedResultFromJson(json, fromJsonT);
}
