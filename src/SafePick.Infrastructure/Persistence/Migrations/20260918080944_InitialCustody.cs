using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafePick.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCustody : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "custody_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    learner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    marked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    client_generated_id = table.Column<Guid>(type: "uuid", nullable: false),
                    server_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    device_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    original_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custody_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "custody_gaps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    learner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reconciliation_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expected_window_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expected_window_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    acknowledged_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_by_custody_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolution = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    opened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custody_gaps", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_custody_events_learner_timestamp",
                table: "custody_events",
                columns: new[] { "learner_id", "server_timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_custody_events_school_timestamp",
                table: "custody_events",
                columns: new[] { "school_id", "server_timestamp" });

            migrationBuilder.CreateIndex(
                name: "ux_custody_events_client_generated_id",
                table: "custody_events",
                column: "client_generated_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_custody_gaps_learner",
                table: "custody_gaps",
                column: "learner_id");

            migrationBuilder.CreateIndex(
                name: "ix_custody_gaps_school_state",
                table: "custody_gaps",
                columns: new[] { "school_id", "state" });

            // =========================================================
            // Immutability guarantee (ADR-0004).
            //
            // Revoke UPDATE and DELETE on the custody_events table from
            // the application role. After this migration, the application
            // physically cannot modify or delete a custody event — even
            // if a bug in the code attempts to. The guarantee is enforced
            // by PostgreSQL's privilege system, not by convention.
            //
            // The application connects as the 'safepick' role. If a
            // different role is used in future environments, this
            // statement must be adjusted.
            // =========================================================
            migrationBuilder.Sql(
                @"REVOKE UPDATE, DELETE ON custody_events FROM safepick_app;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "custody_events");

            migrationBuilder.DropTable(
                name: "custody_gaps");
        }
    }
}