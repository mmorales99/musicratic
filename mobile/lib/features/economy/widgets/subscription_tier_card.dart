import 'package:flutter/material.dart';

class SubscriptionTierCard extends StatelessWidget {
  const SubscriptionTierCard({
    super.key,
    required this.tierName,
    required this.price,
    required this.duration,
    required this.features,
    this.isCurrent = false,
    this.onUpgrade,
  });

  final String tierName;
  final String price;
  final String duration;
  final List<String> features;
  final bool isCurrent;
  final VoidCallback? onUpgrade;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final borderColor = isCurrent
        ? theme.colorScheme.primary
        : theme.colorScheme.outlineVariant;

    return Card(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(
          color: borderColor,
          width: isCurrent ? 2 : 1,
        ),
      ),
      child: ExpansionTile(
        initiallyExpanded: isCurrent,
        leading: isCurrent
            ? Icon(Icons.check_circle, color: theme.colorScheme.primary)
            : const Icon(Icons.circle_outlined),
        title: Row(
          children: [
            Text(
              tierName,
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
            const SizedBox(width: 8),
            if (isCurrent)
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 8,
                  vertical: 2,
                ),
                decoration: BoxDecoration(
                  color: theme.colorScheme.primary.withAlpha(30),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Text(
                  'Current',
                  style: TextStyle(
                    fontSize: 11,
                    color: theme.colorScheme.primary,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
          ],
        ),
        subtitle: Text('$price · $duration'),
        children: [
          Padding(
            padding:
                const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                ...features.map(
                  (f) => Padding(
                    padding: const EdgeInsets.symmetric(vertical: 2),
                    child: Row(
                      children: [
                        const Icon(Icons.check, size: 16,
                            color: Colors.green),
                        const SizedBox(width: 8),
                        Expanded(child: Text(f)),
                      ],
                    ),
                  ),
                ),
                if (onUpgrade != null) ...[
                  const SizedBox(height: 12),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: onUpgrade,
                      child: const Text('Upgrade'),
                    ),
                  ),
                ],
                const SizedBox(height: 8),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
