import { Component, ChangeDetectionStrategy } from "@angular/core";
import { RouterLink } from "@angular/router";

@Component({
  selector: "app-playback",
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="playback-page">
      <h1>Playback</h1>
      <p>Join a hub to view the live queue and now-playing track.</p>
    </section>
  `,
})
export class PlaybackComponent {}
