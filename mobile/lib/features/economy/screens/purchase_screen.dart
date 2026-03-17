import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/purchase_bloc.dart';
import '../bloc/purchase_event.dart';
import '../bloc/purchase_state.dart';
import '../models/economy_models.dart';
import '../widgets/coin_package_card.dart';

class PurchaseScreen extends StatefulWidget {
  const PurchaseScreen({super.key});

  @override
  State<PurchaseScreen> createState() => _PurchaseScreenState();
}

class _PurchaseScreenState extends State<PurchaseScreen> {
  @override
  void initState() {
    super.initState();
    context
        .read<PurchaseBloc>()
        .add(const PurchaseEvent.loadPackages());
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Buy Coins')),
      body: BlocConsumer<PurchaseBloc, PurchaseState>(
        listener: _handleStateChange,
        builder: (context, state) {
          return state.when(
            initial: () => const SizedBox.shrink(),
            loading: () =>
                const Center(child: CircularProgressIndicator()),
            packagesLoaded: (packages) =>
                _PackageGrid(packages: packages),
            purchasing: (packages, packageId) =>
                _PackageGrid(
                  packages: packages,
                  purchasingId: packageId,
                ),
            success: (wallet, packages) =>
                _PackageGrid(packages: packages),
            error: (message, packages) => packages != null
                ? _PackageGrid(packages: packages)
                : _ErrorView(
                    message: message,
                    onRetry: () => context
                        .read<PurchaseBloc>()
                        .add(const PurchaseEvent.loadPackages()),
                  ),
          );
        },
      ),
    );
  }

  void _handleStateChange(
    BuildContext context,
    PurchaseState state,
  ) {
    if (state is PurchaseStateSuccess) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'Purchase successful! Balance: '
            '${state.updatedWallet.balance} coins',
          ),
          backgroundColor: Colors.green,
        ),
      );
    } else if (state is PurchaseStateError) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Purchase failed: ${state.message}'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }
}

class _PackageGrid extends StatelessWidget {
  const _PackageGrid({
    required this.packages,
    this.purchasingId,
  });

  final List<CoinPackage> packages;
  final String? purchasingId;

  @override
  Widget build(BuildContext context) {
    if (packages.isEmpty) {
      return const Center(
        child: Text(
          'No packages available',
          style: TextStyle(fontSize: 18, color: Colors.grey),
        ),
      );
    }

    return GridView.builder(
      padding: const EdgeInsets.all(16),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        childAspectRatio: 0.85,
        crossAxisSpacing: 12,
        mainAxisSpacing: 12,
      ),
      itemCount: packages.length,
      itemBuilder: (context, index) {
        final package = packages[index];
        return CoinPackageCard(
          package: package,
          isPurchasing: purchasingId == package.id,
          onBuy: () {
            context.read<PurchaseBloc>().add(
                  PurchaseEvent.purchasePackage(package: package),
                );
          },
        );
      },
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
