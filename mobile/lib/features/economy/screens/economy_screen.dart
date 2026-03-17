import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class EconomyScreen extends StatelessWidget {
  const EconomyScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Economy')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          _EconomyNavCard(
            icon: Icons.account_balance_wallet,
            title: 'Wallet',
            subtitle: 'View balance and transaction history',
            onTap: () => context.push('/economy/wallet'),
          ),
          const SizedBox(height: 12),
          _EconomyNavCard(
            icon: Icons.monetization_on,
            title: 'Buy Coins',
            subtitle: 'Purchase coin packages',
            onTap: () => context.push('/economy/purchase'),
          ),
        ],
      ),
    );
  }
}

class _EconomyNavCard extends StatelessWidget {
  const _EconomyNavCard({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      child: ListTile(
        leading: Icon(icon, color: theme.colorScheme.primary, size: 32),
        title: Text(title, style: const TextStyle(fontWeight: FontWeight.bold)),
        subtitle: Text(subtitle),
        trailing: const Icon(Icons.chevron_right),
        onTap: onTap,
      ),
    );
  }
}
