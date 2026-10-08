import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '@app/core/auth.service';
import { formatDateTime } from '@app/core/date.util';
import { DriverApprovalService } from './driver-approval.service';
import {
  DecisionSheetComponent,
  type DecisionMode,
} from './decision-sheet.component';
import type {
  DriverApproval,
  DriverApprovalStatus,
} from '@app/core/models';

// ============================================================
// ApprovalsComponent
// The school admin's driver approvals board.
// 5.6b — board, filter tabs.
// 5.6c — approve action.
// 5.6d — reject/revoke sheets (this revision).
// ============================================================

type ViewState = 'loading' | 'error' | 'ready';
type FilterTab = 'Pending' | 'Approved' | 'Rejected' | 'All';

interface TabDef {
  readonly value: FilterTab;
  readonly label: string;
}

@Component({
  selector: 'app-approvals',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    DecisionSheetComponent,
  ],
  templateUrl: './approvals.component.html',
  styleUrl: './approvals.component.scss',
})
export class ApprovalsComponent {
  private readonly auth = inject(AuthService);
  private readonly approvalsService = inject(DriverApprovalService);

  readonly tabs: readonly TabDef[] = [
    { value: 'Pending', label: 'Pending' },
    { value: 'Approved', label: 'Approved' },
    { value: 'Rejected', label: 'Rejected' },
    { value: 'All', label: 'All' },
  ];

  readonly activeTab = signal<FilterTab>('Pending');

  readonly state = signal<ViewState>('loading');
  readonly errorMessage = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);

  readonly allApprovals = signal<readonly DriverApproval[]>([]);
  readonly submittingIds = signal<ReadonlySet<string>>(new Set());

  /** Open decision sheet (reject or revoke). Null when closed. */
  readonly sheetApproval = signal<DriverApproval | null>(null);
  readonly sheetMode = signal<DecisionMode | null>(null);
  readonly sheetSubmitting = signal(false);

  // ---------- derived ----------

  readonly visibleApprovals = computed<readonly DriverApproval[]>(() => {
    const tab = this.activeTab();
    const all = this.allApprovals();
    if (tab === 'All') return all;
    return all.filter((a) => a.status === tab);
  });

  readonly pendingCount = computed(
    () => this.allApprovals().filter((a) => a.status === 'Pending').length
  );

  readonly hasVisibleApprovals = computed(
    () => this.visibleApprovals().length > 0
  );

  // ---------- lifecycle ----------

  ngOnInit(): void {
    void this.load();
  }

  // ---------- data loading ----------

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
      const approvals = await firstValueFrom(
        this.approvalsService.getApprovals(schoolId)
      );
      this.allApprovals.set(approvals);
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
    return 'Could not load driver approvals. Please try again.';
  }

  // ---------- approve ----------

  async approve(approval: DriverApproval): Promise<void> {
    if (this.isSubmitting(approval.approvalId)) return;

    this.actionError.set(null);
    this.addSubmitting(approval.approvalId);

    try {
      await firstValueFrom(
        this.approvalsService.approve(approval.approvalId)
      );
      await this.refetch();
    } catch (error) {
      this.actionError.set(
        this.describeActionError(error, 'approve this driver')
      );
    } finally {
      this.removeSubmitting(approval.approvalId);
    }
  }

  // ---------- reject / revoke ----------

  openReject(approval: DriverApproval): void {
    this.actionError.set(null);
    this.sheetApproval.set(approval);
    this.sheetMode.set('reject');
  }

  openRevoke(approval: DriverApproval): void {
    this.actionError.set(null);
    this.sheetApproval.set(approval);
    this.sheetMode.set('revoke');
  }

  closeSheet(): void {
    if (this.sheetSubmitting()) return;
    this.sheetApproval.set(null);
    this.sheetMode.set(null);
  }

  async submitDecision(reason: string): Promise<void> {
    const approval = this.sheetApproval();
    const mode = this.sheetMode();
    if (!approval || !mode || this.sheetSubmitting()) return;

    this.sheetSubmitting.set(true);

    try {
      if (mode === 'reject') {
        await firstValueFrom(
          this.approvalsService.reject(approval.approvalId, reason)
        );
      } else {
        await firstValueFrom(
          this.approvalsService.revoke(approval.approvalId, reason)
        );
      }
      this.sheetApproval.set(null);
      this.sheetMode.set(null);
      await this.refetch();
    } catch (error) {
      const action = mode === 'reject'
        ? 'reject this driver'
        : 'revoke this approval';
      this.sheetApproval.set(null);
      this.sheetMode.set(null);
      this.actionError.set(this.describeActionError(error, action));
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  // ---------- refetch ----------

  private async refetch(): Promise<void> {
    const schoolId = this.auth.schoolId();
    if (!schoolId) return;

    try {
      const approvals = await firstValueFrom(
        this.approvalsService.getApprovals(schoolId)
      );
      this.allApprovals.set(approvals);
    } catch {
      // Silent — the action already succeeded.
    }
  }

  // ---------- submitting helpers ----------

  private addSubmitting(id: string): void {
    this.submittingIds.update((set) => {
      const next = new Set(set);
      next.add(id);
      return next;
    });
  }

  private removeSubmitting(id: string): void {
    this.submittingIds.update((set) => {
      const next = new Set(set);
      next.delete(id);
      return next;
    });
  }

  isSubmitting(id: string): boolean {
    return this.submittingIds().has(id);
  }

  // ---------- tab switching ----------

  setTab(tab: FilterTab): void {
    this.activeTab.set(tab);
  }

  // ---------- helpers for the template ----------

  formatDateTime(iso: string | null): string {
    return formatDateTime(iso);
  }

  statusLabel(status: DriverApprovalStatus): string {
    switch (status) {
      case 'Pending':  return 'Pending';
      case 'Approved': return 'Approved';
      case 'Rejected': return 'Rejected';
      case 'Revoked':  return 'Revoked';
    }
  }

  private describeActionError(error: unknown, action: string): string {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const status = (error as { status: number }).status;
      if (status === 0) {
        return `Cannot reach the server to ${action}. Check your connection and try again.`;
      }
      if (status === 400) {
        return `The server rejected the request to ${action}. It may have already been decided.`;
      }
      if (status >= 500) {
        return `Something went wrong trying to ${action}. Please try again.`;
      }
    }
    return `Could not ${action}. Please try again.`;
  }
}