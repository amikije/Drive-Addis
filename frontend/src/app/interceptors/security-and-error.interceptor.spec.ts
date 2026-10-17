import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { SecurityAndErrorInterceptor } from './security-and-error.interceptor';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { Router } from '@angular/router';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

describe('SecurityAndErrorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let notificationService: NotificationService;
  let authService: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        {
          provide: HTTP_INTERCEPTORS,
          useClass: SecurityAndErrorInterceptor,
          multi: true
        },
        {
          provide: AuthService,
          useValue: { logout: vi.fn(), isLoggedIn: vi.fn() }
        },
        {
          provide: NotificationService,
          useValue: { warning: vi.fn(), error: vi.fn(), info: vi.fn(), success: vi.fn() }
        },
        {
          provide: Router,
          useValue: { navigate: vi.fn() }
        }
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    notificationService = TestBed.inject(NotificationService);
    authService = TestBed.inject(AuthService);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should set withCredentials and X-Requested-With header on outgoing requests', () => {
    http.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.headers.get('X-Requested-With')).toBe('XMLHttpRequest');
    req.flush({});
  });

  it('should handle 403 Forbidden with user alert', () => {
    http.get('/api/admin-only').subscribe({
      error: () => {
        expect(notificationService.error).toHaveBeenCalledWith(
          'Forbidden (403)',
          expect.stringContaining('do not have permission')
        );
      }
    });

    const req = httpMock.expectOne('/api/admin-only');
    req.flush({}, { status: 403, statusText: 'Forbidden' });
  });

  it('should handle 429 Rate Limit with Retry-After header', () => {
    http.get('/api/rate-limited').subscribe({
      error: () => {
        expect(notificationService.warning).toHaveBeenCalledWith(
          'Rate Limit Exceeded (429)',
          expect.stringContaining('30 seconds')
        );
      }
    });

    const req = httpMock.expectOne('/api/rate-limited');
    req.flush({}, {
      status: 429,
      statusText: 'Too Many Requests',
      headers: { 'Retry-After': '30' }
    });
  });
});
