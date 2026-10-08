using SafePick.Domain.Common;
using SafePick.Domain.Custody.Events;

namespace SafePick.Domain.Custody;

/// <summary>
/// A record of one execution of the reconciliation engine.
/// </summary>
public sealed class ReconciliationRun : AggregateRoot<Guid>
{
    private readonly List<ReconciliationOutcome> _outcomes = new();

    public Guid SchoolId { get; private set; }
    public DateOnly SchoolDay { get; private set; }
    public ReconciliationRunStatus Status { get; private set; }

    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public int ExpectationsEvaluated { get; private set; }
    public int ResolvedCount { get; private set; }
    public int SuppressedCount { get; private set; }
    public int GappedCount { get; private set; }

    public string? FailureReason { get; private set; }

    public IReadOnlyCollection<ReconciliationOutcome> Outcomes
        => _outcomes.AsReadOnly();

    private ReconciliationRun() : base(Guid.Empty) { }

    private ReconciliationRun(
        Guid id,
        Guid schoolId,
        DateOnly schoolDay,
        DateTime startedAt) : base(id)
    {
        SchoolId = schoolId;
        SchoolDay = schoolDay;
        StartedAt = startedAt;
        Status = ReconciliationRunStatus.Started;
    }

    public static ReconciliationRun Begin(
        Guid schoolId,
        DateOnly schoolDay,
        DateTime now)
    {
        if (schoolId == Guid.Empty)
            throw new DomainException("SchoolId is required.");

        return new ReconciliationRun(
            Guid.NewGuid(), schoolId, schoolDay, now);
    }

    public void RecordOutcome(ReconciliationOutcome outcome)
    {
        if (Status != ReconciliationRunStatus.Started)
            throw new DomainException(
                "Cannot record outcomes after the run has completed or failed.");

        ArgumentNullException.ThrowIfNull(outcome);

        _outcomes.Add(outcome);
        ExpectationsEvaluated++;

        switch (outcome.Outcome)
        {
            case ReconciliationOutcome.OutcomeType.Resolved:
                ResolvedCount++;
                break;
            case ReconciliationOutcome.OutcomeType.Suppressed:
                SuppressedCount++;
                break;
            case ReconciliationOutcome.OutcomeType.Gapped:
                GappedCount++;
                break;
        }
    }

    public void Complete(DateTime now)
    {
        if (Status != ReconciliationRunStatus.Started)
            throw new DomainException(
                $"Cannot complete a run in status '{Status}'.");

        Status = ReconciliationRunStatus.Completed;
        CompletedAt = now;

        Raise(new ReconciliationCompleted(
            Id,
            SchoolId,
            SchoolDay,
            ExpectationsEvaluated,
            ResolvedCount,
            SuppressedCount,
            GappedCount,
            now));
    }

    public void Fail(string reason, DateTime now)
    {
        if (Status != ReconciliationRunStatus.Started)
            throw new DomainException(
                $"Cannot fail a run in status '{Status}'.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A failure reason is required.");

        Status = ReconciliationRunStatus.Failed;
        CompletedAt = now;
        FailureReason = reason.Trim();
    }
}