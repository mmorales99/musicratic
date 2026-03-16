import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../bloc/economy_bloc.dart';
import '../bloc/economy_state.dart';

class EconomyScreen extends StatelessWidget {
  const EconomyScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Wallet'),
      ),
      body: BlocBuilder<EconomyBloc, EconomyState>(
        builder: (context, state) {
          return state.when(
            initial: () => const Center(
              child: Text('Load your wallet'),
            ),
            loading: () => const Center(
              child: CircularProgressIndicator(),
            ),
            walletLoaded: (balance) => Center(
              child: Text('Balance: $balance coins'),
            ),
            transactionsLoaded: (transactions) => ListView.builder(
              itemCount: transactions.length,
              itemBuilder: (context, index) {
                return ListTile(
                  title: Text('Transaction ${transactions[index]}'),
                );
              },
            ),
            error: (message) => Center(
              child: Text('Error: $message'),
            ),
          );
        },
      ),
    );
  }
}
