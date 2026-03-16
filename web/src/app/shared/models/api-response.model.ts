export interface ApiEnvelope<T> {
  success: boolean;
  totalItemsInResponse: number;
  hasMoreItems: boolean;
  items: T[];
  audit: ApiAudit;
}

export interface ApiAudit {
  requestId: string;
  timestamp: string;
}

export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance: string;
}
