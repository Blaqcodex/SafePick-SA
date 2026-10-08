import {
  Component,
  EventEmitter,
  Input,
  Output,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { CustodyGap } from '@app/core/models';

// ============================================================
// AcknowledgeSheetComponent
//
// A confirmation sheet for acknowledging an open gap.
//
// Acknowledging marks the gap as seen and moves it from Open
// to Acknowledged — after which the admin can investigate or
// resolve it.
//
// Inputs:
//   gap         — the CustodyGap being acknowledged
//   submitting  — true while the parent is submitting
//
// Outputs:
//   submit  — emits when confirmed
//   dismiss — emits when cancelled
// ============================================================

@Component({
  selector: 'app-acknowledge-sheet',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './acknowledge-sheet.component.html',
  styleUrl: './acknowledge-sheet.component.scss',
})
export class AcknowledgeSheetComponent {
  @Input({ required: true }) gap!: CustodyGap;
  @Input() submitting = false;

  @Output() submit = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();

  onSubmit(): void {
    this.submit.emit();
  }

  onDismiss(): void {
    if (this.submitting) return;
    this.dismiss.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.onDismiss();
  }

  formatWindow(startIso: string, endIso: string): string {
    const start = new Date(startIso);
    const end = new Date(endIso);

    const datePart = start.toLocaleDateString('en-ZA', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    });
    const startTime = start.toLocaleTimeString('en-ZA', {
      hour: '2-digit', minute: '2-digit', hour12: false,
    });
    const endTime = end.toLocaleTimeString('en-ZA', {
      hour: '2-digit', minute: '2-digit', hour12: false,
    });

    return `${datePart} · ${startTime} – ${endTime}`;
  }
}