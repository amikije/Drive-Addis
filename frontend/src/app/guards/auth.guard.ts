import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

/**
 * Ensures user is authenticated before accessing protected routes
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  if (authService.isLoggedIn()) {
    return true;
  }

  notificationService.warning('Authentication Required', 'Please log in to access this portal.');
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/**
 * Role guard for Student routes
 */
export const studentGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  if (!authService.isLoggedIn()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  const role = authService.getUserRole()?.toLowerCase();
  if (role === 'student' || role === 'admin') {
    return true;
  }

  notificationService.error('Access Denied (403)', 'You need a Student account to access the Student Portal.');
  return router.createUrlTree(['/']);
};

/**
 * Role guard for Instructor routes
 */
export const instructorGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  if (!authService.isLoggedIn()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  const role = authService.getUserRole()?.toLowerCase();
  if (role === 'instructor' || role === 'admin') {
    return true;
  }

  notificationService.error('Access Denied (403)', 'Only verified Instructors can access this dashboard.');
  return router.createUrlTree(['/']);
};

/**
 * Role guard for Admin routes
 */
export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  if (!authService.isLoggedIn()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  const role = authService.getUserRole()?.toLowerCase();
  if (role === 'admin') {
    return true;
  }

  notificationService.error('Access Denied (403)', 'Administrative privileges required.');
  return router.createUrlTree(['/']);
};
