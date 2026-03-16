import 'package:freezed_annotation/freezed_annotation.dart';

part 'create_hub_request.freezed.dart';
part 'create_hub_request.g.dart';

@freezed
class CreateHubRequest with _$CreateHubRequest {
  const factory CreateHubRequest({
    required String name,
    @JsonKey(name: 'business_type') required String businessType,
    required List<String> providers,
    @Default('public') String visibility,
  }) = _CreateHubRequest;

  factory CreateHubRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateHubRequestFromJson(json);
}
