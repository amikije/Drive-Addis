import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AppStateStore } from './app-state.store';
import { BookingsService, BookingDto } from './bookings.service';
import { NotificationService } from './notification.service';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

describe('AppStateStore', () => {
  let store: AppStateStore;
  let bookingsService: BookingsService;
  let notificationService: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AppStateStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: NotificationService,
          useValue: {
            info: vi.fn(),
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
          }
        }
      ]
    });

    store = TestBed.inject(AppStateStore);
    bookingsService = TestBed.inject(BookingsService);
    notificationService = TestBed.inject(NotificationService);
  });

  it('computed signals should calculate counts correctly', () => {
    // Arrange: seed bookings signal directly
    const seededBookings: BookingDto[] = [
      { id: 1, studentId: 1, studentName: 'Abeba', instructorId: 1, instructorName: 'Abebe', scheduledAt: '', status: 'Pending' },
      { id: 2, studentId: 1, studentName: 'Abeba', instructorId: 2, instructorName: 'Marta', scheduledAt: '', status: 'Confirmed' },
      { id: 3, studentId: 1, studentName: 'Abeba', instructorId: 3, instructorName: 'Yonas', scheduledAt: '', status: 'Pending' },
      { id: 4, studentId: 1, studentName: 'Abeba', instructorId: 1, instructorName: 'Abebe', scheduledAt: '', status: 'Completed' },
      { id: 5, studentId: 1, studentName: 'Abeba', instructorId: 2, instructorName: 'Marta', scheduledAt: '', status: 'Cancelled' }
    ];

    // Act
    store.bookings.set(seededBookings);

    // Assert: pure computed signals update synchronously without tick
    expect(store.pendingBookingsCount()).toBe(2);
    expect(store.confirmedBookingsCount()).toBe(1);
    expect(store.completedBookingsCount()).toBe(1);
    expect(store.cancelledBookingsCount()).toBe(1);
  });

  it('filteredInstructors should apply multi-facet filters synchronously', () => {
    // Arrange: seed instructors
    store.instructors.set([
      { id: 1, fullName: 'Abebe Kebede', hourlyPrice: 300, averageRating: 4.5, distanceKm: 2, vehicleTypes: ['Manual'] },
      { id: 2, fullName: 'Marta Alemu', hourlyPrice: 600, averageRating: 4.8, distanceKm: 4, vehicleTypes: ['Automatic'] },
      { id: 3, fullName: 'Yonas Tadesse', hourlyPrice: 250, averageRating: 4.2, distanceKm: 6, vehicleTypes: ['Manual'] }
    ]);

    expect(store.totalInstructorsCount()).toBe(3);

    // Filter by transmission
    store.setTransmission('Manual');
    expect(store.filteredInstructors()).toHaveLength(2);

    // Filter by max price
    store.setMaxRate(280);
    expect(store.filteredInstructors()).toHaveLength(1);
    expect(store.filteredInstructors()[0].fullName).toBe('Yonas Tadesse');

    // Reset filters
    store.resetFilters();
    expect(store.filteredInstructors()).toHaveLength(3);
  });

  it('updateBookingStatusOptimistic should update state immediately and commit on success', async () => {
    // Arrange
    store.bookings.set([
      { id: 10, studentId: 1, studentName: 'Selam', instructorId: 2, instructorName: 'Marta', scheduledAt: '', status: 'Pending' }
    ]);

    vi.spyOn(bookingsService, 'updateBookingStatus').mockReturnValue(
      of({ id: 10, status: 'Confirmed', studentPhoneNumber: '+251911000002' })
    );

    // Act
    const observable$ = store.updateBookingStatusOptimistic(10, 'Confirmed');

    // Assert optimistic change before subscription completes
    expect(store.bookings().find(b => b.id === 10)?.status).toBe('Confirmed');

    await new Promise<void>((resolve) => {
      observable$.subscribe({
        next: () => {
          expect(store.bookings().find(b => b.id === 10)?.status).toBe('Confirmed');
          expect(notificationService.success).toHaveBeenCalled();
          resolve();
        }
      });
    });
  });

  it('updateBookingStatusOptimistic should rollback to previous state on server error', async () => {
    // Arrange
    const initialBooking: BookingDto = {
      id: 20,
      studentId: 1,
      studentName: 'Dawit',
      instructorId: 1,
      instructorName: 'Abebe',
      scheduledAt: '',
      status: 'Pending'
    };
    store.bookings.set([initialBooking]);

    vi.spyOn(bookingsService, 'updateBookingStatus').mockReturnValue(
      throwError(() => ({ status: 500, error: 'Internal Server Error' }))
    );

    // Act
    const observable$ = store.updateBookingStatusOptimistic(20, 'Cancelled');

    // Immediate optimistic update
    expect(store.bookings().find(b => b.id === 20)?.status).toBe('Cancelled');

    // Wait for failure and rollback
    await new Promise<void>((resolve) => {
      observable$.subscribe({
        error: () => {
          // Assert state reverted to Pending
          expect(store.bookings().find(b => b.id === 20)?.status).toBe('Pending');
          expect(notificationService.error).toHaveBeenCalledWith(
            'Rollback Triggered',
            expect.stringContaining('Reverted to previous state')
          );
          resolve();
        }
      });
    });
  });
});
