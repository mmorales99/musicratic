import { HttpInterceptorFn } from "@angular/common/http";
import { catchError, throwError } from "rxjs";

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error) => {
      const status = error.status ?? 0;
      const detail =
        error.error?.detail ??
        error.error?.title ??
        error.message ??
        "Unknown error";

      console.error(
        `[HTTP ${status}] ${req.method} ${req.urlWithParams} — ${detail}`,
      );

      return throwError(() => error);
    }),
  );
};
