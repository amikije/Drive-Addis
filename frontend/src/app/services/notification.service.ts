import { Injectable, signal } from '@angular/core';

export interface ToastNotification {
  id: string;
  type: 'success' | 'info' | 'warning' | 'error';
  title: string;
  message: string;
  durationMs?: number;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  // Signal containing active notifications list
  public notifications = signal<ToastNotification[]>([]);

  public show(
    type: 'success' | 'info' | 'warning' | 'error',
    title: string,
    message: string,
    durationMs: number = 4000
  ): string {
    const id = Math.random().toString(36).substring(2, 9);
    const toast: ToastNotification = {
      id,
      type,
      title,
      message,
      durationMs,
      timestamp: new Date()
    };

    this.notifications.update(list => [toast, ...list]);

    if (durationMs > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, durationMs);
    }

    return id;
  }

  public success(title: string, message: string, durationMs?: number): string {
    return this.show('success', title, message, durationMs);
  }

  public error(title: string, message: string, durationMs: number = 6000): string {
    return this.show('error', title, message, durationMs);
  }

  public info(title: string, message: string, durationMs?: number): string {
    return this.show('info', title, message, durationMs);
  }

  public warning(title: string, message: string, durationMs?: number): string {
    return this.show('warning', title, message, durationMs);
  }

  public dismiss(id: string): void {
    this.notifications.update(list => list.filter(n => n.id !== id));
  }

  public clearAll(): void {
    this.notifications.set([]);
  }
}
