import { Injectable, inject, signal, computed, OnDestroy } from "@angular/core";
import {
  createActor,
  Subscription as XStateSubscription,
} from "xstate";
import { firstValueFrom } from "rxjs";
import { EconomyService } from "@app/shared/services/economy.service";
import {
  economyMachine,
  EconomyMachineContext,
} from "@app/machines/economy.machine";
import {
  Wallet,
  Transaction,
  CoinPackage,
  TransactionType,
} from "@app/shared/models/economy.model";

@Injectable()
export class EconomyMachineService implements OnDestroy {
  private readonly economyService = inject(EconomyService);
  private readonly actor = createActor(economyMachine);
  private actorSub: XStateSubscription | null = null;

  private readonly stateValue = signal<string>("idle");
  private readonly ctx = signal<EconomyMachineContext>(
    this.actor.getSnapshot().context,
  );

  readonly state = this.stateValue.asReadonly();
  readonly wallet = computed<Wallet | null>(() => this.ctx().wallet);
  readonly transactions = computed<Transaction[]>(
    () => this.ctx().transactions,
  );
  readonly hasMoreTransactions = computed<boolean>(
    () => this.ctx().hasMoreTransactions,
  );
  readonly coinPackages = computed<CoinPackage[]>(
    () => this.ctx().coinPackages,
  );
  readonly error = computed<string | null>(() => this.ctx().error);

  readonly isLoadingWallet = computed(
    () => this.stateValue() === "loadingWallet",
  );
  readonly isLoadingTransactions = computed(
    () => this.stateValue() === "loadingTransactions",
  );
  readonly isLoadingMore = computed(
    () => this.stateValue() === "loadingMore",
  );
  readonly isLoadingPackages = computed(
    () => this.stateValue() === "loadingPackages",
  );
  readonly isPurchasing = computed(
    () => this.stateValue() === "purchasing",
  );

  constructor() {
    this.actorSub = this.actor.subscribe((snapshot) => {
      this.stateValue.set(snapshot.value as string);
      this.ctx.set(snapshot.context);
    });
    this.actor.start();
  }

  async loadWallet(): Promise<void> {
    this.actor.send({ type: "LOAD_WALLET" });
    try {
      const wallet = await firstValueFrom(this.economyService.getWallet());
      this.actor.send({ type: "WALLET_LOADED", wallet });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load wallet";
      this.actor.send({ type: "LOAD_FAILED", error: message });
    }
  }

  async loadTransactions(filter?: TransactionType | null): Promise<void> {
    if (filter !== undefined) {
      this.actor.send({ type: "SET_TRANSACTION_FILTER", filter });
    } else {
      this.actor.send({ type: "LOAD_TRANSACTIONS" });
    }
    try {
      const ctx = this.ctx();
      const activeFilter = filter !== undefined ? filter : ctx.transactionFilter;
      const envelope = await firstValueFrom(
        this.economyService.getTransactions(
          1,
          ctx.transactionPageSize,
          activeFilter ?? undefined,
        ),
      );
      this.actor.send({
        type: "TRANSACTIONS_LOADED",
        transactions: envelope.items,
        hasMore: envelope.hasMoreItems,
      });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load transactions";
      this.actor.send({ type: "LOAD_FAILED", error: message });
    }
  }

  async loadMoreTransactions(): Promise<void> {
    this.actor.send({ type: "LOAD_MORE_TRANSACTIONS" });
    try {
      const ctx = this.ctx();
      const nextPage = ctx.transactionPage + 1;
      const envelope = await firstValueFrom(
        this.economyService.getTransactions(
          nextPage,
          ctx.transactionPageSize,
          ctx.transactionFilter ?? undefined,
        ),
      );
      this.actor.send({
        type: "MORE_TRANSACTIONS_LOADED",
        transactions: envelope.items,
        hasMore: envelope.hasMoreItems,
      });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load more transactions";
      this.actor.send({ type: "LOAD_FAILED", error: message });
    }
  }

  async loadPackages(): Promise<void> {
    this.actor.send({ type: "LOAD_PACKAGES" });
    try {
      const packages = await firstValueFrom(
        this.economyService.getCoinPackages(),
      );
      this.actor.send({ type: "PACKAGES_LOADED", packages });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load coin packages";
      this.actor.send({ type: "LOAD_FAILED", error: message });
    }
  }

  async purchase(packageId: string): Promise<void> {
    this.actor.send({ type: "PURCHASE", packageId });
    try {
      const session = await firstValueFrom(
        this.economyService.createCheckoutSession(packageId),
      );
      this.actor.send({ type: "CHECKOUT_READY", url: session.url });
      window.location.href = session.url;
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to create checkout";
      this.actor.send({ type: "LOAD_FAILED", error: message });
    }
  }

  retry(): void {
    this.actor.send({ type: "RETRY" });
  }

  ngOnDestroy(): void {
    this.actorSub?.unsubscribe();
    this.actor.stop();
  }
}
