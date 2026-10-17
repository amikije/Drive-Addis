import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { InstructorsService, InstructorDto } from '../../services/instructors.service';
import { BookingsService } from '../../services/bookings.service';
import { AuthService } from '../../services/auth.service';
import { AppStateStore } from '../../services/app-state.store';
import { NotificationService } from '../../services/notification.service';

interface LocationPreset {
  name: string;
  lat: number;
  lng: number;
  landmark: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  public stateStore = inject(AppStateStore);
  public authService = inject(AuthService);
  public notificationService = inject(NotificationService);
  private instructorsService = inject(InstructorsService);
  private bookingsService = inject(BookingsService);
  private router = inject(Router);

  // Search & Filter State
  searchName = '';
  selectedVehicleType = '';
  selectedLocation = '';
  maxPrice: number | null = null;
  minRating: number | null = null;

  locationPresets: LocationPreset[] = [
    { name: 'All Addis Ababa', lat: 0, lng: 0, landmark: 'Anywhere in Addis' },
    { name: 'Bole / Medhanialem', lat: 9.0000, lng: 38.7800, landmark: 'Around Edna Mall & Airport' },
    { name: 'Kazanchis / ECA', lat: 9.0180, lng: 38.7650, landmark: 'Near UNECA & Intercontinental' },
    { name: 'Piassa / Arat Kilo', lat: 9.0350, lng: 38.7500, landmark: 'Churchill Ave & University' },
    { name: 'CMC / Ayat / Megenagna', lat: 9.0200, lng: 38.8200, landmark: 'Around Megenagna Square' },
    { name: 'Sarbet / Mexico / Kera', lat: 8.9950, lng: 38.7400, landmark: 'Near AU HQ & Mexico Square' },
    { name: 'Lebu / Jomo / Haile Garment', lat: 8.9500, lng: 38.7200, landmark: 'Lebu Commercial Center' }
  ];

  // Booking Modal State
  showBookingModal = false;
  selectedInstructor: InstructorDto | null = null;
  bookingDate = '';
  bookingTime = '';
  isBooking = false;
  bookingSuccess = false;
  bookingError = '';
  minDate = '';

  // Available time slots for driving lessons
  availableTimeSlots = [
    { value: '06:00', label: '6:00 AM — Early Morning' },
    { value: '07:30', label: '7:30 AM — Morning' },
    { value: '09:00', label: '9:00 AM — Mid Morning' },
    { value: '10:30', label: '10:30 AM — Late Morning' },
    { value: '13:00', label: '1:00 PM — After Lunch' },
    { value: '14:30', label: '2:30 PM — Afternoon' },
    { value: '16:00', label: '4:00 PM — Late Afternoon' },
    { value: '17:30', label: '5:30 PM — Evening' }
  ];

  ngOnInit(): void {
    // Set minimum date to tomorrow
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.minDate = tomorrow.toISOString().split('T')[0];
    this.bookingDate = this.minDate;
    this.bookingTime = '09:00';

    // Load initial instructors via SignalStore
    this.fetchInstructors();
  }

  fetchInstructors(): void {
    let lat: number | null = null;
    let lng: number | null = null;

    if (this.selectedLocation) {
      const loc = this.locationPresets.find(p => p.name === this.selectedLocation);
      if (loc && (loc.lat !== 0 || loc.lng !== 0)) {
        lat = loc.lat;
        lng = loc.lng;
      }
    }

    this.stateStore.setKeyword(this.searchName);
    this.stateStore.setLocation(this.selectedLocation);
    this.stateStore.setTransmission(this.selectedVehicleType);
    this.stateStore.setMaxRate(this.maxPrice ? Number(this.maxPrice) : null);
    this.stateStore.setMinRating(this.minRating ? Number(this.minRating) : null);

    this.stateStore.loadInstructors({
      name: this.searchName.trim() || null,
      vehicleType: this.selectedVehicleType || null,
      lat: lat,
      lng: lng,
      maxPrice: this.maxPrice ? Number(this.maxPrice) : null,
      minRating: this.minRating ? Number(this.minRating) : null,
      pageSize: 18
    });
  }

  quickFilterLocation(locName: string): void {
    this.selectedLocation = locName === 'All Addis Ababa' ? '' : locName;
    this.fetchInstructors();
  }

  quickFilterTransmission(trans: string): void {
    this.selectedVehicleType = this.selectedVehicleType === trans ? '' : trans;
    this.fetchInstructors();
  }

  applyFilters(): void {
    this.fetchInstructors();
  }

  resetFilters(): void {
    this.searchName = '';
    this.selectedVehicleType = '';
    this.selectedLocation = '';
    this.maxPrice = null;
    this.minRating = null;
    this.stateStore.resetFilters();
    this.fetchInstructors();
  }

  openBookingModal(instructor: InstructorDto): void {
    if (!this.authService.isLoggedIn()) {
      this.notificationService.info('Please Log In', 'Sign in as a student to book lessons.');
      this.router.navigate(['/login'], { queryParams: { returnUrl: '/' } });
      return;
    }

    if (!this.authService.isStudent()) {
      this.notificationService.warning('Student Account Required', 'Only students can book driving lessons.');
      return;
    }

    this.selectedInstructor = instructor;
    this.bookingSuccess = false;
    this.bookingError = '';
    this.bookingTime = '09:00';
    this.showBookingModal = true;
  }

  closeBookingModal(): void {
    this.showBookingModal = false;
    this.selectedInstructor = null;
    this.bookingSuccess = false;
    this.bookingError = '';
  }

  submitBookingDefensive(): void {
    if (!this.selectedInstructor) {
      this.bookingError = 'No instructor selected.';
      return;
    }

    if (!this.bookingDate) {
      this.bookingError = 'Please select a date.';
      return;
    }

    if (!this.bookingTime) {
      this.bookingError = 'Please select a time slot.';
      return;
    }

    // Prevent double-click
    if (this.isBooking) return;

    this.isBooking = true;
    this.bookingError = '';

    // Build the ISO date-time string from the selected date and time slot
    const scheduledDateTime = new Date(`${this.bookingDate}T${this.bookingTime}:00`);

    this.bookingsService.createBooking({
      instructorId: this.selectedInstructor.id,
      scheduledAt: scheduledDateTime.toISOString()
    }).subscribe({
      next: (res) => {
        this.isBooking = false;
        this.bookingSuccess = true;
        this.notificationService.success(
          'Booking Confirmed!',
          `Your lesson with ${this.selectedInstructor?.fullName} is scheduled for ${this.bookingDate} at ${this.bookingTime}.`
        );
        this.stateStore.addLiveEvent(`New lesson booked with ${this.selectedInstructor?.fullName}`, 'success');
      },
      error: (err) => {
        this.isBooking = false;
        this.bookingError = err?.error?.error || 'Failed to submit booking. Please try again.';
      }
    });
  }
}
