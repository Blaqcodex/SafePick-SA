using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafePick.Application.Transport.Drivers.Commands.ApproveDriver;
using SafePick.Application.Transport.Drivers.Commands.RegisterDriver;
using SafePick.Application.Transport.Drivers.Commands.RejectDriver;
using SafePick.Application.Transport.Drivers.Commands.RevokeDriverApproval;
using SafePick.Application.Transport.Drivers.Queries.GetCurrentDriver;
using SafePick.Application.Transport.Drivers.Queries.GetDriverApprovalsBySchool;

namespace SafePick.Api.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize]
public sealed class DriversController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriversController(IMediator mediator) => _mediator = mediator;

    // ---------------------------------------------------------------
    // Commands
    // ---------------------------------------------------------------

    public sealed record RegisterDriverBody(
        string FullName,
        string IdNumber,
        string Phone,
        string Email,
        string Password,
        string VehicleRegistrationNumber,
        string VehicleMake,
        string VehicleModel,
        int VehicleCapacity,
        Guid SchoolId);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<Guid>> Register(
        [FromBody] RegisterDriverBody body,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new RegisterDriverCommand(
            body.FullName, body.IdNumber, body.Phone, body.Email,
            body.Password,
            body.VehicleRegistrationNumber, body.VehicleMake,
            body.VehicleModel, body.VehicleCapacity, body.SchoolId),
            cancellationToken);
        return Ok(id);
    }

    public sealed record DecisionBody(string? Reason);

    [HttpPost("{approvalId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid approvalId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveDriverCommand(approvalId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{approvalId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid approvalId,
        [FromBody] DecisionBody body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new RejectDriverCommand(approvalId, body.Reason ?? string.Empty),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{approvalId:guid}/revoke")]
    public async Task<IActionResult> Revoke(
        Guid approvalId,
        [FromBody] DecisionBody body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new RevokeDriverApprovalCommand(approvalId, body.Reason ?? string.Empty),
            cancellationToken);
        return NoContent();
    }

    // ---------------------------------------------------------------
    // Queries
    // ---------------------------------------------------------------

    [HttpGet("approvals")]
    public async Task<ActionResult<IReadOnlyList<DriverApprovalItemDto>>> GetApprovals(
        [FromQuery] Guid schoolId,
        [FromQuery] DriverApprovalStatusDto? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetDriverApprovalsBySchoolQuery(
                schoolId,
                status is null ? null : (Domain.Transport.DriverApproval.ApprovalStatus)status.Value),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the authenticated driver's own details.
    /// Used by the driver's landing dashboard.
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<CurrentDriverDto>> GetMe(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCurrentDriverQuery(),
            cancellationToken);
        return Ok(result);
    }
}