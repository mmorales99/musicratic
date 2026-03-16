import { Component, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-playback",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="playback-page">
      <h1>Now Playing</h1>
      <p>View the current track and queue.</p>
    </section>
  `,
})
export class PlaybackComponent {}
