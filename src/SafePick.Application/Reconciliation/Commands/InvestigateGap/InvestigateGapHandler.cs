using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;

namespace SafePick.Application.Reconciliation.Commands.InvestigateGap;

public sealed class InvestigateGapHandler
    : IRequestHandler<InvestigateGapCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public InvestigateGapHandler(
        IApplicationDbContext context,
        IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task Handle(
        InvestigateGapCommand request,
        CancellationToken cancellationToken)
    {
        var gap = await _context.CustodyGaps
            .FirstOrDefaultAsync(g => g.Id == request.GapId, cancellationToken)
            ?? throw new NotFoundException(nameof(CustodyGap), request.GapId);

        gap.BeginInvestigation(_clock.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);
    }
}