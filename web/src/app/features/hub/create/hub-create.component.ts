import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
} from "@angular/core";
import { Router } from "@angular/router";
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from "@angular/forms";
import { HubService } from "@app/shared/services/hub.service";
import {
  HubBusinessType,
  MusicProvider,
  HubVisibility,
  CreateHubRequest,
} from "@app/shared/models/hub.model";
import { firstValueFrom } from "rxjs";

@Component({
  selector: "app-hub-create",
  standalone: true,
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="hub-create">
      <h1 class="hub-create__title">Create Hub</h1>

      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="hub-create__form">
        <!-- Name -->
        <div class="form-field">
          <label for="name">Hub Name</label>
          <input
            id="name"
            formControlName="name"
            type="text"
            placeholder="e.g. Café Luna"
            maxlength="50"
          />
          @if (form.controls["name"].touched && form.controls["name"].errors) {
            <span class="form-field__error">
              @if (form.controls["name"].errors?.["required"]) {
                Name is required.
              } @else if (form.controls["name"].errors?.["minlength"]) {
                Name must be at least 3 characters.
              } @else if (form.controls["name"].errors?.["maxlength"]) {
                Name must be at most 50 characters.
              }
            </span>
          }
        </div>

        <!-- Business Type -->
        <div class="form-field">
          <label for="businessType">Business Type</label>
          <select id="businessType" formControlName="businessType">
            @for (bt of businessTypes; track bt.value) {
              <option [value]="bt.value">{{ bt.label }}</option>
            }
          </select>
        </div>

        <!-- Music Providers -->
        <fieldset class="form-field">
          <legend>Music Providers</legend>
          <div class="checkbox-group">
            @for (p of providers; track p.value) {
              <label class="checkbox-label">
                <input
                  type="checkbox"
                  [checked]="selectedProviders().includes(p.value)"
                  (change)="toggleProvider(p.value)"
                />
                {{ p.label }}
              </label>
            }
          </div>
          @if (providerError()) {
            <span class="form-field__error">Select at least one provider.</span>
          }
        </fieldset>

        <!-- Visibility -->
        <div class="form-field">
          <label>Visibility</label>
          <div class="radio-group">
            <label class="radio-label">
              <input type="radio" formControlName="visibility" value="public" />
              Public
            </label>
            <label class="radio-label">
              <input
                type="radio"
                formControlName="visibility"
                value="private"
              />
              Private
            </label>
          </div>
        </div>

        <!-- Error message -->
        @if (errorMessage()) {
          <div class="hub-create__error">{{ errorMessage() }}</div>
        }

        <!-- Actions -->
        <div class="hub-create__actions">
          <button type="button" class="btn btn--secondary" (click)="onCancel()">
            Cancel
          </button>
          <button
            type="submit"
            class="btn btn--primary"
            [disabled]="submitting()"
          >
            {{ submitting() ? "Creating..." : "Create Hub" }}
          </button>
        </div>
      </form>
    </section>
  `,
  styles: [
    `
      .hub-create {
        max-width: 600px;
        margin: 0 auto;
      }
      .hub-create__title {
        font-size: 1.75rem;
        margin-bottom: 1.5rem;
        color: #e0e0e0;
      }
      .hub-create__form {
        display: flex;
        flex-direction: column;
        gap: 1.25rem;
      }
      .form-field {
        display: flex;
        flex-direction: column;
        gap: 0.375rem;
        border: none;
        padding: 0;
        margin: 0;
      }
      .form-field label,
      .form-field legend {
        font-weight: 600;
        color: #c0c0d0;
        font-size: 0.875rem;
      }
      .form-field input[type="text"],
      .form-field select {
        padding: 0.625rem 0.75rem;
        border-radius: 8px;
        border: 1px solid #2a2a4a;
        background: #16213e;
        color: #e0e0e0;
        font-size: 1rem;
      }
      .form-field input[type="text"]:focus,
      .form-field select:focus {
        outline: none;
        border-color: #6c5ce7;
      }
      .form-field__error {
        color: #e74c3c;
        font-size: 0.8rem;
      }
      .checkbox-group,
      .radio-group {
        display: flex;
        gap: 1.25rem;
        flex-wrap: wrap;
      }
      .checkbox-label,
      .radio-label {
        display: flex;
        align-items: center;
        gap: 0.375rem;
        color: #c0c0d0;
        cursor: pointer;
      }
      .hub-create__error {
        padding: 0.75rem;
        border-radius: 8px;
        background: rgba(231, 76, 60, 0.15);
        color: #e74c3c;
        font-size: 0.875rem;
      }
      .hub-create__actions {
        display: flex;
        gap: 0.75rem;
        justify-content: flex-end;
        margin-top: 0.5rem;
      }
      .btn {
        padding: 0.625rem 1.5rem;
        border-radius: 8px;
        border: none;
        font-size: 0.95rem;
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
    `,
  ],
})
export class HubCreateComponent {
  private readonly fb = inject(FormBuilder);
  private readonly hubService = inject(HubService);
  private readonly router = inject(Router);

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly selectedProviders = signal<MusicProvider[]>(["spotify"]);
  readonly providerError = signal(false);

  readonly businessTypes: { value: HubBusinessType; label: string }[] = [
    { value: "bar", label: "Bar" },
    { value: "restaurant", label: "Restaurant" },
    { value: "gym", label: "Gym" },
    { value: "store", label: "Store" },
    { value: "office", label: "Office" },
    { value: "custom", label: "Custom" },
  ];

  readonly providers: { value: MusicProvider; label: string }[] = [
    { value: "spotify", label: "Spotify" },
    { value: "youtube_music", label: "YouTube Music" },
  ];

  readonly form: FormGroup = this.fb.group({
    name: [
      "",
      [Validators.required, Validators.minLength(3), Validators.maxLength(50)],
    ],
    businessType: ["bar" as HubBusinessType],
    visibility: ["public" as HubVisibility],
  });

  toggleProvider(provider: MusicProvider): void {
    const current = this.selectedProviders();
    const updated = current.includes(provider)
      ? current.filter((p) => p !== provider)
      : [...current, provider];
    this.selectedProviders.set(updated);
    this.providerError.set(updated.length === 0);
  }

  async onSubmit(): Promise<void> {
    this.form.markAllAsTouched();
    if (this.selectedProviders().length === 0) {
      this.providerError.set(true);
      return;
    }
    if (this.form.invalid) return;

    this.submitting.set(true);
    this.errorMessage.set(null);

    const request: CreateHubRequest = {
      name: this.form.value.name!,
      businessType: this.form.value.businessType!,
      musicProviders: this.selectedProviders(),
      visibility: this.form.value.visibility!,
    };

    try {
      const hub = await firstValueFrom(this.hubService.createHub(request));
      this.router.navigate(["/hub", hub.id]);
    } catch (err) {
      const message =
        (err as { error?: { detail?: string } })?.error?.detail ??
        (err instanceof Error ? err.message : "Failed to create hub");
      this.errorMessage.set(message);
    } finally {
      this.submitting.set(false);
    }
  }

  onCancel(): void {
    this.router.navigate(["/hub"]);
  }
}
