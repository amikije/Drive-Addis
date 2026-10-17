import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BookingDto {
  id: number;
  studentId: number;
  studentName: string;
  instructorId: number;
  instructorName: string;
  scheduledAt: string;
  status: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled' | string;
  studentPhoneNumber?: string | null;
}

export interface CreateBookingRequest {
  instructorId: number;
  scheduledAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class BookingsService {
  private apiUrl = '/api/bookings';

  constructor(private http: HttpClient) { }

  createBooking(data: CreateBookingRequest): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }

  getBookings(studentId?: number, instructorId?: number): Observable<BookingDto[]> {
    let params = new HttpParams();
    if (studentId) {
      params = params.set('studentId', studentId.toString());
    }
    if (instructorId) {
      params = params.set('instructorId', instructorId.toString());
    }
    return this.http.get<BookingDto[]>(this.apiUrl, { params });
  }

  updateBookingStatus(id: number, status: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled' | string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/status`, { status });
  }
}
