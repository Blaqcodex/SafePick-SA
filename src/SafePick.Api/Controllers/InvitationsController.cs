using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafePick.Application.Identity.Commands.AcceptInvitation;
using SafePick.Application.Identity.Commands.CreateInvitation;
using SafePick.Application.Identity.Queries.GetInvitations;
using SafePick.Application.Identity.Queries.ValidateInvitation;

namespace SafePick.Api.Controllers;

[ApiController]
[Route("api/invitations")]
[Authorize]
public sealed class InvitationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvitationsController(IMediator mediator) => _mediator = mediator;

    // ---------- Operator endpoints ----------

    public sealed record CreateInvitationBody(
        string Email,
        string DisplayName,
        string SchoolName,
        string SchoolContactEmail);

    [HttpPost]
    public async Task<ActionResult<CreateInvitationResult>> Create(
        [FromBody] CreateInvitationBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateInvitationCommand(
            body.Email, body.DisplayName, body.SchoolName, body.SchoolContactEmail),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvitationItemDto>>> Get(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetInvitationsQuery(status),
            cancellationToken);
        return Ok(result);
    }

    // ---------- Invitee endpoints (anonymous) ----------

    /// <summary>
    /// Validates an invitation token. Called by the accept page
    /// to check the token and pre-fill the invitee's email.
    /// Anonymous — the invitee has no session yet.
    /// </summary>
    [HttpGet("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<ValidateInvitationResult>> Validate(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ValidateInvitationQuery(token),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Accepts an invitation: creates the User and marks the
    /// invitation accepted. Anonymous — this is how the invitee
    /// becomes a User in the first place.
    /// </summary>
    public sealed record AcceptInvitationBody(
        string Token,
        string Password);

    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<ActionResult<AcceptInvitationResult>> Accept(
        [FromBody] AcceptInvitationBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AcceptInvitationCommand(body.Token, body.Password),
            cancellationToken);
        return Ok(result);
    }
}