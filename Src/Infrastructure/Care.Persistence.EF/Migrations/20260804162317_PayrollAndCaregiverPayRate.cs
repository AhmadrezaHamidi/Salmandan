using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Care.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class PayrollAndCaregiverPayRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CaregiverPayRate",
                table: "Shifts",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "PayoutId",
                table: "Shifts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CaregiverPayRate",
                table: "AgencyServicePrices",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Payouts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AgencyId = table.Column<long>(type: "bigint", nullable: false),
                    CaregiverId = table.Column<long>(type: "bigint", nullable: false),
                    PeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    ShiftCount = table.Column<int>(type: "int", nullable: false),
                    TotalHours = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModificationTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payouts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_AgencyId_Status_PayoutId",
                table: "Shifts",
                columns: new[] { "AgencyId", "Status", "PayoutId" });

            migrationBuilder.CreateIndex(
                name: "IX_Payouts_AgencyId_Status",
                table: "Payouts",
                columns: new[] { "AgencyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Payouts_CaregiverId_PeriodFrom",
                table: "Payouts",
                columns: new[] { "CaregiverId", "PeriodFrom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payouts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_AgencyId_Status_PayoutId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "CaregiverPayRate",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "PayoutId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "CaregiverPayRate",
                table: "AgencyServicePrices");
        }
    }
}
