import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { InvitationService } from '../../operator/invite/invitation.service';
import { formatDateTime } from '@app/core/date.util';
import type { ValidateInvitationResult } from '@app/core/models';

// ============================================================
// AcceptInvitationComponent
//
// The invitee's onboarding page.
//
// Flow:
//   1. Read ?token= from the URL.
//   2. Validate the token (GET /api/invitations/validate).
//   3. If valid, show a password-setting form.
//   4. On submit (POST /api/invitations/accept), create the User
//      and redirect to /login?setup=complete.
//
// This page is intentionally anonymous — the invitee has no
// account yet.
//
// Route: /accept-invitation?token=...
// ============================================================

type ViewState = 'loading' | 'invalid' | 'ready';
type PasswordMatch = 'ok' | 'mismatch' | 'empty';

@Component({
  selector: 'app-accept-invitation',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './accept-invitation.component.html',
  styleUrl: './accept-invitation.component.scss',
})
export class AcceptInvitationComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly invitationService = inject(InvitationService);

  private readonly token: string;

  // ---------- state ----------

  readonly state = signal<ViewState>('loading');
  readonly invite = signal<ValidateInvitationResult | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly submitting = signal(false);
  readonly showPassword = signal(false);

  // ---------- form ----------

  readonly form = this.fb.nonNullable.group({
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]],
  });

  get password() { return this.form.controls.password; }
  get confirmPassword() { return this.form.controls.confirmPassword; }

  constructor() {
    const token = this.route.snapshot.queryParamMap.get('token');
    this.token = token ?? '';
  }

  // ---------- lifecycle ----------

  ngOnInit(): void {
    void this.validate();
  }

  // ---------- validation ----------

  async validate(): Promise<void> {
    if (!this.token) {
      this.state.set('invalid');
      this.errorMessage.set(
        'No invitation token was provided. Please use the link from your invitation email.'
      );
      return;
    }

    this.state.set('loading');
    this.errorMessage.set(null);

    try {
      const result = await firstValueFrom(
        this.invitationService.validateInvitation(this.token)
      );
      this.invite.set(result);
      this.state.set('ready');
    } catch (error) {
      this.state.set('invalid');
      this.errorMessage.set(this.describeValidateError(error));
    }
  }

  private describeValidateError(error: unknown): string {
    if (typeof error === 'object' && error !== null) {
      const status = (error as { status?: number }).status;
      const body = (error as { error?: unknown }).error;

      // Try to surface the backend's message if present.
      if (typeof body === 'object' && body !== null) {
        const detail = (body as { detail?: string }).detail;
        if (detail) return detail;
      }

      if (status === 404) {
        return 'This invitation link is not valid. It may have been revoked. Please ask the Operator for a new invitation.';
      }
      if (status === 400) {
        return 'This invitation is no longer valid. It may have expired or already been used.';
      }
      if (status === 0) {
        return 'Cannot reach the server. Check your connection and try again.';
      }
    }
    return 'Could not validate your invitation. Please try again.';
  }

  // ---------- submit ----------

  async onSubmit(): Promise<void> {
    if (this.submitting()) return;

    this.form.markAllAsTouched();
    this.errorMessage.set(null);

    if (this.form.invalid) return;

    if (this.password.value !== this.confirmPassword.value) {
      this.confirmPassword.setErrors({ mismatch: true });
      return;
    }

    this.submitting.set(true);

    try {
      await firstValueFrom(
        this.invitationService.acceptInvitation({
          token: this.token,
          password: this.password.value,
        })
      );

      // Success — send them to login with a success flag.
      void this.router.navigate(['/login'], {
        queryParams: { setup: 'complete' },
      });
    } catch (error) {
      this.errorMessage.set(this.describeAcceptError(error));
      this.submitting.set(false);
    }
  }

  private describeAcceptError(error: unknown): string {
    if (typeof error === 'object' && error !== null) {
      const status = (error as { status?: number }).status;
      const body = (error as { error?: unknown }).error;

      if (typeof body === 'object' && body !== null) {
        const detail = (body as { detail?: string }).detail;
        if (detail) return detail;
      }

      if (status === 400) {
        return 'This invitation is no longer valid. It may have expired or already been used.';
      }
      if (status === 0) {
        return 'Cannot reach the server. Check your connection and try again.';
      }
      if (status !== undefined && status >= 500) {
        return 'Something went wrong on our side. Please try again.';
      }
    }
    return 'Could not create your account. Please try again.';
  }

  // ---------- helpers ----------

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  get passwordsMismatch(): boolean {
    return this.confirmPassword.touched &&
           this.password.valid &&
           this.confirmPassword.valid &&
           this.password.value !== this.confirmPassword.value;
  }

  formatExpiry(iso: string): string {
    return formatDateTime(iso);
  }
}