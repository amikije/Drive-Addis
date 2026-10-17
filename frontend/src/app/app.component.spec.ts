import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { AppComponent } from './app.component';
import { describe, it, expect, beforeEach } from 'vitest';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();
  });

  it('should create the app shell', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should have the "DriveAddis" brand title', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app.title).toEqual('DriveAddis');
  });

  it('should return correct role-based dashboard links', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;

    // Anonymous
    app.currentUser = null;
    expect(app.getDashboardLink()).toBe('/');

    // Student
    app.currentUser = { userId: 1, phoneNumber: '+251911', role: 'Student', fullName: 'Test' };
    expect(app.getDashboardLink()).toBe('/student');

    // Instructor
    app.currentUser = { userId: 2, phoneNumber: '+251922', role: 'Instructor', fullName: 'Test' };
    expect(app.getDashboardLink()).toBe('/instructor');

    // Admin
    app.currentUser = { userId: 3, phoneNumber: '+251900', role: 'Admin', fullName: 'Test' };
    expect(app.getDashboardLink()).toBe('/admin');
  });
});
