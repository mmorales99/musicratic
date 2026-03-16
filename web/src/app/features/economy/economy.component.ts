import { Component, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-economy",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="economy-page">
      <h1>Economy</h1>
      <p>Manage your wallet and purchase coins.</p>
    </section>
  `,
})
export class EconomyComponent {}
