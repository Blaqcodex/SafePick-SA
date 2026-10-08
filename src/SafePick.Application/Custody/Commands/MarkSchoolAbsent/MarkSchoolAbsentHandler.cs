using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;
using SafePick.Domain.Transport;

namespace SafePick.Application.Custody.Commands.MarkSchoolAbsent;

public sealed class MarkSchoolAbsentHandler
    : IRequestHandler<MarkSchoolAbsentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _clock;

    public MarkSchoolAbsentHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTime clock)
    {
        _context = context;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Guid> Handle(
        MarkSchoolAbsentCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _context.CustodyEvents
            .FirstOrDefaultAsync(e =>
                e.ClientGeneratedId == request.ClientGeneratedId,
                cancellationToken);

        if (existing is not null)
            return existing.Id;

        var learner = await _context.Learners
            .FirstOrDefaultAsync(l =>
                l.Id == request.LearnerId &&
                l.SchoolId == request.SchoolId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(Learner), request.LearnerId);

        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("No authenticated user.");

        var now = _clock.UtcNow;

        var evt = CustodyEvent.RecordSchoolAbsent(
            schoolId: request.SchoolId,
            learnerId: request.LearnerId,
            markedByUserId: userId,
            note: request.Note,
            clientGeneratedId: request.ClientGeneratedId,
            serverTimestamp: new ServerTimestamp(now),
            now: now);

        _context.CustodyEvents.Add(evt);
        await _context.SaveChangesAsync(cancellationToken);

        return evt.Id;
    }
}