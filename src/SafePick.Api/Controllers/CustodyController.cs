using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafePick.Application.Custody.Commands.BatchConfirmPickups;
using SafePick.Application.Custody.Commands.ConfirmPickup;
using SafePick.Application.Custody.Commands.MarkNotInCar;
using SafePick.Application.Custody.Commands.RecordCorrection;
using SafePick.Application.Custody.Queries.GetCustodyHistoryByLearner;
using SafePick.Application.Custody.Queries.GetCustodyStatusBySchool;
using SafePick.Application.Custody.Queries.GetTodaysRoster;

namespace SafePick.Api.Controllers;

[ApiController]
[Route("api/custody")]
[Authorize]
public sealed class CustodyController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustodyController(IMediator mediator) => _mediator = mediator;

    // ---------------------------------------------------------------
    // Commands
    // ---------------------------------------------------------------

    public sealed record ConfirmPickupBody(
        Guid SchoolId,
        Guid LearnerId,
        Guid DriverId,
        Guid ClientGeneratedId,
        DateTime? DeviceTimestamp);

    [HttpPost("confirm-pickup")]
    public async Task<ActionResult<Guid>> ConfirmPickup(
        [FromBody] ConfirmPickupBody body,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new ConfirmPickupCommand(
            body.SchoolId, body.LearnerId, body.DriverId,
            body.ClientGeneratedId, body.DeviceTimestamp),
            cancellationToken);
        return Ok(id);
    }

    public sealed record MarkNotInCarBody(
        Guid SchoolId,
        Guid LearnerId,
        Guid DriverId,
        Guid ClientGeneratedId,
        string Reason,
        DateTime? DeviceTimestamp);

    [HttpPost("mark-not-in-car")]
    public async Task<ActionResult<Guid>> MarkNotInCar(
        [FromBody] MarkNotInCarBody body,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new MarkNotInCarCommand(
            body.SchoolId, body.LearnerId, body.DriverId,
            body.ClientGeneratedId, body.Reason, body.DeviceTimestamp),
            cancellationToken);
        return Ok(id);
    }

    public sealed record BatchItemBody(
        Guid LearnerId,
        Guid ClientGeneratedId,
        DateTime? DeviceTimestamp);

    public sealed record BatchConfirmBody(
        Guid SchoolId,
        Guid DriverId,
        List<BatchItemBody> Items);

    [HttpPost("batch-confirm")]
    public async Task<ActionResult<BatchConfirmPickupsResult>> BatchConfirm(
        [FromBody] BatchConfirmBody body,
        CancellationToken cancellationToken)
    {
        var items = body.Items
            .Select(i => new BatchPickupItem(
                i.LearnerId, i.ClientGeneratedId, i.DeviceTimestamp))
            .ToList();

        var result = await _mediator.Send(new BatchConfirmPickupsCommand(
            body.SchoolId, body.DriverId, items),
            cancellationToken);
        return Ok(result);
    }

    public sealed record RecordCorrectionBody(
        Guid OriginalEventId,
        Guid SchoolId,
        Guid LearnerId,
        string Reason,
        Guid ClientGeneratedId);

    [HttpPost("record-correction")]
    public async Task<ActionResult<Guid>> RecordCorrection(
        [FromBody] RecordCorrectionBody body,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new RecordCorrectionCommand(
            body.OriginalEventId, body.SchoolId, body.LearnerId,
            body.Reason, body.ClientGeneratedId),
            cancellationToken);
        return Ok(id);
    }

    // ---------------------------------------------------------------
    // Queries
    // ---------------------------------------------------------------

    [HttpGet("roster/{driverId:guid}")]
    public async Task<ActionResult<IReadOnlyList<RosterItemDto>>> GetRoster(
        Guid driverId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetTodaysRosterQuery(driverId, date),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("status/{schoolId:guid}")]
    public async Task<ActionResult<SchoolCustodyBoardDto>> GetStatus(
        Guid schoolId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCustodyStatusBySchoolQuery(schoolId, date),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("history/{learnerId:guid}")]
    public async Task<ActionResult<IReadOnlyList<CustodyHistoryItemDto>>> GetHistory(
        Guid learnerId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetCustodyHistoryByLearnerQuery(learnerId, skip, take),
            cancellationToken);
        return Ok(result);
    }
}