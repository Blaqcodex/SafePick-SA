import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@app/core/api.service';
import type { CustodyGap } from '@app/core/models';

// ============================================================
// GapService
//
// The school admin's window into custody gaps.
//
// Endpoints:
//   GET  /api/reconciliation/gaps/{schoolId}?includeResolved=...
//   POST /api/reconciliation/gaps/{gapId}/acknowledge
//   POST /api/reconciliation/gaps/{gapId}/investigate  { notes }
//   POST /api/reconciliation/gaps/{gapId}/resolve      { resolution }
//
// The backend enforces a strict lifecycle:
//   Open → Acknowledged → Investigating → Resolved → Closed
//
// Invariants:
//   - A gap cannot be resolved from Open.
//   - A gap cannot be investigated before being Acknowledged.
//
// The UI mirrors this state machine exactly. No button is
// offered that the domain will refuse.
// ============================================================

@Injectable({ providedIn: 'root' })
export class GapService {
  private readonly api = inject(ApiService);

  getGaps(
    schoolId: string,
    includeResolved = false
  ): Observable<CustodyGap[]> {
    return this.api.get<CustodyGap[]>(
      `/reconciliation/gaps/${schoolId}`,
      { includeResolved }
    );
  }

  acknowledge(gapId: string): Observable<void> {
    return this.api.post<void>(
      `/reconciliation/gaps/${gapId}/acknowledge`,
      {}
    );
  }

  investigate(gapId: string, notes: string | null): Observable<void> {
    return this.api.post<void>(
      `/reconciliation/gaps/${gapId}/investigate`,
      { notes }
    );
  }

  resolve(gapId: string, resolution: string): Observable<void> {
    return this.api.post<void>(
      `/reconciliation/gaps/${gapId}/resolve`,
      { resolution }
    );
  }
}