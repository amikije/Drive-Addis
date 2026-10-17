import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { AuthService } from './auth.service';

export interface InAppNotification {
  id: number;
  instructorId?: number;
  userId?: number;
  title: string;
  message: string;
  type: 'ApplicationApproved' | 'NewBooking' | string;
  relatedEntityId?: number;
  isRead: boolean;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class InAppNotificationService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = '/api/notifications';

  public notifications = signal<InAppNotification[]>([]);
  public isOpen = signal<boolean>(false);

  public unreadCount = computed(() =>
    this.notifications().filter(n => !n.isRead).length
  );

  public loadNotifications(instructorId?: number): void {
    if (!this.authService.isLoggedIn()) return;

    let params = new HttpParams();
    const instId = instructorId ?? this.authService.getUser()?.instructorId;
    if (instId) {
      params = params.set('instructorId', instId.toString());
    }

    this.http.get<InAppNotification[]>(this.apiUrl, { params }).subscribe({
      next: (list) => {
        this.notifications.set(list || []);
      },
      error: (err) => {
        console.error('Failed to load in-app notifications:', err);
      }
    });
  }

  public markAsRead(id: number): void {
    // Optimistic update
    this.notifications.update(list =>
      list.map(n => n.id === id ? { ...n, isRead: true } : n)
    );

    this.http.patch(`${this.apiUrl}/${id}/read`, {}).subscribe({
      error: (err) => {
        console.error('Failed to mark notification as read:', err);
      }
    });
  }

  public markAllAsRead(instructorId?: number): void {
    const instId = instructorId ?? this.authService.getUser()?.instructorId;

    // Optimistic update
    this.notifications.update(list =>
      list.map(n => ({ ...n, isRead: true }))
    );

    let params = new HttpParams();
    if (instId) {
      params = params.set('instructorId', instId.toString());
    }

    this.http.patch(`${this.apiUrl}/mark-all-read`, {}, { params }).subscribe({
      error: (err) => {
        console.error('Failed to mark all notifications as read:', err);
      }
    });
  }

  public toggleDropdown(): void {
    this.isOpen.update(v => !v);
  }

  public closeDropdown(): void {
    this.isOpen.set(false);
  }
}
