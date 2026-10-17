import { Component, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent implements OnDestroy {
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  step: 'phone' | 'otp' | 'new-password' | 'success' = 'phone';

  phoneNumber: string = '';
  otpCode: string = '';
  devCode: string | null = null;
  telegramUrl: string | null = null;
  telegramBotUsername: string | null = null;
  resetToken: string = '';
  newPassword: string = '';
  confirmPassword: string = '';

  showPassword = false;
  showConfirmPassword = false;
  loading = false;
  errorMessage: string | null = null;

  resendCooldown = 0;
  private timerRef: any;

  ngOnDestroy(): void {
    if (this.timerRef) {
      clearInterval(this.timerRef);
    }
  }

  get hasLength(): boolean {
    return this.newPassword.length >= 8;
  }

  get hasUpper(): boolean {
    return /[A-Z]/.test(this.newPassword);
  }

  get hasLower(): boolean {
    return /[a-z]/.test(this.newPassword);
  }

  get hasNumber(): boolean {
    return /[0-9]/.test(this.newPassword);
  }

  get hasSpecial(): boolean {
    return /[^A-Za-z0-9]/.test(this.newPassword);
  }

  get isPasswordValid(): boolean {
    return this.hasLength && this.hasUpper && this.hasLower && this.hasNumber && this.hasSpecial;
  }

  get passwordsMatch(): boolean {
    return this.newPassword.length > 0 && this.newPassword === this.confirmPassword;
  }

  startCooldown(seconds = 60): void {
    this.resendCooldown = seconds;
    if (this.timerRef) clearInterval(this.timerRef);
    this.timerRef = setInterval(() => {
      if (this.resendCooldown > 0) {
        this.resendCooldown--;
      } else {
        clearInterval(this.timerRef);
      }
    }, 1000);
  }

  fillDemoPhone(phone: string): void {
    this.phoneNumber = phone;
    this.errorMessage = null;
  }

  sendOtp(): void {
    const phone = this.phoneNumber.trim().replace(/\s+/g, '');
    if (!phone) {
      this.errorMessage = 'Please enter your phone number.';
      return;
    }

    if (!/^(\+251|0)[97]\d{8}$/.test(phone)) {
      this.errorMessage = 'Please enter a valid Ethiopian phone number (e.g. 0912345678 or +251912345678).';
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    this.authService.forgotPassword(phone).subscribe({
      next: (res) => {
        this.loading = false;
        this.devCode = res.devCode || null;
        this.telegramUrl = res.telegramUrl || null;
        this.telegramBotUsername = res.telegramBotUsername || null;
        this.step = 'otp';
        this.startCooldown(60);
        this.notificationService.success('Code Generated', 'Open Telegram to receive your code, or enter below.');
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err?.error?.error || 'Failed to send verification code. Please try again.';
      }
    });
  }

  resendOtp(): void {
    if (this.resendCooldown > 0 || this.loading) return;
    this.sendOtp();
  }

  fillDevCode(): void {
    if (this.devCode) {
      this.otpCode = this.devCode;
    }
  }

  verifyOtp(): void {
    const code = this.otpCode.trim();
    if (!code || code.length < 6) {
      this.errorMessage = 'Please enter the 6-digit verification code.';
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    this.authService.verifyResetOtp(this.phoneNumber.trim(), code).subscribe({
      next: (res) => {
        this.loading = false;
        this.resetToken = res.resetToken;
        this.step = 'new-password';
        this.notificationService.success('Code Verified', 'Please enter your new password.');
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err?.error?.error || 'Invalid or expired verification code.';
      }
    });
  }

  submitNewPassword(): void {
    this.errorMessage = null;

    if (!this.isPasswordValid) {
      this.errorMessage = 'Please meet all password requirements before continuing.';
      return;
    }

    if (!this.passwordsMatch) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.loading = true;

    this.authService.resetPassword(this.resetToken, this.newPassword).subscribe({
      next: (res) => {
        this.loading = false;
        this.step = 'success';
        this.notificationService.success('Password Updated', 'Your password has been reset successfully!');
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err?.error?.error || 'Failed to update password. Your session may have expired.';
      }
    });
  }

  backToPhone(): void {
    this.step = 'phone';
    this.errorMessage = null;
    this.otpCode = '';
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
