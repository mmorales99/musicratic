import 'package:flutter/material.dart';

import '../../../shared/models/hub.dart';

class HubCard extends StatelessWidget {
  const HubCard({super.key, required this.hub, required this.onTap});

  final Hub hub;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              _HubAvatar(hub: hub, colorScheme: colorScheme),
              const SizedBox(width: 16),
              Expanded(child: _HubInfo(hub: hub, theme: theme)),
              const SizedBox(width: 8),
              _HubTrailing(hub: hub, colorScheme: colorScheme),
            ],
          ),
        ),
      ),
    );
  }
}

class _HubAvatar extends StatelessWidget {
  const _HubAvatar({required this.hub, required this.colorScheme});

  final Hub hub;
  final ColorScheme colorScheme;

  @override
  Widget build(BuildContext context) {
    return CircleAvatar(
      radius: 24,
      backgroundColor:
          hub.isActive ? colorScheme.primaryContainer : colorScheme.surfaceContainerHighest,
      child: Icon(
        _iconForType(hub.hubType),
        color: hub.isActive ? colorScheme.onPrimaryContainer : colorScheme.outline,
      ),
    );
  }

  static IconData _iconForType(String type) {
    switch (type.toLowerCase()) {
      case 'bar':
      case 'pub':
        return Icons.local_bar;
      case 'cafe':
      case 'coffee':
        return Icons.coffee;
      case 'gym':
      case 'fitness':
        return Icons.fitness_center;
      case 'restaurant':
        return Icons.restaurant;
      case 'retail':
      case 'shop':
        return Icons.storefront;
      default:
        return Icons.music_note;
    }
  }
}

class _HubInfo extends StatelessWidget {
  const _HubInfo({required this.hub, required this.theme});

  final Hub hub;
  final ThemeData theme;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          hub.name,
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.w600,
          ),
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
        const SizedBox(height: 4),
        Row(
          children: [
            Text(
              hub.hubType,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(width: 8),
            if (hub.isActive)
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                decoration: BoxDecoration(
                  color: Colors.green.withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(4),
                ),
                child: Text(
                  'Active',
                  style: theme.textTheme.labelSmall?.copyWith(
                    color: Colors.green.shade700,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
          ],
        ),
        const SizedBox(height: 4),
        Row(
          children: [
            Icon(Icons.people_outline,
                size: 14, color: theme.colorScheme.outline),
            const SizedBox(width: 4),
            Text(
              '${hub.activeUsersCount}',
              style: theme.textTheme.bodySmall,
            ),
            if (hub.averageRating != null) ...[
              const SizedBox(width: 12),
              Icon(Icons.star, size: 14, color: Colors.amber.shade700),
              const SizedBox(width: 2),
              Text(
                hub.averageRating!.toStringAsFixed(1),
                style: theme.textTheme.bodySmall,
              ),
            ],
          ],
        ),
      ],
    );
  }
}

class _HubTrailing extends StatelessWidget {
  const _HubTrailing({required this.hub, required this.colorScheme});

  final Hub hub;
  final ColorScheme colorScheme;

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(
          hub.visibility == 'public' ? Icons.public : Icons.lock_outline,
          size: 16,
          color: colorScheme.outline,
        ),
        const SizedBox(height: 4),
        Icon(Icons.chevron_right, color: colorScheme.outline),
      ],
    );
  }
}
