import 'package:flutter/material.dart';

class HubFilterChips extends StatelessWidget {
  const HubFilterChips({
    super.key,
    this.selectedBusinessType,
    this.selectedVisibility,
    required this.onBusinessTypeChanged,
    required this.onVisibilityChanged,
  });

  final String? selectedBusinessType;
  final String? selectedVisibility;
  final void Function(String?) onBusinessTypeChanged;
  final void Function(String?) onVisibilityChanged;

  static const _businessTypes = [
    'Bar',
    'Cafe',
    'Restaurant',
    'Gym',
    'Retail',
    'Other',
  ];

  static const _visibilities = ['public', 'private'];

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Row(
        children: [
          ..._businessTypes.map((type) => Padding(
                padding: const EdgeInsets.only(right: 8),
                child: FilterChip(
                  label: Text(type),
                  selected: selectedBusinessType == type.toLowerCase(),
                  onSelected: (selected) {
                    onBusinessTypeChanged(
                      selected ? type.toLowerCase() : null,
                    );
                  },
                ),
              )),
          const VerticalDivider(width: 24, thickness: 1, indent: 8, endIndent: 8),
          ..._visibilities.map((vis) => Padding(
                padding: const EdgeInsets.only(right: 8),
                child: FilterChip(
                  label: Text(vis[0].toUpperCase() + vis.substring(1)),
                  selected: selectedVisibility == vis,
                  onSelected: (selected) {
                    onVisibilityChanged(selected ? vis : null);
                  },
                ),
              )),
        ],
      ),
    );
  }
}
