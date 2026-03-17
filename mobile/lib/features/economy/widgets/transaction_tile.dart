import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../models/economy_models.dart';

class TransactionTile extends StatelessWidget {
  const TransactionTile({super.key, required this.transaction});

  final Transaction transaction;

  @override
  Widget build(BuildContext context) {
    final isPositive = transaction.type == TransactionType.credit ||
        transaction.type == TransactionType.refund ||
        transaction.type == TransactionType.reward;

    final amountColor = isPositive ? Colors.green : Colors.red;
    final amountPrefix = isPositive ? '+' : '-';

    return ListTile(
      leading: CircleAvatar(
        backgroundColor: _iconColor.withAlpha(30),
        child: Icon(_icon, color: _iconColor, size: 20),
      ),
      title: Text(transaction.reason),
      subtitle: Text(
        DateFormat.yMMMd().add_Hm().format(transaction.createdAt),
        style: Theme.of(context).textTheme.bodySmall,
      ),
      trailing: Text(
        '$amountPrefix${transaction.amount.abs()}',
        style: TextStyle(
          fontWeight: FontWeight.bold,
          fontSize: 16,
          color: amountColor,
        ),
      ),
    );
  }

  IconData get _icon {
    switch (transaction.type) {
      case TransactionType.credit:
        return Icons.add_circle_outline;
      case TransactionType.debit:
        return Icons.remove_circle_outline;
      case TransactionType.refund:
        return Icons.replay;
      case TransactionType.purchase:
        return Icons.shopping_cart_outlined;
      case TransactionType.reward:
        return Icons.star_outline;
    }
  }

  Color get _iconColor {
    switch (transaction.type) {
      case TransactionType.credit:
        return Colors.green;
      case TransactionType.debit:
        return Colors.red;
      case TransactionType.refund:
        return Colors.orange;
      case TransactionType.purchase:
        return Colors.blue;
      case TransactionType.reward:
        return Colors.amber;
    }
  }
}
