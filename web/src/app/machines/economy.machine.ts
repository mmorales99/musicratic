import { setup, assign } from "xstate";
import {
  Wallet,
  Transaction,
  CoinPackage,
  TransactionType,
} from "@app/shared/models/economy.model";

export interface EconomyMachineContext {
  wallet: Wallet | null;
  transactions: Transaction[];
  transactionPage: number;
  transactionPageSize: number;
  transactionFilter: TransactionType | null;
  hasMoreTransactions: boolean;
  coinPackages: CoinPackage[];
  checkoutUrl: string | null;
  error: string | null;
}

export type EconomyMachineEvent =
  | { type: "LOAD_WALLET" }
  | { type: "WALLET_LOADED"; wallet: Wallet }
  | { type: "LOAD_TRANSACTIONS" }
  | {
      type: "TRANSACTIONS_LOADED";
      transactions: Transaction[];
      hasMore: boolean;
    }
  | { type: "LOAD_MORE_TRANSACTIONS" }
  | {
      type: "MORE_TRANSACTIONS_LOADED";
      transactions: Transaction[];
      hasMore: boolean;
    }
  | { type: "SET_TRANSACTION_FILTER"; filter: TransactionType | null }
  | { type: "LOAD_PACKAGES" }
  | { type: "PACKAGES_LOADED"; packages: CoinPackage[] }
  | { type: "PURCHASE"; packageId: string }
  | { type: "CHECKOUT_READY"; url: string }
  | { type: "PURCHASE_SUCCESS" }
  | { type: "PURCHASE_CANCELLED" }
  | { type: "LOAD_FAILED"; error: string }
  | { type: "RETRY" };

export const economyMachine = setup({
  types: {
    context: {} as EconomyMachineContext,
    events: {} as EconomyMachineEvent,
  },
}).createMachine({
  id: "economy",
  initial: "idle",
  context: {
    wallet: null,
    transactions: [],
    transactionPage: 1,
    transactionPageSize: 20,
    transactionFilter: null,
    hasMoreTransactions: false,
    coinPackages: [],
    checkoutUrl: null,
    error: null,
  },
  states: {
    idle: {
      on: {
        LOAD_WALLET: { target: "loadingWallet" },
        LOAD_PACKAGES: { target: "loadingPackages" },
      },
    },
    loadingWallet: {
      on: {
        WALLET_LOADED: {
          target: "walletReady",
          actions: assign({
            wallet: ({ event }) => event.wallet,
            error: () => null,
          }),
        },
        LOAD_FAILED: {
          target: "error",
          actions: assign({ error: ({ event }) => event.error }),
        },
      },
    },
    walletReady: {
      on: {
        LOAD_WALLET: { target: "loadingWallet" },
        LOAD_TRANSACTIONS: { target: "loadingTransactions" },
        LOAD_MORE_TRANSACTIONS: { target: "loadingMore" },
        LOAD_PACKAGES: { target: "loadingPackages" },
        SET_TRANSACTION_FILTER: {
          target: "loadingTransactions",
          actions: assign({
            transactionFilter: ({ event }) => event.filter,
            transactionPage: () => 1,
            transactions: () => [],
          }),
        },
      },
    },
    loadingTransactions: {
      on: {
        TRANSACTIONS_LOADED: {
          target: "walletReady",
          actions: assign({
            transactions: ({ event }) => event.transactions,
            hasMoreTransactions: ({ event }) => event.hasMore,
            error: () => null,
          }),
        },
        LOAD_FAILED: {
          target: "walletReady",
          actions: assign({ error: ({ event }) => event.error }),
        },
      },
    },
    loadingMore: {
      on: {
        MORE_TRANSACTIONS_LOADED: {
          target: "walletReady",
          actions: assign({
            transactions: ({ context, event }) => [
              ...context.transactions,
              ...event.transactions,
            ],
            transactionPage: ({ context }) => context.transactionPage + 1,
            hasMoreTransactions: ({ event }) => event.hasMore,
          }),
        },
        LOAD_FAILED: {
          target: "walletReady",
          actions: assign({ error: ({ event }) => event.error }),
        },
      },
    },
    loadingPackages: {
      on: {
        PACKAGES_LOADED: {
          target: "packagesReady",
          actions: assign({
            coinPackages: ({ event }) => event.packages,
            error: () => null,
          }),
        },
        LOAD_FAILED: {
          target: "error",
          actions: assign({ error: ({ event }) => event.error }),
        },
      },
    },
    packagesReady: {
      on: {
        LOAD_WALLET: { target: "loadingWallet" },
        PURCHASE: { target: "purchasing" },
      },
    },
    purchasing: {
      on: {
        CHECKOUT_READY: {
          target: "packagesReady",
          actions: assign({ checkoutUrl: ({ event }) => event.url }),
        },
        LOAD_FAILED: {
          target: "packagesReady",
          actions: assign({ error: ({ event }) => event.error }),
        },
      },
    },
    error: {
      on: {
        RETRY: { target: "idle" },
        LOAD_WALLET: { target: "loadingWallet" },
        LOAD_PACKAGES: { target: "loadingPackages" },
      },
    },
  },
});
