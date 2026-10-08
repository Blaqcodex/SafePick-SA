// ============================================================
// Schools — extended with the transport window
// ============================================================

/**
 * The school's transport window — when pickups are expected.
 * Times are HH:mm strings from the API.
 */
export interface TransportWindow {
  opensAt: string;         // e.g. "06:00:00"
  closesAt: string;        // e.g. "07:30:00"
  graceMinutes: number;
  reconciliationAt: string; // closesAt + grace, computed by the server
}

export interface SetTransportWindowRequest {
  opensAt: string;         // "06:00"
  closesAt: string;        // "07:30"
  graceMinutes: number;
}

export interface SetTransportWindowResult {
  schoolId: string;
  opensAt: string;
  closesAt: string;
  graceMinutes: number;
  reconciliationAt: string;
}