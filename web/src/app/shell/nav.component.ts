import { Component, ChangeDetectionStrategy, inject } from "@angular/core";
import { RouterLink, RouterLinkActive } from "@angular/router";
import { AuthMachineService } from "@app/features/auth/machines/auth-machine.service";

@Component({
  selector: "app-nav",
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <nav class="nav">
      <a class="nav__brand" routerLink="/">Musicratic</a>
      <ul class="nav__links">
        <li>
          <a routerLink="/hub" routerLinkActive="active">Hubs</a>
        </li>
        @if (authMachine.isAuthenticated()) {
          <li>
            <a routerLink="/playback" routerLinkActive="active">Playback</a>
          </li>
          <li>
            <a routerLink="/voting" routerLinkActive="active">Voting</a>
          </li>
          <li>
            <a routerLink="/economy" routerLinkActive="active">Economy</a>
          </li>
          <li>
            <a routerLink="/profile" routerLinkActive="active">Profile</a>
          </li>
          <li>
            <a routerLink="/analytics" routerLinkActive="active">Analytics</a>
          </li>
        }
      </ul>
      <div class="nav__auth">
        @if (authMachine.isAuthenticated()) {
          <span class="nav__user">{{ authMachine.user()?.displayName }}</span>
          <button class="nav__button" (click)="onLogout()">Logout</button>
        } @else {
          <a class="nav__button" routerLink="/login">Login</a>
        }
      </div>
    </nav>
  `,
  styles: [
    `
      .nav {
        display: flex;
        align-items: center;
        gap: 2rem;
        padding: 0.75rem 1.5rem;
        background: #1a1a2e;
        color: #fff;
      }
      .nav__brand {
        font-size: 1.25rem;
        font-weight: 700;
        color: #6c5ce7;
      }
      .nav__links {
        display: flex;
        list-style: none;
        gap: 1rem;
        flex: 1;
      }
      .nav__links a {
        color: #ccc;
        padding: 0.25rem 0.5rem;
        border-radius: 4px;
        transition: color 0.2s;
      }
      .nav__links a:hover,
      .nav__links a.active {
        color: #fff;
        text-decoration: none;
      }
      .nav__auth {
        display: flex;
        align-items: center;
        gap: 0.75rem;
      }
      .nav__user {
        color: #a0a0b0;
        font-size: 0.875rem;
      }
      .nav__button {
        padding: 0.375rem 0.75rem;
        font-size: 0.875rem;
        color: #fff;
        background: #6c5ce7;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        text-decoration: none;
      }
      .nav__button:hover {
        background: #5a4bd1;
      }
    `,
  ],
})
export class NavComponent {
  readonly authMachine = inject(AuthMachineService);

  onLogout(): void {
    void this.authMachine.logout();
  }
}
