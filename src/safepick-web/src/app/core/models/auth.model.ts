// ============================================================
// Auth — Login request/response, Role
// Contract source: login endpoint response (confirmed).
// ============================================================

/**
 * The four roles in the system. Strings on the wire —
 * modeled as a union type, not an enum, so they serialize
 * cleanly and are exhaustively checkable.
 */
export type Role = 'Operator' | 'SchoolAdmin' | 'SchoolStaff' | 'Driver';

/** Request body for POST /api/auth/login */
export interface LoginRequest {
  email: string;
  password: string;
}

/** Response from POST /api/auth/login */
export interface LoginResponse {
  accessToken: string;
  email: string;
  role: Role;
  schoolId: string | null;
  driverId: string | null;
}

/**
 * The in-memory representation of the logged-in user.
 * Stored in AuthService as a signal. Contains the profile
 * plus the token, plus a convenience boolean.
 */
export interface AuthSession {
  accessToken: string;
  email: string;
  role: Role;
  schoolId: string | null;
  driverId: string | null;
}