import {
  Component,
  OnDestroy,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '@app/core/auth.service';
import { formatRosterDate, formatTime, toApiDate } from '@app/core/date.util';
import { MonitoringService } from './monitoring.service';
import { ReconciliationService } from './reconciliation.service';
import type {
  CustodyStatus,
  MonitoringRow,
  MonitoringSummary,
} from '@app/core/models';

// ============================================================
// MonitoringComponent
//
// The school admin's monitoring board.
//
// 5.8c — summary, tabs, cards.
// 5.8d — live refresh (this revision).
//
// Live refresh:
//   - Polls every 30 seconds while the tab is visible.
//   - Refreshes immediately when the tab becomes visible.
//   - Pauses when the tab is hidden.
//   - Cleans up on destroy.
// ============================================================

type ViewState = 'loading' | 'error' | 'ready';
type FilterTab = 'All' | 'Pending' | 'PickedUp' | 'NotInCar';

interface TabDef {
  readonly value: FilterTab;
  readonly label: string;
}

/** How often to poll while the tab is visible. */
const POLL_INTERVAL_MS = 30_000;

@Component({
  selector: 'app-monitoring',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './monitoring.component.html',
  styleUrl: './monitoring.component.scss',
})
export class MonitoringComponent implements OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly monitoring = inject(MonitoringService);
  private readonly reconciliation = inject(ReconciliationService);

  // ---------- date context ----------

  readonly today = toApiDate();
  readonly displayDate = formatRosterDate(this.today);

  // ---------- tabs ----------

  readonly tabs: readonly TabDef[] = [
    { value: 'All',       label: 'All' },
    { value: 'Pending',   label: 'Pending' },
    { value: 'PickedUp',  label: 'Picked up' },
    { value: 'NotInCar',  label: 'Not in car' },
  ];

  readonly activeTab = signal<FilterTab>('All');

  // ---------- state ----------

  readonly state = signal<ViewState>('loading');
  readonly errorMessage = signal<string | null>(null);
  readonly summary = signal<MonitoringSummary | null>(null);

  /** The time of the last successful refresh. Null until first load. */
  readonly lastUpdated = signal<Date | null>(null);

  /** A ticking signal to force the "Updated Xs ago" label to re-render. */
  private readonly nowTick = signal(Date.now());

  // ---------- Reconciliation trigger ----------

  /** True while a reconciliation run is in flight. */
  readonly runningReconciliation = signal(false);

  /** The result of the last run, shown in a dismissible banner. */
  readonly reconciliationResult = signal<{
    expectationsEvaluated: number;
    resolvedCount: number;
    suppressedCount: number;
    gappedCount: number;
  } | null>(null);

  /** Any error from the last run. */
  readonly reconciliationError = signal<string | null>(null);

  // ---------- polling ----------

  private pollTimer: ReturnType<typeof setInterval> | null = null;
  private visibilityHandler: (() => void) | null = null;

  // ---------- derived ----------

  readonly rows = computed<readonly MonitoringRow[]>(
    () => this.summary()?.rows ?? []
  );

  readonly visibleRows = computed<readonly MonitoringRow[]>(() => {
    const tab = this.activeTab();
    const all = this.rows();
    if (tab === 'All') return all;
    return all.filter((r) => r.status === tab);
  });

  readonly hasRows = computed(() => this.rows().length > 0);
  readonly hasVisibleRows = computed(() => this.visibleRows().length > 0);

  readonly totalExpected = computed(() => this.summary()?.totalExpected ?? 0);
  readonly pickedUp = computed(() => this.summary()?.pickedUp ?? 0);
  readonly notInCar = computed(() => this.summary()?.notInCar ?? 0);
  readonly openGaps = computed(() => this.summary()?.openGaps ?? 0);

  /**
   * A human-friendly "updated Xs ago" string.
   * Recomputed whenever lastUpdated or nowTick changes.
   */
  readonly updatedLabel = computed(() => {
    const t = this.lastUpdated();
    if (!t) return '';
    // Read nowTick to establish a dependency — this computed
    // re-evaluates every time the tick fires.
    void this.nowTick();
    const seconds = Math.floor((Date.now() - t.getTime()) / 1000);
    if (seconds < 5) return 'Updated just now';
    if (seconds < 60) return `Updated ${seconds}s ago`;
    const minutes = Math.floor(seconds / 60);
    if (minutes === 1) return 'Updated 1 minute ago';
    return `Updated ${minutes} minutes ago`;
  });

  // ---------- lifecycle ----------

  ngOnInit(): void {
    void this.loadInitial();
    this.startPolling();
    this.setupVisibilityListener();
    this.startNowTicker();
  }

  ngOnDestroy(): void {
    this.stopPolling();
    this.teardownVisibilityListener();
    this.stopNowTicker();
  }

  // ---------- initial load ----------

  private async loadInitial(): Promise<void> {
    this.state.set('loading');
    this.errorMessage.set(null);

    await this.fetchOnce();

    if (this.summary() !== null) {
      this.state.set('ready');
    } else if (this.errorMessage() !== null) {
      this.state.set('error');
    }
  }

  /**
   * Explicit retry from the error state.
   */
  async load(): Promise<void> {
    await this.loadInitial();
    this.startPolling();
  }

  /**
   * Fetches the current status once. On success, updates summary and
   * lastUpdated; on failure, sets errorMessage but leaves the last
   * known summary in place (so the board doesn't blank out mid-run).
   */
  private async fetchOnce(): Promise<void> {
    const schoolId = this.auth.schoolId();
    if (!schoolId) {
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
      this.lastUpdated.set(new Date());
      this.errorMessage.set(null);
    } catch (error) {
      this.errorMessage.set(this.describeLoadError(error));
      // If we've never loaded successfully, mark as error.
      if (this.summary() === null) {
        this.state.set('error');
      }
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
    return 'Could not load monitoring data. Please try again.';
  }

  // ---------- Reconciliation ----------

  async runReconciliation(): Promise<void> {
    if (this.runningReconciliation()) return;

    const schoolId = this.auth.schoolId();
    if (!schoolId) {
      this.reconciliationError.set(
        'Your session is missing school information.'
      );
      return;
    }

    this.runningReconciliation.set(true);
    this.reconciliationError.set(null);
    this.reconciliationResult.set(null);

    try {
      const result = await firstValueFrom(
        this.reconciliation.run(schoolId, this.today)
      );

      this.reconciliationResult.set({
        expectationsEvaluated: result.expectationsEvaluated,
        resolvedCount: result.resolvedCount,
        suppressedCount: result.suppressedCount,
        gappedCount: result.gappedCount,
      });

      // Refresh the board so any new gaps and updated counts appear.
      await this.load();
    } catch (error) {
      this.reconciliationError.set(this.describeReconciliationError(error));
    } finally {
      this.runningReconciliation.set(false);
    }
  }

  private describeReconciliationError(error: unknown): string {
    if (typeof error === 'object' && error !== null) {
      const status = (error as { status?: number }).status;
      const body = (error as { error?: unknown }).error;

      // Surface the backend's message when available.
      if (typeof body === 'object' && body !== null) {
        const detail = (body as { detail?: string }).detail;
        if (detail) return detail;
        const message = (body as { message?: string }).message;
        if (message) return message;
      }

      if (status === 400) {
        return 'Reconciliation could not run. Check that a transport window is configured for this school.';
      }
      if (status === 0) {
        return 'Cannot reach the server. Check your connection and try again.';
      }
      if (status !== undefined && status >= 500) {
        return 'Something went wrong on our side. Please try again.';
      }
    }
    return 'Could not run reconciliation. Please try again.';
  }

  dismissReconciliationResult(): void {
    this.reconciliationResult.set(null);
  }

  dismissReconciliationError(): void {
    this.reconciliationError.set(null);
  }

  // ---------- polling ----------

  private startPolling(): void {
    this.stopPolling();
    this.pollTimer = setInterval(() => {
      void this.fetchOnce();
    }, POLL_INTERVAL_MS);
  }

  private stopPolling(): void {
    if (this.pollTimer !== null) {
      clearInterval(this.pollTimer);
      this.pollTimer = null;
    }
  }

  // ---------- visibility ----------

  private setupVisibilityListener(): void {
    this.visibilityHandler = () => {
      if (document.visibilityState === 'visible') {
        // Returning to the page: refresh immediately, then resume polling.
        void this.fetchOnce();
        this.startPolling();
      } else {
        // Leaving the page: stop polling.
        this.stopPolling();
      }
    };
    document.addEventListener('visibilitychange', this.visibilityHandler);
  }

  private teardownVisibilityListener(): void {
    if (this.visibilityHandler !== null) {
      document.removeEventListener('visibilitychange', this.visibilityHandler);
      this.visibilityHandler = null;
    }
  }

  // ---------- now ticker ----------

  private nowTicker: ReturnType<typeof setInterval> | null = null;

  /**
   * Ticks once per second so the "Updated Xs ago" label stays fresh.
   * Cheap: just updates one signal; change detection only touches
   * the elements bound to it.
   */
  private startNowTicker(): void {
    this.stopNowTicker();
    this.nowTicker = setInterval(() => {
      this.nowTick.set(Date.now());
    }, 1000);
  }

  private stopNowTicker(): void {
    if (this.nowTicker !== null) {
      clearInterval(this.nowTicker);
      this.nowTicker = null;
    }
  }

  // ---------- tabs ----------

  setTab(tab: FilterTab): void {
    this.activeTab.set(tab);
  }

  // ---------- helpers ----------

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