using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;

namespace SafePick.Application.Transport.Drivers.Commands.ApproveDriver;

public sealed class ApproveDriverHandler : IRequestHandler<ApproveDriverCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _clock;

    public ApproveDriverHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTime clock)
    {
        _context = context;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(
        ApproveDriverCommand request,
        CancellationToken cancellationToken)
    {
        var approval = await _context.DriverApprovals
            .FirstOrDefaultAsync(a => a.Id == request.ApprovalId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Transport.DriverApproval), request.ApprovalId);

        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("No authenticated user.");

        approval.Approve(userId, _clock.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);
    }
}