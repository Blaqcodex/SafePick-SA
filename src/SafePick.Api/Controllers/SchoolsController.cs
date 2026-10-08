using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafePick.Application.Transport.Schools.Commands.SetTransportWindow;
using SafePick.Application.Transport.Schools.Queries.GetPublicSchools;
using SafePick.Application.Transport.Schools.Queries.GetSchools;
using SafePick.Application.Transport.Schools.Queries.GetTransportWindow;

namespace SafePick.Api.Controllers;

[ApiController]
[Route("api/schools")]
[Authorize]
public sealed class SchoolsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchoolsController(IMediator mediator) => _mediator = mediator;

    // ---------------------------------------------------------------
    // Existing
    // ---------------------------------------------------------------

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SchoolItemDto>>> Get(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetSchoolsQuery(includeInactive), cancellationToken);
        return Ok(result);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<PublicSchoolDto>>> GetPublic(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetPublicSchoolsQuery(), cancellationToken);
        return Ok(result);
    }

    // ---------------------------------------------------------------
    // Transport window
    // ---------------------------------------------------------------

    /// <summary>
    /// Reads the school's transport window. Returns null when none is set.
    /// </summary>
    [HttpGet("{schoolId:guid}/transport-window")]
    public async Task<ActionResult<TransportWindowDto?>> GetTransportWindow(
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetTransportWindowQuery(schoolId), cancellationToken);
        return Ok(result);
    }

    public sealed record SetTransportWindowBody(
        string OpensAt,
        string ClosesAt,
        int GraceMinutes);

    /// <summary>
    /// Sets or replaces the school's transport window.
    /// Times are HH:mm strings (e.g. "06:00").
    /// </summary>
    [HttpPut("{schoolId:guid}/transport-window")]
    public async Task<ActionResult<SetTransportWindowResult>> SetTransportWindow(
        Guid schoolId,
        [FromBody] SetTransportWindowBody body,
        CancellationToken cancellationToken)
    {
        if (!TimeOnly.TryParse(body.OpensAt, out var opensAt))
            return BadRequest(new { message = "OpensAt must be a valid HH:mm time." });

        if (!TimeOnly.TryParse(body.ClosesAt, out var closesAt))
            return BadRequest(new { message = "ClosesAt must be a valid HH:mm time." });

        var result = await _mediator.Send(
            new SetTransportWindowCommand(schoolId, opensAt, closesAt, body.GraceMinutes),
            cancellationToken);

        return Ok(result);
    }
}