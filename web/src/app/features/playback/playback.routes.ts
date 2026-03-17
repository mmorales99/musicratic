import { Routes } from "@angular/router";
import { PlaybackComponent } from "./playback.component";

export const PLAYBACK_ROUTES: Routes = [
  { path: "", component: PlaybackComponent },
  {
    path: "queue/:hubId",
    loadComponent: () =>
      import("./queue/queue.component").then((m) => m.QueueComponent),
  },
  {
    path: "search/:hubId",
    loadComponent: () =>
      import("./search/track-search.component").then(
        (m) => m.TrackSearchComponent,
      ),
  },
  {
    path: "propose/:hubId",
    loadComponent: () =>
      import(
        "./components/track-proposal/track-proposal.component"
      ).then((m) => m.TrackProposalComponent),
  },
];
