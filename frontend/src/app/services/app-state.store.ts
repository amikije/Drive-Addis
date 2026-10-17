import { Injectable, signal, computed, inject } from '@angular/core';
import { InstructorsService, InstructorDto } from './instructors.service';
import { BookingsService, BookingDto, CreateBookingRequest } from './bookings.service';
import { AuthService, UserInfo } from './auth.service';
import { NotificationService } from './notification.service';
import { Observable, tap, catchError, throwError } from 'rxjs';

export interface SearchInstructorFilters {
  lat?: number | null;
  lng?: number | null;
  maxPrice?: number | null;
  minRating?: number | null;
  vehicleType?: string | null;
  name?: string | null;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class AppStateStore {
  private instructorsService = inject(InstructorsService);
  private bookingsService = inject(BookingsService);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);

  // --- Centralized Signal State (Single Source of Truth) ---
  public instructors = signal<InstructorDto[]>([]);
  public bookings = signal<BookingDto[]>([]);
  public loadingInstructors = signal<boolean>(false);
  public loadingBookings = signal<boolean>(false);
  public searchKeyword = signal<string>('');
  public selectedLocation = signal<string>('');
  public selectedTransmission = signal<string>('');
  public maxHourlyRate = signal<number | null>(null);
  public minRatingFilter = signal<number | null>(null);
  public lastSyncTimestamp = signal<Date>(new Date());

  // Real-time live event stream signal (for Module 9 & 10 real-time sync simulation)
  public liveEvents = signal<{ id: string; time: string; text: string; type: 'info' | 'success' | 'warning' }[]>([
    { id: '1', time: 'Just now', text: 'Real-time SignalR bridge initialized', type: 'info' },
    { id: '2', time: '1 min ago', text: 'Instructor Abebe Kebede confirmed Bole route lesson', type: 'success' },
    { id: '3', time: '3 mins ago', text: 'Transport Authority updated verified instructor roster', type: 'info' }
  ]);

  // --- Computed Signals (Derive from state automatically without manual change detection) ---
  public totalInstructorsCount = computed(() => this.instructors().length);

  public filteredInstructors = computed(() => {
    let list = this.instructors();
    const keyword = this.searchKeyword().toLowerCase().trim();
    const transmission = this.selectedTransmission();
    const maxPrice = this.maxHourlyRate();
    const minRating = this.minRatingFilter();

    if (keyword) {
      list = list.filter(i => 
        i.fullName.toLowerCase().includes(keyword)
      );
    }

    if (transmission) {
      list = list.filter(i => 
        i.vehicleTypes && i.vehicleTypes.some(v => v.toLowerCase() === transmission.toLowerCase())
      );
    }

    if (maxPrice !== null && maxPrice > 0) {
      list = list.filter(i => i.hourlyPrice <= maxPrice);
    }

    if (minRating !== null && minRating > 0) {
      list = list.filter(i => i.averageRating >= minRating);
    }

    return list;
  });

  public pendingBookingsCount = computed(() => 
    this.bookings().filter(b => b.status === 'Pending').length
  );

  public confirmedBookingsCount = computed(() => 
    this.bookings().filter(b => b.status === 'Confirmed').length
  );

  public completedBookingsCount = computed(() => 
    this.bookings().filter(b => b.status === 'Completed').length
  );

  public cancelledBookingsCount = computed(() => 
    this.bookings().filter(b => b.status === 'Cancelled').length
  );

  // --- Methods with Optimistic Updates and Rollback (Module 9 & 10) ---

  public loadInstructors(params?: SearchInstructorFilters): void {
    this.loadingInstructors.set(true);
    this.instructorsService.searchInstructors(params || { pageSize: 24 }).subscribe({
      next: (res) => {
        this.instructors.set(res?.items || []);
        this.loadingInstructors.set(false);
        this.lastSyncTimestamp.set(new Date());
      },
      error: (err) => {
        this.loadingInstructors.set(false);
        this.notificationService.error('Network Error', 'Failed to load instructors from API.');
        console.error('Failed to load instructors:', err);
      }
    });
  }

  public loadBookings(studentId?: number, instructorId?: number): void {
    this.loadingBookings.set(true);
    this.bookingsService.getBookings(studentId, instructorId).subscribe({
      next: (list) => {
        this.bookings.set(list || []);
        this.loadingBookings.set(false);
        this.lastSyncTimestamp.set(new Date());
      },
      error: (err) => {
        this.loadingBookings.set(false);
        console.error('Failed to load bookings:', err);
      }
    });
  }

  /**
   * Optimistic update for booking status change with automatic snapshot rollback on failure
   */
  public updateBookingStatusOptimistic(
    bookingId: number,
    newStatus: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled'
  ): Observable<any> {
    // 1. Take snapshot of current state before mutation
    const previousBookings = [...this.bookings()];

    // 2. Apply optimistic update immediately to UI
    this.bookings.update(current => 
      current.map(b => b.id === bookingId ? { ...b, status: newStatus } : b)
    );

    this.notificationService.info(
      'Status Updated (Optimistic)',
      `Booking #${bookingId} marked as ${newStatus}`
    );

    // 3. Send request to server
    return this.bookingsService.updateBookingStatus(bookingId, newStatus).pipe(
      tap((res: any) => {
        if (res && res.studentPhoneNumber) {
          this.bookings.update(current => 
            current.map(b => b.id === bookingId ? { ...b, status: newStatus, studentPhoneNumber: res.studentPhoneNumber } : b)
          );
        }
        this.notificationService.success(
          'Server Confirmed',
          `Booking #${bookingId} successfully synchronized with backend.`
        );
        this.addLiveEvent(`Booking #${bookingId} marked ${newStatus}`, 'success');
      }),
      catchError((err) => {
        // 4. Rollback to snapshot on failure (Module 10 Pattern)
        this.bookings.set(previousBookings);
        this.notificationService.error(
          'Rollback Triggered',
          `Server rejected update for #${bookingId}. Reverted to previous state.`
        );
        return throwError(() => err);
      })
    );
  }

  /**
   * Optimistic booking creation
   */
  public createBookingOptimistic(dto: CreateBookingRequest, instructor: InstructorDto, currentUser: UserInfo | null): Observable<any> {
    const tempId = -Date.now();
    const optimisticBooking: BookingDto = {
      id: tempId,
      studentId: currentUser?.studentId || 0,
      studentName: currentUser?.fullName || 'Current Student',
      instructorId: instructor.id,
      instructorName: instructor.fullName,
      scheduledAt: dto.scheduledAt,
      status: 'Pending'
    };

    // Optimistic push
    const previousBookings = [...this.bookings()];
    this.bookings.update(list => [optimisticBooking, ...list]);

    return this.bookingsService.createBooking(dto).pipe(
      tap((created) => {
        // Replace temp booking with real server response
        this.bookings.update(list => 
          list.map(b => b.id === tempId ? (created || optimisticBooking) : b)
        );
        this.notificationService.success('Booking Confirmed', `Lesson booked with ${instructor.fullName}!`);
        this.addLiveEvent(`New lesson booked with ${instructor.fullName}`, 'success');
      }),
      catchError((err) => {
        // Rollback
        this.bookings.set(previousBookings);
        this.notificationService.error('Booking Failed', err?.error?.error || 'Could not schedule lesson.');
        return throwError(() => err);
      })
    );
  }

  public addLiveEvent(text: string, type: 'info' | 'success' | 'warning' = 'info'): void {
    const newEvent = {
      id: Math.random().toString(36).substring(2, 7),
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' }),
      text,
      type
    };
    this.liveEvents.update(events => [newEvent, ...events.slice(0, 9)]);
  }

  // Filter setters
  public setKeyword(k: string): void { this.searchKeyword.set(k); }
  public setLocation(loc: string): void { this.selectedLocation.set(loc); }
  public setTransmission(t: string): void { this.selectedTransmission.set(t); }
  public setMaxRate(rate: number | null): void { this.maxHourlyRate.set(rate); }
  public setMinRating(rating: number | null): void { this.minRatingFilter.set(rating); }
  
  public resetFilters(): void {
    this.searchKeyword.set('');
    this.selectedLocation.set('');
    this.selectedTransmission.set('');
    this.maxHourlyRate.set(null);
    this.minRatingFilter.set(null);
  }
}
