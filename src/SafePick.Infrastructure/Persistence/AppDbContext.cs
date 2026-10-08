using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;
using SafePick.Domain.Identity;
using SafePick.Domain.Transport;
using SafePick.Infrastructure.Persistence.Outbox;

namespace SafePick.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Identity context
    public DbSet<User> Users => Set<User>();
    public DbSet<Invitation> Invitations => Set<Invitation>();

    // Transport context
    public DbSet<School> Schools => Set<School>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DriverApproval> DriverApprovals => Set<DriverApproval>();
    public DbSet<Learner> Learners => Set<Learner>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    // Custody context
    public DbSet<CustodyEvent> CustodyEvents => Set<CustodyEvent>();
    public DbSet<CustodyGap> CustodyGaps => Set<CustodyGap>();
    public DbSet<ReconciliationRun> ReconciliationRuns => Set<ReconciliationRun>();

    // Infrastructure-only: transactional outbox
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("btree_gist");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}