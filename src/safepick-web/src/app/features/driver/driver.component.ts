import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '@app/core/auth.service';
import { MonitoringService } from '../admin/monitoring/monitoring.service';
import { DriverService } from './driver.service';
import { formatRosterDate, toApiDate } from '@app/core/date.util';
import type {
  CurrentDriver,
  MonitoringSummary,
} from '@app/core/models';

// ============================================================
// DriverComponent
//
// The driver's landing dashboard.
//
// Two states:
//   - Awaiting approval: shows a waiting screen
//   - Approved: shows today's roster summary and a big CTA
//
// Data source:
//   GET /api/drivers/me  (own details + approval status)
//   GET /api/custody/status/{schoolId}?date=...  (today's counts)
//
// Route: /driver
// ============================================================

type ViewState = 'loading' | 'error' | 'ready';

@Component({
  selector: 'app-driver',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './driver.component.html',
  styleUrl: './driver.component.scss',
})
export class DriverComponent {
  private readonly auth = inject(AuthService);
  private readonly driverService = inject(DriverService);
  private readonly monitoring = inject(MonitoringService);

  readonly today = toApiDate();
  readonly displayDate = formatRosterDate(this.today);

  readonly greeting = computed(() => this.computeGreeting());

  // ---------- state ----------

  readonly state = signal<ViewState>('loading');
  readonly errorMessage = signal<string | null>(null);

  readonly driver = signal<CurrentDriver | null>(null);
  readonly summary = signal<MonitoringSummary | null>(null);

  // ---------- derived ----------

  readonly isApproved = computed(
    () => this.driver()?.approvalStatus === 'Approved'
  );

  readonly isAwaitingApproval = computed(
    () => this.driver()?.approvalStatus === 'Pending'
  );

  readonly isRejected = computed(
    () =>
      this.driver()?.approvalStatus === 'Rejected' ||
      this.driver()?.approvalStatus === 'Revoked'
  );

  /** Count of learners on the driver's roster today. */
  readonly expected = computed(() => this.summary()?.totalExpected ?? 0);

  /** Count of learners already picked up today. */
  readonly pickedUp = computed(() => this.summary()?.pickedUp ?? 0);

  /** Count of learners marked not in car today. */
  readonly notInCar = computed(() => this.summary()?.notInCar ?? 0);

  /** Count of learners still pending today. */
  readonly pending = computed(() => this.summary()?.pending ?? 0);

  // ---------- lifecycle ----------

  ngOnInit(): void {
    void this.load();
  }

  // ---------- data ----------

  async load(): Promise<void> {
    this.state.set('loading');
    this.errorMessage.set(null);

    try {
      const driver = await firstValueFrom(this.driverService.getMe());
      this.driver.set(driver);

      // If approved, also load today's summary for the counts.
      if (driver.approvalStatus === 'Approved' && driver.schoolId) {
        try {
          const summary = await firstValueFrom(
            this.monitoring.getStatus(driver.schoolId, this.today)
          );
          this.summary.set(summary);
        } catch {
          // Summary failure is non-fatal — we still show the driver's
          // own info and a working roster button.
        }
      }

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
    return 'Could not load your profile. Please try again.';
  }

  // ---------- helpers ----------

  private computeGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  }
}