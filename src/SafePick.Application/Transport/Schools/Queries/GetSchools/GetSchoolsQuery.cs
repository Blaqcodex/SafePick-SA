using MediatR;

namespace SafePick.Application.Transport.Schools.Queries.GetSchools;

/// <summary>
/// Lists all schools. Used by the Operator for context — to see
/// what schools have already been provisioned.
/// </summary>
public sealed record GetSchoolsQuery(bool IncludeInactive = false)
    : IRequest<IReadOnlyList<SchoolItemDto>>;

public sealed record SchoolItemDto(
    Guid SchoolId,
    string Name,
    string ContactEmail,
    bool IsActive,
    int LearnerCount,
    int DriverCount);