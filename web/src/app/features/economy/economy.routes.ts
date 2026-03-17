import { Routes } from "@angular/router";

export const ECONOMY_ROUTES: Routes = [
  {
    path: "",
    loadComponent: () =>
      import("./wallet/wallet.component").then((m) => m.WalletComponent),
  },
  {
    path: "purchase",
    loadComponent: () =>
      import("./purchase/coin-purchase.component").then(
        (m) => m.CoinPurchaseComponent,
      ),
  },
];
