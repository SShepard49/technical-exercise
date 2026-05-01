import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { errorInterceptor } from './error.interceptor';
import { NotificationService } from '../services/notification.service';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let notifications: { userError: ReturnType<typeof vi.fn>; serverError: ReturnType<typeof vi.fn>; success: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    notifications = {
      userError: vi.fn(),
      serverError: vi.fn(),
      success: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: NotificationService, useValue: notifications },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('shows a userError toast with the API message on a 400 BadRequest', () => {
    http.get('/anywhere').subscribe({
      next: () => {
        // not expected
      },
      error: () => {
        // swallowed; interceptor still toasts
      },
    });

    httpMock.expectOne('/anywhere').flush(
      { success: false, message: 'Name is required.', responseCode: 400 },
      { status: 400, statusText: 'Bad Request' },
    );

    expect(notifications.userError).toHaveBeenCalledWith('Name is required.');
    expect(notifications.serverError).not.toHaveBeenCalled();
  });

  it('falls back to a generic message when the 4xx body has no message', () => {
    http.get('/anywhere').subscribe({ error: () => undefined });

    httpMock.expectOne('/anywhere').flush(null, { status: 422, statusText: 'Unprocessable' });

    expect(notifications.userError).toHaveBeenCalledTimes(1);
    expect(notifications.userError.mock.calls[0]?.[0]).toMatch(/could not be completed/i);
  });

  it('shows a serverError toast on a 500', () => {
    http.get('/anywhere').subscribe({ error: () => undefined });

    httpMock.expectOne('/anywhere').flush(
      { success: false, message: 'boom', responseCode: 500 },
      { status: 500, statusText: 'Internal Server Error' },
    );

    expect(notifications.serverError).toHaveBeenCalledTimes(1);
    expect(notifications.userError).not.toHaveBeenCalled();
  });

  it('treats network failures (status 0) as serverError', () => {
    http.get('/anywhere').subscribe({ error: () => undefined });

    httpMock
      .expectOne('/anywhere')
      .error(new ProgressEvent('error'), { status: 0, statusText: 'Unknown' });

    expect(notifications.serverError).toHaveBeenCalledTimes(1);
  });
});
