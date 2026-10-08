import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { AuthService } from '@app/core/auth.service';
import { homeForRole } from '@app/core/role-home';

// ============================================================
// LoginComponent
//
// The SafePick SA flagship "wow" screen.
//
// Structure:
//   5.3a — shell
//   5.3b — route
//   5.3c — reactive form
//   5.3d — rotating background
//   5.3d+ — branded card with logo
//   5.3e — submit logic
//   5.3f — setup complete banner (this revision)
//
// Route: /login (guarded by guestGuard)
// ============================================================

interface BackgroundImage {
  readonly src: string;
  readonly alt: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  /** Brand */
  readonly brand = 'SafePick SA';
  readonly tagline = 'Verifiable chain of custody for learner transport.';
  readonly logoPath = 'assets/logo.jpg';

  /** Rotating background images */
  readonly backgrounds: readonly BackgroundImage[] = [
    { src: 'assets/backgrounds/bg-1.jpg', alt: '' },
    { src: 'assets/backgrounds/bg-2.jpg', alt: '' },
    { src: 'assets/backgrounds/bg-3.jpg', alt: '' },
  ];

  /**
   * Where to send the user after successful login.
   * Set from the ?returnTo= query parameter, if present.
   * Null means "go to the role's home route."
   */
  private readonly returnTo: string | null;

  /** A one-shot success message shown after invitation acceptance. */
  readonly setupComplete = signal(false);

  /** UI state */
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);

  /** Reactive, typed, validated form */
  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  constructor() {
    const returnTo = this.route.snapshot.queryParamMap.get('returnTo');
    this.returnTo = returnTo && returnTo.startsWith('/') ? returnTo : null;

    const setup = this.route.snapshot.queryParamMap.get('setup');
    if (setup === 'complete') {
      this.setupComplete.set(true);
    }
  }

  get email() {
    return this.form.controls.email;
  }
  get password() {
    return this.form.controls.password;
  }

  togglePasswordVisibility(): void {
    this.showPassword.update((v) => !v);
  }

  errorFor(control: 'email' | 'password'): string | null {
    const c = this.form.controls[control];
    if (!c.touched || c.valid) return null;

    if (c.hasError('required')) {
      return control === 'email'
        ? 'Email is required.'
        : 'Password is required.';
    }
    if (c.hasError('email')) {
      return 'Please enter a valid email address.';
    }
    if (c.hasError('minlength')) {
      return 'Password must be at least 8 characters.';
    }
    return 'Invalid value.';
  }

  // ---------- submit ----------

  onSubmit(): void {
    if (this.submitting()) return;

    // Mark all fields as touched so validation errors appear
    // if the user submitted an invalid form.
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      return;
    }

    // Clear any previous error.
    this.errorMessage.set(null);
    this.submitting.set(true);

    const { email, password } = this.form.getRawValue();

    this.auth
      .login({ email, password })
      .pipe(
        // Always run this, whether login succeeded or failed.
        finalize(() => this.submitting.set(false))
      )
      .subscribe({
        next: (session) => {
          // Session is already persisted by AuthService.
          // Navigate to returnTo if present, otherwise the
          // role's home route.
          const target = this.returnTo ?? homeForRole(session.role);
          void this.router.navigateByUrl(target);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.describeError(error));
        },
      });
  }

  /**
   * Turns an HTTP error into a clean, user-facing message.
   * Never reveals which specific field was wrong.
   */
  private describeError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      // Network error or CORS — status is 0.
      if (error.status === 0) {
        return 'Cannot reach the server. Check your connection and try again.';
      }
      // Invalid credentials.
      if (error.status === 401) {
        return 'Invalid email or password.';
      }
      // Server rejected the request for another reason.
      if (error.status >= 400 && error.status < 500) {
        return 'The request was rejected. Please check your details and try again.';
      }
      // Server error.
      if (error.status >= 500) {
        return 'Something went wrong on our side. Please try again in a moment.';
      }
    }
    return 'An unexpected error occurred. Please try again.';
  }
}