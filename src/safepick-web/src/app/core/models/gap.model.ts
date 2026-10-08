// ============================================================
// Custody Gaps — first-class, lifecycle-bearing
// Contract source: gap board DTO (confirmed).
// ============================================================

/**
 * The gap lifecycle. State transitions are enforced by the
 * backend domain, and the UI mirrors them exactly — the front
 * end will never offer an action the domain will reject.
 */
export type GapState =
  | 'Open'
  | 'Acknowledged'
  | 'Investigating'
  | 'Resolved'
  | 'Closed';

/** One gap in the gap board (GET /api/reconciliation/gaps/{schoolId}) */
export interface CustodyGap {
  gapId: string;
  learnerId: string;
  learnerFullName: string;
  classGroup: string;
  assignmentId: string;
  driverId: string;
  driverName: string;
  state: GapState;
  expectedWindowStart: string;   // ISO 8601 UTC
  expectedWindowEnd: string;     // ISO 8601 UTC
  openedAt: string;              // ISO 8601 UTC
  acknowledgedByUserId: string | null;
  acknowledgedAt: string | null;
  resolvedAt: string | null;
  resolution: string | null;     // NOT "resolutionReason" — the field is "resolution"
}

/** Command: acknowledge an Open gap */
export interface AcknowledgeGapCommand {
  gapId: string;
}

/** Command: move an Acknowledged gap into investigation */
export interface InvestigateGapCommand {
  gapId: string;
  notes: string | null;
}

/** Command: resolve an Acknowledged or Investigating gap */
export interface ResolveGapCommand {
  gapId: string;
  resolution: string;
}