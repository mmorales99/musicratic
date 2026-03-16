import { Component, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-hub",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="hub-page">
      <h1>Hub Discovery</h1>
      <p>Browse and join active hubs.</p>
    </section>
  `,
})
export class HubComponent {}
