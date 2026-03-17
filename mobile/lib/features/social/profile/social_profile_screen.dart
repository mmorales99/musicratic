import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'profile_data.dart';
import 'social_profile_bloc.dart';
import 'social_profile_event.dart';
import 'social_profile_state.dart';

class SocialProfileScreen extends StatelessWidget {
  const SocialProfileScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Profile'),
        actions: [
          BlocBuilder<SocialProfileBloc, SocialProfileState>(
            builder: (context, state) {
              final isLoaded = state is SocialProfileStateLoaded;
              return IconButton(
                icon: Icon(
                  isLoaded && state.isEditing ? Icons.close : Icons.edit,
                ),
                onPressed: isLoaded
                    ? () => context
                        .read<SocialProfileBloc>()
                        .add(const SocialProfileEvent.toggleEdit())
                    : null,
              );
            },
          ),
        ],
      ),
      body: BlocBuilder<SocialProfileBloc, SocialProfileState>(
        builder: (context, state) {
          return state.when(
            initial: () => const Center(child: Text('Load your profile')),
            loading: () => const Center(child: CircularProgressIndicator()),
            loaded: (profile, isEditing) => isEditing
                ? _ProfileEditForm(profile: profile)
                : _ProfileView(profile: profile),
            saving: () => const Center(child: CircularProgressIndicator()),
            error: (message) => Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text('Error: $message'),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () => context
                        .read<SocialProfileBloc>()
                        .add(const SocialProfileEvent.load()),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}

class _ProfileView extends StatelessWidget {
  const _ProfileView({required this.profile});

  final ProfileData profile;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Center(
          child: CircleAvatar(
            radius: 50,
            backgroundImage:
                profile.avatar != null ? NetworkImage(profile.avatar!) : null,
            child: profile.avatar == null
                ? Text(
                    profile.displayName[0].toUpperCase(),
                    style: const TextStyle(fontSize: 36),
                  )
                : null,
          ),
        ),
        const SizedBox(height: 16),
        Center(
          child: Text(
            profile.displayName,
            style: theme.textTheme.headlineSmall,
          ),
        ),
        if (profile.bio != null && profile.bio!.isNotEmpty) ...[
          const SizedBox(height: 8),
          Center(
            child: Text(profile.bio!, style: theme.textTheme.bodyMedium),
          ),
        ],
        const SizedBox(height: 24),
        _StatRow(
          items: [
            _StatItem('Tracks', profile.totalTracksProposed.toString()),
            _StatItem('Upvotes', profile.totalUpvotesReceived.toString()),
            _StatItem('Hubs', profile.hubsVisitedCount.toString()),
          ],
        ),
        if (profile.favoriteGenres.isNotEmpty) ...[
          const SizedBox(height: 24),
          Text('Favorite Genres', style: theme.textTheme.titleMedium),
          const SizedBox(height: 8),
          Wrap(
            spacing: 8,
            children: profile.favoriteGenres
                .map((g) => Chip(label: Text(g)))
                .toList(),
          ),
        ],
        if (profile.memberSince != null) ...[
          const SizedBox(height: 16),
          Text(
            'Member since ${profile.memberSince!.year}',
            style: theme.textTheme.bodySmall,
            textAlign: TextAlign.center,
          ),
        ],
      ],
    );
  }
}

class _StatRow extends StatelessWidget {
  const _StatRow({required this.items});

  final List<_StatItem> items;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
      children: items
          .map((item) => Column(
                children: [
                  Text(
                    item.value,
                    style: Theme.of(context)
                        .textTheme
                        .titleLarge
                        ?.copyWith(fontWeight: FontWeight.bold),
                  ),
                  Text(item.label,
                      style: Theme.of(context).textTheme.bodySmall),
                ],
              ))
          .toList(),
    );
  }
}

class _StatItem {
  const _StatItem(this.label, this.value);
  final String label;
  final String value;
}

class _ProfileEditForm extends StatefulWidget {
  const _ProfileEditForm({required this.profile});

  final ProfileData profile;

  @override
  State<_ProfileEditForm> createState() => _ProfileEditFormState();
}

class _ProfileEditFormState extends State<_ProfileEditForm> {
  late final TextEditingController _nameController;
  late final TextEditingController _bioController;

  @override
  void initState() {
    super.initState();
    _nameController =
        TextEditingController(text: widget.profile.displayName);
    _bioController = TextEditingController(text: widget.profile.bio ?? '');
  }

  @override
  void dispose() {
    _nameController.dispose();
    _bioController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Center(
          child: GestureDetector(
            onTap: _pickAvatar,
            child: Stack(
              children: [
                CircleAvatar(
                  radius: 50,
                  backgroundImage: widget.profile.avatar != null
                      ? NetworkImage(widget.profile.avatar!)
                      : null,
                  child: widget.profile.avatar == null
                      ? Text(
                          widget.profile.displayName[0].toUpperCase(),
                          style: const TextStyle(fontSize: 36),
                        )
                      : null,
                ),
                const Positioned(
                  bottom: 0,
                  right: 0,
                  child: CircleAvatar(
                    radius: 16,
                    child: Icon(Icons.camera_alt, size: 16),
                  ),
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 24),
        TextField(
          controller: _nameController,
          decoration: const InputDecoration(
            labelText: 'Display Name',
            border: OutlineInputBorder(),
          ),
        ),
        const SizedBox(height: 16),
        TextField(
          controller: _bioController,
          decoration: const InputDecoration(
            labelText: 'Bio',
            border: OutlineInputBorder(),
          ),
          maxLines: 3,
        ),
        const SizedBox(height: 24),
        ElevatedButton(
          onPressed: _save,
          child: const Text('Save'),
        ),
      ],
    );
  }

  void _pickAvatar() {
    // Avatar picking handled by platform image picker
  }

  void _save() {
    context.read<SocialProfileBloc>().add(
          SocialProfileEvent.save(fields: {
            'display_name': _nameController.text.trim(),
            'bio': _bioController.text.trim(),
          }),
        );
  }
}
