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
import type { CustodyGap } from '@app/core/models';

// ============================================================
// ResolveSheetComponent
//
// A sheet for resolving a gap.
//
// The resolution is REQUIRED — a minimum of 5 characters. The
// backend also rejects empty resolutions. This ensures that no
// gap is closed without a recorded explanation.
//
// Inputs:
//   gap         — the CustodyGap being resolved
//   submitting  — true while the parent is submitting
//
// Outputs:
//   submit  — emits the resolution string
//   dismiss — emits when cancelled
// ============================================================

@Component({
  selector: 'app-resolve-sheet',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './resolve-sheet.component.html',
  styleUrl: './resolve-sheet.component.scss',
})
export class ResolveSheetComponent {
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) gap!: CustodyGap;
  @Input() submitting = false;

  @Output() submit = new EventEmitter<string>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    resolution: ['', [Validators.required, Validators.minLength(5)]],
  });

  get resolution() {
    return this.form.controls.resolution;
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.submit.emit(this.resolution.value.trim());
  }

  onDismiss(): void {
    if (this.submitting) return;
    this.dismiss.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.onDismiss();
  }
}