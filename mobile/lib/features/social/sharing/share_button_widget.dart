import 'package:flutter/material.dart';
import 'package:share_plus/share_plus.dart';

import 'share_service.dart';

/// Reusable share button that fetches a share link and invokes
/// the platform share sheet.
class ShareButtonWidget extends StatelessWidget {
  const ShareButtonWidget({
    super.key,
    required this.shareService,
    required this.hubId,
    this.hubName,
    this.icon,
  });

  final ShareService shareService;
  final String hubId;
  final String? hubName;
  final Widget? icon;

  @override
  Widget build(BuildContext context) {
    return IconButton(
      icon: icon ?? const Icon(Icons.share),
      tooltip: 'Share',
      onPressed: () => _share(context),
    );
  }

  Future<void> _share(BuildContext context) async {
    try {
      final link = await shareService.getShareLink(hubId);
      final text = hubName != null
          ? 'Check out "$hubName" on Musicratic! $link'
          : 'Check this out on Musicratic! $link';

      await SharePlus.instance.share(ShareParams(text: text));
    } on Exception catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Could not share: $e')),
        );
      }
    }
  }
}
