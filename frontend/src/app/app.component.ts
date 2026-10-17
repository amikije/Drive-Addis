import { Component, OnInit, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService, UserInfo } from './services/auth.service';
import { ThemeService } from './services/theme.service';
import { NotificationService } from './services/notification.service';
import { InAppNotificationService } from './services/in-app-notification.service';
import { AppStateStore } from './services/app-state.store';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'DriveAddis';
  currentUser: UserInfo | null = null;
  mobileMenuOpen = false;

  public themeService = inject(ThemeService);
  public notificationService = inject(NotificationService);
  public inAppNotificationService = inject(InAppNotificationService);
  public stateStore = inject(AppStateStore);
  public authService = inject(AuthService);
  private router = inject(Router);

  @HostListener('document:click')
  onDocumentClick(): void {
    this.inAppNotificationService.closeDropdown();
  }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      if (user) {
        this.inAppNotificationService.loadNotifications(user.instructorId);
      }
    });
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
  }

  getDashboardLink(): string {
    if (!this.currentUser) return '/';
    const role = this.currentUser.role.toLowerCase();
    if (role === 'admin') return '/admin';
    if (role === 'instructor') return '/instructor';
    return '/student';
  }

  isAdminRoute(): boolean {
    return this.router.url.startsWith('/admin');
  }

  logout(): void {
    this.authService.logout();
    this.closeMobileMenu();
    this.notificationService.info('Signed Out', 'You have been logged out successfully.');
    this.router.navigate(['/login']);
  }
}
