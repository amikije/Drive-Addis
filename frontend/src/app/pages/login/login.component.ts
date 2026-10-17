import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  loginForm!: FormGroup;
  errorMessage = '';
  loading = false;
  returnUrl = '';
  showPassword = false;

  // Rate Limiting & Lockout Properties
  failedAttempts: number = 0;
  remainingAttemptsDisplay: number | null = null;
  isLocked: boolean = false;
  lockoutTimerDisplay: string = '15:00';
  private timerInterval: any = null;

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '';

    // Initialize Reactive Form (PDF Module 8)
    this.loginForm = this.fb.group({
      phoneNumber: ['', [Validators.required, Validators.pattern(/^(\+251|0)9[0-9]{8}$/)]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    if (this.authService.isLoggedIn()) {
      this.redirectByRole(this.authService.getUserRole());
      return;
    }

    // Check for active lockout in localStorage
    this.checkExistingLockout();
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  private checkExistingLockout(): void {
    const savedUntil = localStorage.getItem('driveaddis_lockout_until');
    if (savedUntil) {
      const targetTime = parseInt(savedUntil, 10);
      const remainingSecs = Math.ceil((targetTime - Date.now()) / 1000);

      if (remainingSecs > 0) {
        this.startLockoutCountdown(remainingSecs);
        return;
      } else {
        localStorage.removeItem('driveaddis_lockout_until');
        localStorage.removeItem('driveaddis_failed_attempts');
      }
    }

    // Restore failed attempts count if not locked
    const savedAttempts = localStorage.getItem('driveaddis_failed_attempts');
    if (savedAttempts) {
      this.failedAttempts = parseInt(savedAttempts, 10);
      if (this.failedAttempts >= 2 && this.failedAttempts < 5) {
        this.remainingAttemptsDisplay = 5 - this.failedAttempts;
      }
    }
  }

  startLockoutCountdown(seconds: number): void {
    this.isLocked = true;
    this.remainingAttemptsDisplay = null;
    const targetTimestamp = Date.now() + seconds * 1000;
    localStorage.setItem('driveaddis_lockout_until', targetTimestamp.toString());

    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }

    const updateDisplay = () => {
      const now = Date.now();
      const diffSecs = Math.max(0, Math.ceil((targetTimestamp - now) / 1000));

      const mins = Math.floor(diffSecs / 60);
      const secs = diffSecs % 60;
      this.lockoutTimerDisplay = `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;

      if (diffSecs <= 0) {
        clearInterval(this.timerInterval);
        this.timerInterval = null;
        this.isLocked = false;
        this.failedAttempts = 0;
        this.remainingAttemptsDisplay = null;
        this.errorMessage = '';
        localStorage.removeItem('driveaddis_lockout_until');
        localStorage.removeItem('driveaddis_failed_attempts');
        this.notificationService.success(
          'Account Unblocked',
          'The 15-minute security lockout has ended. You may now attempt to sign in.'
        );
      }
    };

    updateDisplay();
    this.timerInterval = setInterval(updateDisplay, 1000);
  }

  fillAdminDemo(): void {
    if (this.isLocked) return;
    this.loginForm.patchValue({
      phoneNumber: '+251900000000',
      password: 'AdminPass123!'
    });
  }

  fillInstructorDemo(): void {
    if (this.isLocked) return;
    this.loginForm.patchValue({
      phoneNumber: '+251911000001',
      password: 'Password123!'
    });
  }

  fillStudentDemo(): void {
    if (this.isLocked) return;
    this.loginForm.patchValue({
      phoneNumber: '+251922000001',
      password: 'Password123!'
    });
  }

  onSubmit(): void {
    if (this.isLocked) {
      this.notificationService.warning(
        'Account Blocked',
        `Sign in is temporarily blocked. Please wait ${this.lockoutTimerDisplay} before trying again.`
      );
      return;
    }

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.errorMessage = 'Please provide valid credentials.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    // Using getRawValue() (PDF Module 8 Pattern)
    const formValues = this.loginForm.getRawValue();

    this.authService.login({
      phoneNumber: formValues.phoneNumber.trim(),
      password: formValues.password
    }).subscribe({
      next: (res) => {
        this.loading = false;
        this.failedAttempts = 0;
        this.remainingAttemptsDisplay = null;
        localStorage.removeItem('driveaddis_failed_attempts');
        localStorage.removeItem('driveaddis_lockout_until');

        this.notificationService.success('Welcome Back', `Signed in successfully as ${res.role}!`);
        if (this.returnUrl) {
          this.router.navigateByUrl(this.returnUrl);
        } else {
          this.redirectByRole(res.role);
        }
      },
      error: (err) => {
        this.loading = false;
        const rawError: string = err?.error?.error || 'Invalid phone number or password. Please try again.';
        this.errorMessage = rawError;

        // Check if backend reports locked
        const isBackendLocked = rawError.toLowerCase().includes('temporarily locked') ||
                                rawError.toLowerCase().includes('locked');

        if (isBackendLocked) {
          // Parse remaining minutes if present in error message
          const matchMinutes = rawError.match(/(\d+)\s+minute/i);
          const minutes = matchMinutes ? parseInt(matchMinutes[1], 10) : 15;
          this.startLockoutCountdown(minutes * 60);
          this.notificationService.error('Account Locked', rawError);
          return;
        }

        // Increment failed attempt counter
        this.failedAttempts++;
        localStorage.setItem('driveaddis_failed_attempts', this.failedAttempts.toString());

        // Lock if 5 failed attempts reached
        if (this.failedAttempts >= 5) {
          this.startLockoutCountdown(15 * 60);
          this.notificationService.error(
            'Account Locked (5 Attempts)',
            'Your account has been temporarily locked for 15 minutes due to 5 consecutive failed login attempts.'
          );
          return;
        }

        // Requirement: User must see remaining try count after trying 2 times
        if (this.failedAttempts >= 2) {
          this.remainingAttemptsDisplay = 5 - this.failedAttempts;
          this.errorMessage = `Invalid phone number or password. You have ${this.remainingAttemptsDisplay} attempt(s) remaining.`;
        } else {
          // First attempt: do not display remaining attempts count
          this.remainingAttemptsDisplay = null;
          this.errorMessage = 'Invalid phone number or password.';
        }

        this.notificationService.error('Sign In Failed', this.errorMessage);
      }
    });
  }

  private redirectByRole(role: string | null): void {
    const r = (role || '').toLowerCase();
    if (r === 'admin') {
      this.router.navigate(['/admin']);
    } else if (r === 'instructor') {
      this.router.navigate(['/instructor']);
    } else if (r === 'student') {
      this.router.navigate(['/student']);
    } else {
      this.router.navigate(['/']);
    }
  }
}
