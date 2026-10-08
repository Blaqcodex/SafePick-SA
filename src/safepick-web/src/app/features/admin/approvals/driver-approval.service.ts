import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@app/core/api.service';
import type { DriverApproval, DriverApprovalStatus } from '@app/core/models';

// ============================================================
// DriverApprovalService
//
// The school admin's window into driver registrations.
//
// Endpoints:
//   GET  /api/drivers/approvals?schoolId=...&status=...
//   POST /api/drivers/{approvalId}/approve
//   POST /api/drivers/{approvalId}/reject   { reason }
//   POST /api/drivers/{approvalId}/revoke   { reason }
//
// Approve has no body.
// Reject and revoke require a reason.
// ============================================================

@Injectable({ providedIn: 'root' })
export class DriverApprovalService {
  private readonly api = inject(ApiService);

  /**
   * Fetches driver approvals for a school.
   * When `status` is provided, filters to just that status.
   */
  getApprovals(
    schoolId: string,
    status?: DriverApprovalStatus
  ): Observable<DriverApproval[]> {
    const query: Record<string, string | undefined> = {
      schoolId,
      status: status ?? undefined,
    };

    return this.api.get<DriverApproval[]>('/drivers/approvals', query);
  }

  /**
   * Approves a driver for the school.
   * No body required.
   */
  approve(approvalId: string): Observable<void> {
    return this.api.post<void>(`/drivers/${approvalId}/approve`, {});
  }

  /**
   * Rejects a driver registration.
   * A reason is required — the driver deserves to know why.
   */
  reject(approvalId: string, reason: string): Observable<void> {
    return this.api.post<void>(`/drivers/${approvalId}/reject`, { reason });
  }

  /**
   * Revokes a prior approval.
   * A reason is required.
   */
  revoke(approvalId: string, reason: string): Observable<void> {
    return this.api.post<void>(`/drivers/${approvalId}/revoke`, { reason });
  }
}