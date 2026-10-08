// ============================================================
// Idempotency helper
//
// Every mutating custody / transport command carries a
// client-generated GUID. If the request fails (flaky 3G,
// app crash, user re-tap), the client retries with the SAME
// id, and the backend deduplicates via its unique constraint.
//
// The critical property: the id must be generated ONCE and
// stored for the lifetime of the logical operation. Never
// regenerate on retry.
// ============================================================

/**
 * Generates a new RFC 4122 version 4 UUID.
 * Uses crypto.randomUUID when available (all modern browsers),
 * falls back to a v4 implementation for older environments.
 */
export function newIdempotencyKey(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  // Fallback for very old browsers
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

/**
 * Returns the current device time as an ISO 8601 UTC string.
 * The server records this alongside its own authoritative
 * timestamp, so a phone with a wrong clock can be reconciled.
 */
export function deviceTimestamp(): string {
  return new Date().toISOString();
}