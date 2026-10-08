import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
  signal,
  OnInit,
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
import { firstValueFrom } from 'rxjs';
import { DriverApprovalService } from '../approvals/driver-approval.service';
import type { DriverApproval } from '@app/core/models';

// ============================================================
// AssignDriverSheetComponent
//
// A bottom sheet for assigning or reassigning a learner to a
// driver's vehicle.
//
// Two modes:
//   'assign'   — learner has no current assignment
//   'reassign' — learner has an existing assignment; the
//                current driver is excluded from the picker
//
// Inputs:
//   schoolId          — the school whose approved drivers to list
//   learnerName       — for the sheet subtitle
//   learnerClassGroup — e.g. "10A"
//   mode              — 'assign' | 'reassign'
//   currentDriverId   — excluded from the picker when reassigning
//   submitting        — true while the parent is submitting
//
// Outputs:
//   submit  — emits the chosen driverId
//   dismiss — emits when cancelled
// ============================================================

export type AssignMode = 'assign' | 'reassign';

@Component({
  selector: 'app-assign-driver-sheet',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './assign-driver-sheet.component.html',
  styleUrl: './assign-driver-sheet.component.scss',
})
export class AssignDriverSheetComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly approvalsService = inject(DriverApprovalService);

  @Input({ required: true }) schoolId!: string;
  @Input({ required: true }) learnerName!: string;
  @Input() learnerClassGroup = '';
  @Input() mode: AssignMode = 'assign';
  @Input() currentDriverId: string | null = null;
  @Input() submitting = false;

  @Output() submit = new EventEmitter<string>();
  @Output() dismiss = new EventEmitter<void>();

  // ---------- driver list state ----------

  readonly loadingDrivers = signal(true);
  readonly driversError = signal<string | null>(null);
  readonly drivers = signal<readonly DriverApproval[]>([]);

  // ---------- form ----------

  readonly form = this.fb.nonNullable.group({
    driverId: ['', Validators.required],
  });

  get driverId() {
    return this.form.controls.driverId;
  }

  // ---------- mode labels ----------

  get title(): string {
    return this.mode === 'assign' ? 'Assign learner' : 'Reassign learner';
  }

  get subtitle(): string {
    return this.learnerClassGroup
      ? `${this.learnerName} · ${this.learnerClassGroup}`
      : this.learnerName;
  }

  get confirmLabel(): string {
    return this.mode === 'assign' ? 'Assign to driver' : 'Reassign to driver';
  }

  // ---------- lifecycle ----------

  ngOnInit(): void {
    void this.loadDrivers();
  }

  async loadDrivers(): Promise<void> {
    this.loadingDrivers.set(true);
    this.driversError.set(null);

    try {
      const approvals = await firstValueFrom(
        this.approvalsService.getApprovals(this.schoolId, 'Approved')
      );

      // Exclude the current driver when reassigning.
      const filtered = this.currentDriverId
        ? approvals.filter((a) => a.driverId !== this.currentDriverId)
        : approvals;

      this.drivers.set(filtered);
    } catch (error) {
      this.driversError.set('Could not load approved drivers.');
    } finally {
      this.loadingDrivers.set(false);
    }
  }

  // ---------- actions ----------

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.submit.emit(this.driverId.value);
  }

  onDismiss(): void {
    if (this.submitting) return;
    this.dismiss.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.onDismiss();
  }
}