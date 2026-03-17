import { Routes } from "@angular/router";
import { AnalyticsComponent } from "./analytics.component";

export const ANALYTICS_ROUTES: Routes = [
  { path: "", component: AnalyticsComponent },
  {
    path: "hub/:hubId",
    loadComponent: () =>
      import(
        "./components/analytics-dashboard/analytics-dashboard.component"
      ).then((m) => m.AnalyticsDashboardComponent),
  },
  {
    path: "hub/:hubId/reports",
    loadComponent: () =>
      import(
        "./components/reports-screen/reports-screen.component"
      ).then((m) => m.ReportsScreenComponent),
  },
];
