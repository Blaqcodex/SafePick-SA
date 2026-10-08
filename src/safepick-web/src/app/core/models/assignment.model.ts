// ============================================================
// Assignments — Learner to Vehicle, over a time range
// Contract source: GET /api/assignments (confirmed).
// ============================================================

export interface Assignment {
  assignmentId: string;
  schoolId: string;
  learnerId: string;
  learnerFullName: string;
  classGroup: string;
  driverId: string;
  driverFullName: string;
  vehicleId: string;
  vehicleRegistration: string;
  validFrom: string;          // ISO 8601 UTC
  validTo: string | null;     // null = still active
}