import 'package:freezed_annotation/freezed_annotation.dart';

part 'search_hubs_params.freezed.dart';

@freezed
class SearchHubsParams with _$SearchHubsParams {
  const factory SearchHubsParams({
    String? name,
    @JsonKey(name: 'business_type') String? businessType,
    String? visibility,
    @Default(1) int page,
    @JsonKey(name: 'page_size') @Default(20) int pageSize,
  }) = _SearchHubsParams;

  const SearchHubsParams._();

  Map<String, dynamic> toQueryParameters() {
    final params = <String, dynamic>{};
    if (name != null && name!.isNotEmpty) params['name'] = name;
    if (businessType != null) params['business_type'] = businessType;
    if (visibility != null) params['visibility'] = visibility;
    params['page'] = page.toString();
    params['page_size'] = pageSize.toString();
    return params;
  }
}
