import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../bloc/hub_bloc.dart';
import '../bloc/hub_event.dart';
import '../bloc/hub_state.dart';
import '../models/create_hub_request.dart';

class HubCreateScreen extends StatefulWidget {
  const HubCreateScreen({super.key});

  @override
  State<HubCreateScreen> createState() => _HubCreateScreenState();
}

class _HubCreateScreenState extends State<HubCreateScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  String _businessType = 'venue';
  String _visibility = 'public';
  final Set<String> _selectedProviders = {'spotify'};

  static const _providerOptions = [
    ('spotify', 'Spotify'),
    ('youtube_music', 'YouTube Music'),
    ('local', 'Local Library'),
  ];

  @override
  void dispose() {
    _nameController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Create Hub')),
      body: BlocConsumer<HubBloc, HubState>(
        listener: _onStateChanged,
        builder: (context, state) {
          final isCreating = state is HubStateCreating;
          return Form(
            key: _formKey,
            child: ListView(
              padding: const EdgeInsets.all(16),
              children: [
                _buildNameField(isCreating),
                const SizedBox(height: 16),
                _buildTypeDropdown(isCreating),
                const SizedBox(height: 16),
                _buildProvidersSection(isCreating),
                const SizedBox(height: 16),
                _buildVisibilitySection(isCreating),
                const SizedBox(height: 32),
                _buildSubmitButton(context, isCreating),
              ],
            ),
          );
        },
      ),
    );
  }

  void _onStateChanged(BuildContext context, HubState state) {
    if (state is HubStateCreated) {
      context.go('/hub/${state.hub.id}');
    }
    if (state is HubStateError) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(state.message),
          backgroundColor: Theme.of(context).colorScheme.error,
        ),
      );
    }
  }

  Widget _buildNameField(bool disabled) {
    return TextFormField(
      controller: _nameController,
      enabled: !disabled,
      decoration: const InputDecoration(
        labelText: 'Hub Name',
        hintText: 'Enter hub name (3-50 characters)',
        border: OutlineInputBorder(),
        prefixIcon: Icon(Icons.music_note),
      ),
      maxLength: 50,
      validator: (value) {
        if (value == null || value.trim().isEmpty) {
          return 'Hub name is required';
        }
        if (value.trim().length < 3) {
          return 'Name must be at least 3 characters';
        }
        if (value.trim().length > 50) {
          return 'Name must be at most 50 characters';
        }
        return null;
      },
    );
  }

  Widget _buildTypeDropdown(bool disabled) {
    return DropdownButtonFormField<String>(
      value: _businessType,
      decoration: const InputDecoration(
        labelText: 'Business Type',
        border: OutlineInputBorder(),
        prefixIcon: Icon(Icons.business),
      ),
      items: const [
        DropdownMenuItem(value: 'venue', child: Text('Venue')),
        DropdownMenuItem(value: 'portable', child: Text('Portable')),
      ],
      onChanged: disabled
          ? null
          : (value) {
              if (value != null) {
                setState(() => _businessType = value);
              }
            },
    );
  }

  Widget _buildProvidersSection(bool disabled) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Music Providers',
          style: Theme.of(context).textTheme.titleSmall,
        ),
        const SizedBox(height: 8),
        ..._providerOptions.map(
          (option) => CheckboxListTile(
            title: Text(option.$2),
            value: _selectedProviders.contains(option.$1),
            onChanged: disabled
                ? null
                : (checked) {
                    setState(() {
                      if (checked == true) {
                        _selectedProviders.add(option.$1);
                      } else {
                        _selectedProviders.remove(option.$1);
                      }
                    });
                  },
            controlAffinity: ListTileControlAffinity.leading,
            contentPadding: EdgeInsets.zero,
            dense: true,
          ),
        ),
        if (_selectedProviders.isEmpty)
          Padding(
            padding: const EdgeInsets.only(top: 4),
            child: Text(
              'Select at least one provider',
              style: TextStyle(
                color: Theme.of(context).colorScheme.error,
                fontSize: 12,
              ),
            ),
          ),
      ],
    );
  }

  Widget _buildVisibilitySection(bool disabled) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('Visibility', style: Theme.of(context).textTheme.titleSmall),
        const SizedBox(height: 8),
        RadioListTile<String>(
          title: const Text('Public'),
          subtitle: const Text('Discoverable by nearby users'),
          value: 'public',
          groupValue: _visibility,
          onChanged: disabled
              ? null
              : (value) => setState(() => _visibility = value!),
          contentPadding: EdgeInsets.zero,
          dense: true,
        ),
        RadioListTile<String>(
          title: const Text('Private'),
          subtitle: const Text('Only accessible via QR code or link'),
          value: 'private',
          groupValue: _visibility,
          onChanged: disabled
              ? null
              : (value) => setState(() => _visibility = value!),
          contentPadding: EdgeInsets.zero,
          dense: true,
        ),
      ],
    );
  }

  Widget _buildSubmitButton(BuildContext context, bool isCreating) {
    return SizedBox(
      width: double.infinity,
      child: FilledButton.icon(
        onPressed: isCreating ? null : () => _onSubmit(context),
        icon: isCreating
            ? const SizedBox(
                width: 18,
                height: 18,
                child: CircularProgressIndicator(strokeWidth: 2),
              )
            : const Icon(Icons.add),
        label: Text(isCreating ? 'Creating...' : 'Create Hub'),
      ),
    );
  }

  void _onSubmit(BuildContext context) {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedProviders.isEmpty) return;

    final request = CreateHubRequest(
      name: _nameController.text.trim(),
      businessType: _businessType,
      providers: _selectedProviders.toList(),
      visibility: _visibility,
    );

    context.read<HubBloc>().add(HubEvent.createHub(request: request));
  }
}
