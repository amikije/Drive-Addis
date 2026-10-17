import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { BookingsService, BookingDto } from '../../services/bookings.service';
import { AuthService, UserInfo } from '../../services/auth.service';
import { AppStateStore } from '../../services/app-state.store';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-instructor-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './instructor-dashboard.component.html',
  styleUrl: './instructor-dashboard.component.css'
})
export class InstructorDashboardComponent implements OnInit {
  public stateStore = inject(AppStateStore);
  public authService = inject(AuthService);
  public notificationService = inject(NotificationService);
  private bookingsService = inject(BookingsService);
  private router = inject(Router);

  currentUser: UserInfo | null = null;
  statusFilter = signal<string>('ALL');
  hourlyRate = 350;

  // Filtered Bookings computed signal (Module 8 & 9)
  filteredBookings = computed(() => {
    const list = this.stateStore.bookings();
    const filter = this.statusFilter();
    if (filter === 'ALL') return list;
    return list.filter(b => b.status.toUpperCase() === filter.toUpperCase());
  });

  // Estimated Earnings computed signal (Module 8)
  estimatedEarnings = computed(() => 
    this.stateStore.completedBookingsCount() * this.hourlyRate
  );

  ngOnInit(): void {
    this.currentUser = this.authService.getUser();

    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    this.fetchBookings();
  }

  fetchBookings(): void {
    const instructorId = this.currentUser?.instructorId;
    this.stateStore.loadBookings(undefined, instructorId);
  }

  setStatusFilter(f: string): void {
    this.statusFilter.set(f);
  }

  updateStatusOptimistic(booking: BookingDto, newStatus: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled'): void {
    // Module 9 & 10: Optimistic UI update with automatic rollback
    this.stateStore.updateBookingStatusOptimistic(booking.id, newStatus).subscribe();
  }
}
