import 'package:flutter_bloc/flutter_bloc.dart';

import 'social_profile_event.dart';
import 'social_profile_repository.dart';
import 'social_profile_state.dart';

class SocialProfileBloc
    extends Bloc<SocialProfileEvent, SocialProfileState> {
  SocialProfileBloc({required SocialProfileRepository repository})
      : _repository = repository,
        super(const SocialProfileState.initial()) {
    on<SocialProfileEventLoad>(_onLoad);
    on<SocialProfileEventToggleEdit>(_onToggleEdit);
    on<SocialProfileEventSave>(_onSave);
    on<SocialProfileEventUploadAvatar>(_onUploadAvatar);
  }

  final SocialProfileRepository _repository;

  Future<void> _onLoad(
    SocialProfileEventLoad event,
    Emitter<SocialProfileState> emit,
  ) async {
    emit(const SocialProfileState.loading());
    try {
      final profile = await _repository.getMyProfile();
      emit(SocialProfileState.loaded(profile: profile));
    } on Exception catch (e) {
      emit(SocialProfileState.error(message: e.toString()));
    }
  }

  void _onToggleEdit(
    SocialProfileEventToggleEdit event,
    Emitter<SocialProfileState> emit,
  ) {
    final current = state;
    if (current is SocialProfileStateLoaded) {
      emit(current.copyWith(isEditing: !current.isEditing));
    }
  }

  Future<void> _onSave(
    SocialProfileEventSave event,
    Emitter<SocialProfileState> emit,
  ) async {
    emit(const SocialProfileState.saving());
    try {
      final profile = await _repository.updateProfile(event.fields);
      emit(SocialProfileState.loaded(profile: profile));
    } on Exception catch (e) {
      emit(SocialProfileState.error(message: e.toString()));
    }
  }

  Future<void> _onUploadAvatar(
    SocialProfileEventUploadAvatar event,
    Emitter<SocialProfileState> emit,
  ) async {
    emit(const SocialProfileState.saving());
    try {
      await _repository.uploadAvatar(event.imageBytes, event.fileName);
      final profile = await _repository.getMyProfile();
      emit(SocialProfileState.loaded(profile: profile));
    } on Exception catch (e) {
      emit(SocialProfileState.error(message: e.toString()));
    }
  }
}
