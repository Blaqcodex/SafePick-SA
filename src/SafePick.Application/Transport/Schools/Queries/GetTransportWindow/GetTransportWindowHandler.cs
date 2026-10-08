using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;

namespace SafePick.Application.Transport.Schools.Queries.GetTransportWindow;

public sealed class GetTransportWindowHandler
    : IRequestHandler<GetTransportWindowQuery, TransportWindowDto?>
{
    private readonly IApplicationDbContext _context;

    public GetTransportWindowHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransportWindowDto?> Handle(
        GetTransportWindowQuery request,
        CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.Id == request.SchoolId, cancellationToken);

        if (school is null) return null;

        var window = school.TransportWindows.FirstOrDefault();
        if (window is null) return null;

        return new TransportWindowDto(
            OpensAt: window.OpensAt,
            ClosesAt: window.ClosesAt,
            GraceMinutes: (int)window.GracePeriod.TotalMinutes,
            ReconciliationAt: window.ReconciliationAt);
    }
}