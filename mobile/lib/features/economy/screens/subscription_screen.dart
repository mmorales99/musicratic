import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/subscription_bloc.dart';
import '../bloc/subscription_event.dart';
import '../bloc/subscription_state.dart';
import '../models/economy_models.dart';
import '../widgets/subscription_tier_card.dart';

class SubscriptionScreen extends StatefulWidget {
  const SubscriptionScreen({super.key, required this.hubId});

  final String hubId;

  @override
  State<SubscriptionScreen> createState() => _SubscriptionScreenState();
}

class _SubscriptionScreenState extends State<SubscriptionScreen> {
  @override
  void initState() {
    super.initState();
    context.read<SubscriptionBloc>().add(
          SubscriptionEvent.loadSubscription(hubId: widget.hubId),
        );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Subscription')),
      body: BlocBuilder<SubscriptionBloc, SubscriptionState>(
        builder: (context, state) {
          return state.when(
            initial: () => const SizedBox.shrink(),
            loading: () =>
                const Center(child: CircularProgressIndicator()),
            loaded: (subscription) => _LoadedView(
              subscription: subscription,
              hubId: widget.hubId,
            ),
            noSubscription: (hubId) => _NoSubscriptionView(
              hubId: hubId,
            ),
            error: (message) => _ErrorView(
              message: message,
              onRetry: () => context.read<SubscriptionBloc>().add(
                    SubscriptionEvent.loadSubscription(
                      hubId: widget.hubId,
                    ),
                  ),
            ),
          );
        },
      ),
    );
  }
}

class _LoadedView extends StatelessWidget {
  const _LoadedView({
    required this.subscription,
    required this.hubId,
  });

  final Subscription subscription;
  final String hubId;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final daysRemaining =
        subscription.expiresAt.difference(DateTime.now()).inDays;
    final trialDaysRemaining = subscription.trialEndsAt != null
        ? subscription.trialEndsAt!.difference(DateTime.now()).inDays
        : null;

    return RefreshIndicator(
      onRefresh: () async {
        context.read<SubscriptionBloc>().add(
              SubscriptionEvent.loadSubscription(hubId: hubId),
            );
      },
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // Current tier badge
          Card(
            child: Padding(
              padding: const EdgeInsets.all(20),
              child: Column(
                children: [
                  Icon(
                    _tierIcon(subscription.tier),
                    size: 48,
                    color: theme.colorScheme.primary,
                  ),
                  const SizedBox(height: 12),
                  Text(
                    _tierLabel(subscription.tier),
                    style: theme.textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  if (subscription.isActive)
                    Chip(
                      label: const Text('Active'),
                      backgroundColor: Colors.green.withAlpha(30),
                      labelStyle:
                          const TextStyle(color: Colors.green),
                    )
                  else
                    Chip(
                      label: const Text('Inactive'),
                      backgroundColor: Colors.red.withAlpha(30),
                      labelStyle: const TextStyle(color: Colors.red),
                    ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),

          // Trial / expiration info
          if (trialDaysRemaining != null && trialDaysRemaining > 0)
            Card(
              color: Colors.amber.withAlpha(30),
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Row(
                  children: [
                    const Icon(Icons.timer_outlined,
                        color: Colors.amber),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        'Trial ends in $trialDaysRemaining days',
                        style: const TextStyle(
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          if (daysRemaining > 0 && trialDaysRemaining == null)
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Row(
                  children: [
                    const Icon(Icons.calendar_today_outlined),
                    const SizedBox(width: 12),
                    Text('$daysRemaining days remaining'),
                  ],
                ),
              ),
            ),
          const SizedBox(height: 24),

          // Tier comparison
          Text(
            'Plans',
            style: theme.textTheme.titleLarge,
          ),
          const SizedBox(height: 12),
          SubscriptionTierCard(
            tierName: 'Free Trial',
            price: '€0',
            duration: '30 days',
            features: const [
              '1 hub',
              '1 list per hub',
              'Spotify only',
              'Basic analytics',
              'Ads shown',
            ],
            isCurrent: subscription.tier == SubscriptionTier.freeTrial,
          ),
          const SizedBox(height: 8),
          SubscriptionTierCard(
            tierName: 'Monthly',
            price: '€14.99/mo',
            duration: 'Rolling',
            features: const [
              'Up to 3 hubs',
              '5 lists per hub',
              'Spotify + YouTube',
              'Full analytics',
              'No ads',
              '2 sub-owners',
            ],
            isCurrent: subscription.tier == SubscriptionTier.monthly,
            onUpgrade: subscription.tier == SubscriptionTier.freeTrial
                ? () => _upgrade(context, SubscriptionTier.monthly)
                : null,
          ),
          const SizedBox(height: 8),
          SubscriptionTierCard(
            tierName: 'Annual',
            price: '€119.99/yr',
            duration: 'Rolling (save 33%)',
            features: const [
              'Up to 10 hubs',
              'Unlimited lists',
              'All music sources + local storage',
              'Full analytics + CSV export',
              'No ads',
              '10 sub-owners',
              'Custom QR branding',
              'API access',
              'Priority support',
            ],
            isCurrent: subscription.tier == SubscriptionTier.annual,
            onUpgrade: subscription.tier != SubscriptionTier.annual
                ? () => _upgrade(context, SubscriptionTier.annual)
                : null,
          ),
        ],
      ),
    );
  }

  void _upgrade(BuildContext context, SubscriptionTier tier) {
    context.read<SubscriptionBloc>().add(
          SubscriptionEvent.upgradeTier(hubId: hubId, tier: tier),
        );
  }

  String _tierLabel(SubscriptionTier tier) {
    switch (tier) {
      case SubscriptionTier.freeTrial:
        return 'Free Trial';
      case SubscriptionTier.monthly:
        return 'Monthly';
      case SubscriptionTier.annual:
        return 'Annual';
      case SubscriptionTier.event:
        return 'Event License';
    }
  }

  IconData _tierIcon(SubscriptionTier tier) {
    switch (tier) {
      case SubscriptionTier.freeTrial:
        return Icons.explore_outlined;
      case SubscriptionTier.monthly:
        return Icons.workspace_premium_outlined;
      case SubscriptionTier.annual:
        return Icons.diamond_outlined;
      case SubscriptionTier.event:
        return Icons.event_outlined;
    }
  }
}

class _NoSubscriptionView extends StatelessWidget {
  const _NoSubscriptionView({required this.hubId});

  final String hubId;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.card_membership_outlined,
              size: 64,
              color: theme.colorScheme.primary,
            ),
            const SizedBox(height: 16),
            const Text(
              'No active subscription',
              style: TextStyle(fontSize: 18),
            ),
            const SizedBox(height: 8),
            const Text(
              'Start a free 30-day trial to unlock all features.',
              textAlign: TextAlign.center,
              style: TextStyle(color: Colors.grey),
            ),
            const SizedBox(height: 24),
            ElevatedButton.icon(
              onPressed: () {
                context.read<SubscriptionBloc>().add(
                      SubscriptionEvent.startTrial(hubId: hubId),
                    );
              },
              icon: const Icon(Icons.rocket_launch_outlined),
              label: const Text('Start Free Trial'),
            ),
          ],
        ),
      ),
    );
  }
}

class _ErrorView extends StatelessWidget {
  const _ErrorView({required this.message, required this.onRetry});

  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 64, color: Colors.red),
            const SizedBox(height: 16),
            Text(message, textAlign: TextAlign.center),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: onRetry,
              child: const Text('Retry'),
            ),
          ],
        ),
      ),
    );
  }
}
