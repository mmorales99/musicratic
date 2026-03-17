import 'package:flutter/material.dart';

import '../models/economy_models.dart';

class CoinPackageCard extends StatelessWidget {
  const CoinPackageCard({
    super.key,
    required this.package,
    required this.onBuy,
    this.isPurchasing = false,
  });

  final CoinPackage package;
  final VoidCallback onBuy;
  final bool isPurchasing;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            if (package.bonusCoins > 0)
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 8,
                  vertical: 2,
                ),
                decoration: BoxDecoration(
                  color: Colors.amber,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(
                  '+${package.bonusCoins} bonus',
                  style: const TextStyle(
                    fontSize: 11,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87,
                  ),
                ),
              ),
            const SizedBox(height: 8),
            Icon(
              Icons.monetization_on,
              size: 40,
              color: theme.colorScheme.primary,
            ),
            const SizedBox(height: 8),
            Text(
              '${package.coinAmount}',
              style: theme.textTheme.headlineSmall?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            Text(
              package.name,
              style: theme.textTheme.bodySmall,
            ),
            const Spacer(),
            SizedBox(
              width: double.infinity,
              child: isPurchasing
                  ? const Center(
                      child: SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      ),
                    )
                  : ElevatedButton(
                      onPressed: onBuy,
                      child: Text(
                        '\$${package.priceUsd.toStringAsFixed(2)}',
                      ),
                    ),
            ),
          ],
        ),
      ),
    );
  }
}
