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
    path: ":hubId/members",
    loadComponent: () =>
      import(
        "./components/member-management/member-management.component"
      ).then((m) => m.MemberManagementComponent),
  },
  {
    path: ":hubId/roles",
    loadComponent: () =>
      import(
        "./components/role-assignment/role-assignment.component"
      ).then((m) => m.RoleAssignmentComponent),
  },
  {
    path: ":hubId/lists/:listId",
    loadComponent: () =>
      import("./lists/list-detail.component").then(
        (m) => m.ListDetailComponent,
      ),
  },
];
