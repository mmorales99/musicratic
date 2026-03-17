import 'package:freezed_annotation/freezed_annotation.dart';

import 'profile_data.dart';

part 'social_profile_state.freezed.dart';

@freezed
class SocialProfileState with _$SocialProfileState {
  const factory SocialProfileState.initial() = SocialProfileStateInitial;
  const factory SocialProfileState.loading() = SocialProfileStateLoading;
  const factory SocialProfileState.loaded({
    required ProfileData profile,
    @Default(false) bool isEditing,
  }) = SocialProfileStateLoaded;
  const factory SocialProfileState.saving() = SocialProfileStateSaving;
  const factory SocialProfileState.error({required String message}) =
      SocialProfileStateError;
}
