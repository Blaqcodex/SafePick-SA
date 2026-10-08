import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { formatDateTime } from '@app/core/date.util';
import { InvitationService } from './invite/invitation.service';
import { SchoolService } from './invite/school.service';
import { InviteSheetComponent } from './invite/invite-sheet.component';
import { InviteResultComponent } from './invite/invite-result.component';
import type {
  Invitation,
  InvitationStatus,
  School,
  CreateInvitationRequest,
  CreateInvitationResult,
} from '@app/core/models';

// ============================================================
// OperatorComponent
// 5.10c — board, tabs.
// 5.10d — invite sheet + result panel (this revision).
// ============================================================

type ViewState = 'loading' | 'error' | 'ready';
type OperatorTab = 'Invitations' | 'Schools';

interface TabDef {
  readonly value: OperatorTab;
  readonly label: string;
}

@Component({
  selector: 'app-operator',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    InviteSheetComponent,
    InviteResultComponent,
  ],
  templateUrl: './operator.component.html',
  styleUrl: './operator.component.scss',
})
export class OperatorComponent {
  private readonly invitationService = inject(InvitationService);
  private readonly schoolService = inject(SchoolService);

  readonly tabs: readonly TabDef[] = [
    { value: 'Invitations', label: 'Invitations' },
    { value: 'Schools', label: 'Schools' },
  ];

  readonly activeTab = signal<OperatorTab>('Invitations');

  readonly state = signal<ViewState>('loading');
  readonly errorMessage = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);

  readonly invitations = signal<readonly Invitation[]>([]);
  readonly schools = signal<readonly School[]>([]);

  // ---------- sheet + result ----------

  readonly inviteSheetOpen = signal(false);
  readonly sheetSubmitting = signal(false);

  /**
   * When set, the board is replaced by the result panel.
   * Cleared by the Done button.
   */
  readonly createdResult = signal<CreateInvitationResult | null>(null);

  // ---------- derived ----------

  readonly hasInvitations = computed(() => this.invitations().length > 0);
  readonly hasSchools = computed(() => this.schools().length > 0);
  readonly invitationCount = computed(() => this.invitations().length);
  readonly schoolCount = computed(() => this.schools().length);

  /** True when the result panel is showing. */
  readonly showingResult = computed(() => this.createdResult() !== null);

  // ---------- lifecycle ----------

  ngOnInit(): void {
    void this.load();
  }

  // ---------- data ----------

  async load(): Promise<void> {
    this.state.set('loading');
    this.errorMessage.set(null);

    try {
      const [invitations, schools] = await Promise.all([
        firstValueFrom(this.invitationService.getInvitations()),
        firstValueFrom(this.schoolService.getSchools()),
      ]);

      this.invitations.set(invitations);
      this.schools.set(schools);
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
    return 'Could not load data. Please try again.';
  }

  // ---------- invite flow ----------

  openInviteSheet(): void {
    this.actionError.set(null);
    this.inviteSheetOpen.set(true);
  }

  closeInviteSheet(): void {
    if (this.sheetSubmitting()) return;
    this.inviteSheetOpen.set(false);
  }

  async submitInvite(request: CreateInvitationRequest): Promise<void> {
    if (this.sheetSubmitting()) return;

    this.sheetSubmitting.set(true);
    this.actionError.set(null);

    try {
      const result = await firstValueFrom(
        this.invitationService.createInvitation(request)
      );

      this.inviteSheetOpen.set(false);
      this.createdResult.set(result);

      // Refresh the underlying lists so when we return, they're current.
      await this.refreshLists();
    } catch (error) {
      this.inviteSheetOpen.set(false);
      this.actionError.set(this.describeActionError(error, 'create this invitation'));
    } finally {
      this.sheetSubmitting.set(false);
    }
  }

  private async refreshLists(): Promise<void> {
    try {
      const [invitations, schools] = await Promise.all([
        firstValueFrom(this.invitationService.getInvitations()),
        firstValueFrom(this.schoolService.getSchools()),
      ]);
      this.invitations.set(invitations);
      this.schools.set(schools);
    } catch {
      // Silent — the panel already shows the result.
    }
  }

  onResultDone(): void {
    this.createdResult.set(null);
    this.activeTab.set('Invitations');
  }

  // ---------- tabs ----------

  setTab(tab: OperatorTab): void {
    this.activeTab.set(tab);
  }

  // ---------- helpers ----------

  formatDateTime(iso: string | null): string {
    return formatDateTime(iso);
  }

  statusLabel(status: InvitationStatus): string {
    switch (status) {
      case 'Pending':  return 'Pending';
      case 'Accepted': return 'Accepted';
      case 'Expired':  return 'Expired';
      case 'Revoked':  return 'Revoked';
    }
  }

  private describeActionError(error: unknown, action: string): string {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const status = (error as { status: number }).status;
      if (status === 0) return `Cannot reach the server to ${action}.`;
      if (status === 400) return `The server rejected the request to ${action}. The email may already belong to a user.`;
      if (status >= 500) return `Something went wrong trying to ${action}.`;
    }
    return `Could not ${action}.`;
  }
}