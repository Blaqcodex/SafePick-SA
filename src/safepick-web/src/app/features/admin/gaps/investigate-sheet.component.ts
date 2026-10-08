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
import type { CustodyGap } from '@app/core/models';

// ============================================================
// InvestigateSheetComponent
//
// A sheet for beginning an investigation on an acknowledged gap.
//
// The note is optional — the backend accepts null. But prompting
// for it encourages the admin to record their intent.
//
// Inputs:
//   gap         — the CustodyGap being investigated
//   submitting  — true while the parent is submitting
//
// Outputs:
//   submit  — emits the note string (may be empty)
//   dismiss — emits when cancelled
// ============================================================

@Component({
  selector: 'app-investigate-sheet',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './investigate-sheet.component.html',
  styleUrl: './investigate-sheet.component.scss',
})
export class InvestigateSheetComponent {
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) gap!: CustodyGap;
  @Input() submitting = false;

  @Output() submit = new EventEmitter<string>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    notes: [''],
  });

  get notes() {
    return this.form.controls.notes;
  }

  onSubmit(): void {
    this.submit.emit(this.notes.value.trim());
  }

  onDismiss(): void {
    if (this.submitting) return;
    this.dismiss.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.onDismiss();
  }
}