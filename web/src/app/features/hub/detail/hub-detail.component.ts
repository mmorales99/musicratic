import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
} from "@angular/core";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { ReactiveFormsModule, FormBuilder, Validators } from "@angular/forms";
import { HubDetailMachineService } from "../machines/hub-detail-machine.service";
import { RequireRoleDirective } from "@app/shared/directives/require-role.directive";
import { UserRole } from "@app/shared/models/user-role.model";
import { RoleService } from "@app/shared/services/role.service";
import { ShareButtonComponent } from "@app/features/social/components/share-button/share-button.component";
import { HubReviewsComponent } from "@app/features/social/components/hub-reviews/hub-reviews.component";

type TabId = "info" | "settings" | "lists" | "reviews";

@Component({
  selector: "app-hub-detail",
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    RequireRoleDirective,
    ShareButtonComponent,
    HubReviewsComponent,
  ],
  providers: [HubDetailMachineService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="hub-detail">
      @if (machine.isLoading()) {
        <div class="hub-detail__loading">Loading hub...</div>
      } @else if (machine.error()) {
        <div class="hub-detail__error">
          <p>{{ machine.error() }}</p>
          <button class="btn btn--primary" (click)="machine.retry()">
            Retry
          </button>
        </div>
      } @else if (machine.hub()) {
        @let hub = machine.hub()!;
        <!-- Header -->
        <div class="hub-detail__header">
          <div>
            <h1>{{ hub.name }}</h1>
            <span class="hub-detail__type">{{ hub.businessType }}</span>
            <span
              class="hub-detail__status"
              [class.hub-detail__status--active]="hub.isActive && !hub.isPaused"
              [class.hub-detail__status--paused]="hub.isPaused"
              [class.hub-detail__status--inactive]="!hub.isActive"
            >
              {{
                hub.isPaused ? "Paused" : hub.isActive ? "Active" : "Inactive"
              }}
            </span>
          </div>
          <div class="hub-detail__code-block">
            <span class="hub-detail__code">{{ hub.code }}</span>
            <button class="btn btn--small" (click)="copyCode(hub.code)">
              {{ codeCopied() ? "Copied!" : "Copy Code" }}
            </button>
          </div>
        </div>

        <!-- QR + Deep Link -->
        <div class="hub-detail__qr-section">
          <img
            [src]="hub.qrUrl"
            [alt]="'QR code for ' + hub.name"
            class="hub-detail__qr"
          />
          <div class="hub-detail__links">
            <a [href]="hub.directLink" target="_blank" class="btn btn--small">
              Open Direct Link
            </a>
            <button
              class="btn btn--small"
              (click)="downloadQr(hub.qrUrl, hub.code)"
            >
              Download QR
            </button>
            <app-share-button shareType="hub" [entityId]="hub.id" />
          </div>
        </div>

        <!-- Actions -->
        <div class="hub-detail__actions" *appRequireRole="HubManager">
          @if (!hub.isActive) {
            <button
              class="btn btn--primary"
              [disabled]="machine.isSaving()"
              (click)="machine.executeAction('activate')"
            >
              Activate
            </button>
          }
          @if (hub.isActive && !hub.isPaused) {
            <button
              class="btn btn--secondary"
              [disabled]="machine.isSaving()"
              (click)="machine.executeAction('pause')"
            >
              Pause
            </button>
            <button
              class="btn btn--secondary"
              [disabled]="machine.isSaving()"
              (click)="machine.executeAction('deactivate')"
            >
              Deactivate
            </button>
          }
          @if (hub.isPaused) {
            <button
              class="btn btn--primary"
              [disabled]="machine.isSaving()"
              (click)="machine.executeAction('resume')"
            >
              Resume
            </button>
          }
          <button
            class="btn btn--danger"
            [disabled]="machine.isSaving()"
            (click)="confirmDelete()"
          >
            Delete
          </button>
        </div>

        <!-- Tabs -->
        <nav class="hub-detail__tabs">
          @for (tab of tabs; track tab.id) {
            <button
              class="hub-detail__tab"
              [class.hub-detail__tab--active]="activeTab() === tab.id"
              (click)="activeTab.set(tab.id)"
            >
              {{ tab.label }}
            </button>
          }
        </nav>

        <!-- Tab Content -->
        <div class="hub-detail__tab-content">
          @switch (activeTab()) {
            @case ("info") {
              @if (machine.isEditing()) {
                <form
                  [formGroup]="editForm"
                  (ngSubmit)="onSaveEdit()"
                  class="edit-form"
                >
                  <div class="form-field">
                    <label for="editName">Name</label>
                    <input id="editName" formControlName="name" type="text" />
                  </div>
                  <div class="form-field">
                    <label for="editVisibility">Visibility</label>
                    <select id="editVisibility" formControlName="visibility">
                      <option value="public">Public</option>
                      <option value="private">Private</option>
                    </select>
                  </div>
                  <div class="edit-form__actions">
                    <button
                      type="button"
                      class="btn btn--secondary"
                      (click)="machine.cancelEdit()"
                    >
                      Cancel
                    </button>
                    <button
                      type="submit"
                      class="btn btn--primary"
                      [disabled]="machine.isSaving()"
                    >
                      Save
                    </button>
                  </div>
                </form>
              } @else {
                <dl class="info-grid">
                  <dt>Name</dt>
                  <dd>{{ hub.name }}</dd>
                  <dt>Type</dt>
                  <dd>{{ hub.businessType }}</dd>
                  <dt>Visibility</dt>
                  <dd>{{ hub.visibility }}</dd>
                  <dt>Created</dt>
                  <dd>{{ hub.createdAt }}</dd>
                  <dt>Subscription</dt>
                  <dd>{{ hub.subscriptionTier }}</dd>
                </dl>
                <div *appRequireRole="HubManager">
                  <button class="btn btn--secondary" (click)="onEdit()">
                    Edit Hub
                  </button>
                </div>
                <div
                  class="hub-detail__member-links"
                  *appRequireRole="HubManager"
                >
                  <a
                    [routerLink]="['/hub', hub.id, 'members']"
                    class="btn btn--secondary btn--small"
                  >
                    Manage Members
                  </a>
                  <a
                    [routerLink]="['/hub', hub.id, 'roles']"
                    class="btn btn--secondary btn--small"
                  >
                    Assign Roles
                  </a>
                </div>
              }
            }
            @case ("settings") {
              @if (machine.settings(); as s) {
                <dl class="info-grid">
                  <dt>Allow Proposals</dt>
                  <dd>{{ s.allowProposals ? "Yes" : "No" }}</dd>
                  <dt>Auto-Skip Threshold</dt>
                  <dd>{{ s.autoSkipThreshold }}%</dd>
                  <dt>Voting Window</dt>
                  <dd>{{ s.votingWindowSeconds }}s</dd>
                  <dt>Max Queue Size</dt>
                  <dd>{{ s.maxQueueSize }}</dd>
                  <dt>Providers</dt>
                  <dd>{{ s.allowedProviders.join(", ") }}</dd>
                </dl>
              }
            }
            @case ("lists") {
              <div class="lists-section">
                @if (machine.lists().length === 0) {
                  <p class="lists-section__empty">No lists yet.</p>
                }
                @for (list of machine.lists(); track list.id) {
                  <a
                    class="list-card"
                    [routerLink]="['/hub', hub.id, 'lists', list.id]"
                  >
                    <span class="list-card__name">{{ list.name }}</span>
                    <span class="list-card__meta">
                      {{ list.trackCount }} tracks · {{ list.playMode }}
                    </span>
                  </a>
                }
              </div>
            }
            @case ("reviews") {
              <app-hub-reviews [hubId]="hub.id" />
            }
          }
        </div>
      }
    </section>
  `,
  styles: [
    `
      .hub-detail {
        max-width: 800px;
        margin: 0 auto;
      }
      .hub-detail__loading,
      .hub-detail__error {
        text-align: center;
        padding: 3rem;
        color: #a0a0b0;
      }
      .hub-detail__error p {
        margin-bottom: 1rem;
        color: #e74c3c;
      }
      .hub-detail__header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        flex-wrap: wrap;
        gap: 1rem;
        margin-bottom: 1.5rem;
      }
      .hub-detail__header h1 {
        font-size: 1.75rem;
        color: #e0e0e0;
        margin: 0;
      }
      .hub-detail__type {
        display: inline-block;
        padding: 0.2rem 0.6rem;
        border-radius: 4px;
        background: #2a2a4a;
        color: #a0a0b0;
        font-size: 0.8rem;
        margin-left: 0.5rem;
      }
      .hub-detail__status {
        display: inline-block;
        padding: 0.2rem 0.6rem;
        border-radius: 4px;
        font-size: 0.8rem;
        margin-left: 0.5rem;
      }
      .hub-detail__status--active {
        background: rgba(46, 213, 115, 0.15);
        color: #2ed573;
      }
      .hub-detail__status--paused {
        background: rgba(255, 165, 2, 0.15);
        color: #ffa502;
      }
      .hub-detail__status--inactive {
        background: rgba(160, 160, 176, 0.15);
        color: #a0a0b0;
      }
      .hub-detail__code-block {
        display: flex;
        align-items: center;
        gap: 0.5rem;
      }
      .hub-detail__code {
        font-family: monospace;
        font-size: 1.25rem;
        font-weight: 700;
        color: #6c5ce7;
        letter-spacing: 0.1rem;
      }
      .hub-detail__qr-section {
        display: flex;
        align-items: center;
        gap: 1.5rem;
        margin-bottom: 1.5rem;
        flex-wrap: wrap;
      }
      .hub-detail__qr {
        width: 140px;
        height: 140px;
        border-radius: 8px;
        background: #fff;
      }
      .hub-detail__links {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .hub-detail__actions {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
        margin-bottom: 1.5rem;
      }
      .hub-detail__tabs {
        display: flex;
        gap: 0;
        border-bottom: 2px solid #2a2a4a;
        margin-bottom: 1.25rem;
      }
      .hub-detail__tab {
        padding: 0.625rem 1.25rem;
        background: none;
        border: none;
        color: #a0a0b0;
        cursor: pointer;
        font-size: 0.95rem;
        font-weight: 600;
        border-bottom: 2px solid transparent;
        margin-bottom: -2px;
      }
      .hub-detail__tab--active {
        color: #6c5ce7;
        border-bottom-color: #6c5ce7;
      }
      .info-grid {
        display: grid;
        grid-template-columns: 160px 1fr;
        gap: 0.5rem 1rem;
        margin-bottom: 1rem;
      }
      .info-grid dt {
        color: #a0a0b0;
        font-weight: 600;
      }
      .info-grid dd {
        color: #e0e0e0;
        margin: 0;
      }
      .edit-form {
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }
      .edit-form__actions {
        display: flex;
        gap: 0.5rem;
        justify-content: flex-end;
      }
      .form-field {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
      }
      .form-field label {
        font-weight: 600;
        color: #c0c0d0;
        font-size: 0.875rem;
      }
      .form-field input[type="text"],
      .form-field select {
        padding: 0.5rem 0.75rem;
        border-radius: 8px;
        border: 1px solid #2a2a4a;
        background: #16213e;
        color: #e0e0e0;
        font-size: 1rem;
      }
      .lists-section__empty {
        color: #a0a0b0;
        text-align: center;
        padding: 2rem;
      }
      .list-card {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0.75rem 1rem;
        margin-bottom: 0.5rem;
        border-radius: 8px;
        background: #16213e;
        text-decoration: none;
        color: #e0e0e0;
        border: 1px solid #2a2a4a;
        transition: border-color 0.2s;
      }
      .list-card:hover {
        border-color: #6c5ce7;
      }
      .list-card__name {
        font-weight: 600;
      }
      .list-card__meta {
        color: #a0a0b0;
        font-size: 0.85rem;
      }
      .hub-detail__member-links {
        display: flex;
        gap: 0.5rem;
        margin-top: 0.75rem;
      }
      .btn {
        padding: 0.5rem 1.25rem;
        border-radius: 8px;
        border: none;
        font-size: 0.9rem;
        cursor: pointer;
        font-weight: 600;
      }
      .btn--primary {
        background: #6c5ce7;
        color: #fff;
      }
      .btn--primary:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }
      .btn--secondary {
        background: transparent;
        color: #a0a0b0;
        border: 1px solid #2a2a4a;
      }
      .btn--danger {
        background: rgba(231, 76, 60, 0.15);
        color: #e74c3c;
        border: 1px solid #e74c3c;
      }
      .btn--small {
        padding: 0.375rem 0.875rem;
        font-size: 0.85rem;
      }
    `,
  ],
})
export class HubDetailComponent implements OnInit {
  protected readonly machine = inject(HubDetailMachineService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly roleService = inject(RoleService);

  protected readonly HubManager = UserRole.HubManager;

  readonly activeTab = signal<TabId>("info");
  readonly codeCopied = signal(false);

  readonly tabs: { id: TabId; label: string }[] = [
    { id: "info", label: "Info" },
    { id: "settings", label: "Settings" },
    { id: "lists", label: "Lists" },
    { id: "reviews", label: "Reviews" },
  ];

  readonly editForm = this.fb.group({
    name: [
      "",
      [Validators.required, Validators.minLength(3), Validators.maxLength(50)],
    ],
    visibility: ["public"],
  });

  ngOnInit(): void {
    const hubId = this.route.snapshot.paramMap.get("id");
    if (hubId) {
      this.machine.load(hubId);
      this.roleService.fetchRole(hubId);
    }
  }

  onEdit(): void {
    const hub = this.machine.hub();
    if (hub) {
      this.editForm.patchValue({ name: hub.name, visibility: hub.visibility });
    }
    this.machine.edit();
  }

  onSaveEdit(): void {
    if (this.editForm.invalid) return;
    this.machine.save({
      name: this.editForm.value.name ?? undefined,
      visibility:
        (this.editForm.value.visibility as "public" | "private") ?? undefined,
    });
  }

  async copyCode(code: string): Promise<void> {
    await navigator.clipboard.writeText(code);
    this.codeCopied.set(true);
    setTimeout(() => this.codeCopied.set(false), 2000);
  }

  downloadQr(url: string, code: string): void {
    const link = document.createElement("a");
    link.href = url;
    link.download = `hub-${code}-qr.png`;
    link.click();
  }

  confirmDelete(): void {
    if (
      confirm(
        "Are you sure you want to delete this hub? This cannot be undone.",
      )
    ) {
      this.machine.executeAction("delete").then(() => {
        if (this.machine.state() === "idle") {
          this.router.navigate(["/hub"]);
        }
      });
    }
  }
}
