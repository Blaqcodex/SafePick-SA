import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@app/core/api.service';

// ============================================================
// ReconciliationService
//
// Triggers the reconciliation engine for a school and day.
//
// Endpoint:
//   POST /api/reconciliation/run
//
// The engine derives every expectation for the day, compares them
// to the custody events that exist, and opens gaps for learners
// who were expected but never recorded.
//
// In production, this runs as a scheduled job at window close.
// For the demo, it's triggered on demand from the monitoring board.
// ============================================================

export interface RunReconciliationResult {
  reconciliationRunId: string | null;
  schoolId: string;
  schoolDay: string;
  dryRun: boolean;
  expectationsEvaluated: number;
  resolvedCount: number;
  suppressedCount: number;
  gappedCount: number;
  items: RunReconciliationItem[];
}

export interface RunReconciliationItem {
  learnerId: string;
  learnerFullName: string;
  classGroup: string;
  assignmentId: string;
  outcome: string;
  custodyEventId: string | null;
  gapId: string | null;
}

@Injectable({ providedIn: 'root' })
export class ReconciliationService {
  private readonly api = inject(ApiService);

  run(schoolId: string, schoolDay: string): Observable<RunReconciliationResult> {
    return this.api.post<RunReconciliationResult>('/reconciliation/run', {
      schoolId,
      schoolDay,
      dryRun: false,
    });
  }
}