import 'dart:typed_data';

import '../../../shared/api/bff_api_client.dart';
import 'profile_data.dart';

abstract class SocialProfileRepository {
  Future<ProfileData> getMyProfile();
  Future<ProfileData> updateProfile(Map<String, dynamic> fields);
  Future<String> uploadAvatar(Uint8List imageBytes, String fileName);
}

class SocialProfileRepositoryImpl implements SocialProfileRepository {
  SocialProfileRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<ProfileData> getMyProfile() async {
    final response = await _apiClient.get('/profile/me');
    return ProfileData.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<ProfileData> updateProfile(Map<String, dynamic> fields) async {
    final response = await _apiClient.put('/profile/me', data: fields);
    return ProfileData.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<String> uploadAvatar(Uint8List imageBytes, String fileName) async {
    final response = await _apiClient.post(
      '/profile/me/avatar',
      data: {
        'file_name': fileName,
        'file_data': imageBytes.toList(),
      },
    );
    final data = response.data as Map<String, dynamic>;
    return data['avatar_url'] as String;
  }
}
