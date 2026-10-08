import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '@app/core/auth.service';
import { formatDisplayDate } from '@app/core/date.util';
import { LearnerService } from '../learners/learner.service';
import { AssignDriverSheetComponent } from './assign-driver-sheet.component';
import { UnassignSheetComponent } from './unassign-sheet.component';
import type { Assignment } from '@app/core/models';

// ============================================================
// AssignmentsComponent
// 5.7d-1 — board.
// 5.7d-2 — reassign.
// 5.7d-3 — unassign (this revision).
// ============================================================

type ViewState = 'loading' | 'error' | 'ready';

@Component({
  selector: 'app-assignments',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    AssignDriverSheetComponent,
    UnassignSheetComponent,
  ],
  templateUrl: './assignments.component.html',
  styleUrl: './assignments.component.scss',
})
export class AssignmentsComponent {
  /** Public — referenced by the template for the schoolId. */
  readonly auth = inject(AuthService);

  private readonly learnerService = inject(LearnerService);

  readonly state = signal<ViewState>('loading');
  readonly errorMessage = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);
  readonly assignments = signal<readonly Assignment[]>([]);

  readonly reassignFor = signal<Assignment | null>(null);
  readonly unassignFor = signal<Assignment | null>(null);
  readonly sheetSubmitting = signal(false);

  readonly count = computed(() => this.assignments().length);
  readonly hasAssignments = computed(() => this.assignments().length > 0);

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    this.state.set('loading');
    this.errorMessage.set(null);

    const schoolId = this.auth.schoolId();
    if (!schoolId) {
      this.state.set('error');
      this.errorMessage.set(
        'Your session is missing school information. Please sign in again.'
      );
      return;
    }

    try {
      const assignments = await firstValueFrom(
        this.learnerService.getAssignments(schoolId)
      );
      this.assignments.set(assignments);
      this.state.set('ready');
    } catch (error) {
      this.state.set('error');
      this.errorMessage.set(this.describeLoadError(error));
    }
  }

  private describeLoadError(error: unknown): string {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const status = (error as { status: number }).status;
      if (status === 0) {
        return 'Cannot reach the server. Check your connection and try again.';
      }
      if (status >= 500) {
        return 'Something went wrong on our side. Please try again in a moment.';
      }
    }
    return 'Could not load assignments. Please try again.';
  }

  private async refetch(): Promise<void> {
    const schoolId = this.auth.schoolId();
    if (!schoolId) return;

    try {
      const assignments = await firstValueFrom(
        this.learnerService.getAssignments(schoolId)
      );
      this.assignments.set(assignments);
    } catch { /* silent */ }
  }

  // ---------- reassign ----------

  openReassign(assignment: Assignment): void {
    this.actionError.set(null);
    this.reassignFor.set(assignment);
  }

  async submitReassign(newDriverId: string): Promise<void> {
    const assignment = this.reassignFor();
    if (!assignment || this.sheetSubmitting()) return;

    this.sheetSubmitting.set(true);
    try {
      await firstValueFrom(
        this.learnerService.reassign(assignment.learnerId, newDriverId, null)
      );
      this.reassignFor.set(null);
      await this.refetch();
    } catch (error) {
      this.reassignFor.set(null);
      this.actionError.set(
        this.describeActionError(error, 'reassign this learner')
      );
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  // ---------- unassign ----------

  openUnassign(assignment: Assignment): void {
    this.actionError.set(null);
    this.unassignFor.set(assignment);
  }

  async submitUnassign(_note: string): Promise<void> {
    const assignment = this.unassignFor();
    if (!assignment || this.sheetSubmitting()) return;

    this.sheetSubmitting.set(true);
    try {
      await firstValueFrom(
        this.learnerService.unassign(assignment.assignmentId, null)
      );
      this.unassignFor.set(null);
      await this.refetch();
    } catch (error) {
      this.unassignFor.set(null);
      this.actionError.set(
        this.describeActionError(error, 'unassign this learner')
      );
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  closeSheet(): void {
    if (this.sheetSubmitting()) return;
    this.reassignFor.set(null);
    this.unassignFor.set(null);
  }

  formatDate(iso: string | null): string {
    if (!iso) return '';
    const date = new Date(iso);
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return formatDisplayDate(`${y}-${m}-${d}`);
  }

  private describeActionError(error: unknown, action: string): string {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const status = (error as { status: number }).status;
      if (status === 0) return `Cannot reach the server to ${action}.`;
      if (status === 400) return `The server rejected the request to ${action}.`;
      if (status >= 500) return `Something went wrong trying to ${action}.`;
    }
    return `Could not ${action}.`;
  }
}