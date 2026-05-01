import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

import { NotificationService } from '../../core/services/notification.service';
import { API_BASE_URL } from '../../core/tokens';

@Component({
  selector: 'app-api-error-test-page',
  imports: [MatCardModule, MatButtonModule, MatIconModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './api-error-test.page.html',
  styleUrl: './api-error-test.page.scss',
})
export class ApiErrorTestPage {
  private readonly http = inject(HttpClient);
  private readonly notifications = inject(NotificationService);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  protected showSuccessToast(): void {
    this.notifications.success('Manual success toast triggered from API Error Test.');
  }

  protected trigger4xxToast(): void {
    this.http.get(`${this.apiBaseUrl}/__toast-test__/missing`).subscribe({
      next: () => {
        // This request should fail. If not, surface a visible signal to the tester.
        this.notifications.userError('Expected a 4xx response, but the request succeeded.');
      },
      error: () => {
        // Error toasts are handled globally by errorInterceptor.
      },
    });
  }

  protected triggerNetworkToast(): void {
    this.http.get('http://127.0.0.1:1/__toast-test__/network-failure').subscribe({
      next: () => {
        this.notifications.userError('Expected a network failure, but the request succeeded.');
      },
      error: () => {
        // Error toasts are handled globally by errorInterceptor.
      },
    });
  }
}
