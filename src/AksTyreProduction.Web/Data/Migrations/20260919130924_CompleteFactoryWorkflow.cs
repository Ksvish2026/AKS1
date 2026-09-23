using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AksTyreProduction.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompleteFactoryWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedCompletion",
                table: "RetreadJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QcPassedAt",
                table: "RetreadJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyForDispatchAt",
                table: "RetreadJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Repairs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PatchSize",
                table: "Repairs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "Repairs",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RetreadJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProductionCostSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MarkupPercentSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SellingPriceSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_RetreadJobs_RetreadJobId",
                        column: x => x.RetreadJobId,
                        principalTable: "RetreadJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_RetreadJobId",
                table: "Invoices",
                column: "RetreadJobId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropColumn(
                name: "EstimatedCompletion",
                table: "RetreadJobs");

            migrationBuilder.DropColumn(
                name: "QcPassedAt",
                table: "RetreadJobs");

            migrationBuilder.DropColumn(
                name: "ReadyForDispatchAt",
                table: "RetreadJobs");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "PatchSize",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Repairs");
        }
    }
}
