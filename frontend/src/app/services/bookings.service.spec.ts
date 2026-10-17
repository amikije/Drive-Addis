import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { BookingsService, BookingDto } from './bookings.service';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { firstValueFrom } from 'rxjs';

describe('BookingsService', () => {
  let service: BookingsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        BookingsService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(BookingsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Ensures no stray or unhandled HTTP requests
  });

  it('createBooking should issue POST to /api/bookings and return created booking', async () => {
    // Arrange
    const payload = { instructorId: 2, scheduledAt: '2026-10-20T09:00:00Z' };
    const mockResponse: BookingDto = {
      id: 55,
      studentId: 1,
      studentName: 'Abeba Bikila',
      instructorId: 2,
      instructorName: 'Kassahun Desta',
      scheduledAt: '2026-10-20T09:00:00Z',
      status: 'Pending'
    };

    // Act
    const promise = firstValueFrom(service.createBooking(payload));

    // Assert request
    const req = httpMock.expectOne('/api/bookings');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);

    req.flush(mockResponse);
    const result = await promise;

    expect(result).toEqual(mockResponse);
  });

  it('getBookings should issue GET with query parameters', async () => {
    // Arrange
    const mockBookings: BookingDto[] = [
      {
        id: 1,
        studentId: 5,
        studentName: 'Student 5',
        instructorId: 2,
        instructorName: 'Instructor 2',
        scheduledAt: '2026-10-21T07:00:00Z',
        status: 'Confirmed'
      }
    ];

    // Act
    const promise = firstValueFrom(service.getBookings(5, 2));

    // Assert
    const req = httpMock.expectOne(r => 
      r.url === '/api/bookings' &&
      r.params.get('studentId') === '5' &&
      r.params.get('instructorId') === '2'
    );
    expect(req.request.method).toBe('GET');

    req.flush(mockBookings);
    const result = await promise;

    expect(result).toHaveLength(1);
    expect(result[0].status).toBe('Confirmed');
  });

  it('updateBookingStatus should issue PATCH to /api/bookings/{id}/status', async () => {
    // Arrange
    const bookingId = 42;
    const newStatus = 'Confirmed';
    const mockResponse = { id: 42, status: 'Confirmed', studentPhoneNumber: '+251911223344' };

    // Act
    const promise = firstValueFrom(service.updateBookingStatus(bookingId, newStatus));

    // Assert
    const req = httpMock.expectOne(`/api/bookings/${bookingId}/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: newStatus });

    req.flush(mockResponse);
    const result = await promise;

    expect(result.status).toBe('Confirmed');
    expect(result.studentPhoneNumber).toBe('+251911223344');
  });

  it('surfaces backend 400 error to caller on validation failure', async () => {
    // Act
    const promise = firstValueFrom(service.createBooking({ instructorId: 1, scheduledAt: '' }));

    // Assert
    const req = httpMock.expectOne('/api/bookings');
    req.flush({ error: 'Booking must be at least 2 hours in the future.' }, { status: 400, statusText: 'Bad Request' });

    await expect(promise).rejects.toBeTruthy();
  });
});
