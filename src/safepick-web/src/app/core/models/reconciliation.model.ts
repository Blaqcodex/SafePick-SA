// ============================================================
// Reconciliation — Runs, expectations
// ============================================================

export type ReconciliationRunStatus = 'Started' | 'Completed' | 'Failed';

export interface ReconciliationRun {
  runId: string;
  schoolId: string;
  date: string;                // YYYY-MM-DD
  status: ReconciliationRunStatus;
  startedAt: string;           // ISO 8601 UTC
  completedAt: string | null;
  expectedCount: number;
  resolvedCount: number;
  suppressedCount: number;
  gappedCount: number;
}

/** One derived expectation for a learner on a day */
export interface CustodyExpectation {
  learnerId: string;
  learnerFullName: string;
  classGroup: string;
  driverId: string;
  driverName: string;
  vehicleId: string;
  vehicleRegistration: string;
  expectedWindowStart: string;  // ISO 8601 UTC
  expectedWindowEnd: string;    // ISO 8601 UTC
  isSuppressed: boolean;        // e.g. school marked absent
}

/** Command: trigger reconciliation for a school and day */
export interface RunReconciliationCommand {
  schoolId: string;
  date: string;                 // YYYY-MM-DD
}