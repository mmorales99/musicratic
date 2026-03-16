import { Component, ChangeDetectionStrategy } from "@angular/core";
import { RouterLink, RouterLinkActive } from "@angular/router";

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
      </ul>
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
    `,
  ],
})
export class NavComponent {}
