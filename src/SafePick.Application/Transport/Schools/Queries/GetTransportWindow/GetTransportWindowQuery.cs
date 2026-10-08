using MediatR;

namespace SafePick.Application.Transport.Schools.Queries.GetTransportWindow;

/// <summary>
/// Reads the current transport window for a school.
/// Returns null when no window is configured.
/// </summary>
public sealed record GetTransportWindowQuery(Guid SchoolId)
    : IRequest<TransportWindowDto?>;

public sealed record TransportWindowDto(
    TimeOnly OpensAt,
    TimeOnly ClosesAt,
    int GraceMinutes,
    TimeOnly ReconciliationAt);