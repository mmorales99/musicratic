import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
} from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { HubJoinMachineService } from "../machines/hub-join-machine.service";

@Component({
  selector: "app-hub-join",
  standalone: true,
  imports: [FormsModule],
  providers: [HubJoinMachineService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="hub-join">
      <h1 class="hub-join__title">Join a Hub</h1>
      <p class="hub-join__subtitle">
        Enter the hub code shared by your venue to join and start voting on
        music.
      </p>

      <form class="hub-join__form" (ngSubmit)="onSubmit()">
        <div class="form-field">
          <label for="hubCode">Hub Code</label>
          <input
            id="hubCode"
            type="text"
            [ngModel]="machine.code()"
            (ngModelChange)="machine.setCode($event)"
            name="hubCode"
            placeholder="e.g. ABC123"
            maxlength="20"
            autocomplete="off"
            [disabled]="machine.isAttaching()"
          />
        </div>

        @if (machine.error(); as err) {
          <div class="hub-join__error">{{ err }}</div>
        }

        <div class="hub-join__actions">
          <button
            type="submit"
            class="btn btn--primary"
            [disabled]="machine.isAttaching() || !machine.code().trim()"
          >
            {{ machine.isAttaching() ? "Joining..." : "Join Hub" }}
          </button>
        </div>
      </form>
    </section>
  `,
  styles: [
    `
      .hub-join {
        max-width: 480px;
        margin: 3rem auto 0;
        text-align: center;
      }
      .hub-join__title {
        font-size: 1.75rem;
        color: #e0e0e0;
        margin: 0 0 0.5rem;
      }
      .hub-join__subtitle {
        color: #a0a0b0;
        margin-bottom: 2rem;
        font-size: 0.95rem;
      }
      .hub-join__form {
        display: flex;
        flex-direction: column;
        gap: 1.25rem;
        text-align: left;
      }
      .form-field {
        display: flex;
        flex-direction: column;
        gap: 0.375rem;
      }
      .form-field label {
        font-weight: 600;
        color: #c0c0d0;
        font-size: 0.875rem;
      }
      .form-field input[type="text"] {
        padding: 0.75rem 1rem;
        border-radius: 8px;
        border: 1px solid #2a2a4a;
        background: #16213e;
        color: #e0e0e0;
        font-size: 1.125rem;
        font-family: monospace;
        letter-spacing: 0.15rem;
        text-align: center;
        text-transform: uppercase;
      }
      .form-field input:focus {
        outline: none;
        border-color: #6c5ce7;
      }
      .form-field input:disabled {
        opacity: 0.6;
      }
      .hub-join__error {
        padding: 0.75rem;
        border-radius: 8px;
        background: rgba(231, 76, 60, 0.15);
        color: #e74c3c;
        font-size: 0.875rem;
        text-align: center;
      }
      .hub-join__actions {
        display: flex;
        justify-content: center;
      }
      .btn {
        padding: 0.75rem 2rem;
        border-radius: 8px;
        border: none;
        font-size: 1rem;
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
    `,
  ],
})
export class HubJoinComponent implements OnInit {
  protected readonly machine = inject(HubJoinMachineService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly navigating = signal(false);

  ngOnInit(): void {
    const codeFromUrl = this.route.snapshot.paramMap.get("code");
    if (codeFromUrl) {
      this.machine.setCode(codeFromUrl);
      this.onSubmit();
    }
  }

  async onSubmit(): Promise<void> {
    await this.machine.submit();

    if (this.machine.isAttached()) {
      const hubId = this.machine.attachment()?.hubId;
      if (hubId) {
        this.navigating.set(true);
        this.router.navigate(["/hub", hubId]);
      }
    }
  }
}
