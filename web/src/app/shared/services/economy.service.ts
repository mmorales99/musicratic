import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "./bff-api.service";
import {
  Wallet,
  Transaction,
  CoinPackage,
  Subscription,
  TrackPrice,
  CheckoutSession,
} from "@app/shared/models/economy.model";
import { ApiEnvelope } from "@app/shared/models/api-response.model";

@Injectable({ providedIn: "root" })
export class EconomyService {
  private readonly api = inject(BffApiService);

  getWallet(): Observable<Wallet> {
    return this.api.get<Wallet>("/wallet");
  }

  getTransactions(
    page: number,
    pageSize: number,
    type?: string,
  ): Observable<ApiEnvelope<Transaction>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (type) {
      params.set("type", type);
    }
    return this.api.get<ApiEnvelope<Transaction>>(
      `/wallet/transactions?${params.toString()}`,
    );
  }

  getCoinPackages(): Observable<CoinPackage[]> {
    return this.api.get<CoinPackage[]>("/coin-packages");
  }

  getSubscription(hubId: string): Observable<Subscription> {
    return this.api.get<Subscription>(
      `/subscriptions/${encodeURIComponent(hubId)}`,
    );
  }

  getTrackPrice(
    durationSeconds: number,
    hotnessScore: number,
  ): Observable<TrackPrice> {
    const params = new URLSearchParams({
      durationSeconds: durationSeconds.toString(),
      hotnessScore: hotnessScore.toString(),
    });
    return this.api.get<TrackPrice>(`/pricing/track?${params.toString()}`);
  }

  createCheckoutSession(coinPackageId: string): Observable<CheckoutSession> {
    return this.api.post<CheckoutSession>("/purchases/checkout", {
      coinPackageId,
    });
  }

  startTrial(hubId: string): Observable<Subscription> {
    return this.api.post<Subscription>(
      `/subscriptions/${encodeURIComponent(hubId)}/trial`,
      {},
    );
  }
}
