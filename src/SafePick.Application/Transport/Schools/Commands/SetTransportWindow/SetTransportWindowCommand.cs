using MediatR;

namespace SafePick.Application.Transport.Schools.Commands.SetTransportWindow;

/// <summary>
/// Sets or replaces the transport window for a school.
///
/// The window defines when pickups are expected. Reconciliation
/// cannot run without one — it derives every expectation from
/// the school's window.
///
/// The command replaces all existing windows with a single new
/// one. This keeps the model simple: one window per school for now.
/// </summary>
public sealed record SetTransportWindowCommand(
    Guid SchoolId,
    TimeOnly OpensAt,
    TimeOnly ClosesAt,
    int GraceMinutes)
    : IRequest<SetTransportWindowResult>;

public sealed record SetTransportWindowResult(
    Guid SchoolId,
    TimeOnly OpensAt,
    TimeOnly ClosesAt,
    int GraceMinutes,
    TimeOnly ReconciliationAt);