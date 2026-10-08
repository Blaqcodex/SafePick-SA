import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@app/core/api.service';
import type {
  Invitation,
  CreateInvitationRequest,
  CreateInvitationResult,
  ValidateInvitationResult,
  AcceptInvitationRequest,
  AcceptInvitationResult,
} from '@app/core/models';

// ============================================================
// InvitationService
//
// Two roles:
//   1. The Operator's endpoints (auth required):
//      GET  /api/invitations
//      POST /api/invitations
//
//   2. The invitee's endpoints (anonymous):
//      GET  /api/invitations/validate?token=...
//      POST /api/invitations/accept
//
// The anonymous endpoints power the accept-invitation page.
// ============================================================

@Injectable({ providedIn: 'root' })
export class InvitationService {
  private readonly api = inject(ApiService);

  // ---------- Operator endpoints ----------

  getInvitations(status?: string): Observable<Invitation[]> {
    const query: Record<string, string | undefined> = {
      status: status || undefined,
    };
    return this.api.get<Invitation[]>('/invitations', query);
  }

  createInvitation(
    request: CreateInvitationRequest
  ): Observable<CreateInvitationResult> {
    return this.api.post<CreateInvitationResult>('/invitations', request);
  }

  // ---------- Invitee endpoints (anonymous) ----------

  /**
   * Validates an invitation token. Called by the accept page
   * to confirm the token and pre-fill the invitee's info.
   */
  validateInvitation(token: string): Observable<ValidateInvitationResult> {
    return this.api.get<ValidateInvitationResult>('/invitations/validate', {
      token,
    });
  }

  /**
   * Accepts an invitation: creates the User and marks the
   * invitation accepted. Returns the new user's info.
   */
  acceptInvitation(
    request: AcceptInvitationRequest
  ): Observable<AcceptInvitationResult> {
    return this.api.post<AcceptInvitationResult>(
      '/invitations/accept',
      request
    );
  }
}