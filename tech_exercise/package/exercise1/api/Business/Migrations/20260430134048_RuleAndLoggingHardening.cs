using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateAPI.Migrations
{
    /// <inheritdoc />
    public partial class RuleAndLoggingHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AstronautDuty_PersonId",
                table: "AstronautDuty");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CareerStartDate",
                table: "AstronautDetail",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ProcessLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Method = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Person_Name",
                table: "Person",
                column: "Name",
                unique: true);

            //Only one active duty at a time per person
            migrationBuilder.CreateIndex(
                name: "IX_AstronautDuty_PersonId",
                table: "AstronautDuty",
                column: "PersonId",
                unique: true,
                filter: "\"DutyEndDate\" IS NULL");

            //A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time.
            migrationBuilder.CreateIndex(
                name: "IX_AstronautDuty_PersonId_DutyTitle_DutyStartDate",
                table: "AstronautDuty",
                columns: new[] { "PersonId", "DutyTitle", "DutyStartDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessLog");

            migrationBuilder.DropIndex(
                name: "IX_Person_Name",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_AstronautDuty_PersonId",
                table: "AstronautDuty");

            migrationBuilder.DropIndex(
                name: "IX_AstronautDuty_PersonId_DutyTitle_DutyStartDate",
                table: "AstronautDuty");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CareerStartDate",
                table: "AstronautDetail",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_AstronautDuty_PersonId",
                table: "AstronautDuty",
                column: "PersonId");
        }
    }
}
