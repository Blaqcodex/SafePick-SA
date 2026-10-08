namespace SafePick.Domain.Custody;

/// <summary>
/// The state of a reconciliation run.
///
/// A run is the system's periodic evaluation of expectations against
/// custody events. It begins, executes, and completes (or fails). Each
/// run is recorded so the system can prove it did its job — even on
/// days when nothing went wrong.
/// </summary>
public enum ReconciliationRunStatus
{
    Started = 0,
    Completed = 1,
    Failed = 2
}