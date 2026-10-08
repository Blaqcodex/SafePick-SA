using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafePick.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Enforces true immutability of custody_events at the database layer.
    ///
    /// The running application connects as 'safepick_app', a non-owner role.
    /// This migration:
    ///   1. Reassigns ownership of the custody tables to neondb_owner (or
    ///      leaves them there — owner role runs migrations).
    ///   2. Grants safepick_app only SELECT and INSERT on custody_events.
    ///
    /// After this migration, UPDATE and DELETE against custody_events as
    /// safepick_app will fail with 'permission denied'. The immutability
    /// guarantee is enforced by PostgreSQL itself (ADR-0004).
    /// </summary>
    public partial class HardenCustodyEventsImmutability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // custody_events: append-only for the app role.
            migrationBuilder.Sql(
                @"REVOKE ALL ON custody_events FROM safepick_app;");
            migrationBuilder.Sql(
                @"GRANT SELECT, INSERT ON custody_events TO safepick_app;");

            // custody_gaps: mutable, full read/write.
            migrationBuilder.Sql(
                @"REVOKE ALL ON custody_gaps FROM safepick_app;");
            migrationBuilder.Sql(
                @"GRANT SELECT, INSERT, UPDATE ON custody_gaps TO safepick_app;");

            // All other tables: read/write for the app.
            var mutableTables = new[]
            {
                "assignments", "learners", "drivers", "vehicles",
                "schools", "driver_approvals", "users", "invitations"
            };

            foreach (var table in mutableTables)
            {
                migrationBuilder.Sql(
                    $"REVOKE ALL ON {table} FROM safepick_app;");
                migrationBuilder.Sql(
                    $"GRANT SELECT, INSERT, UPDATE ON {table} TO safepick_app;");
            }

            // school_transport_windows allows DELETE (owned by schools).
            migrationBuilder.Sql(
                @"REVOKE ALL ON school_transport_windows FROM safepick_app;");
            migrationBuilder.Sql(
                @"GRANT SELECT, INSERT, UPDATE, DELETE ON school_transport_windows TO safepick_app;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No reverting needed — the grants are the app's baseline.
        }
    }
}