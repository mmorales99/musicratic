import 'package:flutter/material.dart';

class ManualCodeEntryWidget extends StatefulWidget {
  const ManualCodeEntryWidget({
    super.key,
    required this.onCodeChanged,
    required this.onJoinPressed,
    required this.isLoading,
    this.errorMessage,
  });

  final void Function(String code) onCodeChanged;
  final VoidCallback onJoinPressed;
  final bool isLoading;
  final String? errorMessage;

  @override
  State<ManualCodeEntryWidget> createState() => _ManualCodeEntryWidgetState();
}

class _ManualCodeEntryWidgetState extends State<ManualCodeEntryWidget> {
  final _controller = TextEditingController();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.qr_code_2,
            size: 80,
            color: theme.colorScheme.primary.withValues(alpha: 0.6),
          ),
          const SizedBox(height: 24),
          Text(
            'Enter Hub Code',
            style: theme.textTheme.headlineSmall,
          ),
          const SizedBox(height: 8),
          Text(
            'Enter the code displayed at the venue or shared by the hub owner',
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 32),
          TextField(
            controller: _controller,
            textCapitalization: TextCapitalization.characters,
            textAlign: TextAlign.center,
            style: theme.textTheme.headlineMedium?.copyWith(
              letterSpacing: 4,
              fontWeight: FontWeight.bold,
            ),
            decoration: InputDecoration(
              hintText: 'CAFELUNA23',
              hintStyle: theme.textTheme.headlineMedium?.copyWith(
                letterSpacing: 4,
                color: theme.colorScheme.outline,
              ),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
              ),
              filled: true,
              fillColor: theme.colorScheme.surfaceContainerHighest,
              errorText: widget.errorMessage,
            ),
            onChanged: widget.onCodeChanged,
            onSubmitted: (_) => widget.onJoinPressed(),
          ),
          const SizedBox(height: 24),
          SizedBox(
            width: double.infinity,
            height: 48,
            child: FilledButton.icon(
              onPressed: widget.isLoading ? null : widget.onJoinPressed,
              icon: widget.isLoading
                  ? const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.login),
              label: Text(widget.isLoading ? 'Joining...' : 'Join Hub'),
            ),
          ),
        ],
      ),
    );
  }
}
