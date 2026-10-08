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
import type { DriverApproval } from '@app/core/models';

// ============================================================
// DecisionSheetComponent
//
// A bottom sheet for reject and revoke decisions on a driver
// approval. Both actions require a reason.
//
// Inputs:
//   approval    — the DriverApproval being decided
//   mode        — 'reject' | 'revoke'
//   submitting  — true while the parent is submitting
//
// Outputs:
//   submitReason — emits the reason string
//   dismiss      — emits when the user cancels
// ============================================================

export type DecisionMode = 'reject' | 'revoke';

@Component({
  selector: 'app-decision-sheet',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './decision-sheet.component.html',
  styleUrl: './decision-sheet.component.scss',
})
export class DecisionSheetComponent {
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) approval!: DriverApproval;
  @Input({ required: true }) mode!: DecisionMode;
  @Input() submitting = false;

  @Output() submitReason = new EventEmitter<string>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    reason: ['', [Validators.required, Validators.minLength(3)]],
  });

  get reason() {
    return this.form.controls.reason;
  }

  // ---------- mode-dependent labels ----------

  get title(): string {
    return this.mode === 'reject'
      ? 'Reject driver registration'
      : 'Revoke driver approval';
  }

  get question(): string {
    return this.mode === 'reject'
      ? 'Why is this driver being rejected?'
      : 'Why is this approval being revoked?';
  }

  get confirmLabel(): string {
    return this.mode === 'reject' ? 'Reject driver' : 'Revoke approval';
  }

  get placeholder(): string {
    return this.mode === 'reject'
      ? 'Explain why this driver is not suitable for your school'
      : 'Explain why this driver is no longer approved';
  }

  get vehicleLine(): string {
    const v = this.approval.vehicle;
    return `${v.registration} · ${v.make} ${v.model}`;
  }

  // ---------- actions ----------

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.submitReason.emit(this.reason.value.trim());
  }

  onDismiss(): void {
    if (this.submitting) return;
    this.dismiss.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.onDismiss();
    }
  }
}