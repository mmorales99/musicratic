import { Routes } from "@angular/router";
import { HubComponent } from "./hub.component";

export const HUB_ROUTES: Routes = [
  { path: "", component: HubComponent },
  {
    path: "join",
    loadComponent: () =>
      import("./join/hub-join.component").then((m) => m.HubJoinComponent),
  },
  {
    path: "create",
    loadComponent: () =>
      import("./create/hub-create.component").then((m) => m.HubCreateComponent),
  },
  {
    path: ":id",
    loadComponent: () =>
      import("./detail/hub-detail.component").then((m) => m.HubDetailComponent),
  },
  {
    path: ":hubId/lists/:listId",
    loadComponent: () =>
      import("./lists/list-detail.component").then(
        (m) => m.ListDetailComponent,
      ),
  },
];
