import 'package:flutter/material.dart';

/// Reusable star rating widget, supports interactive and readonly modes.
class StarRatingWidget extends StatelessWidget {
  const StarRatingWidget({
    super.key,
    required this.rating,
    this.maxRating = 5,
    this.onRatingChanged,
    this.size = 28,
    this.color,
    this.unselectedColor,
  });

  final int rating;
  final int maxRating;

  /// If null, the widget is in readonly mode.
  final ValueChanged<int>? onRatingChanged;
  final double size;
  final Color? color;
  final Color? unselectedColor;

  bool get isInteractive => onRatingChanged != null;

  @override
  Widget build(BuildContext context) {
    final activeColor = color ?? Colors.amber;
    final inactiveColor = unselectedColor ?? Colors.grey.shade300;

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: List.generate(maxRating, (index) {
        final starIndex = index + 1;
        final isFilled = starIndex <= rating;
        return GestureDetector(
          onTap: isInteractive ? () => onRatingChanged!(starIndex) : null,
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 2),
            child: Icon(
              isFilled ? Icons.star : Icons.star_border,
              size: size,
              color: isFilled ? activeColor : inactiveColor,
            ),
          ),
        );
      }),
    );
  }
}
