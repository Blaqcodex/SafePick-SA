import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '@app/core/auth.service';
import { formatRosterDate, formatTime, toApiDate } from '@app/core/date.util';
import { DriverRosterService } from './roster.service';
import { NotInCarSheetComponent } from './not-in-car-sheet.component';
import type { CustodyStatus, RosterRow } from '@app/core/models';

// ============================================================
// RosterComponent
//
// The driver's roster for today.
//
// 5.5b — shell, states, and data loading.
// 5.5c — learner cards.
// 5.5d — confirm-pickup action.
// 5.5e — not-in-car bottom sheet.
// ============================================================

type ViewState = 'loading' | 'error' | 'ready';

@Component({
  selector: 'app-roster',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    NotInCarSheetComponent,
  ],
  templateUrl: './roster.component.html',
  styleUrl: './roster.component.scss',
})
export class RosterComponent {
  private readonly auth = inject(AuthService);
  private readonly rosterService = inject(DriverRosterService);

  // ---------- date context ----------

  readonly today = toApiDate();
  readonly displayDate = formatRosterDate(this.today);

  // ---------- view state ----------

  readonly state = signal<ViewState>('loading');
  readonly errorMessage = signal<string | null>(null);
  readonly rows = signal<readonly RosterRow[]>([]);

  readonly submittingIds = signal<ReadonlySet<string>>(new Set());
  readonly actionError = signal<string | null>(null);

  readonly sheetFor = signal<RosterRow | null>(null);
  readonly sheetSubmitting = signal(false);

  // ---------- derived ----------

  readonly vehicleRegistration = computed(
    () => this.rows()[0]?.vehicleRegistration ?? ''
  );
  readonly learnerCount = computed(() => this.rows().length);
  readonly hasRows = computed(() => this.rows().length > 0);

  // ---------- lifecycle ----------

  ngOnInit(): void {
    void this.load();
  }

  // ---------- data loading ----------

  async load(): Promise<void> {
    this.state.set('loading');
    this.errorMessage.set(null);

    const driverId = this.auth.driverId();
    if (!driverId) {
      this.state.set('error');
      this.errorMessage.set(
        'Your session is missing driver information. Please sign in again.'
      );
      return;
    }

    try {
      const rows = await firstValueFrom(
        this.rosterService.getRoster(driverId, this.today)
      );
      this.rows.set(rows);
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
    return 'Could not load today’s roster. Please try again.';
  }

  // ---------- confirm pickup ----------

  async confirmPickup(row: RosterRow): Promise<void> {
    if (this.isSubmitting(row.learnerId)) return;

    const driverId = this.auth.driverId();
    if (!driverId) {
      this.actionError.set('Your session is missing driver information.');
      return;
    }

    this.actionError.set(null);
    this.addSubmitting(row.learnerId);

    try {
      await firstValueFrom(this.rosterService.confirmPickup(row, driverId));
      await this.refetch();
    } catch (error) {
      this.actionError.set(
        this.describeActionError(error, 'confirm this pickup')
      );
    } finally {
      this.removeSubmitting(row.learnerId);
    }
  }

  // ---------- not in car ----------

  openNotInCarSheet(row: RosterRow): void {
    this.actionError.set(null);
    this.sheetFor.set(row);
  }

  closeSheet(): void {
    if (this.sheetSubmitting()) return;
    this.sheetFor.set(null);
  }

  async submitNotInCar(reason: string): Promise<void> {
    const row = this.sheetFor();
    if (!row || this.sheetSubmitting()) return;

    const driverId = this.auth.driverId();
    if (!driverId) {
      this.sheetFor.set(null);
      this.actionError.set('Your session is missing driver information.');
      return;
    }

    this.sheetSubmitting.set(true);

    try {
      await firstValueFrom(
        this.rosterService.markNotInCar(row, driverId, reason)
      );
      this.sheetFor.set(null);
      await this.refetch();
    } catch (error) {
      this.sheetFor.set(null);
      this.actionError.set(
        this.describeActionError(error, 'mark this learner as not in car')
      );
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  // ---------- refetch ----------

  private async refetch(): Promise<void> {
    const driverId = this.auth.driverId();
    if (!driverId) return;

    try {
      const rows = await firstValueFrom(
        this.rosterService.getRoster(driverId, this.today)
      );
      this.rows.set(rows);
    } catch {
      // Refetch failures are silent — the action already succeeded server-side.
    }
  }

  // ---------- submitting helpers ----------

  private addSubmitting(learnerId: string): void {
    this.submittingIds.update((set) => {
      const next = new Set(set);
      next.add(learnerId);
      return next;
    });
  }

  private removeSubmitting(learnerId: string): void {
    this.submittingIds.update((set) => {
      const next = new Set(set);
      next.delete(learnerId);
      return next;
    });
  }

  isSubmitting(learnerId: string): boolean {
    return this.submittingIds().has(learnerId);
  }

  // ---------- helpers ----------

  dismissActionError(): void {
    this.actionError.set(null);
  }

  private describeActionError(error: unknown, action: string): string {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const status = (error as { status: number }).status;
      if (status === 0) {
        return `Cannot reach the server to ${action}. Check your connection and try again.`;
      }
      if (status === 400) {
        return `The server rejected the request to ${action}. It may have already been recorded.`;
      }
      if (status >= 500) {
        return `Something went wrong trying to ${action}. Please try again.`;
      }
    }
    return `Could not ${action}. Please try again.`;
  }

  formatTime(iso: string | null): string {
    return formatTime(iso);
  }

  statusLabel(status: CustodyStatus): string {
    switch (status) {
      case 'PickedUp':          return 'Confirmed';
      case 'NotInCar':          return 'Not in car';
      case 'SchoolMarkedAbsent':return 'Absent';
      case 'Corrected':         return 'Corrected';
      case 'Gapped':            return 'Gap opened';
      case 'Pending':           return 'Pending';
    }
  }
}