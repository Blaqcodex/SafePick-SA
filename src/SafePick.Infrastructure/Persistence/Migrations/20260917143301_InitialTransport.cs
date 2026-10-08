using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SafePick.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialTransport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,");

            migrationBuilder.CreateTable(
                name: "assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    learner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    assigned_by_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignments", x => x.id);
                });

            // PostgreSQL exclusion constraint: a learner cannot have two
            // overlapping assignments. This is the *hard* guarantee that
            // makes double-assignment impossible at the storage layer,
            // not merely checked in application code. Requires the
            // btree_gist extension, enabled above.
            migrationBuilder.Sql(
                @"ALTER TABLE assignments
                  ADD CONSTRAINT no_overlapping_assignments
                  EXCLUDE USING gist (
                      learner_id WITH =,
                      tstzrange(valid_from, valid_to, '[)') WITH &&
                  );");

            migrationBuilder.CreateTable(
                name: "driver_approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_driver_approvals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "drivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IdNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "learners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    class_grade = table.Column<int>(type: "integer", nullable: false),
                    class_letter = table.Column<char>(type: "character(1)", maxLength: 1, nullable: false),
                    parent_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    parent_phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    parent_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    parent_consent = table.Column<bool>(type: "boolean", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "schools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Make = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Model = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicles_drivers_driver_id",
                        column: x => x.driver_id,
                        principalTable: "drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "school_transport_windows",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpensAt = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ClosesAt = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    GracePeriod = table.Column<TimeSpan>(type: "interval", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_transport_windows", x => x.id);
                    table.ForeignKey(
                        name: "FK_school_transport_windows_schools_school_id",
                        column: x => x.school_id,
                        principalTable: "schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assignments_school_learner_valid_to",
                table: "assignments",
                columns: new[] { "school_id", "learner_id", "valid_to" });

            migrationBuilder.CreateIndex(
                name: "ix_assignments_vehicle_valid_to",
                table: "assignments",
                columns: new[] { "vehicle_id", "valid_to" });

            migrationBuilder.CreateIndex(
                name: "IX_driver_approvals_DriverId_SchoolId",
                table: "driver_approvals",
                columns: new[] { "DriverId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_learners_SchoolId_IsActive",
                table: "learners",
                columns: new[] { "SchoolId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_school_transport_windows_school_id",
                table: "school_transport_windows",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_driver_id",
                table: "vehicles",
                column: "driver_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_RegistrationNumber",
                table: "vehicles",
                column: "RegistrationNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assignments");

            migrationBuilder.DropTable(
                name: "driver_approvals");

            migrationBuilder.DropTable(
                name: "learners");

            migrationBuilder.DropTable(
                name: "school_transport_windows");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "schools");

            migrationBuilder.DropTable(
                name: "drivers");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:btree_gist", ",,");
        }
    }
}