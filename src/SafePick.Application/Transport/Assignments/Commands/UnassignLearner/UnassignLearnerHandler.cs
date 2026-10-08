using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Transport;

namespace SafePick.Application.Transport.Assignments.Commands.UnassignLearner;

public sealed class UnassignLearnerHandler : IRequestHandler<UnassignLearnerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _clock;

    public UnassignLearnerHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTime clock)
    {
        _context = context;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(
        UnassignLearnerCommand request,
        CancellationToken cancellationToken)
    {
        var assignment = await _context.Assignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Assignment), request.AssignmentId);

        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("No authenticated user.");

        var terminatedAt = request.TerminatedAt ?? _clock.UtcNow;

        assignment.Terminate(terminatedAt, userId, _clock.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);
    }
}