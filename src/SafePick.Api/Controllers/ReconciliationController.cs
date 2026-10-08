using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafePick.Application.Custody.Queries.GetExpectedPickupsForWindow;
using SafePick.Application.Custody.Queries.GetOpenGapsBySchool;
using SafePick.Application.Reconciliation.Commands.AcknowledgeGap;
using SafePick.Application.Reconciliation.Commands.InvestigateGap;
using SafePick.Application.Reconciliation.Commands.ResolveGap;
using SafePick.Application.Reconciliation.Commands.RunReconciliation;

namespace SafePick.Api.Controllers;

/// <summary>
/// Exposes the reconciliation engine and the gap lifecycle.
///
/// - Run reconciliation for a school and day (the guarantor engine)
/// - View the expectations that were evaluated
/// - View the gap board
/// - Acknowledge, investigate, and resolve gaps
///
/// In a production system, reconciliation is triggered by a scheduled
/// background job at transport-window close. For the academic demo,
/// it is triggered on demand via POST /run — which lets us demonstrate
/// the guarantee that "no expected pickup is silently unaccounted for".
/// </summary>
[ApiController]
[Route("api/reconciliation")]
[Authorize]
public sealed class ReconciliationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReconciliationController(IMediator mediator) => _mediator = mediator;

    // ---------------------------------------------------------------
    // Engine
    // ---------------------------------------------------------------

    public sealed record RunReconciliationBody(
        Guid SchoolId,
        DateOnly SchoolDay,
        bool DryRun);

    [HttpPost("run")]
    public async Task<ActionResult<RunReconciliationResult>> Run(
        [FromBody] RunReconciliationBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RunReconciliationCommand(body.SchoolId, body.SchoolDay, body.DryRun),
            cancellationToken);
        return Ok(result);
    }

    // ---------------------------------------------------------------
    // Queries
    // ---------------------------------------------------------------

    [HttpGet("expectations/{schoolId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ExpectedPickupDto>>> GetExpectations(
        Guid schoolId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetExpectedPickupsForWindowQuery(schoolId, date),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("gaps/{schoolId:guid}")]
    public async Task<ActionResult<IReadOnlyList<CustodyGapDto>>> GetGaps(
        Guid schoolId,
        [FromQuery] bool includeResolved = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetOpenGapsBySchoolQuery(schoolId, includeResolved),
            cancellationToken);
        return Ok(result);
    }

    // ---------------------------------------------------------------
    // Gap lifecycle
    // ---------------------------------------------------------------

    [HttpPost("gaps/{gapId:guid}/acknowledge")]
    public async Task<IActionResult> AcknowledgeGap(
        Guid gapId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new AcknowledgeGapCommand(gapId), cancellationToken);
        return NoContent();
    }

    [HttpPost("gaps/{gapId:guid}/investigate")]
    public async Task<IActionResult> InvestigateGap(
        Guid gapId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new InvestigateGapCommand(gapId), cancellationToken);
        return NoContent();
    }

    public sealed record ResolveGapBody(
        string Resolution,
        Guid? ResolvedByCustodyEventId);

    [HttpPost("gaps/{gapId:guid}/resolve")]
    public async Task<IActionResult> ResolveGap(
        Guid gapId,
        [FromBody] ResolveGapBody body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ResolveGapCommand(gapId, body.Resolution, body.ResolvedByCustodyEventId),
            cancellationToken);
        return NoContent();
    }
}