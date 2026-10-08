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

// ============================================================
// ParentContactSheetComponent
//
// A bottom sheet for setting the parent contact on a learner.
//
// Inputs:
//   learnerName — the learner's full name (for the subtitle)
//   submitting  — true while the parent is submitting
//
// Outputs:
//   submit  — emits { email, phone, name, consentRecorded }
//   dismiss — emits when the user cancels
//
// POPIA: consent is required. The form cannot submit without it.
// ============================================================

export interface ParentContactPayload {
  email: string;
  phone: string | null;
  name: string | null;
  consentRecorded: boolean;
}

@Component({
  selector: 'app-parent-contact-sheet',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './parent-contact-sheet.component.html',
  styleUrl: './parent-contact-sheet.component.scss',
})
export class ParentContactSheetComponent {
  private readonly fb = inject(FormBuilder);

  @Input() learnerName = '';
  @Input() submitting = false;

  @Output() submit = new EventEmitter<ParentContactPayload>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    name: [''],
    consentRecorded: [false, [Validators.requiredTrue]],
  });

  get email()    { return this.form.controls.email; }
  get phone()    { return this.form.controls.phone; }
  get name()     { return this.form.controls.name; }
  get consent()  { return this.form.controls.consentRecorded; }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.submit.emit({
      email: this.email.value.trim(),
      phone: this.phone.value.trim() || null,
      name: this.name.value.trim() || null,
      consentRecorded: this.consent.value,
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