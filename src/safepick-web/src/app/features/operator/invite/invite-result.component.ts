import {
  Component,
  EventEmitter,
  Input,
  Output,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { formatDateTime } from '@app/core/date.util';
import type { CreateInvitationResult } from '@app/core/models';

// ============================================================
// InviteResultComponent
//
// Shown after an invitation is created. Displays the raw token
// (which is available exactly once), the invite link, and the
// delivery status.
//
// The Operator can copy the link or the token, then click Done
// to return to the board.
// ============================================================

@Component({
  selector: 'app-invite-result',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './invite-result.component.html',
  styleUrl: './invite-result.component.scss',
})
export class InviteResultComponent {
  @Input({ required: true }) result!: CreateInvitationResult;
  @Output() done = new EventEmitter<void>();

  readonly linkCopied = signal(false);
  readonly tokenCopied = signal(false);

  async copyLink(): Promise<void> {
    const ok = await this.tryCopy(this.result.inviteUrl);
    if (ok) {
      this.linkCopied.set(true);
      setTimeout(() => this.linkCopied.set(false), 2000);
    }
  }

  async copyToken(): Promise<void> {
    const ok = await this.tryCopy(this.result.rawToken);
    if (ok) {
      this.tokenCopied.set(true);
      setTimeout(() => this.tokenCopied.set(false), 2000);
    }
  }

  onDone(): void {
    this.done.emit();
  }

  formatExpiry(iso: string): string {
    return formatDateTime(iso);
  }

  private async tryCopy(text: string): Promise<boolean> {
    try {
      await navigator.clipboard.writeText(text);
      return true;
    } catch {
      return false;
    }
  }
}