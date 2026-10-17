import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface InstructorDto {
  id: number;
  fullName: string;
  hourlyPrice: number;
  averageRating: number;
  distanceKm: number;
  vehicleTypes: string[];
}

export interface PagedInstructorsResult {
  items: InstructorDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class InstructorsService {
  private apiUrl = '/api/instructors';

  constructor(private http: HttpClient) { }

  searchInstructors(filters: {
    lat?: number | null;
    lng?: number | null;
    maxPrice?: number | null;
    minRating?: number | null;
    vehicleType?: string | null;
    name?: string | null;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<PagedInstructorsResult> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, value.toString());
      }
    });
    return this.http.get<PagedInstructorsResult>(`${this.apiUrl}/search`, { params });
  }

  verifyInstructor(id: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/verify`, {});
  }

  rejectInstructor(id: number, reason: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/reject`, { reason });
  }
}
