using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;
using SafePick.Domain.Custody;
using SafePick.Domain.Transport;

namespace SafePick.Application.Reconciliation.Commands.RunReconciliation;

/// <summary>
/// The reconciliation engine.
///
/// CRITICAL BEHAVIOR: running reconciliation multiple times for the
/// same day must be idempotent. A gap, once opened for a learner,
/// is a permanent fact. Subsequent runs must NOT create a second
/// gap for the same expectation.
/// </summary>
public sealed class RunReconciliationHandler
    : IRequestHandler<RunReconciliationCommand, RunReconciliationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public RunReconciliationHandler(
        IApplicationDbContext context,
        IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<RunReconciliationResult> Handle(
        RunReconciliationCommand request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.Id == request.SchoolId, cancellationToken)
            ?? throw new NotFoundException(nameof(School), request.SchoolId);

        var window = school.TransportWindows.FirstOrDefault()
            ?? throw new DomainException(
                "School has no transport window configured; cannot reconcile.");

        var assignments = await _context.Assignments
            .Where(a => a.SchoolId == request.SchoolId && a.ValidTo == null)
            .ToListAsync(cancellationToken);

        if (assignments.Count == 0)
        {
            return EmptyResult(request);
        }

        var expectations = assignments
            .Select(a => CustodyExpectation.Derive(a, window, request.SchoolDay))
            .ToList();

        var learnerIds = assignments.Select(a => a.LearnerId).Distinct().ToList();

        var learners = await _context.Learners
            .Where(l => learnerIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        var dayStart = request.SchoolDay
            .ToDateTime(TimeOnly.MinValue)
            .ToUniversalTime();
        var dayEnd = dayStart.AddDays(1);

        var dayStartTs = new ServerTimestamp(dayStart);
        var dayEndTs = new ServerTimestamp(dayEnd);

        var events = await _context.CustodyEvents
            .Where(e =>
                e.SchoolId == request.SchoolId &&
                learnerIds.Contains(e.LearnerId) &&
                e.ServerTimestamp >= dayStartTs &&
                e.ServerTimestamp < dayEndTs)
            .ToListAsync(cancellationToken);

        // --- Load existing gaps for the day, indexed by learner -------------
        //
        // CRITICAL: this is what prevents duplicate gaps. Indexed by
        // LearnerId alone — a learner has exactly one expectation per
        // school day, so at most one gap. No DateTime comparison, no
        // Kind/precision subtleties.
        var existingGaps = await _context.CustodyGaps
            .Where(g =>
                g.SchoolId == request.SchoolId &&
                learnerIds.Contains(g.LearnerId))
            .ToListAsync(cancellationToken);

        var existingGapByLearner = existingGaps
            .GroupBy(g => g.LearnerId)
            .ToDictionary(
                grp => grp.Key,
                grp => grp.OrderByDescending(g => g.OpenedAt).First());

        ReconciliationRun? run = null;
        if (!request.DryRun)
        {
            run = ReconciliationRun.Begin(
                request.SchoolId, request.SchoolDay, now);
            _context.ReconciliationRuns.Add(run);
        }

        var items = new List<ReconciliationItemResult>();
        int resolvedCount = 0, suppressedCount = 0, gappedCount = 0;

        foreach (var expectation in expectations)
        {
            var isPastReconciliation = now >= expectation.ReconciliationAt;

            var resolvingEvent = events
                .Where(e => e.LearnerId == expectation.LearnerId)
                .Where(e => expectation.IsResolvedBy(e.ServerTimestamp.Value))
                .OrderByDescending(e => e.ServerTimestamp.Value)
                .FirstOrDefault();

            ReconciliationOutcome.OutcomeType outcome;
            Guid? custodyEventId = null;
            Guid? gapId = null;
            string outcomeLabel;

            if (resolvingEvent?.Type == CustodyEventType.SchoolMarkedAbsent)
            {
                outcome = ReconciliationOutcome.OutcomeType.Suppressed;
                custodyEventId = resolvingEvent.Id;
                outcomeLabel = "Suppressed";
                suppressedCount++;
            }
            else if (resolvingEvent is not null)
            {
                outcome = ReconciliationOutcome.OutcomeType.Resolved;
                custodyEventId = resolvingEvent.Id;
                outcomeLabel = "Resolved";
                resolvedCount++;
            }
            else if (isPastReconciliation)
            {
                existingGapByLearner.TryGetValue(expectation.LearnerId, out var existingGap);

                if (existingGap is not null)
                {
                    outcome = ReconciliationOutcome.OutcomeType.Gapped;
                    outcomeLabel = existingGap.State == GapState.Resolved
                        || existingGap.State == GapState.Closed
                        ? "AlreadyGappedResolved"
                        : "AlreadyGapped";
                    gapId = existingGap.Id;
                }
                else
                {
                    outcome = ReconciliationOutcome.OutcomeType.Gapped;
                    outcomeLabel = "Gapped";

                    if (!request.DryRun && run is not null)
                    {
                        var gap = CustodyGap.Open(
                            schoolId: expectation.SchoolId,
                            learnerId: expectation.LearnerId,
                            assignmentId: expectation.AssignmentId,
                            reconciliationRunId: run.Id,
                            expectedWindowStart: expectation.WindowStart,
                            expectedWindowEnd: expectation.WindowEnd,
                            now: now);

                        _context.CustodyGaps.Add(gap);
                        gapId = gap.Id;
                    }

                    gappedCount++;
                }
            }
            else
            {
                continue;
            }

            learners.TryGetValue(expectation.LearnerId, out var learner);

            if (!request.DryRun && run is not null)
            {
                var outcomeRecord = new ReconciliationOutcome(
                    expectation.LearnerId,
                    expectation.AssignmentId,
                    outcome,
                    custodyEventId,
                    gapId);

                run.RecordOutcome(outcomeRecord);
            }

            items.Add(new ReconciliationItemResult(
                LearnerId: expectation.LearnerId,
                LearnerFullName: learner?.FullName ?? "(unknown)",
                ClassGroup: learner?.ClassGroup.ToString() ?? "(unknown)",
                AssignmentId: expectation.AssignmentId,
                Outcome: outcomeLabel,
                CustodyEventId: custodyEventId,
                GapId: gapId));
        }

        if (!request.DryRun && run is not null)
        {
            run.Complete(now);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new RunReconciliationResult(
            ReconciliationRunId: run?.Id,
            SchoolId: request.SchoolId,
            SchoolDay: request.SchoolDay,
            DryRun: request.DryRun,
            ExpectationsEvaluated: items.Count,
            ResolvedCount: resolvedCount,
            SuppressedCount: suppressedCount,
            GappedCount: gappedCount,
            Items: items);
    }

    private static RunReconciliationResult EmptyResult(RunReconciliationCommand request)
        => new(
            ReconciliationRunId: null,
            SchoolId: request.SchoolId,
            SchoolDay: request.SchoolDay,
            DryRun: request.DryRun,
            ExpectationsEvaluated: 0,
            ResolvedCount: 0,
            SuppressedCount: 0,
            GappedCount: 0,
            Items: Array.Empty<ReconciliationItemResult>());
}