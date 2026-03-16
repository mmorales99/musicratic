import 'package:flutter_bloc/flutter_bloc.dart';

import '../repository/profile_repository.dart';
import 'profile_event.dart';
import 'profile_state.dart';

class ProfileBloc extends Bloc<ProfileEvent, ProfileState> {
  ProfileBloc({required ProfileRepository repository})
      : _repository = repository,
        super(const ProfileState.initial()) {
    on<ProfileEventLoadProfile>(_onLoadProfile);
    on<ProfileEventUpdateProfile>(_onUpdateProfile);
    on<ProfileEventLoadPublicProfile>(_onLoadPublicProfile);
  }

  final ProfileRepository _repository;

  Future<void> _onLoadProfile(
    ProfileEventLoadProfile event,
    Emitter<ProfileState> emit,
  ) async {
    emit(const ProfileState.loading());
    try {
      final profile = await _repository.getMyProfile();
      emit(ProfileState.loaded(profile: profile));
    } on Exception catch (e) {
      emit(ProfileState.error(message: e.toString()));
    }
  }

  Future<void> _onUpdateProfile(
    ProfileEventUpdateProfile event,
    Emitter<ProfileState> emit,
  ) async {
    emit(const ProfileState.loading());
    try {
      await _repository.updateProfile(event.fields);
      final profile = await _repository.getMyProfile();
      emit(ProfileState.loaded(profile: profile));
    } on Exception catch (e) {
      emit(ProfileState.error(message: e.toString()));
    }
  }

  Future<void> _onLoadPublicProfile(
    ProfileEventLoadPublicProfile event,
    Emitter<ProfileState> emit,
  ) async {
    emit(const ProfileState.loading());
    try {
      final profile = await _repository.getPublicProfile(event.userId);
      emit(ProfileState.loaded(profile: profile));
    } on Exception catch (e) {
      emit(ProfileState.error(message: e.toString()));
    }
  }
}
