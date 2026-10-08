using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SafePick.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dispatched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_day = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expectations_evaluated = table.Column<int>(type: "integer", nullable: false),
                    resolved_count = table.Column<int>(type: "integer", nullable: false),
                    suppressed_count = table.Column<int>(type: "integer", nullable: false),
                    gapped_count = table.Column<int>(type: "integer", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reconciliation_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_outcomes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    learner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    outcome = table.Column<int>(type: "integer", nullable: false),
                    custody_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    gap_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reconciliation_run_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reconciliation_outcomes", x => x.id);
                    table.ForeignKey(
                        name: "FK_reconciliation_outcomes_reconciliation_runs_reconciliation_~",
                        column: x => x.reconciliation_run_id,
                        principalTable: "reconciliation_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_undispatched",
                table: "outbox_messages",
                columns: new[] { "dispatched_at", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_outcomes_run",
                table: "reconciliation_outcomes",
                column: "reconciliation_run_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_runs_school_day",
                table: "reconciliation_runs",
                columns: new[] { "school_id", "school_day" });

            // =========================================================
            // Least-privilege grants for the three new tables.
            //
            // On Neon, ownership is not reassigned — the default owner
            // role (neondb_owner) owns the tables, and safepick_app is
            // granted only what the running application needs.
            // =========================================================

            // ----- outbox_messages: mutable work queue -----
            migrationBuilder.Sql(
                @"REVOKE ALL ON outbox_messages FROM safepick_app;");
            migrationBuilder.Sql(
                @"GRANT SELECT, INSERT, UPDATE ON outbox_messages TO safepick_app;");

            // ----- reconciliation_runs: mutable record of a run -----
            migrationBuilder.Sql(
                @"REVOKE ALL ON reconciliation_runs FROM safepick_app;");
            migrationBuilder.Sql(
                @"GRANT SELECT, INSERT, UPDATE ON reconciliation_runs TO safepick_app;");

            // ----- reconciliation_outcomes: owned by runs -----
            // EF Core may issue DELETE on owned collection rows in some
            // scenarios, so we grant it here. This is the only table
            // outside the identity/transport contexts where DELETE is
            // granted to the app role.
            migrationBuilder.Sql(
                @"REVOKE ALL ON reconciliation_outcomes FROM safepick_app;");
            migrationBuilder.Sql(
                @"GRANT SELECT, INSERT, UPDATE, DELETE ON reconciliation_outcomes TO safepick_app;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "reconciliation_outcomes");

            migrationBuilder.DropTable(
                name: "reconciliation_runs");
        }
    }
}