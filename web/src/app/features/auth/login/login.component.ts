import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
} from "@angular/core";
import { Router } from "@angular/router";
import { AuthMachineService } from "../machines/auth-machine.service";

@Component({
  selector: "app-login",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="login-page">
      <div class="login-card">
        <h1 class="login-card__title">Musicratic</h1>
        <p class="login-card__subtitle">
          Collaborative music, democratic voting.
        </p>
        <button
          class="login-card__button"
          (click)="onLogin()"
          [disabled]="authMachine.isLoading()"
        >
          Login with Musicratic
        </button>
        @if (authMachine.error(); as errorMsg) {
          <p class="login-card__error">{{ errorMsg }}</p>
        }
      </div>
    </section>
  `,
  styles: [
    `
      .login-page {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 80vh;
      }
      .login-card {
        text-align: center;
        padding: 3rem 2rem;
        border-radius: 12px;
        background: #16213e;
        max-width: 400px;
        width: 100%;
      }
      .login-card__title {
        font-size: 2rem;
        color: #6c5ce7;
        margin-bottom: 0.5rem;
      }
      .login-card__subtitle {
        color: #a0a0b0;
        margin-bottom: 2rem;
      }
      .login-card__button {
        display: inline-block;
        padding: 0.75rem 2rem;
        font-size: 1rem;
        font-weight: 600;
        color: #fff;
        background: #6c5ce7;
        border: none;
        border-radius: 8px;
        cursor: pointer;
        transition: background 0.2s;
      }
      .login-card__button:hover:not(:disabled) {
        background: #5a4bd1;
      }
      .login-card__button:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }
      .login-card__error {
        color: #e74c3c;
        margin-top: 1rem;
        font-size: 0.875rem;
      }
    `,
  ],
})
export class LoginComponent implements OnInit {
  readonly authMachine = inject(AuthMachineService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    if (this.authMachine.isAuthenticated()) {
      this.router.navigate(["/hub"]);
    }
  }

  onLogin(): void {
    this.authMachine.login();
  }
}
