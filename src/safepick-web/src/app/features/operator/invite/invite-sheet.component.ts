import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { CreateInvitationRequest } from '@app/core/models';

// ============================================================
// InviteSheetComponent
//
// A bottom sheet for issuing a School Admin invitation.
//
// The Operator provides:
//   - Admin email + display name
//   - School name + contact email
//
// On submit, the parent creates the School + Invitation and
// displays the returned token.
// ============================================================

@Component({
  selector: 'app-invite-sheet',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './invite-sheet.component.html',
  styleUrl: './invite-sheet.component.scss',
})
export class InviteSheetComponent {
  private readonly fb = inject(FormBuilder);

  @Input() submitting = false;
  @Output() submitInvite = new EventEmitter<CreateInvitationRequest>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    displayName: ['', [Validators.required, Validators.minLength(2)]],
    schoolName: ['', [Validators.required, Validators.minLength(2)]],
    schoolContactEmail: ['', [Validators.required, Validators.email]],
  });

  get email()              { return this.form.controls.email; }
  get displayName()        { return this.form.controls.displayName; }
  get schoolName()         { return this.form.controls.schoolName; }
  get schoolContactEmail() { return this.form.controls.schoolContactEmail; }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.submitInvite.emit({
      email: this.email.value.trim().toLowerCase(),
      displayName: this.displayName.value.trim(),
      schoolName: this.schoolName.value.trim(),
      schoolContactEmail: this.schoolContactEmail.value.trim().toLowerCase(),
    });
  }

  onDismiss(): void {
    if (this.submitting) return;
    this.dismiss.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.onDismiss();
  }
}