import {
  Component,
  ChangeDetectionStrategy,
  inject,
  input,
  OnInit,
  OnDestroy,
} from "@angular/core";
import { Subscription } from "rxjs";
import { VoteService } from "@app/shared/services/vote.service";
import { QueueService } from "@app/shared/services/queue.service";
import { ToastService, ToastType } from "@app/shared/services/toast.service";
import { SkipReason } from "@app/shared/models/vote.model";

const SKIP_REASON_LABELS: Record<SkipReason, string> = {
  owner_downvote: "Owner downvote",
  vote_threshold: "65% community downvote",
  manual_skip: "Manual skip by owner",
};

const SKIP_REASON_TYPES: Record<SkipReason, ToastType> = {
  owner_downvote: "info",
  vote_threshold: "warning",
  manual_skip: "neutral",
};

@Component({
  selector: "app-skip-notification",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: ``,
})
export class SkipNotificationComponent implements OnInit, OnDestroy {
  private readonly voteService = inject(VoteService);
  private readonly queueService = inject(QueueService);
  private readonly toastService = inject(ToastService);

  readonly hubId = input.required<string>();

  private subs: Subscription[] = [];

  ngOnInit(): void {
    this.subs.push(
      this.voteService.onSkipTriggered().subscribe((msg) => {
        if (msg.hubId !== this.hubId()) return;

        const reason = msg.payload.reason;
        const label = SKIP_REASON_LABELS[reason] ?? reason;
        const type = SKIP_REASON_TYPES[reason] ?? "neutral";
        const title = `Track skipped — ${label}`;

        let detail: string | null = null;
        if (msg.payload.refundAmount !== null && msg.payload.refundAmount > 0) {
          detail = `You'll receive ${msg.payload.refundAmount} coins refund`;
        }

        this.toastService.show(type, title, detail, 5000);
      }),
    );

    this.subs.push(
      this.queueService.onTrackSkipped().subscribe((msg) => {
        if (msg.hubId !== this.hubId()) return;

        const reason = msg.payload.reason;
        const label = SKIP_REASON_LABELS[reason] ?? reason;
        const type = SKIP_REASON_TYPES[reason] ?? "neutral";

        this.toastService.show(type, `Track skipped — ${label}`, null, 5000);
      }),
    );
  }

  ngOnDestroy(): void {
    this.subs.forEach((s) => s.unsubscribe());
  }
}
