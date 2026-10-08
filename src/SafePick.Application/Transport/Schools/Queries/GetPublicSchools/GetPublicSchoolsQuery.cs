using MediatR;

namespace SafePick.Application.Transport.Schools.Queries.GetPublicSchools;

/// <summary>
/// A minimal, public list of schools. Used by the driver
/// registration form's school dropdown.
///
/// Exposes ONLY the school's id and name — no contact emails,
/// no counts, no internal data.
/// </summary>
public sealed record GetPublicSchoolsQuery()
    : IRequest<IReadOnlyList<PublicSchoolDto>>;

public sealed record PublicSchoolDto(
    Guid SchoolId,
    string Name);