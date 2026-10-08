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
import type {
  TransportWindow,
  SetTransportWindowRequest,
} from '@app/core/models';

// ============================================================
// TransportWindowSheetComponent
//
// A bottom sheet for setting or editing the school's transport
// window. One window per school — this replaces the existing one.
// ============================================================

@Component({
  selector: 'app-transport-window-sheet',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './transport-window-sheet.component.html',
  styleUrl: './transport-window-sheet.component.scss',
})
export class TransportWindowSheetComponent {
  private readonly fb = inject(FormBuilder);

  @Input() existing: TransportWindow | null = null;
  @Input() submitting = false;

  @Output() submit = new EventEmitter<SetTransportWindowRequest>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    opensAt: ['06:00', [Validators.required]],
    closesAt: ['07:30', [Validators.required]],
    graceMinutes: [15, [Validators.required, Validators.min(0), Validators.max(240)]],
  });

  get opensAt()      { return this.form.controls.opensAt; }
  get closesAt()     { return this.form.controls.closesAt; }
  get graceMinutes() { return this.form.controls.graceMinutes; }

  ngOnInit(): void {
    if (this.existing) {
      // Trim "HH:mm:ss" to "HH:mm" for the time input.
      this.form.patchValue({
        opensAt: this.existing.opensAt.substring(0, 5),
        closesAt: this.existing.closesAt.substring(0, 5),
        graceMinutes: this.existing.graceMinutes,
      });
    }
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.submit.emit({
      opensAt: this.opensAt.value,
      closesAt: this.closesAt.value,
      graceMinutes: Number(this.graceMinutes.value),
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