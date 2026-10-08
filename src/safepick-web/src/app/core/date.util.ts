// ============================================================
// Date utilities
//
// Used across the app for:
//   - formatting the API date parameter (YYYY-MM-DD)
//   - formatting display dates (Fri, 19 Sep 2026)
//   - formatting timestamps (06:42)
//   - formatting ISO timestamps with date + time
// ============================================================

export function toApiDate(date: Date = new Date()): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export function formatDisplayDate(apiDate: string): string {
  const [y, m, d] = apiDate.split('-').map(Number);
  const date = new Date(y, m - 1, d);
  return date.toLocaleDateString('en-ZA', {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
}

export function formatRosterDate(apiDate: string): string {
  if (apiDate === toApiDate()) {
    return `Today — ${formatDisplayDate(apiDate)}`;
  }
  return formatDisplayDate(apiDate);
}

export function formatTime(isoUtc: string | null | undefined): string {
  if (!isoUtc) return '';
  const date = new Date(isoUtc);
  return date.toLocaleTimeString('en-ZA', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  });
}

/**
 * Formats an ISO 8601 UTC timestamp as a friendly date + time.
 * Example: "19 Sep 2026, 11:43".
 */
export function formatDateTime(isoUtc: string | null | undefined): string {
  if (!isoUtc) return '';
  const date = new Date(isoUtc);
  return date.toLocaleString('en-ZA', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  });
}

/**
 * Formats an expected transport window as a friendly range.
 * Example: "19 Sep 2026 · 06:00 – 07:30".
 */
export function formatWindow(startIso: string, endIso: string): string {
  const start = new Date(startIso);
  const end = new Date(endIso);

  const datePart = start.toLocaleDateString('en-ZA', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });

  const startTime = start.toLocaleTimeString('en-ZA', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  });

  const endTime = end.toLocaleTimeString('en-ZA', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  });

  return `${datePart} · ${startTime} – ${endTime}`;
}