import { Injectable, OnDestroy } from "@angular/core";
import { Observable, Subject, share, filter, map } from "rxjs";
import { webSocket, WebSocketSubject } from "rxjs/webSocket";
import { environment } from "@env/environment";

export interface WsMessage<T> {
  type: string;
  hubId: string;
  payload: T;
  timestamp: string;
}

@Injectable({ providedIn: "root" })
export class WebSocketService implements OnDestroy {
  private socket$: WebSocketSubject<WsMessage<unknown>> | null = null;
  private messages$: Observable<WsMessage<unknown>> | null = null;
  private readonly destroy$ = new Subject<void>();

  connect(hubId: string): void {
    if (this.socket$) {
      this.disconnect();
    }

    const url = `${environment.wsUrl}?hubId=${encodeURIComponent(hubId)}`;
    this.socket$ = webSocket<WsMessage<unknown>>(url);
    this.messages$ = this.socket$.pipe(share());
  }

  on<T>(messageType: string): Observable<WsMessage<T>> {
    if (!this.messages$) {
      return new Observable();
    }

    return this.messages$.pipe(
      filter((msg) => msg.type === messageType),
      map((msg) => msg as WsMessage<T>),
    );
  }

  send<T>(message: WsMessage<T>): void {
    if (this.socket$) {
      this.socket$.next(message as WsMessage<unknown>);
    }
  }

  disconnect(): void {
    if (this.socket$) {
      this.socket$.complete();
      this.socket$ = null;
      this.messages$ = null;
    }
  }

  ngOnDestroy(): void {
    this.disconnect();
    this.destroy$.next();
    this.destroy$.complete();
  }
}
