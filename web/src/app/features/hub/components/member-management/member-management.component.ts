import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
  OnInit,
  DestroyRef,
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { MemberService } from "../../services/member.service";
import {
  Member,
  ROLE_BADGE_COLORS,
  RoleBadgeColor,
} from "../../models/member.model";
import { UserRole } from "@app/shared/models/user-role.model";

@Component({
  selector: "app-member-management",
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="members">
      <div class="members__header">
        <h1>Members</h1>
        <div class="members__search">
          <input
            type="text"
            placeholder="Search by name..."
            [ngModel]="searchQuery()"
            (ngModelChange)="searchQuery.set($event)"
            class="members__search-input"
          />
        </div>
      </div>

      @if (loading()) {
        <div class="members__loading">Loading members...</div>
      } @else if (error()) {
        <div class="members__error">
          <p>{{ error() }}</p>
          <button class="btn btn--primary" (click)="loadMembers()">
            Retry
          </button>
        </div>
      } @else if (filteredMembers().length === 0) {
        <div class="members__empty">
          <p>
            @if (searchQuery()) {
              No members matching "{{ searchQuery() }}".
            } @else {
              No members found.
            }
          </p>
        </div>
      } @else {
        <ul class="members__list">
          @for (member of filteredMembers(); track member.userId) {
            <li class="member-row">
              <div class="member-row__avatar">
                @if (member.avatarUrl) {
                  <img
                    [src]="member.avatarUrl"
                    [alt]="member.displayName"
                    class="member-row__avatar-img"
                  />
                } @else {
                  <span class="member-row__avatar-placeholder">👤</span>
                }
              </div>
              <div class="member-row__info">
                <span class="member-row__name">{{ member.displayName }}</span>
                <span class="member-row__email">{{ member.email }}</span>
              </div>
              <span
                class="member-row__badge"
                [attr.data-color]="getBadgeColor(member.role)"
              >
                {{ member.roleName }}
              </span>
              <button
                class="btn btn--danger btn--small"
                [disabled]="removing() === member.userId"
                (click)="confirmRemove(member)"
              >
                {{ removing() === member.userId ? "Removing..." : "Remove" }}
              </button>
            </li>
          }
        </ul>
      }

      <!-- Confirmation dialog -->
      @if (pendingRemoval(); as member) {
        <div class="dialog-overlay" (click)="cancelRemove()">
          <div class="dialog" (click)="$event.stopPropagation()">
            <h3>Remove Member</h3>
            <p>
              Are you sure you want to remove
              <strong>{{ member.displayName }}</strong> from this hub?
            </p>
            <div class="dialog__actions">
              <button class="btn btn--secondary" (click)="cancelRemove()">
                Cancel
              </button>
              <button
                class="btn btn--danger"
                [disabled]="removing() === member.userId"
                (click)="executeRemove(member)"
              >
                Remove
              </button>
            </div>
          </div>
        </div>
      }
    </section>
  `,
  styles: `
    .members {
      padding: 1.5rem;
      max-width: 800px;
      margin: 0 auto;
    }
    .members__header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      flex-wrap: wrap;
      gap: 1rem;
    }
    .members__search-input {
      padding: 0.5rem 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 0.9rem;
      min-width: 200px;
    }
    .members__loading,
    .members__error,
    .members__empty {
      text-align: center;
      padding: 3rem 1rem;
      color: #666;
    }
    .members__list {
      list-style: none;
      padding: 0;
      margin: 0;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .member-row {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
      background: #fff;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
    }
    .member-row__avatar-img {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      object-fit: cover;
    }
    .member-row__avatar-placeholder {
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f3f4f6;
      border-radius: 50%;
      font-size: 1.2rem;
    }
    .member-row__info {
      flex: 1;
      display: flex;
      flex-direction: column;
    }
    .member-row__name {
      font-weight: 500;
    }
    .member-row__email {
      font-size: 0.8rem;
      color: #888;
    }
    .member-row__badge {
      font-size: 0.75rem;
      padding: 0.2rem 0.6rem;
      border-radius: 999px;
      font-weight: 500;
    }
    .member-row__badge[data-color="gray"] {
      background: #e5e7eb;
      color: #374151;
    }
    .member-row__badge[data-color="blue"] {
      background: #dbeafe;
      color: #1d4ed8;
    }
    .member-row__badge[data-color="green"] {
      background: #d1fae5;
      color: #065f46;
    }
    .member-row__badge[data-color="gold"] {
      background: #fef3c7;
      color: #92400e;
    }

    .dialog-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.4);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }
    .dialog {
      background: #fff;
      padding: 1.5rem;
      border-radius: 12px;
      max-width: 400px;
      width: 90%;
    }
    .dialog h3 {
      margin: 0 0 0.75rem;
    }
    .dialog p {
      margin: 0 0 1.25rem;
      color: #555;
    }
    .dialog__actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.5rem;
    }
  `,
})
export class MemberManagementComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly memberService = inject(MemberService);
  private readonly destroyRef = inject(DestroyRef);

  readonly members = signal<Member[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly searchQuery = signal("");
  readonly removing = signal<string | null>(null);
  readonly pendingRemoval = signal<Member | null>(null);

  readonly filteredMembers = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const all = this.members();
    if (!query) return all;
    return all.filter((m) => m.displayName.toLowerCase().includes(query));
  });

  private hubId = "";

  ngOnInit(): void {
    this.hubId =
      this.route.snapshot.paramMap.get("hubId") ??
      this.route.parent?.snapshot.paramMap.get("id") ??
      "";
    this.loadMembers();
  }

  loadMembers(): void {
    this.loading.set(true);
    this.error.set(null);

    this.memberService
      .getMembers(this.hubId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (members) => {
          this.members.set(members);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err?.error?.detail ?? "Failed to load members.");
          this.loading.set(false);
        },
      });
  }

  getBadgeColor(role: UserRole): RoleBadgeColor {
    return ROLE_BADGE_COLORS.get(role) ?? "gray";
  }

  confirmRemove(member: Member): void {
    this.pendingRemoval.set(member);
  }

  cancelRemove(): void {
    this.pendingRemoval.set(null);
  }

  executeRemove(member: Member): void {
    this.removing.set(member.userId);

    this.memberService
      .removeMember(this.hubId, member.userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.members.update((list) =>
            list.filter((m) => m.userId !== member.userId),
          );
          this.removing.set(null);
          this.pendingRemoval.set(null);
        },
        error: () => {
          this.removing.set(null);
          this.pendingRemoval.set(null);
        },
      });
  }
}
