import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '@app/core/auth.service';
import { formatRosterDate, formatTime, toApiDate } from '@app/core/date.util';
import { MonitoringService } from './monitoring/monitoring.service';
import type {
  CustodyStatus,
  MonitoringRow,
  MonitoringSummary,
} from '@app/core/models';

// ============================================================
// AdminComponent
//
// The School Admin's landing dashboard.
//
// Shows:
//   - A greeting and today's date
//   - Today's custody status (four clickable cards)
//   - Quick actions
//   - Recent activity (last few custody events)
//
// Data source: GET /api/custody/status/{schoolId}?date=...
// (reused from the monitoring board — no new endpoint)
//
// Route: /admin
// ============================================================

type ViewState = 'loading' | 'error' | 'ready';

/** The maximum number of recent events shown on the dashboard. */
const RECENT_LIMIT = 5;

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss',
})
export class AdminComponent {
  private readonly auth = inject(AuthService);
  private readonly monitoring = inject(MonitoringService);

  // ---------- context ----------

  readonly today = toApiDate();
  readonly displayDate = formatRosterDate(this.today);
  readonly schoolId = this.auth.schoolId;

  /** Time-based greeting: Good morning / afternoon / evening. */
  readonly greeting = computed(() => this.computeGreeting());

  // ---------- state ----------

  readonly state = signal<ViewState>('loading');
  readonly errorMessage = signal<string | null>(null);
  readonly summary = signal<MonitoringSummary | null>(null);

  // ---------- derived ----------

  readonly totalExpected = computed(() => this.summary()?.totalExpected ?? 0);
  readonly pickedUp = computed(() => this.summary()?.pickedUp ?? 0);
  readonly notInCar = computed(() => this.summary()?.notInCar ?? 0);
  readonly openGaps = computed(() => this.summary()?.openGaps ?? 0);

  readonly hasSummary = computed(() => this.summary() !== null);

  /**
   * The recent activity list — the last few rows, newest first,
   * filtered to only those with a resolved status (picked up /
   * not in car / absent / corrected). Pending rows are excluded
   * because they haven't happened yet.
   */
  readonly recentActivity = computed<readonly MonitoringRow[]>(() => {
    const rows = this.summary()?.rows ?? [];
    return rows
      .filter((r) => r.status !== 'Pending')
      .sort((a, b) => {
        const ta = a.resolvedAt ? Date.parse(a.resolvedAt) : 0;
        const tb = b.resolvedAt ? Date.parse(b.resolvedAt) : 0;
        return tb - ta;
      })
      .slice(0, RECENT_LIMIT);
  });

  readonly hasRecentActivity = computed(
    () => this.recentActivity().length > 0
  );

  // ---------- lifecycle ----------

  ngOnInit(): void {
    void this.load();
  }

  // ---------- data ----------

  async load(): Promise<void> {
    this.state.set('loading');
    this.errorMessage.set(null);

    const schoolId = this.schoolId();
    if (!schoolId) {
      this.state.set('error');
      this.errorMessage.set(
        'Your session is missing school information. Please sign in again.'
      );
      return;
    }

    try {
      const summary = await firstValueFrom(
        this.monitoring.getStatus(schoolId, this.today)
      );
      this.summary.set(summary);
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
    return 'Could not load today’s dashboard. Please try again.';
  }

  // ---------- helpers ----------

  private computeGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  }

  formatTime(iso: string | null): string {
    return formatTime(iso);
  }

  statusLabel(status: CustodyStatus): string {
    switch (status) {
      case 'PickedUp':          return 'Picked up';
      case 'NotInCar':          return 'Not in car';
      case 'SchoolMarkedAbsent':return 'Absent';
      case 'Corrected':         return 'Corrected';
      case 'Gapped':            return 'Gap';
      case 'Pending':           return 'Pending';
    }
  }

  isResolved(status: CustodyStatus): boolean {
    return status === 'PickedUp' || status === 'Corrected';
  }
}