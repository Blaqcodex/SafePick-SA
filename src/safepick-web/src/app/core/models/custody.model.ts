// ============================================================
// Custody — Events, statuses, roster, monitoring board
// Contract source: roster + monitoring DTOs (confirmed).
// ============================================================

/**
 * The status of a learner's custody on a given day.
 * Strings on the wire — union, not enum.
 */
export type CustodyStatus =
  | 'Pending'
  | 'PickedUp'
  | 'NotInCar'
  | 'SchoolMarkedAbsent'
  | 'Corrected'
  | 'Gapped';

/** One row in the driver's roster (GET /api/custody/roster/{driverId}) */
export interface RosterRow {
  learnerId: string;
  learnerFullName: string;
  classGroup: string;
  assignmentId: string;
  schoolId: string;
  vehicleId: string;
  vehicleRegistration: string;
  status: CustodyStatus;
  custodyEventId: string | null;
  resolvedAt: string | null;
  note: string | null;
}

/** One row in the school monitoring board (GET /api/custody/status/{schoolId}) */
export interface MonitoringRow {
  learnerId: string;
  learnerFullName: string;
  classGroup: string;
  driverId: string;
  driverName: string;
  vehicleId: string;
  vehicleRegistration: string;
  status: CustodyStatus;
  custodyEventId: string | null;
  resolvedAt: string | null;
}

/** Summary counts at the top of the monitoring board */
export interface MonitoringSummary {
  schoolId: string;
  date: string;
  totalExpected: number;
  pickedUp: number;
  notInCar: number;
  markedAbsent: number;
  pending: number;
  openGaps: number;
  rows: MonitoringRow[];
}

// ---------- Commands ----------
//
// These mirror the backend's ConfirmPickupBody, MarkNotInCarBody,
// and BatchConfirmBody. Every command carries the learner's schoolId
// (from the roster row), the driverId (from the session), a
// client-generated idempotency key, and a device timestamp.

export interface ConfirmPickupCommand {
  schoolId: string;
  learnerId: string;
  driverId: string;
  clientGeneratedId: string;
  deviceTimestamp: string | null;
}

export interface MarkNotInCarCommand {
  schoolId: string;
  learnerId: string;
  driverId: string;
  clientGeneratedId: string;
  reason: string;
  deviceTimestamp: string | null;
}

export interface BatchConfirmPickupsCommand {
  schoolId: string;
  driverId: string;
  items: Array<{
    learnerId: string;
    clientGeneratedId: string;
    deviceTimestamp: string | null;
  }>;
}