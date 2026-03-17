import {
  Component,
  ChangeDetectionStrategy,
  inject,
  input,
  signal,
} from "@angular/core";
import { SharingService } from "../../services/sharing.service";
import { ShareType, ShareLink } from "../../models/share.model";
import { firstValueFrom } from "rxjs";

@Component({
  selector: "app-share-button",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      class="share-btn"
      [class.share-btn--copied]="copied()"
      [disabled]="sharing()"
      (click)="onShare()"
      [attr.aria-label]="'Share ' + shareType()"
    >
      @if (sharing()) {
        <span class="share-btn__spinner">⏳</span>
      } @else if (copied()) {
        <span>✓ Link Copied</span>
      } @else if (canNativeShare) {
        <span>📤 Share</span>
      } @else {
        <span>🔗 Copy Link</span>
      }
    </button>
  `,
  styles: `
    .share-btn {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      padding: 0.5rem 1rem;
      border: 1px solid #d1d5db;
      border-radius: 0.5rem;
      background: white;
      cursor: pointer;
      font-size: 0.875rem;
      transition: all 0.15s ease;
    }
    .share-btn:hover:not(:disabled) {
      border-color: #9ca3af;
      background: #f9fafb;
    }
    .share-btn--copied {
      border-color: #10b981;
      color: #10b981;
    }
    .share-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
  `,
})
export class ShareButtonComponent {
  readonly shareType = input.required<ShareType>();
  readonly entityId = input.required<string>();

  private readonly sharingService = inject(SharingService);

  readonly sharing = signal(false);
  readonly copied = signal(false);
  readonly canNativeShare = this.sharingService.canUseNativeShare();

  private copiedTimer: ReturnType<typeof setTimeout> | null = null;

  async onShare(): Promise<void> {
    this.sharing.set(true);
    try {
      const shareLink = await firstValueFrom(
        this.sharingService.getShareLink(this.shareType(), this.entityId()),
      );
      await this.performShare(shareLink);
    } catch {
      // API call failed — nothing to share
    } finally {
      this.sharing.set(false);
    }
  }

  private async performShare(shareLink: ShareLink): Promise<void> {
    if (this.canNativeShare) {
      try {
        await this.sharingService.share(shareLink);
        return;
      } catch {
        // Native share cancelled — fall through to clipboard
      }
    }
    await navigator.clipboard.writeText(shareLink.url);
    this.showCopiedFeedback();
  }

  private showCopiedFeedback(): void {
    this.copied.set(true);
    if (this.copiedTimer) clearTimeout(this.copiedTimer);
    this.copiedTimer = setTimeout(() => this.copied.set(false), 2000);
  }
}
