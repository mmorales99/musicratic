import 'dart:typed_data';

import 'package:freezed_annotation/freezed_annotation.dart';

part 'social_profile_event.freezed.dart';

@freezed
class SocialProfileEvent with _$SocialProfileEvent {
  const factory SocialProfileEvent.load() = SocialProfileEventLoad;
  const factory SocialProfileEvent.toggleEdit() = SocialProfileEventToggleEdit;
  const factory SocialProfileEvent.save({
    required Map<String, dynamic> fields,
  }) = SocialProfileEventSave;
  const factory SocialProfileEvent.uploadAvatar({
    required Uint8List imageBytes,
    required String fileName,
  }) = SocialProfileEventUploadAvatar;
}
