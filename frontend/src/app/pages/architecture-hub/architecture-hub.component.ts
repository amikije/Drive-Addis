import { Component, signal, computed, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, of } from 'rxjs';
import { exhaustMap, delay, switchMap, tap } from 'rxjs/operators';
import { AppStateStore } from '../../services/app-state.store';
import { NotificationService } from '../../services/notification.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-architecture-hub',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  templateUrl: './architecture-hub.component.html',
  styleUrl: './architecture-hub.component.css'
})
export class ArchitectureHubComponent {
  public stateStore = inject(AppStateStore);
  public notificationService = inject(NotificationService);
  public authService = inject(AuthService);
  private fb = inject(FormBuilder);

  activeTab: 'module8' | 'module9' | 'module10' | 'module11' | 'checklist' = 'module8';

  // --- Module 8: Signals & Reactive Forms Demo ---
  // Writable Signals
  lessonPriceSignal = signal<number>(350);
  lessonHoursSignal = signal<number>(4);
  discountRateSignal = signal<number>(10); // 10%

  // Computed Signals (Module 8: Read-only, automatically updates without manual change detection)
  subtotalSignal = computed(() => this.lessonPriceSignal() * this.lessonHoursSignal());
  discountAmountSignal = computed(() => (this.subtotalSignal() * this.discountRateSignal()) / 100);
  finalTotalSignal = computed(() => this.subtotalSignal() - this.discountAmountSignal());

  // Reactive Form with dynamic validation & getRawValue() (Module 8)
  instructorForm: FormGroup;
  formRawValueJson = '';

  // --- Module 9: Defensive RxJS & @defer blocks Demo ---
  // Rage click simulation using exhaustMap
  private clickSubject = new Subject<number>();
  rapidClicksSent = 0;
  requestsProcessed = 0;
  requestsIgnored = 0;
  isProcessingRequest = false;

  // @defer trigger test
  showHeavyComponent = false;
  deferTriggerType = 'interaction';

  // --- Module 10: Optimistic UI with Rollback Demo ---
  optimisticCounter = signal<number>(42);
  optimisticHistory = signal<string[]>(['Initial state: counter = 42']);
  isOptimisticLoading = signal<boolean>(false);

  // --- Module 11: Security & JWT / Rate Limiting Demo ---
  sampleJwtToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZWlkIjoiMTAxIiwicm9sZSI6Ikluc3RydWN0b3IiLCJtb2JpbGVwaG9uZSI6IisyNTE5MTEwMDAwMDEiLCJleHAiOjE3NzIxOTIwMDB9.sampleSignature';
  decodedJwtPayload: any = null;
  rateLimitCount = 0;
  rateLimitMax = 5;
  rateLimitResetSeconds = 0;
  isRateLimited = false;

  constructor() {
    // Initialize Reactive Form (Module 8)
    this.instructorForm = this.fb.group({
      fullName: ['Abebe Kebede', [Validators.required, Validators.minLength(3)]],
      phoneNumber: ['+251911000001', [Validators.required, Validators.pattern(/^(\+251|0)9[0-9]{8}$/)]],
      hourlyRate: [350, [Validators.required, Validators.min(100), Validators.max(2000)]],
      vehicleType: ['Manual', Validators.required],
      licenseNumber: [{ value: 'RTA-ADDIS-88219', disabled: true }] // Disabled field testing getRawValue()
    });

    this.updateFormRawValue();
    this.instructorForm.valueChanges.subscribe(() => {
      this.updateFormRawValue();
    });

    // Set up Defensive RxJS pipeline using exhaustMap (Module 9)
    this.clickSubject.pipe(
      tap(() => {
        if (this.isProcessingRequest) {
          this.requestsIgnored++;
        }
      }),
      exhaustMap((clickIndex) => {
        this.isProcessingRequest = true;
        this.requestsProcessed++;
        return of(`Lesson booking #${clickIndex} processed`).pipe(
          delay(1200),
          tap(() => {
            this.isProcessingRequest = false;
          })
        );
      })
    ).subscribe();

    this.decodeSampleJwt();
  }

  // --- Module 8 Methods ---
  incrementHours(): void {
    this.lessonHoursSignal.update(h => h + 1);
  }

  decrementHours(): void {
    if (this.lessonHoursSignal() > 1) {
      this.lessonHoursSignal.update(h => h - 1);
    }
  }

  setRate(rate: number): void {
    this.lessonPriceSignal.set(rate);
  }

  updateFormRawValue(): void {
    // Module 8: getRawValue() retrieves all form data including disabled fields!
    this.formRawValueJson = JSON.stringify(this.instructorForm.getRawValue(), null, 2);
  }

  // --- Module 9 Methods ---
  triggerRapidClick(): void {
    this.rapidClicksSent++;
    this.clickSubject.next(this.rapidClicksSent);
  }

  resetClickTester(): void {
    this.rapidClicksSent = 0;
    this.requestsProcessed = 0;
    this.requestsIgnored = 0;
    this.isProcessingRequest = false;
  }

  // --- Module 10 Optimistic UI Methods ---
  triggerOptimisticAction(shouldFail: boolean): void {
    const previous = this.optimisticCounter();
    this.isOptimisticLoading.set(true);

    // 1. Instant UI update
    this.optimisticCounter.update(v => v + 1);
    this.optimisticHistory.update(h => [
      `[OPTIMISTIC] UI updated immediately to ${previous + 1}`,
      ...h.slice(0, 7)
    ]);
    this.notificationService.info('Optimistic Update', `Counter incremented to ${previous + 1}`);

    // 2. Simulate server response
    setTimeout(() => {
      this.isOptimisticLoading.set(false);
      if (shouldFail) {
        // Rollback to previous state
        this.optimisticCounter.set(previous);
        this.optimisticHistory.update(h => [
          `[ROLLBACK] Server responded with 500 error. Restored counter to ${previous}`,
          ...h.slice(0, 7)
        ]);
        this.notificationService.error('Server Rejected (500)', 'Optimistic action rolled back safely.');
      } else {
        this.optimisticHistory.update(h => [
          `[CONFIRMED] Server 200 OK. State change committed permanently.`,
          ...h.slice(0, 7)
        ]);
        this.notificationService.success('Server Confirmed (200)', 'State successfully synchronized.');
      }
    }, 1000);
  }

  // --- Module 11 Security Methods ---
  decodeSampleJwt(): void {
    try {
      this.decodedJwtPayload = {
        sub: '1234567890',
        nameid: '101',
        role: 'Instructor',
        mobilephone: '+251911000001',
        iss: 'DriveAddis.Api',
        aud: 'DriveAddis.Client',
        exp: new Date(Date.now() + 15 * 60 * 1000).toLocaleTimeString() + ' (15 min lifespan)'
      };
    } catch {
      this.decodedJwtPayload = {};
    }
  }

  simulateRateLimitRequest(): void {
    if (this.isRateLimited) return;

    this.rateLimitCount++;
    if (this.rateLimitCount > this.rateLimitMax) {
      this.isRateLimited = true;
      this.rateLimitResetSeconds = 10;
      this.notificationService.warning(
        'HTTP 429 Too Many Requests',
        'Rate limit hit (5 attempts/min). Retry-After: 10 seconds.'
      );

      const interval = setInterval(() => {
        this.rateLimitResetSeconds--;
        if (this.rateLimitResetSeconds <= 0) {
          clearInterval(interval);
          this.isRateLimited = false;
          this.rateLimitCount = 0;
          this.notificationService.info('Rate Limit Reset', 'You can now submit requests again.');
        }
      }, 1000);
    } else {
      this.notificationService.success(
        'Request Allowed (200 OK)',
        `Request ${this.rateLimitCount}/${this.rateLimitMax} passed rate limit filter.`
      );
    }
  }
}
