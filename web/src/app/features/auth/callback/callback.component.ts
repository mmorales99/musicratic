import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
} from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { AuthMachineService } from "../machines/auth-machine.service";

@Component({
  selector: "app-auth-callback",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="callback-page">
      @if (errorMessage()) {
        <div class="callback-error">
          <h2>Authentication Failed</h2>
          <p>{{ errorMessage() }}</p>
          <a routerLink="/login">Back to Login</a>
        </div>
      } @else {
        <div class="callback-loading">
          <p>Signing you in...</p>
        </div>
      }
    </section>
  `,
  styles: [
    `
      .callback-page {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 80vh;
      }
      .callback-loading {
        text-align: center;
        color: #a0a0b0;
        font-size: 1.25rem;
      }
      .callback-error {
        text-align: center;
        padding: 2rem;
        background: #16213e;
        border-radius: 12px;
        max-width: 400px;
      }
      .callback-error h2 {
        color: #e74c3c;
        margin-bottom: 0.5rem;
      }
      .callback-error p {
        color: #a0a0b0;
        margin-bottom: 1rem;
      }
      .callback-error a {
        color: #6c5ce7;
        text-decoration: underline;
      }
    `,
  ],
})
export class CallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authMachine = inject(AuthMachineService);

  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    const code = this.route.snapshot.queryParamMap.get("code");
    const state = this.route.snapshot.queryParamMap.get("state");

    if (!code || !state) {
      this.errorMessage.set("Missing authorization code or state parameter.");
      return;
    }

    void this.processCallback(code, state);
  }

  private async processCallback(code: string, state: string): Promise<void> {
    try {
      await this.authMachine.handleCallback(code, state);
      this.router.navigate(["/hub"]);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Authentication failed";
      this.errorMessage.set(message);
    }
  }
}
