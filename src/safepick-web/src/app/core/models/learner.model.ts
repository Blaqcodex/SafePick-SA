// ============================================================
// Learners, ParentContact, GradeClass
// Contract source: GET /api/learners (confirmed).
// ============================================================

export interface GradeClass {
  grade: number;    // 1–12
  letter: string;   // A–Z (single character)
}

export interface ParentContact {
  email: string;              // required
  phone: string | null;
  name: string | null;
  consentRecorded: boolean;   // POPIA consent
}

export interface Learner {
  learnerId: string;
  schoolId: string;
  fullName: string;
  gradeClass: GradeClass;
  parentContact: ParentContact | null;
  isActive: boolean;

  // Derived enrichment from the API — the learner's current assignment,
  // if any. Null when the learner is unassigned.
  currentAssignmentId: string | null;
  currentDriverId: string | null;
  currentDriverName: string | null;
  currentVehicleRegistration: string | null;
}

// ---------- Commands ----------

export interface AddLearnerCommand {
  schoolId: string;
  fullName: string;
  grade: number;
  classLetter: string;
}

export interface SetParentContactCommand {
  email: string;
  phone: string | null;
  name: string | null;
  consentRecorded: boolean;
}