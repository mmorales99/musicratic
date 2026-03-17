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
  TierLimits,
  ROLE_BADGE_COLORS,
  RoleBadgeColor,
} from "../../models/member.model";
import { UserRole, ROLE_LABELS } from "@app/shared/models/user-role.model";

interface RoleOption {
  value: UserRole;
  label: string;
}

const ASSIGNABLE_ROLES: RoleOption[] = [
  { value: UserRole.Visitor, label: "Visitor" },
  { value: UserRole.User, label: "User" },
  { value: UserRole.ListOwner, label: "List Owner" },
  { value: UserRole.HubManager, label: "Hub Manager" },
];

@Component({
  selector: "app-role-assignment",
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="roles">
      <h1>Role Assignment</h1>

      @if (loading()) {
        <div class="roles__loading">Loading members...</div>
      } @else if (error()) {
        <div class="roles__error">
          <p>{{ error() }}</p>
          <button class="btn btn--primary" (click)="loadData()">Retry</button>
        </div>
      } @else {
        <!-- Tier Limits Warning -->
        @if (tierLimits(); as limits) {
          <div class="roles__limits">
            <div
              class="roles__limit-badge"
              [class.roles__limit-badge--warn]="
                limits.currentSubListOwners >= limits.maxSubListOwners
              "
            >
              List Owners: {{ limits.currentSubListOwners }}/{{
                limits.maxSubListOwners
              }}
            </div>
            <div
              class="roles__limit-badge"
              [class.roles__limit-badge--warn]="
                limits.currentSubHubManagers >= limits.maxSubHubManagers
              "
            >
              Hub Managers: {{ limits.currentSubHubManagers }}/{{
                limits.maxSubHubManagers
              }}
            </div>
          </div>
        }

        <!-- Member Selection -->
        <div class="roles__form">
          <div class="roles__field">
            <label for="memberSelect">Select Member</label>
            <select
              id="memberSelect"
              [ngModel]="selectedUserId()"
              (ngModelChange)="onMemberSelected($event)"
            >
              <option value="">— Choose a member —</option>
              @for (member of members(); track member.userId) {
                <option [value]="member.userId">
                  {{ member.displayName }} ({{ member.roleName }})
                </option>
              }
            </select>
          </div>

          @if (selectedMember(); as member) {
            <div class="roles__current">
              <span>Current role:</span>
              <span
                class="roles__badge"
                [attr.data-color]="getBadgeColor(member.role)"
              >
                {{ member.roleName }}
              </span>
            </div>

            <div class="roles__field">
              <label for="roleSelect">New Role</label>
              <select
                id="roleSelect"
                [ngModel]="targetRole()"
                (ngModelChange)="targetRole.set($event)"
              >
                <option [ngValue]="null">— Choose role —</option>
                @for (option of availableRoles(); track option.value) {
                  <option [ngValue]="option.value">
                    {{ option.label }}
                  </option>
                }
              </select>
            </div>

            <!-- Limit Warning -->
            @if (limitWarning(); as warning) {
              <div class="roles__warning">⚠ {{ warning }}</div>
            }

            <button
              class="btn btn--primary"
              [disabled]="!canAssign() || saving()"
              (click)="confirmAssign()"
            >
              {{ saving() ? "Saving..." : actionLabel() }}
            </button>

            @if (successMsg()) {
              <div class="roles__success">{{ successMsg() }}</div>
            }
          }
        </div>
      }
    </section>
  `,
  styles: `
    .roles {
      padding: 1.5rem;
      max-width: 600px;
      margin: 0 auto;
    }
    .roles__loading,
    .roles__error {
      text-align: center;
      padding: 3rem 1rem;
      color: #666;
    }
    .roles__limits {
      display: flex;
      gap: 0.75rem;
      margin-bottom: 1.5rem;
      flex-wrap: wrap;
    }
    .roles__limit-badge {
      padding: 0.4rem 0.75rem;
      border-radius: 6px;
      background: #e0e7ff;
      color: #3730a3;
      font-size: 0.85rem;
      font-weight: 500;
    }
    .roles__limit-badge--warn {
      background: #fef3c7;
      color: #92400e;
    }

    .roles__form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .roles__field {
      display: flex;
      flex-direction: column;
      gap: 0.3rem;
    }
    .roles__field label {
      font-weight: 500;
      font-size: 0.9rem;
    }
    .roles__field select {
      padding: 0.5rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 0.9rem;
    }

    .roles__current {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.9rem;
    }
    .roles__badge {
      font-size: 0.75rem;
      padding: 0.2rem 0.6rem;
      border-radius: 999px;
      font-weight: 500;
    }
    .roles__badge[data-color="gray"] {
      background: #e5e7eb;
      color: #374151;
    }
    .roles__badge[data-color="blue"] {
      background: #dbeafe;
      color: #1d4ed8;
    }
    .roles__badge[data-color="green"] {
      background: #d1fae5;
      color: #065f46;
    }
    .roles__badge[data-color="gold"] {
      background: #fef3c7;
      color: #92400e;
    }

    .roles__warning {
      padding: 0.6rem 0.75rem;
      background: #fef3c7;
      color: #92400e;
      border-radius: 6px;
      font-size: 0.85rem;
    }
    .roles__success {
      padding: 0.6rem 0.75rem;
      background: #d1fae5;
      color: #065f46;
      border-radius: 6px;
      font-size: 0.85rem;
    }
  `,
})
export class RoleAssignmentComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly memberService = inject(MemberService);
  private readonly destroyRef = inject(DestroyRef);

  readonly members = signal<Member[]>([]);
  readonly tierLimits = signal<TierLimits | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly saving = signal(false);
  readonly successMsg = signal<string | null>(null);

  readonly selectedUserId = signal<string>("");
  readonly targetRole = signal<UserRole | null>(null);

  readonly selectedMember = computed(() => {
    const id = this.selectedUserId();
    if (!id) return null;
    return this.members().find((m) => m.userId === id) ?? null;
  });

  readonly availableRoles = computed((): RoleOption[] => {
    const member = this.selectedMember();
    if (!member) return [];
    return ASSIGNABLE_ROLES.filter((r) => r.value !== member.role);
  });

  readonly actionLabel = computed((): string => {
    const member = this.selectedMember();
    const target = this.targetRole();
    if (!member || target === null) return "Assign Role";
    return target > member.role ? "Promote" : "Demote";
  });

  readonly canAssign = computed((): boolean => {
    return (
      this.selectedMember() !== null &&
      this.targetRole() !== null &&
      !this.saving()
    );
  });

  readonly limitWarning = computed((): string | null => {
    const target = this.targetRole();
    const limits = this.tierLimits();
    if (!target || !limits) return null;

    if (
      target === UserRole.ListOwner &&
      limits.currentSubListOwners >= limits.maxSubListOwners
    ) {
      return `Tier limit reached: max ${limits.maxSubListOwners} List Owners. You have ${limits.currentSubListOwners}/${limits.maxSubListOwners}.`;
    }
    if (
      target === UserRole.HubManager &&
      limits.currentSubHubManagers >= limits.maxSubHubManagers
    ) {
      return `Tier limit reached: max ${limits.maxSubHubManagers} Hub Managers. You have ${limits.currentSubHubManagers}/${limits.maxSubHubManagers}.`;
    }
    return null;
  });

  private hubId = "";

  ngOnInit(): void {
    this.hubId =
      this.route.snapshot.paramMap.get("hubId") ??
      this.route.parent?.snapshot.paramMap.get("id") ??
      "";
    this.loadData();
  }

  loadData(): void {
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

    this.memberService
      .getTierLimits(this.hubId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (limits) => this.tierLimits.set(limits),
        error: () => {
          /* non-critical — limits just won't display */
        },
      });
  }

  onMemberSelected(userId: string): void {
    this.selectedUserId.set(userId);
    this.targetRole.set(null);
    this.successMsg.set(null);
  }

  confirmAssign(): void {
    const member = this.selectedMember();
    const target = this.targetRole();
    if (!member || target === null) return;

    this.saving.set(true);
    this.successMsg.set(null);

    const roleName = ROLE_LABELS.get(target) ?? target.toString();

    const action$ =
      target > member.role
        ? this.memberService.promoteMember(
            this.hubId,
            member.userId,
            roleName.toLowerCase().replace(/ /g, "_"),
          )
        : this.memberService.demoteMember(
            this.hubId,
            member.userId,
            roleName.toLowerCase().replace(/ /g, "_"),
          );

    action$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saving.set(false);
        this.successMsg.set(`${member.displayName} is now ${roleName}.`);
        this.loadData();
        this.targetRole.set(null);
      },
      error: () => {
        this.saving.set(false);
      },
    });
  }

  getBadgeColor(role: UserRole): RoleBadgeColor {
    return ROLE_BADGE_COLORS.get(role) ?? "gray";
  }
}
