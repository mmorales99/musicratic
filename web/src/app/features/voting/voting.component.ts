import { Component, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-voting",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="voting-page">
      <h1>Voting</h1>
      <p>Cast your votes on queued tracks.</p>
    </section>
  `,
})
export class VotingComponent {}
