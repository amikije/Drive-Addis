import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { studentGuard, instructorGuard, adminGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('Auth Guards', () => {
  let authServiceMock: Partial<AuthService>;
  let router: Router;

  const mockRoute = {} as ActivatedRouteSnapshot;
  const mockState = { url: '/protected' } as RouterStateSnapshot;

  beforeEach(() => {
    authServiceMock = {
      isLoggedIn: vi.fn(),
      getUserRole: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        {
          provide: NotificationService,
          useValue: { warning: vi.fn(), error: vi.fn(), info: vi.fn(), success: vi.fn() }
        }
      ]
    });

    router = TestBed.inject(Router);
  });

  describe('studentGuard', () => {
    it('redirects unauthenticated user to /login with returnUrl', () => {
      vi.spyOn(authServiceMock, 'isLoggedIn').mockReturnValue(false);

      const result = TestBed.runInInjectionContext(() => studentGuard(mockRoute, mockState));

      expect(result).toEqual(router.createUrlTree(['/login'], { queryParams: { returnUrl: '/protected' } }));
    });

    it('allows access for Student role', () => {
      vi.spyOn(authServiceMock, 'isLoggedIn').mockReturnValue(true);
      vi.spyOn(authServiceMock, 'getUserRole').mockReturnValue('Student');

      const result = TestBed.runInInjectionContext(() => studentGuard(mockRoute, mockState));

      expect(result).toBe(true);
    });

    it('denies access (redirects to /) for non-student role', () => {
      vi.spyOn(authServiceMock, 'isLoggedIn').mockReturnValue(true);
      vi.spyOn(authServiceMock, 'getUserRole').mockReturnValue('Instructor');

      const result = TestBed.runInInjectionContext(() => studentGuard(mockRoute, mockState));

      expect(result).toEqual(router.createUrlTree(['/']));
    });
  });

  describe('instructorGuard', () => {
    it('allows access for Instructor role', () => {
      vi.spyOn(authServiceMock, 'isLoggedIn').mockReturnValue(true);
      vi.spyOn(authServiceMock, 'getUserRole').mockReturnValue('Instructor');

      const result = TestBed.runInInjectionContext(() => instructorGuard(mockRoute, mockState));

      expect(result).toBe(true);
    });
  });

  describe('adminGuard', () => {
    it('allows access only for Admin role', () => {
      vi.spyOn(authServiceMock, 'isLoggedIn').mockReturnValue(true);
      vi.spyOn(authServiceMock, 'getUserRole').mockReturnValue('Admin');

      const result = TestBed.runInInjectionContext(() => adminGuard(mockRoute, mockState));

      expect(result).toBe(true);
    });
  });
});
