import { Component, OnInit, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AdminService, AdminDashboardDto, InstructorAdminListItemDto, StudentAdminListItemDto } from '../../services/admin.service';
import { InstructorsService } from '../../services/instructors.service';
import { BookingDto } from '../../services/bookings.service';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  public authService = inject(AuthService);
  public themeService = inject(ThemeService);
  public notificationService = inject(NotificationService);
  private adminService = inject(AdminService);
  private instructorsService = inject(InstructorsService);
  private router = inject(Router);

  dashboardStats: AdminDashboardDto | null = null;
  instructors: InstructorAdminListItemDto[] = [];
  bookings: BookingDto[] = [];
  students: StudentAdminListItemDto[] = [];

  activeTab: 'overview' | 'instructors' | 'bookings' | 'students' | 'payments' | 'settings' = 'overview';
  instructorFilter: 'all' | 'wait' | 'noid' | 'sus' = 'all';
  studentFilter: 'all' | 'with-bookings' | 'no-bookings' = 'all';
  searchQuery: string = '';

  loadingStats = false;
  loadingInstructors = false;
  loadingBookings = false;
  loadingStudents = false;
  errorMessage = '';

  // Drawer
  showDrawer = false;
  selectedInstructor: InstructorAdminListItemDto | null = null;

  // Toast
  toastMessage = '';
  showToast = false;
  private toastTimeout: any;

  // Reject Modal
  showRejectModal = false;
  rejectInstructorId: number | null = null;
  rejectInstructorName = '';
  rejectReason = '';
  isRejecting = false;

  // Photo Modal
  showPhotoModal = false;
  viewPhotoUrl: string | null = null;
  viewPhotoInstructorName = '';

  currentDateStr = '';

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.showPhotoModal) {
      this.closePhotoModal();
    } else if (this.showRejectModal) {
      this.closeRejectModal();
    } else if (this.showDrawer) {
      this.closeDrawer();
    }
  }

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    this.currentDateStr = new Date().toLocaleDateString('en-GB', {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
    });

    this.loadStats();
    this.loadInstructors();
    this.loadBookings();
    this.loadStudents();
  }

  loadStats(): void {
    this.loadingStats = true;
    this.adminService.getDashboard().subscribe({
      next: (stats) => {
        this.dashboardStats = stats;
        this.loadingStats = false;
      },
      error: (err) => {
        this.loadingStats = false;
        console.error('Failed to load dashboard stats:', err);
      }
    });
  }

  loadInstructors(): void {
    this.loadingInstructors = true;
    this.adminService.getInstructors().subscribe({
      next: (data) => {
        this.instructors = data || [];
        this.loadingInstructors = false;
      },
      error: (err) => {
        this.loadingInstructors = false;
        this.errorMessage = 'Failed to load instructors list.';
        console.error(err);
      }
    });
  }

  loadBookings(): void {
    this.loadingBookings = true;
    this.adminService.getAllBookings().subscribe({
      next: (data) => {
        this.bookings = data || [];
        this.loadingBookings = false;
      },
      error: (err) => {
        this.loadingBookings = false;
        console.error('Failed to load bookings:', err);
      }
    });
  }

  loadStudents(): void {
    this.loadingStudents = true;
    this.adminService.getStudents().subscribe({
      next: (data) => {
        this.students = data || [];
        this.loadingStudents = false;
      },
      error: (err) => {
        this.loadingStudents = false;
        console.error('Failed to load students:', err);
      }
    });
  }

  get greeting(): string {
    const h = new Date().getHours();
    return h < 12 ? 'Good morning' : h < 18 ? 'Good afternoon' : 'Good evening';
  }

  get pendingInstructorsCount(): number {
    return this.instructors.filter(x => x.verificationStatus === 'Pending').length;
  }

  get instructorsWithNoLicenseCount(): number {
    return this.instructors.filter(x => !x.licensePhotoUrl).length;
  }

  get suspendedCount(): number {
    return this.instructors.filter(x => x.verificationStatus === 'Rejected').length;
  }

  get pendingBookingsCount(): number {
    return this.bookings.filter(x => x.status === 'Pending').length;
  }

  get filteredInstructors(): InstructorAdminListItemDto[] {
    const q = this.searchQuery.toLowerCase().trim();
    return this.instructors.filter(x => {
      const matchesFilter =
        this.instructorFilter === 'all' ||
        (this.instructorFilter === 'wait' && x.verificationStatus === 'Pending') ||
        (this.instructorFilter === 'noid' && !x.licensePhotoUrl) ||
        (this.instructorFilter === 'sus' && x.verificationStatus === 'Rejected');

      const matchesSearch = !q || (x.fullName + ' ' + x.phoneNumber).toLowerCase().includes(q);
      return matchesFilter && matchesSearch;
    });
  }

  get filteredStudents(): StudentAdminListItemDto[] {
    const q = this.searchQuery.toLowerCase().trim();
    return this.students.filter(s => {
      const matchesFilter =
        this.studentFilter === 'all' ||
        (this.studentFilter === 'with-bookings' && s.totalBookings > 0) ||
        (this.studentFilter === 'no-bookings' && s.totalBookings === 0);

      const matchesSearch =
        !q ||
        (s.fullName + ' ' + s.phoneNumber + ' ' + (s.licenseNumber || '')).toLowerCase().includes(q);

      return matchesFilter && matchesSearch;
    });
  }

  get studentsWithBookingsCount(): number {
    return this.students.filter(s => s.totalBookings > 0).length;
  }

  get studentsWithoutBookingsCount(): number {
    return this.students.filter(s => s.totalBookings === 0).length;
  }

  get upcomingBookings(): BookingDto[] {
    return this.bookings.slice(0, 5);
  }

  onSearchInput(): void {
    if (this.activeTab !== 'instructors' && this.activeTab !== 'students') {
      this.activeTab = 'instructors';
    }
  }

  setTab(tab: 'overview' | 'instructors' | 'bookings' | 'students' | 'payments' | 'settings'): void {
    this.activeTab = tab;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  setInstructorFilter(filter: 'all' | 'wait' | 'noid' | 'sus'): void {
    this.instructorFilter = filter;
    this.activeTab = 'instructors';
  }

  setStudentFilter(filter: 'all' | 'with-bookings' | 'no-bookings'): void {
    this.studentFilter = filter;
    this.activeTab = 'students';
  }

  openDrawer(instructor: InstructorAdminListItemDto): void {
    this.selectedInstructor = instructor;
    this.showDrawer = true;
  }

  closeDrawer(): void {
    this.showDrawer = false;
  }

  openFirstPending(): void {
    const first = this.instructors.find(x => x.verificationStatus === 'Pending');
    if (first) {
      this.openDrawer(first);
    } else {
      this.setInstructorFilter('wait');
    }
  }

  verifyInstructor(id: number, fromDrawer = false): void {
    this.instructorsService.verifyInstructor(id).subscribe({
      next: () => {
        const ins = this.instructors.find(i => i.id === id);
        const name = ins ? ins.fullName : 'Instructor';
        if (ins) {
          ins.verificationStatus = 'Verified';
        }
        if (this.selectedInstructor && this.selectedInstructor.id === id) {
          this.selectedInstructor.verificationStatus = 'Verified';
        }
        this.say(`${name} is verified`);
        this.notificationService.success('Instructor Verified', `${name} has been verified and is active!`);
        this.loadStats();
        if (fromDrawer) {
          this.closeDrawer();
        }
      },
      error: (err) => {
        this.say(err?.error?.error || 'Verification failed');
        this.notificationService.error('Verification Failed', err?.error?.error || 'Failed to verify instructor.');
      }
    });
  }

  openRejectModal(instructor: InstructorAdminListItemDto): void {
    this.rejectInstructorId = instructor.id;
    this.rejectInstructorName = instructor.fullName;
    this.rejectReason = 'Missing or invalid driving instructor license documentation.';
    this.showRejectModal = true;
  }

  closeRejectModal(): void {
    this.showRejectModal = false;
    this.rejectInstructorId = null;
  }

  confirmReject(): void {
    if (!this.rejectInstructorId || !this.rejectReason.trim()) {
      alert('Please provide a reason for rejecting this instructor.');
      return;
    }

    this.isRejecting = true;
    const instructorId = this.rejectInstructorId;
    const reason = this.rejectReason.trim();

    this.instructorsService.rejectInstructor(instructorId, reason).subscribe({
      next: () => {
        this.isRejecting = false;
        const ins = this.instructors.find(i => i.id === instructorId);
        const name = ins ? ins.fullName : 'Instructor';
        if (ins) {
          ins.verificationStatus = 'Rejected';
          ins.rejectionReason = reason;
        }
        if (this.selectedInstructor && this.selectedInstructor.id === instructorId) {
          this.selectedInstructor.verificationStatus = 'Rejected';
          this.selectedInstructor.rejectionReason = reason;
        }
        this.say(`${name} was rejected`);
        this.notificationService.warning('Instructor Rejected', `${name} rejected: ${reason}`);
        this.closeRejectModal();
        this.closeDrawer();
        this.loadStats();
      },
      error: (err) => {
        this.isRejecting = false;
        this.say(err?.error?.error || 'Rejection failed');
        this.notificationService.error('Rejection Failed', err?.error?.error || 'Failed to reject instructor.');
      }
    });
  }

  openPhotoModal(instructor: InstructorAdminListItemDto): void {
    this.viewPhotoUrl = instructor.licensePhotoUrl || null;
    this.viewPhotoInstructorName = instructor.fullName;
    this.showPhotoModal = true;
  }

  closePhotoModal(): void {
    this.showPhotoModal = false;
    this.viewPhotoUrl = null;
  }

  confirmBooking(booking: BookingDto): void {
    booking.status = 'Confirmed';
    this.say(`Booking for ${booking.studentName || 'Student'} confirmed`);
    this.notificationService.success('Booking Confirmed', `Booking #${booking.id} has been confirmed.`);
  }

  say(message: string): void {
    this.toastMessage = message;
    this.showToast = true;
    clearTimeout(this.toastTimeout);
    this.toastTimeout = setTimeout(() => {
      this.showToast = false;
    }, 2400);
  }

  onBellClick(): void {
    if (this.pendingInstructorsCount > 0 || this.pendingBookingsCount > 0) {
      this.say(`${this.pendingInstructorsCount} pending approval, ${this.pendingBookingsCount} unconfirmed booking`);
    } else {
      this.say('All caught up! Nothing needs your attention.');
    }
  }

  onComingSoon(feature: string): void {
    this.say(`${feature} is not part of this demo`);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  logout(): void {
    this.authService.logout();
    this.notificationService.info('Signed Out', 'You have been logged out.');
    this.router.navigate(['/login']);
  }

  // Sparkline generator matching mockup: (80x28 viewBox)
  getSparkline(data: number[]): string {
    if (!data || data.length < 2) return '';
    const max = Math.max(...data, 1);
    return data
      .map((v, i) => `${(i * (80 / (data.length - 1))).toFixed(1)},${(26 - (v / max) * 22).toFixed(1)}`)
      .join(' ');
  }

  get sparklineStudents(): string {
    return this.getSparkline([2, 3, 3, 5, 6, 8, this.dashboardStats?.totalStudents || 10]);
  }

  get sparklineInstructors(): string {
    return this.getSparkline([3, 3, 4, 4, 5, 5, this.instructors.length || 6]);
  }

  get sparklineBookings(): string {
    return this.getSparkline([0, 0, 0, 1, 1, 2, this.bookings.length || 3]);
  }

  get sparklinePending(): string {
    return this.getSparkline([0, 1, 0, 0, 1, 1, this.pendingInstructorsCount]);
  }
}
