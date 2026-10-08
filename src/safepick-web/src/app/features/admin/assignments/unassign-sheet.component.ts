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
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { Assignment } from '@app/core/models';

// ============================================================
// UnassignSheetComponent
//
// A confirmation sheet for terminating an assignment.
//
// Inputs:
//   assignment  — the assignment being terminated
//   submitting  — true while the parent is submitting
//
// Outputs:
//   submit  — emits an optional note (may be empty string)
//   dismiss — emits when cancelled
// ============================================================

@Component({
  selector: 'app-unassign-sheet',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './unassign-sheet.component.html',
  styleUrl: './unassign-sheet.component.scss',
})
export class UnassignSheetComponent {
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) assignment!: Assignment;
  @Input() submitting = false;

  @Output() submit = new EventEmitter<string>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    note: [''],
  });

  get note() {
    return this.form.controls.note;
  }

  onSubmit(): void {
    this.submit.emit(this.note.value.trim());
  }

  onDismiss(): void {
    if (this.submitting) return;
    this.dismiss.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.onDismiss();
  }
}