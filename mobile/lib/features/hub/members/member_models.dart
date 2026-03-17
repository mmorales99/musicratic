import 'package:freezed_annotation/freezed_annotation.dart';

part 'member_models.freezed.dart';
part 'member_models.g.dart';

@freezed
class HubMember with _$HubMember {
  const factory HubMember({
    required String id,
    @JsonKey(name: 'user_id') required String userId,
    @JsonKey(name: 'display_name') required String displayName,
    String? avatar,
    required String role,
    @JsonKey(name: 'joined_at') DateTime? joinedAt,
    @JsonKey(name: 'is_active') @Default(true) bool isActive,
  }) = _HubMember;

  factory HubMember.fromJson(Map<String, dynamic> json) =>
      _$HubMemberFromJson(json);
}
