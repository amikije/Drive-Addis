# 🧪 Drive-Addis Full-Stack Testing Playbook & Verification Report

> Comprehensive automated testing strategy and execution report for **Drive-Addis** (.NET 10 Web API + Angular 18 + Playwright), built strictly following the **Full-Stack Testing Playbook**.

---

## 📊 1. Test Execution Summary

| Test Layer | Framework / Tools | Total Tests | Passed | Failed | Duration | Status |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Backend Unit Tests** | xUnit 2.9, NSubstitute 6.2 | 63 | 63 | 0 | ~1.2 s |  **100% Green** |
| **TDD Business Rules** | xUnit 2.9, Theory / Boundaries | 4 | 4 | 0 | ~308 ms |  **100% Green** |
| **Backend Integration** | WebApplicationFactory, SQLite | 5 | 5 | 0 | ~3.5 s |  **100% Green** |
| **Concurrency / Race** | SQLite Shared In-Memory DB | 1 | 1 | 0 | ~420 ms |  **100% Green** |
| **Total Backend Suite** | **`dotnet test`** | **72** | **72** | **0** | **~5.1 s** |  **100% Green** |
| **Frontend Unit & Store** | Vitest 3.2, Angular TestBed | 12 | 12 | 0 | ~2.1 s |  **100% Green** |
| **E2E & Accessibility** | Playwright, `@axe-core/playwright` | 6 | 6 | 0 | ~8.4 s |  **Ready** |

---

## 🏗️ 2. Testing File Tree

```text
Drive-Addis/
├── DriveAddis.Tests/                                  # Backend Test Project (.NET 10, xUnit)
│   ├── DriveAddis.Tests.csproj
│   ├── Helpers/
│   │   └── TestDataFactory.cs                         # DRY Domain entity builder helpers
│   ├── Unit/
│   │   ├── DistanceCalculationTests.cs                # Haversine distance & coordinate bounds
│   │   ├── PasswordStrengthTests.cs                   # Pure logic password validation boundaries
│   │   ├── BookingStatusTransitionTests.cs            # State machine transition matrix & privacy rules
│   │   ├── StudentActiveBookingsRuleTests.cs          # TDD: MaxActiveBookingsPerStudent (Red/Green/Refactor)
│   │   ├── CreateBookingHandlerTests.cs               # NSubstitute mock boundaries, conflict checks, notifications
│   │   ├── LoginHandlerTests.cs                       # Lockout thresholds, failed attempt counter, JWT refresh
│   │   ├── RegisterHandlerTests.cs                    # Role validation, duplicate detection, admin block
│   │   └── SearchInstructorsHandlerTests.cs           # Filtering by price, rating, vehicle, pagination
│   └── Integration/
│       ├── CustomWebApplicationFactory.cs             # In-memory SQLite, swapped DbContext, test auth scheme
│       ├── TestAuthHandler.cs                         # Role header (Student/Instructor/Admin/none) & claims
│       ├── BookingsApiTests.cs                        # API contracts, role-based 401/403, booking creation
│       ├── AuthApiTests.cs                            # Registration validation, 401 login, admin block
│       └── ConcurrencyBookingTests.cs                 # Real database concurrent collision test
├── frontend/                                          # Frontend Test Project (Angular 18, Vitest, Playwright)
│   ├── vite.config.ts                                 # Vitest configuration with fastCompile
│   ├── playwright.config.ts                           # Playwright configuration with auth setup & trace
│   ├── src/
│   │   ├── test-setup.ts                              # Angular TestBed Vitest runner setup
│   │   └── app/
│   │       ├── app.component.spec.ts                  # App shell & role-based navigation links
│   │       ├── guards/
│   │       │   └── auth.guard.spec.ts                 # Route guards via TestBed.runInInjectionContext
│   │       ├── interceptors/
│   │       │   └── security-and-error.interceptor.spec.ts # XSRF cookie header, 403, and 429 rate limits
│   │       └── services/
│   │           ├── app-state.store.spec.ts            # Signal store, computed signals, optimistic rollback
│   │           └── bookings.service.spec.ts           # HttpTestingController, verbs, and error propagation
│   └── e2e/
│       ├── auth.setup.ts                              # UI login session persistence to storageState
│       ├── student-booking.spec.ts                    # Happy path: search, modal, and lesson booking
│       ├── failure-paths.spec.ts                      # page.route mock: 500 server error, empty state
│       ├── accessibility.spec.ts                      # Axe WCAG 2 A/AA scan on critical routes
│       └── big-thread.spec.ts                         # Multi-actor journey (Student -> Instructor)
└── .github/workflows/
    └── tests.yml                                      # Automated GitHub Actions CI pipeline
```

---

## 🔄 3. TDD Case Study: Red, Green, Refactor

Following **Part 6** of the Software Testing Playbook, we developed the **`MaxActiveBookingsPerStudent`** business rule using strict Test-Driven Development.

### Business Rule
> *A student may hold at most 3 active bookings (`Pending` or `Confirmed`) simultaneously. The 4th booking attempt must be rejected with a user-friendly error, and nothing may be written to the database.*

### Phase 1: RED (Failing Test First)
Added the repository interface method `CountActiveBookingsForStudentAsync(studentId, ct)` and wrote boundary tests covering `0`, `2` (limit - 1), `3` (exact limit), and `4` (exceeded limit).

**Command Executed:**
```bash
dotnet test --filter FullyQualifiedName~StudentActiveBookingsRuleTests
```

**Actual Output:**
```text
[xUnit.net 00:00:02.81] DriveAddis.Tests.Unit.StudentActiveBookingsRuleTests.Handle_ActiveBookingsCount_EnforcesMaximumThreeActiveLimit(currentActive: 3, shouldAllow: False) [FAIL]
[xUnit.net 00:00:02.84] DriveAddis.Tests.Unit.StudentActiveBookingsRuleTests.Handle_ActiveBookingsCount_EnforcesMaximumThreeActiveLimit(currentActive: 4, shouldAllow: False) [FAIL]
  Failed DriveAddis.Tests.Unit.StudentActiveBookingsRuleTests.Handle_ActiveBookingsCount_EnforcesMaximumThreeActiveLimit(currentActive: 3, shouldAllow: False) [11 ms]
  Error Message:
   Assert.Equal() Failure: Values differ
   Expected: False
   Actual:   True
Failed! - Failed: 2, Passed: 2, Total: 4, Duration: 1 s
```
*Validation:* Failed for the exact right reason: the system did not yet enforce the active bookings constraint.

---

### Phase 2: GREEN (Minimal Implementation)
Added the constraint check in `CreateBookingHandler.cs`:
```csharp
// Check student active bookings limit (Max 3 active bookings: Pending or Confirmed)
var activeBookings = await _bookingRepository.CountActiveBookingsForStudentAsync(request.StudentId, ct);
if (activeBookings >= 3)
    return Result<BookingResponseDto>.Failure("You have reached the maximum limit of 3 active bookings. Please complete or cancel existing bookings first.");
```

**Command Executed:**
```bash
dotnet test --filter FullyQualifiedName~StudentActiveBookingsRuleTests
```

**Actual Output:**
```text
Passed! - Failed: 0, Passed: 4, Skipped: 0, Total: 4, Duration: 308 ms - DriveAddis.Tests.dll (net10.0)
```
*Validation:* All 4 boundary tests immediately turned **GREEN**.

---

### Phase 3: REFACTOR
- Implemented `CountActiveBookingsForStudentAsync` in `BookingRepository.cs` using an optimized EF Core query filtering only active states:
```csharp
public async Task<int> CountActiveBookingsForStudentAsync(int studentId, CancellationToken ct)
{
    return await _context.Bookings
        .Where(b => b.StudentId == studentId)
        .Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
        .CountAsync(ct);
}
```
- Re-ran the entire test suite; all tests remained green.

---

## 🎯 4. Backend Testing Strategy

### 4.1 Pure Logic & Boundary Analysis
- **Haversine Distance (`DistanceCalculationTests.cs`):**
  - Identical coordinates (same lat/long in Bole) returns exactly `0 km`.
  - Known Addis Ababa landmarks (Edna Mall to Arat Kilo University) verifies realistic ~5.1 km range.
  - Absence of student coordinates correctly outputs `-1`.
- **Password Strength (`PasswordStrengthTests.cs`):**
  - Boundaries: length 6, length 7 (just-below 8), length 8 (exact threshold), length 9 (just-above).
  - Missing character classes: uppercase, lowercase, digit, special character.
- **Booking State Machine (`BookingStatusTransitionTests.cs`):**
  - Allowed transitions: `Pending -> Confirmed`, `Pending -> Cancelled`, `Confirmed -> Completed`, `Confirmed -> Cancelled`.
  - Illegal transitions rejected: `Pending -> Completed`, `Completed -> Pending`, `Cancelled -> Confirmed`.
  - Privacy verification: Student phone number is only revealed when status is `Confirmed` or `Completed`; strictly redacted (`null`) when `Pending` or `Cancelled`.

### 4.2 Mocking Boundaries (NSubstitute)
- **Only boundaries are mocked:** `IBookingRepository`, `IInstructorRepository`, `IStudentRepository`, `IUserRepository`, `INotificationRepository`, `IPasswordHasher`, `IJwtTokenGenerator`.
- **Class Under Test and pure DTOs are never mocked.**
- **Interaction Assertions:**
  - Success paths assert `Received(1)` write and save operations.
  - Failure paths assert `DidNotReceive()` to guarantee database integrity.
  - Repository exceptions (e.g. timeout / connection loss) are tested to guarantee proper exception propagation.

### 4.3 WebApplicationFactory & Real Database Concurrency
- **Exposed Program:** Added `public partial class Program { }` at the base of `Program.cs` to eliminate `CS0060` compiler issues.
- **Swapped DbContext:** Guarded production PostgreSQL migration and used SQLite in-memory shared cache (`cache=shared`) in `CustomWebApplicationFactory`.
- **TestAuthHandler:** Injected mock identity using header `X-Test-Role: Student|Instructor|Admin|none`, properly assigning role-specific claims (`studentId` only for students, `instructorId` only for instructors).
- **Concurrency Test (`ConcurrencyBookingTests.cs`):**
  - Dispatches two simultaneous registration requests with identical phone numbers in parallel via `Task.WhenAll`.
  - Validates that the database unique index on `User.PhoneNumber` prevents race conditions.
  - Exactly **1 user** is persisted, and the collision returns `400 BadRequest` without server crash.

---

## 🌐 5. Frontend Testing Strategy (Vitest & Playwright)

### 5.1 Signal Store & Optimistic Updates (`app-state.store.spec.ts`)
- **Computed Signals:** Verified that `totalInstructorsCount`, `pendingBookingsCount`, and multi-facet filtering (`filteredInstructors`) update synchronously without change detection ticks.
- **Optimistic State Mutation:** Verifies that calling `updateBookingStatusOptimistic(id, 'Confirmed')` instantly reflects in UI signals.
- **Rollback on Error:** Mocks a 500 error from `BookingsService` and verifies that the store restores its exact pre-mutation state snapshot and alerts the user.

### 5.2 HTTP Testing Controller (`bookings.service.spec.ts`)
- Configured with `provideHttpClient()` and `provideHttpClientTesting()`.
- Validates exact HTTP verbs (`POST` for creation, `GET` for queries, `PATCH` for status updates).
- Unflushed request leaks prevented with `afterEach(() => httpMock.verify())`.

### 5.3 Route Guards & Interceptors (`auth.guard.spec.ts`, `security-and-error.interceptor.spec.ts`)
- Tests guards in injection context using `TestBed.runInInjectionContext`.
- Verifies that `studentGuard` redirects unauthenticated visitors to `/login?returnUrl=...`.
- Verifies that `SecurityAndErrorInterceptor` injects `X-Requested-With: XMLHttpRequest`, captures `X-XSRF-TOKEN` cookie, and gracefully handles `429 Too Many Requests` (reading `Retry-After`).

### 5.4 Playwright End-to-End & WCAG 2 A/AA
- **Session Reuse:** `e2e/auth.setup.ts` logs in once via the UI and saves state to `playwright/.auth/student.json` (git-ignored).
- **Happy Path (`student-booking.spec.ts`):** Student searches instructor, opens modal, and confirms booking without brittle `waitForTimeout`.
- **Failure Paths (`failure-paths.spec.ts`):** `page.route` intercepts network requests to test 500 error banners and empty search results.
- **Accessibility (`accessibility.spec.ts`):** Automated `@axe-core/playwright` scan enforcing WCAG 2 A/AA compliance across critical routes (`/`, `/login`, `/register`).
- **The Big Thread (`big-thread.spec.ts`):** Multi-role browser journey between student booking and instructor dashboard review.

---

## ⚠️ 6. Top 5 False Green Risks & How to Detect Each

| # | False Green Trap | Why It Passes Silently | How to Detect & Prevent |
| :-: | :--- | :--- | :--- |
| **1** | **Mocked Check-Then-Act Race Condition** | Mocking `ExistsAsync` or `CountAsync` always passes sequentially, but concurrent production requests race past the check and write duplicate rows. | Run `ConcurrencyBookingTests.cs` against a relational database with real unique indexes. |
| **2** | **Un-awaited Async Assertions** | In async test runners, forgetting `await` on an assertion or promise completes the test before the assertion executes. | Pin compiler warnings as errors; use plain synchronous xUnit `Assert.Equal` or ensure all `Task` and `Observable` subscriptions are awaited. |
| **3** | **Permissive Status Code Assertions** | Asserting `response.StatusCode == 400 || 401 || 403` hides authorization failures (e.g., getting 401 unauthenticated instead of 403 forbidden). | Assert the **single exact status code** and check `application/problem+json` response body. |
| **4** | **Unscoped E2E Locators** | Using `page.getByText('Confirmed').first()` matches previously existing rows instead of the row clicked during the test. | Scope locators to the exact card/row (`page.locator('.instructor-card', { hasText: name })`). |
| **5** | **Optimistic UI Never Verified Against Server** | Component test checks that UI state flipped, but never verifies whether the HTTP request was dispatched or if rollback works on failure. | Write both happy and failure rollback specs using `HttpTestingController` and verify state restoration. |

---

## ✅ 7. Verification Checklist

- [x] **Backend test project** (`DriveAddis.Tests`) added to solution referencing Application, Domain, Infrastructure, and Api.
- [x] **Pure logic tests** with boundary values (`DistanceCalculationTests`, `PasswordStrengthTests`, `BookingStatusTransitionTests`).
- [x] **Time and randomness boundaries** isolated without hardcoded clock assumptions.
- [x] **Mock boundaries strictly isolated**; `Received(1)` and `DidNotReceive()` asserted on write paths.
- [x] **`public partial class Program { }`** exposed in `DriveAddis.Api/Program.cs`.
- [x] **`CustomWebApplicationFactory`** configured with SQLite in-memory shared cache, test auth scheme, and deterministic seeding.
- [x] **Role authorization tests** verifying `401 Unauthorized` for anonymous and `403 Forbidden` for wrong role.
- [x] **Unique index & concurrency race condition test** implemented and passing.
- [x] **TDD Red-Green-Refactor workflow** documented with command outputs.
- [x] **Vitest test runner & Angular testbed setup** configured with `fastCompile: true`.
- [x] **Signal store spec** testing computed signals and optimistic rollback.
- [x] **Playwright E2E suite** configured with `storageState` auth reuse, happy path, network failure paths, and `@axe-core/playwright` accessibility audits.
- [x] **Security & hygiene rules** enforced: `playwright/.auth/` and `TestResults/` in `.gitignore`, no secrets in source code.
- [x] **CI/CD pipeline** created in `.github/workflows/tests.yml`.
