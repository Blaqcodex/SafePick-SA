namespace SafePick.Domain.Custody;

/// <summary>
/// The lifecycle of a custody gap.
///
/// A gap is not a notification — it is a *state* that must be driven to
/// closure. The transitions are strictly ordered: a gap can only move
/// forward, and every transition is recorded for audit.
///
///   Open           — reconciliation detected it; nobody has seen it yet.
///   Acknowledged   — a school admin has seen it and taken responsibility.
///   Investigating  — the school is actively determining what happened.
///   Resolved       — the cause is known and recorded.
///   Closed         — final. Reopening creates a *new* gap referencing this one.
/// </summary>
public enum GapState
{
    Open = 0,
    Acknowledged = 1,
    Investigating = 2,
    Resolved = 3,
    Closed = 4
}