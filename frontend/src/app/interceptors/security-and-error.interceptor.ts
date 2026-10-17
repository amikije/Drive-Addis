import { Injectable, inject } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { Router } from '@angular/router';

@Injectable()
export class SecurityAndErrorInterceptor implements HttpInterceptor {
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Clone request with Security headers (XSRF, Content Type) and withCredentials
    let headersConfig: Record<string, string> = {
      'X-Requested-With': 'XMLHttpRequest'
    };

    // XSRF Double-Submit Cookie Pattern (Module 10)
    const xsrfToken = this.getXsrfCookie();
    if (xsrfToken) {
      headersConfig['X-XSRF-TOKEN'] = xsrfToken;
    }

    const modifiedReq = request.clone({
      setHeaders: headersConfig,
      withCredentials: true // Crucial for HttpOnly cookies
    });

    return next.handle(modifiedReq).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMsg = 'An unexpected error occurred.';

        if (error.status === 401) {
          // If login failed, let the login component display the credential/rate-limiting message
          if (!request.url.includes('/api/auth/login')) {
            // Unauthorized - Session expired or invalid token
            this.notificationService.warning('Session Expired', 'Please sign in again to continue.');
            this.authService.logout();
            this.router.navigate(['/login']);
          }
        } else if (error.status === 403) {
          // Forbidden - Resource/Role based authorization failure (Module 11)
          this.notificationService.error('Forbidden (403)', 'You do not have permission for this resource.');
        } else if (error.status === 429) {
          // Rate Limiting (Module 11 - 429 Too Many Requests with Retry-After header)
          const retryAfter = error.headers.get('Retry-After') || '10';
          this.notificationService.warning(
            'Rate Limit Exceeded (429)',
            `Too many requests. Please wait ${retryAfter} seconds before retrying.`
          );
        } else if (error.status === 0) {
          // Connection / CORS issue
          this.notificationService.error('Network Error', 'Unable to reach backend server. Check API connection.');
        }

        return throwError(() => error);
      })
    );
  }

  private getXsrfCookie(): string | null {
    if (typeof document === 'undefined') return null;
    const match = document.cookie.match(new RegExp('(^|;\\s*)XSRF-TOKEN=([^;]*)'));
    return match ? decodeURIComponent(match[2]) : null;
  }
}
