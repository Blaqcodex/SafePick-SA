using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafePick.Application.Transport.Assignments.Commands.AssignLearnerToVehicle;
using SafePick.Application.Transport.Assignments.Commands.ReassignLearner;
using SafePick.Application.Transport.Assignments.Commands.UnassignLearner;
using SafePick.Application.Transport.Assignments.Queries.GetAssignmentsBySchool;

namespace SafePick.Api.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public sealed class AssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssignmentsController(IMediator mediator) => _mediator = mediator;

    // ---------------------------------------------------------------
    // Commands
    // ---------------------------------------------------------------

    public sealed record AssignBody(Guid LearnerId, Guid DriverId, DateTime? ValidFrom);

    [HttpPost]
    public async Task<ActionResult<Guid>> Assign(
        [FromBody] AssignBody body,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new AssignLearnerToVehicleCommand(
            body.LearnerId, body.DriverId, body.ValidFrom),
            cancellationToken);
        return Ok(id);
    }

    public sealed record ReassignBody(Guid LearnerId, Guid NewDriverId, DateTime? ValidFrom);

    [HttpPost("reassign")]
    public async Task<ActionResult<Guid>> Reassign(
        [FromBody] ReassignBody body,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new ReassignLearnerCommand(
            body.LearnerId, body.NewDriverId, body.ValidFrom),
            cancellationToken);
        return Ok(id);
    }

    public sealed record UnassignBody(Guid AssignmentId, DateTime? TerminatedAt);

    [HttpPost("unassign")]
    public async Task<IActionResult> Unassign(
        [FromBody] UnassignBody body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnassignLearnerCommand(
            body.AssignmentId, body.TerminatedAt),
            cancellationToken);
        return NoContent();
    }

    // ---------------------------------------------------------------
    // Queries
    // ---------------------------------------------------------------

    /// <summary>
    /// Lists assignments for a school. By default, only currently-open
    /// assignments (validTo IS NULL) are returned; pass
    /// includeTerminated=true to also include historical ones.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssignmentItemDto>>> GetBySchool(
        [FromQuery] Guid schoolId,
        [FromQuery] bool includeTerminated = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAssignmentsBySchoolQuery(schoolId, includeTerminated),
            cancellationToken);
        return Ok(result);
    }
}