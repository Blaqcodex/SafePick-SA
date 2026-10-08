using MediatR;

namespace SafePick.Application.Identity.Commands.SeedOperator;

/// <summary>
/// Requests that the Operator account be seeded, if it does not yet exist.
///
/// This command is issued once per application startup. The handler is
/// idempotent: if the Operator already exists, it does nothing and returns
/// the existing user's id.
/// </summary>
public sealed record SeedOperatorCommand(
    string Email,
    string Password,
    string DisplayName) : IRequest<Guid>;