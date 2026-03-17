import 'package:flutter/material.dart';

import '../models/economy_models.dart';

class TransactionFilterChips extends StatelessWidget {
  const TransactionFilterChips({
    super.key,
    required this.activeFilter,
    required this.onFilterChanged,
  });

  final TransactionType? activeFilter;
  final ValueChanged<TransactionType?> onFilterChanged;

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Row(
        children: [
          _chip(context, label: 'All', value: null),
          const SizedBox(width: 8),
          _chip(context,
              label: 'Credit', value: TransactionType.credit),
          const SizedBox(width: 8),
          _chip(context,
              label: 'Debit', value: TransactionType.debit),
          const SizedBox(width: 8),
          _chip(context,
              label: 'Refund', value: TransactionType.refund),
          const SizedBox(width: 8),
          _chip(context,
              label: 'Purchase', value: TransactionType.purchase),
          const SizedBox(width: 8),
          _chip(context,
              label: 'Reward', value: TransactionType.reward),
        ],
      ),
    );
  }

  Widget _chip(
    BuildContext context, {
    required String label,
    required TransactionType? value,
  }) {
    final isSelected = activeFilter == value;
    return FilterChip(
      label: Text(label),
      selected: isSelected,
      onSelected: (_) => onFilterChanged(value),
    );
  }
}
