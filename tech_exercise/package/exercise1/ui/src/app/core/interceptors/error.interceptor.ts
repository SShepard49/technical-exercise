import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { NotificationService } from '../services/notification.service';
import { BaseResponse } from '../models';

const isBaseResponse = (value: unknown): value is BaseResponse => {
  return (
    !!value &&
    typeof value === 'object' &&
    'message' in value &&
    typeof (value as { message: unknown }).message === 'string'
  );
};

/**
 * Maps HTTP failures to one of two toast categories:
 *   - 4xx -> userError (orange) using BaseResponse.message from the API
 *   - 5xx or status === 0 (network/CORS) -> serverError (red), generic copy
 * The original error is still rethrown so callers can drive their own UI.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse) {
        if (err.status >= 400 && err.status < 500) {
          const message = isBaseResponse(err.error)
            ? err.error.message
            : 'Your request could not be completed.';
          notifications.userError(message);
        } else {
          // Covers 5xx, status 0 (network/CORS), and any unexpected non-4xx codes.
          // Leave the original error in the console for diagnosis; show generic copy.
          console.error('[errorInterceptor]', err);
          notifications.serverError(
            'The Stargate API could not be reached or returned an unexpected error.',
          );
        }
      }
      return throwError(() => err);
    }),
  );
};
