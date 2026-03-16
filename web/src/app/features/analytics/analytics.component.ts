import { Component, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-analytics",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="analytics-page">
      <h1>Analytics</h1>
      <p>Hub statistics and performance reports.</p>
    </section>
  `,
})
export class AnalyticsComponent {}
