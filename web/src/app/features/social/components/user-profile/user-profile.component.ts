import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { ReactiveFormsModule, FormBuilder, Validators } from "@angular/forms";
import { DatePipe } from "@angular/common";
import { ProfileService } from "../../services/profile.service";
import { UserProfile } from "../../models/profile.model";
import { AuthService } from "@app/shared/services/auth.service";
import { ShareButtonComponent } from "../share-button/share-button.component";

@Component({
  selector: "app-user-profile",
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, ShareButtonComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="user-profile">
      @if (loading()) {
        <div class="user-profile__loading">Loading profile...</div>
      } @else if (error(); as err) {
        <div class="user-profile__error">
          <p>{{ err }}</p>
          <button class="btn btn--primary" (click)="load()">Retry</button>
        </div>
      } @else if (profile(); as p) {
        @if (editing()) {
          <form
            [formGroup]="editForm"
            (ngSubmit)="saveProfile()"
            class="user-profile__edit"
          >
            <div class="user-profile__avatar-edit">
              @if (avatarPreview()) {
                <img
                  [src]="avatarPreview()"
                  alt="Avatar preview"
                  class="user-profile__avatar"
                />
              } @else if (p.avatarUrl) {
                <img
                  [src]="p.avatarUrl"
                  [alt]="p.displayName"
                  class="user-profile__avatar"
                />
              } @else {
                <div class="user-profile__avatar-placeholder">
                  {{ p.displayName.charAt(0).toUpperCase() }}
                </div>
              }
              <label class="btn btn--small">
                Change Avatar
                <input
                  type="file"
                  accept="image/*"
                  hidden
                  (change)="onAvatarSelected($event)"
                />
              </label>
            </div>
            <label class="form-field">
              Display Name
              <input formControlName="displayName" maxlength="50" />
            </label>
            <label class="form-field">
              Bio
              <textarea
                formControlName="bio"
                maxlength="140"
                rows="3"
              ></textarea>
              <span class="form-field__hint">
                {{ editForm.get("bio")?.value?.length || 0 }}/140
              </span>
            </label>
            <label class="form-field">
              Favorite Genres (comma-separated)
              <input formControlName="genres" />
            </label>
            <div class="user-profile__edit-actions">
              <button
                type="submit"
                class="btn btn--primary"
                [disabled]="saving() || editForm.invalid"
              >
                {{ saving() ? "Saving..." : "Save" }}
              </button>
              <button
                type="button"
                class="btn btn--secondary"
                (click)="cancelEdit()"
              >
                Cancel
              </button>
            </div>
          </form>
        } @else {
          <div class="user-profile__header">
            @if (p.avatarUrl) {
              <img
                [src]="p.avatarUrl"
                [alt]="p.displayName"
                class="user-profile__avatar"
              />
            } @else {
              <div class="user-profile__avatar-placeholder">
                {{ p.displayName.charAt(0).toUpperCase() }}
              </div>
            }
            <div class="user-profile__info">
              <h1>{{ p.displayName }}</h1>
              @if (p.bio) {
                <p class="user-profile__bio">{{ p.bio }}</p>
              }
              @if (p.favoriteGenres.length) {
                <div class="user-profile__genres">
                  @for (genre of p.favoriteGenres; track genre) {
                    <span class="chip">{{ genre }}</span>
                  }
                </div>
              }
              <span class="user-profile__member-since">
                Member since {{ p.memberSince | date }}
              </span>
            </div>
            <div class="user-profile__header-actions">
              @if (isOwnProfile()) {
                <button class="btn btn--secondary" (click)="startEdit()">
                  Edit Profile
                </button>
              }
              <app-share-button shareType="profile" [entityId]="p.id" />
            </div>
          </div>

          <div class="user-profile__stats">
            <div class="stat-card">
              <span class="stat-card__value">
                {{ p.stats.totalProposals }}
              </span>
              <span class="stat-card__label">Proposals</span>
            </div>
            <div class="stat-card">
              <span class="stat-card__value">
                {{ p.stats.totalUpvotes }}
              </span>
              <span class="stat-card__label">Upvotes</span>
            </div>
            <div class="stat-card">
              <span class="stat-card__value">
                {{ p.stats.hubsVisited }}
              </span>
              <span class="stat-card__label">Hubs Visited</span>
            </div>
          </div>

          @if (p.stats.activeVoterBadge || p.stats.topContributorBadge) {
            <div class="user-profile__badges">
              @if (p.stats.activeVoterBadge) {
                <span class="badge">Active Voter</span>
              }
              @if (p.stats.topContributorBadge) {
                <span class="badge">Top Contributor</span>
              }
            </div>
          }
        }
      }
    </section>
  `,
})
export class UserProfileComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly profileService = inject(ProfileService);
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly profile = signal<UserProfile | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly editing = signal(false);
  readonly saving = signal(false);
  readonly avatarPreview = signal<string | null>(null);
  readonly isOwnProfile = signal(false);

  private selectedAvatarFile: File | null = null;

  readonly editForm = this.fb.nonNullable.group({
    displayName: ["", [Validators.required, Validators.maxLength(50)]],
    bio: ["", Validators.maxLength(140)],
    genres: [""],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    const userId = this.route.snapshot.paramMap.get("userId") ?? "me";
    const currentUser = this.authService.currentUser();
    this.isOwnProfile.set(userId === "me" || userId === currentUser?.id);

    this.loading.set(true);
    this.error.set(null);

    this.profileService.getProfile(userId).subscribe({
      next: (p: UserProfile) => {
        this.profile.set(p);
        this.loading.set(false);
      },
      error: (err: { error?: { detail?: string } }) => {
        this.error.set(err?.error?.detail ?? "Failed to load profile");
        this.loading.set(false);
      },
    });
  }

  startEdit(): void {
    const p = this.profile();
    if (!p) return;
    this.editForm.patchValue({
      displayName: p.displayName,
      bio: p.bio,
      genres: p.favoriteGenres.join(", "),
    });
    this.editing.set(true);
    this.avatarPreview.set(null);
    this.selectedAvatarFile = null;
  }

  cancelEdit(): void {
    this.editing.set(false);
    this.avatarPreview.set(null);
    this.selectedAvatarFile = null;
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.selectedAvatarFile = file;
    const reader = new FileReader();
    reader.onload = () => this.avatarPreview.set(reader.result as string);
    reader.readAsDataURL(file);
  }

  saveProfile(): void {
    if (this.editForm.invalid) return;
    this.saving.set(true);
    const { displayName, bio, genres } = this.editForm.getRawValue();
    const favoriteGenres = genres
      .split(",")
      .map((g: string) => g.trim())
      .filter((g: string) => g.length > 0);

    const save$ = this.selectedAvatarFile
      ? this.profileService.uploadAvatar(this.selectedAvatarFile)
      : null;

    const finalize = (): void => {
      this.profileService
        .updateProfile({ displayName, bio, favoriteGenres })
        .subscribe({
          next: (updated: UserProfile) => {
            this.profile.set(updated);
            this.editing.set(false);
            this.saving.set(false);
          },
          error: () => this.saving.set(false),
        });
    };

    if (save$) {
      save$.subscribe({ next: () => finalize(), error: () => finalize() });
    } else {
      finalize();
    }
  }
}
