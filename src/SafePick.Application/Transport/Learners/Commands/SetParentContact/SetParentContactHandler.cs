using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Transport;

namespace SafePick.Application.Transport.Learners.Commands.SetParentContact;

public sealed class SetParentContactHandler : IRequestHandler<SetParentContactCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public SetParentContactHandler(IApplicationDbContext context, IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task Handle(
        SetParentContactCommand request,
        CancellationToken cancellationToken)
    {
        var learner = await _context.Learners
            .FirstOrDefaultAsync(l => l.Id == request.LearnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Learner), request.LearnerId);

        var contact = new ParentContact(
            request.Email,
            request.Phone,
            request.Name,
            request.ConsentRecorded);

        learner.SetParentContact(contact, _clock.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);
    }
}