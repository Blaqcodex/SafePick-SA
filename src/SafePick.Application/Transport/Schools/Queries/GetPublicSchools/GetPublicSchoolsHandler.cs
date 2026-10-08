using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;

namespace SafePick.Application.Transport.Schools.Queries.GetPublicSchools;

public sealed class GetPublicSchoolsHandler
    : IRequestHandler<GetPublicSchoolsQuery, IReadOnlyList<PublicSchoolDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicSchoolsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PublicSchoolDto>> Handle(
        GetPublicSchoolsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Schools
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new PublicSchoolDto(s.Id, s.Name))
            .ToListAsync(cancellationToken);
    }
}