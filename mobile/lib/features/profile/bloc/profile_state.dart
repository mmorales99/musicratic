import 'package:freezed_annotation/freezed_annotation.dart';

part 'profile_state.freezed.dart';

@freezed
class ProfileState with _$ProfileState {
  const factory ProfileState.initial() = ProfileStateInitial;
  const factory ProfileState.loading() = ProfileStateLoading;
  const factory ProfileState.loaded({
    required Map<String, dynamic> profile,
  }) = ProfileStateLoaded;
  const factory ProfileState.error({required String message}) =
      ProfileStateError;
}
