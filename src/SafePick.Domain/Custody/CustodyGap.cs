using SafePick.Domain.Common;
using SafePick.Domain.Custody.Events;

namespace SafePick.Domain.Custody;

/// <summary>
/// A first-class representation of an unresolved custody expectation.
///
/// A gap is opened by the reconciliation engine when a learner was
/// expected at pickup, the window closed (plus grace period), and no
/// custody event resolved the expectation. From that moment, the gap is
/// owned by the school and driven through its lifecycle until closed.
///
/// A gap never silently disappears. It has a state machine, an owner, a
/// trail of transitions, and it must reach Closed.
///
/// State transitions are strictly forward-only. Any attempt to move
/// backwards, or to skip ahead (e.g., Open → Resolved without
/// Investigating), is rejected by the aggregate.
/// </summary>
public sealed class CustodyGap : AggregateRoot<Guid>
{
    public Guid SchoolId { get; private set; }
    public Guid LearnerId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public Guid ReconciliationRunId { get; private set; }

    public DateTime ExpectedWindowStart { get; private set; }
    public DateTime ExpectedWindowEnd { get; private set; }

    public GapState State { get; private set; }

    public Guid? AcknowledgedByUserId { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }

    public Guid? ResolvedByUserId { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    public Guid? ResolvedByCustodyEventId { get; private set; }
    public string? Resolution { get; private set; }

    public DateTime OpenedAt { get; private set; }

    // ---- EF Core materialization -------------------------------------------

    private CustodyGap() : base(Guid.Empty) { }

    // ---- Private canonical constructor -------------------------------------

    private CustodyGap(
        Guid id,
        Guid schoolId,
        Guid learnerId,
        Guid assignmentId,
        Guid reconciliationRunId,
        DateTime expectedWindowStart,
        DateTime expectedWindowEnd,
        DateTime openedAt) : base(id)
    {
        SchoolId = schoolId;
        LearnerId = learnerId;
        AssignmentId = assignmentId;
        ReconciliationRunId = reconciliationRunId;
        ExpectedWindowStart = expectedWindowStart;
        ExpectedWindowEnd = expectedWindowEnd;
        State = GapState.Open;
        OpenedAt = openedAt;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Opens a new gap. Invoked only by the reconciliation engine.
    /// </summary>
    public static CustodyGap Open(
        Guid schoolId,
        Guid learnerId,
        Guid assignmentId,
        Guid reconciliationRunId,
        DateTime expectedWindowStart,
        DateTime expectedWindowEnd,
        DateTime now)
    {
        if (schoolId == Guid.Empty) throw new DomainException("SchoolId is required.");
        if (learnerId == Guid.Empty) throw new DomainException("LearnerId is required.");
        if (assignmentId == Guid.Empty)
            throw new DomainException("AssignmentId is required.");
        if (reconciliationRunId == Guid.Empty)
            throw new DomainException("ReconciliationRunId is required.");

        if (expectedWindowEnd <= expectedWindowStart)
            throw new DomainException(
                "Expected window end must be after its start.");

        var gap = new CustodyGap(
            Guid.NewGuid(),
            schoolId,
            learnerId,
            assignmentId,
            reconciliationRunId,
            expectedWindowStart,
            expectedWindowEnd,
            now);

        gap.Raise(new CustodyGapOpened(
            gap.Id, schoolId, learnerId, assignmentId, reconciliationRunId,
            expectedWindowStart, expectedWindowEnd, now));

        return gap;
    }

    // ---- Lifecycle transitions --------------------------------------------

    /// <summary>
    /// Acknowledges the gap. Only valid from Open. A school admin has
    /// seen the gap and taken responsibility for it.
    /// </summary>
    public void Acknowledge(Guid userId, DateTime now)
    {
        if (State != GapState.Open)
            throw new DomainException(
                $"Cannot acknowledge a gap in state '{State}'. Expected 'Open'.");
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required.");

        State = GapState.Acknowledged;
        AcknowledgedByUserId = userId;
        AcknowledgedAt = now;
    }

    /// <summary>
    /// Moves the gap into active investigation. Only valid from Acknowledged.
    /// </summary>
    public void BeginInvestigation(DateTime now)
    {
        if (State != GapState.Acknowledged)
            throw new DomainException(
                $"Cannot begin investigation from state '{State}'. " +
                "Expected 'Acknowledged'.");

        State = GapState.Investigating;
    }

    /// <summary>
    /// Resolves the gap with a recorded explanation. Valid from
    /// Acknowledged or Investigating. A late-arriving custody event can
    /// also resolve a gap directly from Open — that path is
    /// <see cref="ResolveByCustodyEvent"/>.
    /// </summary>
    public void Resolve(
        Guid resolvedByUserId,
        string resolution,
        Guid? resolvedByCustodyEventId,
        DateTime now)
    {
        if (State != GapState.Acknowledged && State != GapState.Investigating)
            throw new DomainException(
                $"Cannot resolve a gap in state '{State}'. " +
                "Expected 'Acknowledged' or 'Investigating'.");

        if (resolvedByUserId == Guid.Empty)
            throw new DomainException("ResolvedByUserId is required.");

        if (string.IsNullOrWhiteSpace(resolution))
            throw new DomainException(
                "A resolution reason is required to close a gap.");

        State = GapState.Resolved;
        ResolvedByUserId = resolvedByUserId;
        ResolvedByCustodyEventId = resolvedByCustodyEventId;
        Resolution = resolution.Trim();
        ResolvedAt = now;

        Raise(new CustodyGapResolved(
            Id, SchoolId, LearnerId, resolvedByCustodyEventId,
            resolvedByUserId, resolution.Trim(), now));
    }

    /// <summary>
    /// Resolves the gap directly from Open because a late custody event
    /// arrived (e.g., an offline driver synced after the window closed).
    /// The gap closes automatically — no human needed.
    /// </summary>
    public void ResolveByCustodyEvent(Guid custodyEventId, DateTime now)
    {
        if (State != GapState.Open)
            throw new DomainException(
                $"Cannot auto-resolve a gap in state '{State}'. Expected 'Open'.");

        if (custodyEventId == Guid.Empty)
            throw new DomainException("CustodyEventId is required.");

        State = GapState.Resolved;
        ResolvedByCustodyEventId = custodyEventId;
        Resolution = "Resolved by a late-arriving custody event.";
        ResolvedAt = now;

        Raise(new CustodyGapResolved(
            Id, SchoolId, LearnerId, custodyEventId,
            ResolvedByUserId: null, Resolution, now));
    }

    /// <summary>
    /// Closes a resolved gap. Final state. Valid only from Resolved.
    /// </summary>
    public void Close(DateTime now)
    {
        if (State != GapState.Resolved)
            throw new DomainException(
                $"Cannot close a gap in state '{State}'. Expected 'Resolved'.");

        State = GapState.Closed;
    }
}