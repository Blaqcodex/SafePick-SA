using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;

namespace SafePick.Application.Custody.Commands.RecordCorrection;

public sealed class RecordCorrectionHandler
    : IRequestHandler<RecordCorrectionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _clock;

    public RecordCorrectionHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTime clock)
    {
        _context = context;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Guid> Handle(
        RecordCorrectionCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _context.CustodyEvents
            .FirstOrDefaultAsync(e =>
                e.ClientGeneratedId == request.ClientGeneratedId,
                cancellationToken);

        if (existing is not null)
            return existing.Id;

        var original = await _context.CustodyEvents
            .FirstOrDefaultAsync(e => e.Id == request.OriginalEventId,
                cancellationToken)
            ?? throw new NotFoundException(
                nameof(CustodyEvent), request.OriginalEventId);

        if (original.SchoolId != request.SchoolId)
            throw new ForbiddenAccessException(
                "Cannot correct an event from a different school.");

        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("No authenticated user.");

        var now = _clock.UtcNow;

        var evt = CustodyEvent.RecordCorrection(
            originalEventId: original.Id,
            schoolId: request.SchoolId,
            learnerId: request.LearnerId,
            correctedByUserId: userId,
            reason: request.Reason,
            clientGeneratedId: request.ClientGeneratedId,
            serverTimestamp: new ServerTimestamp(now),
            now: now);

        _context.CustodyEvents.Add(evt);
        await _context.SaveChangesAsync(cancellationToken);

        return evt.Id;
    }
}