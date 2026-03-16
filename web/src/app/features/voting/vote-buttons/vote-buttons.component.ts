import {
  Component,
  ChangeDetectionStrategy,
  inject,
  input,
  OnInit,
  OnDestroy,
  OnChanges,
  SimpleChanges,
} from "@angular/core";
import { AuthService } from "@app/shared/services/auth.service";
import { VoteMachineService } from "../machines/vote-machine.service";

@Component({
  selector: "app-vote-buttons",
  standalone: true,
  providers: [VoteMachineService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="vote-buttons" [class.vote-buttons--disabled]="!canVote()">
      <!-- Upvote -->
      <button
        class="vote-btn vote-btn--up"
        [class.vote-btn--active]="machine.currentVote() === 'up'"
        [class.vote-btn--pulse]="machine.currentVote() === 'up'"
        [disabled]="!canVote() || machine.isVoting()"
        (click)="onVoteUp()"
        aria-label="Upvote"
      >
        <span class="vote-btn__icon">👍</span>
        <span class="vote-btn__count">{{ machine.upCount() }}</span>
      </button>

      <!-- Downvote -->
      <button
        class="vote-btn vote-btn--down"
        [class.vote-btn--active]="machine.currentVote() === 'down'"
        [class.vote-btn--pulse]="machine.currentVote() === 'down'"
        [disabled]="!canVote() || machine.isVoting()"
        (click)="onVoteDown()"
        aria-label="Downvote"
      >
        <span class="vote-btn__icon">👎</span>
        <span class="vote-btn__count">{{ machine.downCount() }}</span>
      </button>

      @if (machine.error(); as err) {
        <span class="vote-buttons__error">{{ err }}</span>
      }
    </div>
  `,
  styles: [
    `
      .vote-buttons {
        display: flex;
        align-items: center;
        gap: 1rem;
        justify-content: center;
      }
      .vote-buttons--disabled {
        opacity: 0.5;
        pointer-events: none;
      }
      .vote-btn {
        display: flex;
        align-items: center;
        gap: 0.4rem;
        padding: 0.5rem 1rem;
        border: 2px solid transparent;
        border-radius: 24px;
        background: #f0f0f0;
        cursor: pointer;
        font-size: 1rem;
        transition:
          background 0.2s,
          border-color 0.2s,
          transform 0.15s;
      }
      .vote-btn:hover:not(:disabled) {
        transform: scale(1.05);
      }
      .vote-btn:active:not(:disabled) {
        transform: scale(0.95);
      }
      .vote-btn:disabled {
        cursor: not-allowed;
        opacity: 0.6;
      }
      .vote-btn--up.vote-btn--active {
        background: #d4edda;
        border-color: #27ae60;
      }
      .vote-btn--down.vote-btn--active {
        background: #f8d7da;
        border-color: #c0392b;
      }
      .vote-btn--pulse {
        animation: pulse 0.3s ease-out;
      }
      .vote-btn__icon {
        font-size: 1.25rem;
      }
      .vote-btn__count {
        font-weight: 600;
        min-width: 1.5rem;
        text-align: center;
      }
      .vote-buttons__error {
        font-size: 0.75rem;
        color: #c0392b;
      }
      @keyframes pulse {
        0% {
          transform: scale(1);
        }
        50% {
          transform: scale(1.15);
        }
        100% {
          transform: scale(1);
        }
      }
    `,
  ],
})
export class VoteButtonsComponent implements OnInit, OnDestroy, OnChanges {
  protected readonly machine = inject(VoteMachineService);
  private readonly authService = inject(AuthService);

  readonly hubId = input.required<string>();
  readonly entryId = input.required<string>();
  readonly disabled = input<boolean>(false);

  protected readonly canVote = this.authService.authenticated;

  ngOnInit(): void {
    const hub = this.hubId();
    const entry = this.entryId();
    if (hub && entry) {
      this.machine.init(hub, entry);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (
      (changes["hubId"] && !changes["hubId"].firstChange) ||
      (changes["entryId"] && !changes["entryId"].firstChange)
    ) {
      const hub = this.hubId();
      const entry = this.entryId();
      if (hub && entry) {
        this.machine.init(hub, entry);
      }
    }
  }

  protected onVoteUp(): void {
    this.machine.voteUp();
  }

  protected onVoteDown(): void {
    this.machine.voteDown();
  }

  ngOnDestroy(): void {
    this.machine.reset();
  }
}
