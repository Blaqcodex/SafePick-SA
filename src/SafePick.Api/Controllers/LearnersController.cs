using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafePick.Application.Custody.Commands.MarkSchoolAbsent;
using SafePick.Application.Transport.Learners.Commands.AddLearner;
using SafePick.Application.Transport.Learners.Commands.SetParentContact;
using SafePick.Application.Transport.Learners.Queries.GetLearnersBySchool;

namespace SafePick.Api.Controllers;

[ApiController]
[Route("api/learners")]
[Authorize]
public sealed class LearnersController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnersController(IMediator mediator) => _mediator = mediator;

    // ---------------------------------------------------------------
    // Commands
    // ---------------------------------------------------------------

    public sealed record AddLearnerBody(
        Guid SchoolId,
        string FullName,
        int Grade,
        char ClassLetter);

    [HttpPost]
    public async Task<ActionResult<Guid>> Add(
        [FromBody] AddLearnerBody body,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new AddLearnerCommand(
            body.SchoolId, body.FullName, body.Grade, body.ClassLetter),
            cancellationToken);
        return Ok(id);
    }

    public sealed record ParentContactBody(
        string Email,
        string? Phone,
        string? Name,
        bool ConsentRecorded);

    [HttpPut("{learnerId:guid}/parent-contact")]
    public async Task<IActionResult> SetParentContact(
        Guid learnerId,
        [FromBody] ParentContactBody body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetParentContactCommand(
            learnerId, body.Email, body.Phone, body.Name, body.ConsentRecorded),
            cancellationToken);
        return NoContent();
    }

    public sealed record MarkAbsentBody(
        Guid SchoolId,
        string? Note,
        Guid ClientGeneratedId);

    /// <summary>
    /// Marks a learner as absent before the pickup window opens.
    /// Suppresses the expectation for the day.
    /// </summary>
    [HttpPost("{learnerId:guid}/mark-absent")]
    public async Task<ActionResult<Guid>> MarkAbsent(
        Guid learnerId,
        [FromBody] MarkAbsentBody body,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new MarkSchoolAbsentCommand(
            body.SchoolId, learnerId, body.Note, body.ClientGeneratedId),
            cancellationToken);
        return Ok(id);
    }

    // ---------------------------------------------------------------
    // Queries
    // ---------------------------------------------------------------

    /// <summary>
    /// Lists the learners at a school. By default, inactive learners are
    /// excluded; pass includeInactive=true to include them.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LearnerItemDto>>> GetBySchool(
        [FromQuery] Guid schoolId,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetLearnersBySchoolQuery(schoolId, includeInactive),
            cancellationToken);
        return Ok(result);
    }
}