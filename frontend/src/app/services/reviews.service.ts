import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateReviewRequest {
  bookingId: number;
  rating: number;
  comment?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReviewsService {
  private apiUrl = '/api/reviews';

  constructor(private http: HttpClient) {}

  createReview(data: CreateReviewRequest): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }
}
