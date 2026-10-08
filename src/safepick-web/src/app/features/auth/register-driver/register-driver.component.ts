import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '@app/core/api.service';
import type { PublicSchool } from '@app/core/models';

// ============================================================
// RegisterDriverComponent
//
// The driver's self-registration page.
//
// Two-step flow:
//   Step 1 — Personal details (name, ID, phone, email, password)
//   Step 2 — Vehicle + school (registration, make, model, capacity, school)
//   Step 3 — Success state
//
// Public — no auth guard. A driver has no account yet.
//
// Route: /register-driver
// ============================================================

type Step = 'personal' | 'vehicle' | 'done';

@Component({
  selector: 'app-register-driver',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './register-driver.component.html',
  styleUrl: './register-driver.component.scss',
})
export class RegisterDriverComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly api = inject(ApiService);

  // ---------- step control ----------

  readonly step = signal<Step>('personal');
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);

  // ---------- schools ----------

  readonly schools = signal<readonly PublicSchool[]>([]);
  readonly schoolsLoading = signal(true);
  readonly schoolsError = signal<string | null>(null);

  // ---------- forms ----------

  readonly personalForm = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    idNumber: ['', [Validators.required, Validators.minLength(6)]],
    phone: ['', [Validators.required, Validators.minLength(9)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  readonly vehicleForm = this.fb.nonNullable.group({
    vehicleRegistrationNumber: ['', [Validators.required]],
    vehicleMake: ['', [Validators.required]],
    vehicleModel: ['', [Validators.required]],
    vehicleCapacity: [
      1,
      [Validators.required, Validators.min(1), Validators.max(100)],
    ],
    schoolId: ['', [Validators.required]],
  });

  // Convenience getters
  get fullName()    { return this.personalForm.controls.fullName; }
  get idNumber()    { return this.personalForm.controls.idNumber; }
  get phone()       { return this.personalForm.controls.phone; }
  get email()       { return this.personalForm.controls.email; }
  get password()    { return this.personalForm.controls.password; }

  get vehicleReg()  { return this.vehicleForm.controls.vehicleRegistrationNumber; }
  get vehicleMake() { return this.vehicleForm.controls.vehicleMake; }
  get vehicleModel(){ return this.vehicleForm.controls.vehicleModel; }
  get capacity()    { return this.vehicleForm.controls.vehicleCapacity; }
  get schoolId()    { return this.vehicleForm.controls.schoolId; }

  // ---------- derived ----------

  readonly stepNumber = computed(() => {
    switch (this.step()) {
      case 'personal': return 1;
      case 'vehicle':  return 2;
      case 'done':     return 3;
    }
  });

  readonly stepTitle = computed(() => {
    switch (this.step()) {
      case 'personal': return 'Your details';
      case 'vehicle':  return 'Your vehicle and school';
      case 'done':     return '';
    }
  });

  readonly stepSubtitle = computed(() => {
    switch (this.step()) {
      case 'personal': return 'Tell us who you are.';
      case 'vehicle':  return 'Tell us about your vehicle and the school you’ll serve.';
      case 'done':     return '';
    }
  });

  // ---------- lifecycle ----------

  ngOnInit(): void {
    void this.loadSchools();
  }

  // ---------- schools ----------

  async loadSchools(): Promise<void> {
    this.schoolsLoading.set(true);
    this.schoolsError.set(null);

    try {
      const schools = await firstValueFrom(
        this.api.get<PublicSchool[]>('/schools/public')
      );
      this.schools.set(schools);
    } catch {
      this.schoolsError.set(
        'Could not load the school list. Please check your connection and try again.'
      );
    } finally {
      this.schoolsLoading.set(false);
    }
  }

  // ---------- navigation ----------

  goToVehicleStep(): void {
    this.personalForm.markAllAsTouched();
    if (this.personalForm.invalid) return;

    this.step.set('vehicle');
  }

  goBackToPersonal(): void {
    this.step.set('personal');
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  // ---------- submit ----------

  async onSubmit(): Promise<void> {
    if (this.submitting()) return;

    this.personalForm.markAllAsTouched();
    this.vehicleForm.markAllAsTouched();

    if (this.personalForm.invalid || this.vehicleForm.invalid) return;

    this.errorMessage.set(null);
    this.submitting.set(true);

    const p = this.personalForm.getRawValue();
    const v = this.vehicleForm.getRawValue();

    try {
      await firstValueFrom(
        this.api.post<string>('/drivers/register', {
          fullName: p.fullName.trim(),
          idNumber: p.idNumber.trim(),
          phone: p.phone.trim(),
          email: p.email.trim().toLowerCase(),
          password: p.password,
          vehicleRegistrationNumber: v.vehicleRegistrationNumber.trim().toUpperCase(),
          vehicleMake: v.vehicleMake.trim(),
          vehicleModel: v.vehicleModel.trim(),
          vehicleCapacity: Number(v.vehicleCapacity),
          schoolId: v.schoolId,
        })
      );

      this.step.set('done');
    } catch (error) {
      this.errorMessage.set(this.describeSubmitError(error));
    } finally {
      this.submitting.set(false);
    }
  }

  goToLogin(): void {
    void this.router.navigate(['/login']);
  }

  private describeSubmitError(error: unknown): string {
    if (typeof error === 'object' && error !== null) {
      const status = (error as { status?: number }).status;
      const body = (error as { error?: unknown }).error;

      if (typeof body === 'object' && body !== null) {
        const detail = (body as { detail?: string }).detail;
        if (detail) return detail;
      }

      if (status === 400) {
        return 'We couldn’t complete the registration. Please check your details.';
      }
      if (status === 0) {
        return 'Cannot reach the server. Please check your connection.';
      }
      if (status !== undefined && status >= 500) {
        return 'Something went wrong on our side. Please try again in a moment.';
      }
    }
    return 'Could not complete the registration. Please try again.';
  }

  // ---------- school helpers ----------

  schoolName(id: string): string {
    return this.schools().find((s) => s.schoolId === id)?.name ?? '';
  }
}