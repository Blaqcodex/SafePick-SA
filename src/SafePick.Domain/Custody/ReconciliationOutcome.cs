using SafePick.Domain.Common;

namespace SafePick.Domain.Custody;

/// <summary>
/// The outcome of reconciling a single expectation.
///
/// Every expected pickup must resolve to exactly one of these outcomes.
/// There is no "unknown" state — the reconciliation engine guarantees
/// that every expectation is accounted for.
/// </summary>
public sealed class ReconciliationOutcome : ValueObject
{
    public enum OutcomeType
    {
        /// <summary>A custody event resolved the expectation. No action needed.</summary>
        Resolved = 0,

        /// <summary>The expectation was suppressed (learner marked absent).</summary>
        Suppressed = 1,

        /// <summary>No resolving event was found; a custody gap was opened.</summary>
        Gapped = 2
    }

    public Guid LearnerId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public OutcomeType Outcome { get; private set; }
    public Guid? CustodyEventId { get; private set; }
    public Guid? GapId { get; private set; }

    // EF Core materialization constructor.
    private ReconciliationOutcome()
    {
    }

    public ReconciliationOutcome(
        Guid learnerId,
        Guid assignmentId,
        OutcomeType outcome,
        Guid? custodyEventId,
        Guid? gapId)
    {
        if (learnerId == Guid.Empty)
            throw new DomainException("LearnerId is required.");
        if (assignmentId == Guid.Empty)
            throw new DomainException("AssignmentId is required.");

        switch (outcome)
        {
            case OutcomeType.Resolved when custodyEventId is null:
                throw new DomainException(
                    "A Resolved outcome must reference a custody event.");
            case OutcomeType.Gapped when gapId is null:
                throw new DomainException(
                    "A Gapped outcome must reference the opened gap.");
        }

        LearnerId = learnerId;
        AssignmentId = assignmentId;
        Outcome = outcome;
        CustodyEventId = custodyEventId;
        GapId = gapId;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return LearnerId;
        yield return AssignmentId;
        yield return Outcome;
        yield return CustodyEventId;
        yield return GapId;
    }
}