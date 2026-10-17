import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

export interface LoginResponse {
  role: string;
  userId: number;
  studentId?: number | null;
  instructorId?: number | null;
  fullName?: string | null;
  phoneNumber?: string | null;
}

export interface UserInfo {
  userId: number;
  role: 'Student' | 'Instructor' | 'Admin' | string;
  phoneNumber?: string;
  studentId?: number;
  instructorId?: number;
  fullName?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/auth';
  private currentUserSubject = new BehaviorSubject<UserInfo | null>(this.loadUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  register(data: any): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/register`, data).pipe(
      tap(response => {
        if (response && response.userId) {
          this.saveAuth(response);
        }
      })
    );
  }

  login(data: any): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, data).pipe(
      tap(response => {
        if (response && response.userId) {
          this.saveAuth(response);
        }
      })
    );
  }

  private saveAuth(auth: LoginResponse): void {
    localStorage.setItem('auth_role', auth.role || '');
    localStorage.setItem('auth_userId', auth.userId ? auth.userId.toString() : '');

    if (auth.studentId != null) {
      localStorage.setItem('auth_studentId', auth.studentId.toString());
    } else {
      localStorage.removeItem('auth_studentId');
    }

    if (auth.instructorId != null) {
      localStorage.setItem('auth_instructorId', auth.instructorId.toString());
    } else {
      localStorage.removeItem('auth_instructorId');
    }

    if (auth.fullName) {
      localStorage.setItem('auth_fullName', auth.fullName);
    } else {
      localStorage.removeItem('auth_fullName');
    }

    if (auth.phoneNumber) {
      localStorage.setItem('auth_phoneNumber', auth.phoneNumber);
    } else {
      localStorage.removeItem('auth_phoneNumber');
    }

    const userInfo: UserInfo = {
      userId: auth.userId,
      role: auth.role,
      studentId: auth.studentId ?? undefined,
      instructorId: auth.instructorId ?? undefined,
      fullName: auth.fullName ?? undefined,
      phoneNumber: auth.phoneNumber ?? undefined
    };
    this.currentUserSubject.next(userInfo);
  }

  private loadUserFromStorage(): UserInfo | null {
    const role = localStorage.getItem('auth_role');
    const userIdStr = localStorage.getItem('auth_userId');
    if (!role || !userIdStr) return null;

    const studentIdStr = localStorage.getItem('auth_studentId');
    const instructorIdStr = localStorage.getItem('auth_instructorId');

    return {
      userId: parseInt(userIdStr, 10),
      role: role,
      studentId: studentIdStr ? parseInt(studentIdStr, 10) : undefined,
      instructorId: instructorIdStr ? parseInt(instructorIdStr, 10) : undefined,
      fullName: localStorage.getItem('auth_fullName') || undefined,
      phoneNumber: localStorage.getItem('auth_phoneNumber') || undefined
    };
  }

  logout(): void {
    localStorage.removeItem('auth_role');
    localStorage.removeItem('auth_userId');
    localStorage.removeItem('auth_studentId');
    localStorage.removeItem('auth_instructorId');
    localStorage.removeItem('auth_fullName');
    localStorage.removeItem('auth_phoneNumber');
    this.currentUserSubject.next(null);
  }

  getUser(): UserInfo | null {
    return this.currentUserSubject.value;
  }

  getUserRole(): string | null {
    const user = this.getUser();
    return user ? user.role : localStorage.getItem('auth_role');
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('auth_userId');
  }

  isStudent(): boolean {
    return this.getUserRole()?.toLowerCase() === 'student';
  }

  isInstructor(): boolean {
    return this.getUserRole()?.toLowerCase() === 'instructor';
  }

  isAdmin(): boolean {
    return this.getUserRole()?.toLowerCase() === 'admin';
  }

  forgotPassword(phoneNumber: string): Observable<{ message: string; devCode?: string; telegramUrl?: string; telegramBotUsername?: string }> {
    return this.http.post<{ message: string; devCode?: string; telegramUrl?: string; telegramBotUsername?: string }>(`${this.apiUrl}/forgot-password`, { phoneNumber });
  }

  verifyResetOtp(phoneNumber: string, code: string): Observable<{ message: string; resetToken: string }> {
    return this.http.post<{ message: string; resetToken: string }>(`${this.apiUrl}/verify-reset-otp`, { phoneNumber, code });
  }

  resetPassword(resetToken: string, newPassword: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/reset-password`, { resetToken, newPassword });
  }
}
