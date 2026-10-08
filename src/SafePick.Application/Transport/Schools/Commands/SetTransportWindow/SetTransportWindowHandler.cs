using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;
using SafePick.Domain.Transport;

namespace SafePick.Application.Transport.Schools.Commands.SetTransportWindow;

public sealed class SetTransportWindowHandler
    : IRequestHandler<SetTransportWindowCommand, SetTransportWindowResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public SetTransportWindowHandler(
        IApplicationDbContext context,
        IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<SetTransportWindowResult> Handle(
        SetTransportWindowCommand request,
        CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.Id == request.SchoolId, cancellationToken)
            ?? throw new NotFoundException(nameof(School), request.SchoolId);

        var now = _clock.UtcNow;

        // Replace all existing windows with the single new one.
        school.ClearTransportWindows(now);

        var window = new TransportWindow(
            request.OpensAt,
            request.ClosesAt,
            TimeSpan.FromMinutes(request.GraceMinutes));

        school.AddTransportWindow(window);

        await _context.SaveChangesAsync(cancellationToken);

        return new SetTransportWindowResult(
            SchoolId: school.Id,
            OpensAt: window.OpensAt,
            ClosesAt: window.ClosesAt,
            GraceMinutes: request.GraceMinutes,
            ReconciliationAt: window.ReconciliationAt);
    }
}