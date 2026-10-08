using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;

namespace SafePick.Application.Reconciliation.Commands.ResolveGap;

public sealed class ResolveGapHandler : IRequestHandler<ResolveGapCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _clock;

    public ResolveGapHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTime clock)
    {
        _context = context;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(
        ResolveGapCommand request,
        CancellationToken cancellationToken)
    {
        var gap = await _context.CustodyGaps
            .FirstOrDefaultAsync(g => g.Id == request.GapId, cancellationToken)
            ?? throw new NotFoundException(nameof(CustodyGap), request.GapId);

        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("No authenticated user.");

        gap.Resolve(
            resolvedByUserId: userId,
            resolution: request.Resolution,
            resolvedByCustodyEventId: request.ResolvedByCustodyEventId,
            now: _clock.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);
    }
}