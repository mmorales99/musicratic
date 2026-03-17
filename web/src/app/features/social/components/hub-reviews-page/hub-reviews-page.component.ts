import { Component, ChangeDetectionStrategy, inject } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { HubReviewsComponent } from "../hub-reviews/hub-reviews.component";

@Component({
  selector: "app-hub-reviews-page",
  standalone: true,
  imports: [HubReviewsComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: ` <app-hub-reviews [hubId]="hubId" /> `,
})
export class HubReviewsPageComponent {
  readonly hubId: string;

  constructor() {
    const route = inject(ActivatedRoute);
    this.hubId = route.snapshot.paramMap.get("hubId") ?? "";
  }
}
