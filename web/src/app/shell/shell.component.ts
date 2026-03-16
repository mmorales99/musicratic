import { Component, ChangeDetectionStrategy } from "@angular/core";
import { NavComponent } from "./nav.component";

@Component({
  selector: "app-shell",
  standalone: true,
  imports: [NavComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="shell">
      <app-nav />
      <main class="shell__content">
        <ng-content />
      </main>
    </div>
  `,
  styles: [
    `
      .shell {
        display: flex;
        flex-direction: column;
        min-height: 100vh;
      }
      .shell__content {
        flex: 1;
        padding: 1rem;
        max-width: 1200px;
        margin: 0 auto;
        width: 100%;
      }
    `,
  ],
})
export class ShellComponent {}
