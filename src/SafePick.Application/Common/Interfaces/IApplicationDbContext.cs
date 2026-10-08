using SafePick.Domain.Identity;
using SafePick.Domain.Transport;
using SafePick.Domain.Custody;
using Microsoft.EntityFrameworkCore;

namespace SafePick.Application.Common.Interfaces;

/// <summary>
/// The Application layer's view of persistence.
///
/// Exposes the aggregate roots the application needs to read and write.
/// Deliberately lives in the Application layer — so the application
/// depends on an abstraction, not on EF Core's concrete context.
/// </summary>
public interface IApplicationDbContext
{
    // Identity context
    DbSet<User> Users { get; }
    DbSet<Invitation> Invitations { get; }

    // Transport context
    DbSet<School> Schools { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<DriverApproval> DriverApprovals { get; }
    DbSet<Learner> Learners { get; }
    DbSet<Assignment> Assignments { get; }

    // Custody context
    DbSet<CustodyEvent> CustodyEvents { get; }
    DbSet<CustodyGap> CustodyGaps { get; }
    
    DbSet<ReconciliationRun> ReconciliationRuns { get; }   // ← NEW

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}