using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Identity;

namespace SafePick.Application.Identity.Commands.SeedOperator;

/// <summary>
/// Handles seeding of the Operator account.
///
/// Idempotent by design: if the Operator exists, no new user is created
/// and no domain event is raised. This makes it safe to run on every
/// application startup.
/// </summary>
public sealed class SeedOperatorHandler : IRequestHandler<SeedOperatorCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public SeedOperatorHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(
        SeedOperatorCommand request,
        CancellationToken cancellationToken)
    {
        var existingOperator = await _context.Users
            .FirstOrDefaultAsync(u => u.Role == Role.Operator, cancellationToken);

        if (existingOperator is not null)
            return existingOperator.Id;

        var passwordHash = _passwordHasher.Hash(request.Password);

        var op = User.SeedOperator(
            request.Email,
            passwordHash,
            request.DisplayName);

        _context.Users.Add(op);
        await _context.SaveChangesAsync(cancellationToken);

        return op.Id;
    }
}