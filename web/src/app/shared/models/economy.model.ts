export type TransactionType =
  | "credit"
  | "debit"
  | "refund"
  | "purchase"
  | "reward";

export type CurrencyCode = "EUR" | "USD";

export interface Wallet {
  userId: string;
  balance: number;
  currency: CurrencyCode;
}

export interface Transaction {
  id: string;
  amount: number;
  type: TransactionType;
  reason: string;
  referenceId: string | null;
  createdAt: string;
}

export interface CoinPackage {
  id: string;
  name: string;
  coinAmount: number;
  priceUsd: number;
  bonusCoins: number;
}

export interface CheckoutSession {
  sessionId: string;
  url: string;
}

export type SubscriptionTier = "free_trial" | "monthly" | "annual" | "event";

export interface Subscription {
  hubId: string;
  tier: SubscriptionTier;
  startedAt: string;
  expiresAt: string;
  trialEndsAt: string | null;
  isActive: boolean;
}

export interface TrackPrice {
  baseCost: number;
  hotnessMultiplier: number;
  finalCost: number;
  hotnessLabel: string;
}

export interface SubscriptionTierInfo {
  tier: SubscriptionTier;
  label: string;
  priceLabel: string;
  hubLimit: number;
  listLimit: number | null;
  subOwnerLimit: number;
  features: string[];
}

export const SUBSCRIPTION_TIERS: SubscriptionTierInfo[] = [
  {
    tier: "free_trial",
    label: "Free Trial",
    priceLabel: "€0 / 30 days",
    hubLimit: 1,
    listLimit: 1,
    subOwnerLimit: 0,
    features: [
      "1 hub",
      "1 list per hub",
      "Spotify only",
      "Basic analytics",
      "Ads shown to visitors",
    ],
  },
  {
    tier: "monthly",
    label: "Monthly",
    priceLabel: "€14.99/mo",
    hubLimit: 3,
    listLimit: 5,
    subOwnerLimit: 2,
    features: [
      "Up to 3 hubs",
      "5 lists per hub",
      "Spotify + YouTube",
      "Full analytics",
      "No ads",
      "2 sub-owners",
    ],
  },
  {
    tier: "annual",
    label: "Annual",
    priceLabel: "€119.99/yr (save 33%)",
    hubLimit: 10,
    listLimit: null,
    subOwnerLimit: 10,
    features: [
      "Up to 10 hubs",
      "Unlimited lists",
      "All sources + local storage",
      "Full analytics + CSV export",
      "No ads",
      "Custom QR branding",
      "API access",
      "Priority support",
      "10 sub-owners",
    ],
  },
  {
    tier: "event",
    label: "Event",
    priceLabel: "From €9.99",
    hubLimit: 1,
    listLimit: null,
    subOwnerLimit: 10,
    features: [
      "All Annual features",
      "For event duration only",
      "6h / 12h / 24h / 48h options",
    ],
  },
];
