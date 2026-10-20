import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

interface LocationPreset {
  name: string;
  lat: number;
  lng: number;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  selectedRole: 'Student' | 'Instructor' = 'Student';
  registerForm!: FormGroup;

  locationPresets: LocationPreset[] = [
    { name: 'Bole / Medhanialem', lat: 9.0000, lng: 38.7800 },
    { name: 'Kazanchis / ECA', lat: 9.0180, lng: 38.7650 },
    { name: 'Piassa / Arat Kilo', lat: 9.0350, lng: 38.7500 },
    { name: 'CMC / Ayat / Megenagna', lat: 9.0200, lng: 38.8200 },
    { name: 'Sarbet / Mexico / Kera', lat: 8.9950, lng: 38.7400 },
    { name: 'Lebu / Jomo / Haile Garment', lat: 8.9500, lng: 38.7200 }
  ];

  errorMessage = '';
  loading = false;

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^(\+251|0)9[0-9]{8}$/)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      hourlyPrice: [350, [Validators.min(100), Validators.max(2500)]],
      selectedLocation: ['Bole / Medhanialem'],
      licensePhotoUrl: ['', [Validators.required]],
      vehicleType: ['Manual', [Validators.required]],
      vehicleModel: ['', [Validators.required]],
      plateNumber: ['', [Validators.required]]
    });
  }

  selectRole(role: 'Student' | 'Instructor'): void {
    this.selectedRole = role;
    this.errorMessage = '';

    const licenseControl = this.registerForm.get('licensePhotoUrl');
    const vehicleTypeControl = this.registerForm.get('vehicleType');
    const vehicleModelControl = this.registerForm.get('vehicleModel');
    const plateNumberControl = this.registerForm.get('plateNumber');

    if (role === 'Instructor') {
      licenseControl?.setValidators([Validators.required]);
      vehicleTypeControl?.setValidators([Validators.required]);
      vehicleModelControl?.setValidators([Validators.required]);
      plateNumberControl?.setValidators([Validators.required]);
    } else {
      licenseControl?.clearValidators();
      vehicleTypeControl?.clearValidators();
      vehicleModelControl?.clearValidators();
      plateNumberControl?.clearValidators();
    }
    licenseControl?.updateValueAndValidity();
    vehicleTypeControl?.updateValueAndValidity();
    vehicleModelControl?.updateValueAndValidity();
    plateNumberControl?.updateValueAndValidity();
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        this.errorMessage = 'File is too large. Maximum size is 5MB.';
        this.registerForm.get('licensePhotoUrl')?.setValue('');
        return;
      }
      const reader = new FileReader();
      reader.onload = () => {
        this.registerForm.get('licensePhotoUrl')?.setValue(reader.result as string);
        this.errorMessage = '';
      };
      reader.readAsDataURL(file);
    }
  }

  get passwordValue(): string {
    return this.registerForm?.get('password')?.value || '';
  }

  get hasMinLength(): boolean {
    return this.passwordValue.length >= 8;
  }

  get hasUpperCase(): boolean {
    return /[A-Z]/.test(this.passwordValue);
  }

  get hasLowerCase(): boolean {
    return /[a-z]/.test(this.passwordValue);
  }

  get hasNumber(): boolean {
    return /[0-9]/.test(this.passwordValue);
  }

  get hasSpecialChar(): boolean {
    return /[\W_]/.test(this.passwordValue);
  }

  get isPasswordStrong(): boolean {
    return this.hasMinLength && this.hasUpperCase && this.hasLowerCase && this.hasNumber && this.hasSpecialChar;
  }

  onSubmit(): void {
    if (this.registerForm.invalid || !this.isPasswordStrong) {
      this.registerForm.markAllAsTouched();
      this.errorMessage = 'Please fix the errors in the form and ensure your password meets all requirements.';
      return;
    }

    const formValues = this.registerForm.getRawValue();

    if (formValues.password !== formValues.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const payload: any = {
      fullName: formValues.fullName.trim(),
      phoneNumber: formValues.phoneNumber.trim(),
      password: formValues.password,
      role: this.selectedRole
    };

    if (this.selectedRole === 'Instructor') {
      const loc = this.locationPresets.find(p => p.name === formValues.selectedLocation);
      payload.hourlyPrice = formValues.hourlyPrice ? Number(formValues.hourlyPrice) : 350;
      payload.latitude = loc ? loc.lat : 9.0000;
      payload.longitude = loc ? loc.lng : 38.7800;
      payload.licensePhotoUrl = formValues.licensePhotoUrl;
      payload.vehicleType = formValues.vehicleType;
      payload.vehicleModel = formValues.vehicleModel?.trim();
      payload.plateNumber = formValues.plateNumber?.trim();
    }

    this.authService.register(payload).subscribe({
      next: (res) => {
        this.loading = false;
        this.notificationService.success('Registration Successful', `Welcome to DriveAddis as a ${this.selectedRole}!`);
        if (this.selectedRole === 'Instructor') {
          this.router.navigate(['/instructor']);
        } else {
          this.router.navigate(['/student']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err?.error?.error || 'Registration failed. Phone number may already be in use.';
        this.notificationService.error('Registration Error', this.errorMessage);
      }
    });
  }
}
