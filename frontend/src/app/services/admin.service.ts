import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BookingDto } from './bookings.service';

export interface AdminDashboardDto {
  totalStudents: number;
  totalInstructors: number;
  verifiedInstructors: number;
  unverifiedInstructors: number;
  totalBookings: number;
  pendingBookings: number;
  confirmedBookings: number;
  completedBookings: number;
  cancelledBookings: number;
  totalReviews: number;
  platformAverageRating: number;
}

export interface InstructorAdminListItemDto {
  id: number;
  fullName: string;
  phoneNumber: string;
  hourlyPrice: number;
  verificationStatus: string;
  rejectionReason?: string;
  licensePhotoUrl?: string;
  createdAt: string;
}

export interface StudentAdminListItemDto {
  id: number;
  fullName: string;
  phoneNumber: string;
  licenseNumber?: string | null;
  createdAt: string;
  totalBookings: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = '/api/admin';

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<AdminDashboardDto> {
    return this.http.get<AdminDashboardDto>(`${this.apiUrl}/dashboard`);
  }

  getInstructors(unverifiedOnly?: boolean, search?: string): Observable<InstructorAdminListItemDto[]> {
    let params = new HttpParams();
    if (unverifiedOnly !== undefined && unverifiedOnly !== null) {
      params = params.set('unverifiedOnly', unverifiedOnly.toString());
    }
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<InstructorAdminListItemDto[]>(`${this.apiUrl}/instructors`, { params });
  }

  getAllBookings(): Observable<BookingDto[]> {
    return this.http.get<BookingDto[]>(`${this.apiUrl}/bookings`);
  }

  getStudents(search?: string): Observable<StudentAdminListItemDto[]> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<StudentAdminListItemDto[]>(`${this.apiUrl}/students`, { params });
  }
}
