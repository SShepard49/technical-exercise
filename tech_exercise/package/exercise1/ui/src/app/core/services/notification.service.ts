import { Injectable, inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

/**
 * Wraps ngx-toastr with three semantic methods that map to the API
 * response categories the UI cares about: success, user-data error
 * (HTTP 4xx), and server error (HTTP 5xx / network failure).
 *
 * The current UI is read-only, so success() exists for completeness
 * but is not invoked. userError and serverError are driven by the
 * error HTTP interceptor.
 */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly toastr = inject(ToastrService);

  success(message: string, title = 'Success'): void {
    this.toastr.success(message, title, { timeOut: 3000 });
  }

  userError(message: string, title = 'Check your input'): void {
    this.toastr.warning(message, title, { timeOut: 5000 });
  }

  serverError(message: string, title = 'Something went wrong'): void {
    this.toastr.error(message, title, { timeOut: 6000 });
  }
}
