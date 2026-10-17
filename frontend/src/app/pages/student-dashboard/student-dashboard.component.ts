import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { BookingsService, BookingDto } from '../../services/bookings.service';
import { ReviewsService } from '../../services/reviews.service';
import { AuthService, UserInfo } from '../../services/auth.service';
import { InstructorsService, InstructorDto } from '../../services/instructors.service';
import { AppStateStore } from '../../services/app-state.store';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.css'
})
export class StudentDashboardComponent implements OnInit {
  public stateStore = inject(AppStateStore);
  public authService = inject(AuthService);
  public notificationService = inject(NotificationService);
  private bookingsService = inject(BookingsService);
  private reviewsService = inject(ReviewsService);
  private instructorsService = inject(InstructorsService);
  private router = inject(Router);

  currentUser: UserInfo | null = null;
  statusFilter = signal<string>('ALL');

  // Search & direct booking
  searchQuery = '';
  showBookingModal = false;
  selectedInstructor: InstructorDto | null = null;
  bookingDate = '';
  bookingTime = '09:00';
  isBooking = false;
  bookingSuccessMsg = '';
  bookingModalError = '';

  // Review Modal
  showReviewModal = false;
  selectedBookingForReview: BookingDto | null = null;
  reviewRating = 5;
  reviewComment = '';
  isSubmittingReview = false;
  reviewError = '';
  reviewSuccess = false;

  // Filtered Bookings computed signal (Module 8 & 9)
  filteredBookings = computed(() => {
    const list = this.stateStore.bookings();
    const filter = this.statusFilter();
    if (filter === 'ALL') return list;
    return list.filter(b => b.status.toUpperCase() === filter.toUpperCase());
  });

  completedCount = computed(() => this.stateStore.bookings().filter(b => b.status === 'Completed').length);
  confirmedCount = computed(() => this.stateStore.bookings().filter(b => b.status === 'Confirmed').length);
  pendingCount   = computed(() => this.stateStore.bookings().filter(b => b.status === 'Pending').length);

  ngOnInit(): void {
    this.currentUser = this.authService.getUser();
    
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.bookingDate = tomorrow.toISOString().split('T')[0];

    this.refreshData();
  }

  refreshData(): void {
    const studentId = this.currentUser?.studentId;
    this.stateStore.loadBookings(studentId, undefined);
    this.stateStore.loadInstructors({ pageSize: 6 });
  }

  setStatusFilter(filter: string): void {
    this.statusFilter.set(filter);
  }

  cancelBookingOptimistic(booking: BookingDto): void {
    if (!confirm(`Cancel your scheduled lesson with ${booking.instructorName}?`)) {
      return;
    }

    // Module 9 & 10 Optimistic UI Pattern
    this.stateStore.updateBookingStatusOptimistic(booking.id, 'Cancelled').subscribe();
  }

  // Booking Modal
  openBookingModal(instructor: InstructorDto): void {
    this.selectedInstructor = instructor;
    this.bookingSuccessMsg = '';
    this.bookingModalError = '';
    this.showBookingModal = true;
  }

  closeBookingModal(): void {
    this.showBookingModal = false;
    this.selectedInstructor = null;
    this.bookingSuccessMsg = '';
  }

  submitBooking(): void {
    if (!this.selectedInstructor || !this.bookingDate || !this.bookingTime) {
      this.bookingModalError = 'Please select a valid date and time.';
      return;
    }

    this.isBooking = true;
    this.bookingModalError = '';

    const scheduledDateTime = new Date(`${this.bookingDate}T${this.bookingTime}:00`);

    this.stateStore.createBookingOptimistic(
      { instructorId: this.selectedInstructor.id, scheduledAt: scheduledDateTime.toISOString() },
      this.selectedInstructor,
      this.currentUser
    ).subscribe({
      next: () => {
        this.isBooking = false;
        this.bookingSuccessMsg = 'Lesson request booked successfully!';
        setTimeout(() => this.closeBookingModal(), 1200);
      },
      error: (err) => {
        this.isBooking = false;
        this.bookingModalError = err?.error?.error || 'Failed to book lesson.';
      }
    });
  }

  // Review Modal
  openReviewModal(booking: BookingDto): void {
    this.selectedBookingForReview = booking;
    this.reviewRating = 5;
    this.reviewComment = '';
    this.reviewError = '';
    this.reviewSuccess = false;
    this.showReviewModal = true;
  }

  closeReviewModal(): void {
    this.showReviewModal = false;
    this.selectedBookingForReview = null;
  }

  submitReview(): void {
    if (!this.selectedBookingForReview) return;

    this.isSubmittingReview = true;
    this.reviewError = '';

    this.reviewsService.createReview({
      bookingId: this.selectedBookingForReview.id,
      rating: this.reviewRating,
      comment: this.reviewComment.trim() || undefined
    }).subscribe({
      next: () => {
        this.isSubmittingReview = false;
        this.reviewSuccess = true;
        this.notificationService.success('Review Published', 'Thank you for rating your instructor!');
        setTimeout(() => {
          this.closeReviewModal();
          this.refreshData();
        }, 1200);
      },
      error: (err) => {
        this.isSubmittingReview = false;
        this.reviewError = err?.error?.error || 'Failed to submit review. You may have already reviewed this booking.';
      }
    });
  }
}
