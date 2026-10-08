import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '@app/core/auth.service';
import { formatDateTime, formatWindow } from '@app/core/date.util';
import { GapService } from './gap.service';
import { AcknowledgeSheetComponent } from './acknowledge-sheet.component';
import { InvestigateSheetComponent } from './investigate-sheet.component';
import { ResolveSheetComponent } from './resolve-sheet.component';
import type { CustodyGap, GapState } from '@app/core/models';

// ============================================================
// GapsComponent
// 5.9b — the board.
// 5.9c — acknowledge + investigate.
// 5.9d — resolve (this revision).
// ============================================================

type ViewState = 'loading' | 'error' | 'ready';
type FilterTab = 'Open' | 'Acknowledged' | 'Investigating' | 'Resolved';
type ActiveSheet = 'acknowledge' | 'investigate' | 'resolve' | null;

interface TabDef {
  readonly value: FilterTab;
  readonly label: string;
}

@Component({
  selector: 'app-gaps',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    AcknowledgeSheetComponent,
    InvestigateSheetComponent,
    ResolveSheetComponent,
  ],
  templateUrl: './gaps.component.html',
  styleUrl: './gaps.component.scss',
})
export class GapsComponent {
  private readonly auth = inject(AuthService);
  private readonly gapService = inject(GapService);

  readonly tabs: readonly TabDef[] = [
    { value: 'Open',          label: 'Open' },
    { value: 'Acknowledged',  label: 'Acknowledged' },
    { value: 'Investigating', label: 'Investigating' },
    { value: 'Resolved',      label: 'Resolved' },
  ];

  readonly activeTab = signal<FilterTab>('Open');

  readonly state = signal<ViewState>('loading');
  readonly errorMessage = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);
  readonly allGaps = signal<readonly CustodyGap[]>([]);

  readonly activeSheet = signal<ActiveSheet>(null);
  readonly sheetGap = signal<CustodyGap | null>(null);
  readonly sheetSubmitting = signal(false);

  // ---------- derived ----------

  readonly visibleGaps = computed<readonly CustodyGap[]>(() => {
    const tab = this.activeTab();
    const gaps = this.allGaps();

    switch (tab) {
      case 'Open':          return gaps.filter((g) => g.state === 'Open');
      case 'Acknowledged':  return gaps.filter((g) => g.state === 'Acknowledged');
      case 'Investigating': return gaps.filter((g) => g.state === 'Investigating');
      case 'Resolved':
        return gaps.filter((g) => g.state === 'Resolved' || g.state === 'Closed');
    }
  });

  readonly hasVisibleGaps = computed(() => this.visibleGaps().length > 0);
  readonly openCount = computed(
    () => this.allGaps().filter((g) => g.state === 'Open').length
  );

  readonly headerSummary = computed(() => {
    const count = this.allGaps().filter(
      (g) => g.state !== 'Resolved' && g.state !== 'Closed'
    ).length;
    if (count === 0) return 'No unresolved gaps. All clear.';
    if (count === 1) return '1 unresolved gap';
    return `${count} unresolved gaps`;
  });

  // ---------- lifecycle ----------

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
      const gaps = await firstValueFrom(this.gapService.getGaps(schoolId, true));
      this.allGaps.set(gaps);
      this.state.set('ready');
    } catch (error) {
      this.state.set('error');
      this.errorMessage.set(this.describeLoadError(error));
    }
  }

  private describeLoadError(error: unknown): string {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const status = (error as { status: number }).status;
      if (status === 0) return 'Cannot reach the server.';
      if (status >= 500) return 'Something went wrong on our side.';
    }
    return 'Could not load custody gaps. Please try again.';
  }

  private async refetch(): Promise<void> {
    const schoolId = this.auth.schoolId();
    if (!schoolId) return;

    try {
      const gaps = await firstValueFrom(this.gapService.getGaps(schoolId, true));
      this.allGaps.set(gaps);
    } catch { /* silent */ }
  }

  // ---------- acknowledge ----------

  openAcknowledge(gap: CustodyGap): void {
    this.actionError.set(null);
    this.sheetGap.set(gap);
    this.activeSheet.set('acknowledge');
  }

  async submitAcknowledge(): Promise<void> {
    const gap = this.sheetGap();
    if (!gap || this.sheetSubmitting()) return;

    this.sheetSubmitting.set(true);
    try {
      await firstValueFrom(this.gapService.acknowledge(gap.gapId));
      this.closeSheet();
      await this.refetch();
      this.activeTab.set('Acknowledged');
    } catch (error) {
      this.closeSheet();
      this.actionError.set(this.describeActionError(error, 'acknowledge this gap'));
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  // ---------- investigate ----------

  openInvestigate(gap: CustodyGap): void {
    this.actionError.set(null);
    this.sheetGap.set(gap);
    this.activeSheet.set('investigate');
  }

  async submitInvestigate(notes: string): Promise<void> {
    const gap = this.sheetGap();
    if (!gap || this.sheetSubmitting()) return;

    this.sheetSubmitting.set(true);
    try {
      await firstValueFrom(
        this.gapService.investigate(gap.gapId, notes || null)
      );
      this.closeSheet();
      await this.refetch();
      this.activeTab.set('Investigating');
    } catch (error) {
      this.closeSheet();
      this.actionError.set(this.describeActionError(error, 'begin investigating this gap'));
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  // ---------- resolve ----------

  openResolve(gap: CustodyGap): void {
    this.actionError.set(null);
    this.sheetGap.set(gap);
    this.activeSheet.set('resolve');
  }

  async submitResolve(resolution: string): Promise<void> {
    const gap = this.sheetGap();
    if (!gap || this.sheetSubmitting()) return;

    this.sheetSubmitting.set(true);
    try {
      await firstValueFrom(this.gapService.resolve(gap.gapId, resolution));
      this.closeSheet();
      await this.refetch();
      this.activeTab.set('Resolved');
    } catch (error) {
      this.closeSheet();
      this.actionError.set(this.describeActionError(error, 'resolve this gap'));
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  // ---------- sheet closing ----------

  closeSheet(): void {
    if (this.sheetSubmitting()) return;
    this.activeSheet.set(null);
    this.sheetGap.set(null);
  }

  // ---------- tabs ----------

  setTab(tab: FilterTab): void {
    this.activeTab.set(tab);
  }

  // ---------- helpers ----------

  formatDateTime(iso: string | null): string {
    return formatDateTime(iso);
  }

  formatWindow(start: string, end: string): string {
    return formatWindow(start, end);
  }

  stateLabel(state: GapState): string {
    switch (state) {
      case 'Open':          return 'Open';
      case 'Acknowledged':  return 'Acknowledged';
      case 'Investigating': return 'Investigating';
      case 'Resolved':      return 'Resolved';
      case 'Closed':        return 'Closed';
    }
  }

  private describeActionError(error: unknown, action: string): string {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const status = (error as { status: number }).status;
      if (status === 0) return `Cannot reach the server to ${action}.`;
      if (status === 400) return `The server rejected the request to ${action}. It may already have been moved.`;
      if (status >= 500) return `Something went wrong trying to ${action}.`;
    }
    return `Could not ${action}.`;
  }
}