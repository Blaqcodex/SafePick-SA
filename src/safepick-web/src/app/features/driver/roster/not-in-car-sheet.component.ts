import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
  signal,
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
import type { RosterRow } from '@app/core/models';

// ============================================================
// NotInCarSheetComponent
//
// A bottom sheet for marking a learner as not in the car.
//
// 5.5e — reason capture + submission.
//
// Inputs:
//   row  — the RosterRow to mark
//
// Outputs:
//   submitReason  — emits the chosen reason (and optional note)
//   dismiss       — emits when the user cancels or clicks away
//
// The component is presentational. The parent performs the
// actual API call and closes the sheet on success.
// ============================================================

interface ReasonOption {
  readonly value: string;
  readonly label: string;
  readonly requiresNote: boolean;
}

@Component({
  selector: 'app-not-in-car-sheet',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './not-in-car-sheet.component.html',
  styleUrl: './not-in-car-sheet.component.scss',
})
export class NotInCarSheetComponent {
  private readonly fb = inject(FormBuilder);

  /** The learner being marked. */
  @Input({ required: true }) row!: RosterRow;

  /** True while the parent is submitting. Disables the form. */
  @Input() submitting = false;

  /** Emitted on submit with the final reason string. */
  @Output() submitReason = new EventEmitter<string>();

  /** Emitted when the sheet should close without submitting. */
  @Output() dismiss = new EventEmitter<void>();

  /** The predefined reason options. */
  readonly reasons: readonly ReasonOption[] = [
    { value: 'Parent collected them',        label: 'Parent collected them',    requiresNote: false },
    { value: 'Learner is sick / absent',     label: 'Learner is sick / absent', requiresNote: false },
    { value: 'Learner didn\u2019t show up',  label: 'Learner didn\u2019t show up', requiresNote: false },
    { value: 'Learner went home early',      label: 'Learner went home early',  requiresNote: false },
    { value: 'Other',                        label: 'Other — specify below',   requiresNote: true  },
  ];

  readonly form = this.fb.nonNullable.group({
    reason: ['', Validators.required],
    note: [''],
  });

  get reason() { return this.form.controls.reason; }
  get note()   { return this.form.controls.note; }

  /** Does the currently selected reason require a note? */
  get selectedRequiresNote(): boolean {
    const selected = this.reason.value;
    const option = this.reasons.find((r) => r.value === selected);
    return option?.requiresNote ?? false;
  }

  /**
   * The final reason string that will be sent.
   * For predefined options: the reason itself.
   * For "Other": "Other — {note}".
   */
  private buildFinalReason(): string {
    const selected = this.reason.value.trim();
    const note = this.note.value.trim();

    if (selected === 'Other') {
      return note ? `Other — ${note}` : 'Other';
    }
    if (note) {
      return `${selected} — ${note}`;
    }
    return selected;
  }

  onSubmit(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid) return;
    if (this.selectedRequiresNote && !this.note.value.trim()) {
      this.note.setErrors({ required: true });
      return;
    }

    this.submitReason.emit(this.buildFinalReason());
  }

  onDismiss(): void {
    if (this.submitting) return;
    this.dismiss.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    // Only dismiss if the click was on the backdrop itself,
    // not on the sheet content.
    if (event.target === event.currentTarget) {
      this.onDismiss();
    }
  }
}