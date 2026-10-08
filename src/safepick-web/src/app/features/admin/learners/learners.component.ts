import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '@app/core/auth.service';
import { LearnerService } from './learner.service';
import {
  AddLearnerSheetComponent,
  type AddLearnerPayload,
} from './add-learner-sheet.component';
import {
  ParentContactSheetComponent,
  type ParentContactPayload,
} from './parent-contact-sheet.component';
import { AssignDriverSheetComponent } from '../assignments/assign-driver-sheet.component';
import { TransportWindowService } from './transport-window.service';
import { TransportWindowSheetComponent } from './transport-window-sheet.component';
import type {
  Learner,
  TransportWindow,
  SetTransportWindowRequest,
} from '@app/core/models';

// ============================================================
// LearnersComponent
//
// The school admin's learner management board.
//
// 5.7b — board, filter tabs, search, status pills.
// 5.7c — add learner + parent contact sheets.
// 5.7d-4 — assign-to-driver action (this revision).
//
// Route: /admin/learners
// ============================================================

type ViewState = 'loading' | 'error' | 'ready';
type FilterTab = 'All' | 'Assigned' | 'Unassigned' | 'NoContact';
type LearnerDisplayState = 'Assigned' | 'Unassigned' | 'NoContact' | 'Inactive';

interface TabDef {
  readonly value: FilterTab;
  readonly label: string;
}

interface LearnerRow {
  readonly learner: Learner;
  readonly displayState: LearnerDisplayState;
  readonly displayLabel: string;
  readonly classLabel: string;
}

/** The sheets this component can show. */
type ActiveSheet = 'add' | 'contact' | 'assign' | 'transport-window' | null;

@Component({
  selector: 'app-learners',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    AddLearnerSheetComponent,
    ParentContactSheetComponent,
    AssignDriverSheetComponent,
    TransportWindowSheetComponent,
  ],
  templateUrl: './learners.component.html',
  styleUrl: './learners.component.scss',
})
export class LearnersComponent {
  /** Public — referenced by the template for the schoolId. */
  readonly auth = inject(AuthService);

  private readonly learnerService = inject(LearnerService);
  private readonly transportWindowService = inject(TransportWindowService);

  // ---------- tabs ----------

  readonly tabs: readonly TabDef[] = [
    { value: 'All', label: 'All' },
    { value: 'Assigned', label: 'Assigned' },
    { value: 'Unassigned', label: 'Unassigned' },
    { value: 'NoContact', label: 'No contact' },
  ];

  readonly activeTab = signal<FilterTab>('All');
  readonly searchTerm = signal('');

  private readonly debouncedSearch = signal('');
  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  // ---------- state ----------

  readonly state = signal<ViewState>('loading');
  readonly errorMessage = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);
  readonly allLearners = signal<readonly Learner[]>([]);

  // ---------- sheet state ----------

  readonly activeSheet = signal<ActiveSheet>(null);
  readonly sheetSubmitting = signal(false);

  /** The learner the contact sheet is targeting. */
  readonly contactSheetLearner = signal<Learner | null>(null);

  /** The learner the assign sheet is targeting. */
  readonly assignSheetLearner = signal<Learner | null>(null);

  // ---------- transport window state ----------

  readonly transportWindow = signal<TransportWindow | null>(null);
  readonly transportWindowLoading = signal(true);
  readonly transportWindowSheetOpen = signal(false);
  readonly transportWindowSubmitting = signal(false);

  // ---------- derived ----------

  private readonly allRows = computed<readonly LearnerRow[]>(() => {
    const severityRank: Record<LearnerDisplayState, number> = {
      NoContact: 0,
      Unassigned: 1,
      Assigned: 2,
      Inactive: 3,
    };

    return this.allLearners()
      .map((l) => this.rowOf(l))
      .sort((a, b) => {
        const sa = severityRank[a.displayState];
        const sb = severityRank[b.displayState];
        if (sa !== sb) return sa - sb;
        return a.learner.fullName.localeCompare(b.learner.fullName);
      });
  });

  readonly visibleLearners = computed<readonly LearnerRow[]>(() => {
    const tab = this.activeTab();
    const term = this.debouncedSearch().trim().toLowerCase();

    let rows = this.allRows();

    if (tab !== 'All') {
      rows = rows.filter((r) => this.tabMatches(tab, r.displayState));
    }

    if (term.length > 0) {
      rows = rows.filter((r) => {
        const name = r.learner.fullName.toLowerCase();
        const cls = r.classLabel.toLowerCase();
        return name.includes(term) || cls.includes(term);
      });
    }

    return rows;
  });

  readonly hasVisibleLearners = computed(
    () => this.visibleLearners().length > 0
  );

  readonly totalCount = computed(() => this.allLearners().length);

  readonly isSearching = computed(
    () => this.debouncedSearch().trim().length > 0
  );

  // ---------- lifecycle ----------

  ngOnInit(): void {
    void this.load();
    void this.loadTransportWindow();
  }

  // ---------- data ----------

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
      const learners = await firstValueFrom(
        this.learnerService.getLearners(schoolId)
      );
      this.allLearners.set(learners);
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
    return 'Could not load learners. Please try again.';
  }

  private async refetch(): Promise<void> {
    const schoolId = this.auth.schoolId();
    if (!schoolId) return;

    try {
      const learners = await firstValueFrom(
        this.learnerService.getLearners(schoolId)
      );
      this.allLearners.set(learners);
    } catch {
      // Silent — the action already succeeded.
    }
  }

  // ---------- Add learner ----------

  openAddSheet(): void {
    this.actionError.set(null);
    this.activeSheet.set('add');
  }

  async submitAddLearner(payload: AddLearnerPayload): Promise<void> {
    if (this.sheetSubmitting()) return;

    const schoolId = this.auth.schoolId();
    if (!schoolId) {
      this.actionError.set('Your session is missing school information.');
      return;
    }

    this.sheetSubmitting.set(true);

    try {
      const learnerId = await firstValueFrom(
        this.learnerService.addLearner({
          schoolId,
          fullName: payload.fullName,
          grade: payload.grade,
          classLetter: payload.classLetter,
        })
      );

      await this.refetch();

      // Chain into the contact sheet for the newly created learner.
      this.contactSheetLearner.set({
        learnerId,
        schoolId,
        fullName: payload.fullName,
        gradeClass: { grade: payload.grade, letter: payload.classLetter },
        parentContact: null,
        isActive: true,
        currentAssignmentId: null,
        currentDriverId: null,
        currentDriverName: null,
        currentVehicleRegistration: null,
      });

      this.activeSheet.set('contact');
    } catch (error) {
      this.actionError.set(
        this.describeActionError(error, 'create this learner')
      );
      this.activeSheet.set(null);
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  // ---------- Parent contact ----------

  openContactSheet(learner: Learner): void {
    this.actionError.set(null);
    this.contactSheetLearner.set(learner);
    this.activeSheet.set('contact');
  }

  async submitParentContact(payload: ParentContactPayload): Promise<void> {
    if (this.sheetSubmitting()) return;

    const learner = this.contactSheetLearner();
    if (!learner) return;

    this.sheetSubmitting.set(true);

    try {
      await firstValueFrom(
        this.learnerService.setParentContact(learner.learnerId, {
          email: payload.email,
          phone: payload.phone,
          name: payload.name,
          consentRecorded: payload.consentRecorded,
        })
      );

      await this.refetch();
      this.closeSheet();
    } catch (error) {
      this.actionError.set(
        this.describeActionError(error, 'save the parent contact')
      );
      this.closeSheet();
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  // ---------- Assign to driver ----------

  openAssignSheet(learner: Learner): void {
    this.actionError.set(null);
    this.assignSheetLearner.set(learner);
    this.activeSheet.set('assign');
  }

  async submitAssign(driverId: string): Promise<void> {
    if (this.sheetSubmitting()) return;

    const learner = this.assignSheetLearner();
    if (!learner) return;

    this.sheetSubmitting.set(true);

    try {
      await firstValueFrom(
        this.learnerService.assign({
          learnerId: learner.learnerId,
          driverId,
          validFrom: null,
        })
      );

      await this.refetch();
      this.closeSheet();
    } catch (error) {
      this.actionError.set(
        this.describeActionError(error, 'assign this learner')
      );
      this.closeSheet();
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  // ---------- transport window ----------

  async loadTransportWindow(): Promise<void> {
    this.transportWindowLoading.set(true);
    const schoolId = this.auth.schoolId();
    if (!schoolId) {
      this.transportWindowLoading.set(false);
      return;
    }

    try {
      const window = await firstValueFrom(
        this.transportWindowService.getWindow(schoolId)
      );
      this.transportWindow.set(window);
    } catch {
      // Silent — the card shows the "not configured" state on error too.
      this.transportWindow.set(null);
    } finally {
      this.transportWindowLoading.set(false);
    }
  }

  openTransportWindowSheet(): void {
    this.actionError.set(null);
    this.activeSheet.set('transport-window');
  }

  async submitTransportWindow(request: SetTransportWindowRequest): Promise<void> {
    if (this.transportWindowSubmitting()) return;

    const schoolId = this.auth.schoolId();
    if (!schoolId) {
      this.actionError.set('Your session is missing school information.');
      this.activeSheet.set(null);
      return;
    }

    this.transportWindowSubmitting.set(true);

    try {
      const result = await firstValueFrom(
        this.transportWindowService.setWindow(schoolId, request)
      );
      this.transportWindow.set({
        opensAt: result.opensAt,
        closesAt: result.closesAt,
        graceMinutes: result.graceMinutes,
        reconciliationAt: result.reconciliationAt,
      });
      this.activeSheet.set(null);
    } catch (error) {
      this.actionError.set(
        this.describeActionError(error, 'save the transport window')
      );
      this.activeSheet.set(null);
    } finally {
      this.transportWindowSubmitting.set(false);
    }
  }

  /** Formats "HH:mm:ss" as "HH:mm". */
  formatTimeShort(time: string): string {
    return time.substring(0, 5);
  }

  // ---------- sheet closing ----------

  closeSheet(): void {
    if (this.sheetSubmitting()) return;
    this.activeSheet.set(null);
    this.contactSheetLearner.set(null);
    this.assignSheetLearner.set(null);
  }

  // ---------- search ----------

  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    if (this.searchTimer !== null) clearTimeout(this.searchTimer);

    this.searchTimer = setTimeout(() => {
      this.debouncedSearch.set(value);
      this.searchTimer = null;
    }, 200);
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.debouncedSearch.set('');
  }

  // ---------- tabs ----------

  setTab(tab: FilterTab): void {
    this.activeTab.set(tab);
  }

  // ---------- helpers ----------

  /** Public — the template uses this when opening the assign sheet. */
  classLabel(learner: Learner): string {
    return `${learner.gradeClass.grade}${learner.gradeClass.letter}`;
  }

  private rowOf(learner: Learner): LearnerRow {
    const displayState = this.displayStateOf(learner);
    return {
      learner,
      displayState,
      displayLabel: this.labelOf(displayState),
      classLabel: this.classLabel(learner),
    };
  }

  private displayStateOf(learner: Learner): LearnerDisplayState {
    if (!learner.isActive) return 'Inactive';
    if (!learner.parentContact) return 'NoContact';
    if (learner.currentAssignmentId) return 'Assigned';
    return 'Unassigned';
  }

  private labelOf(state: LearnerDisplayState): string {
    switch (state) {
      case 'Assigned':   return 'Assigned';
      case 'Unassigned': return 'Unassigned';
      case 'NoContact':  return 'No contact';
      case 'Inactive':   return 'Inactive';
    }
  }

  private tabMatches(tab: FilterTab, state: LearnerDisplayState): boolean {
    switch (tab) {
      case 'All':         return true;
      case 'Assigned':    return state === 'Assigned';
      case 'Unassigned':  return state === 'Unassigned';
      case 'NoContact':   return state === 'NoContact';
    }
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