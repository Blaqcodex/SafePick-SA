using MediatR;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Transport;

namespace SafePick.Application.Transport.Learners.Commands.AddLearner;

public sealed class AddLearnerHandler : IRequestHandler<AddLearnerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public AddLearnerHandler(IApplicationDbContext context, IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<Guid> Handle(
        AddLearnerCommand request,
        CancellationToken cancellationToken)
    {
        var gradeClass = new GradeClass(request.Grade, request.ClassLetter);

        var learner = Learner.Register(
            request.SchoolId,
            request.FullName,
            gradeClass,
            _clock.UtcNow);

        _context.Learners.Add(learner);
        await _context.SaveChangesAsync(cancellationToken);

        return learner.Id;
    }
}