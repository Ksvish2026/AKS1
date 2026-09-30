using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AksTyreProduction.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CaptureQcOperator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QcOperatorId",
                table: "RetreadJobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QcOperatorNameSnapshot",
                table: "RetreadJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetreadJobs_QcOperatorId",
                table: "RetreadJobs",
                column: "QcOperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_RetreadJobs_Operators_QcOperatorId",
                table: "RetreadJobs",
                column: "QcOperatorId",
                principalTable: "Operators",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RetreadJobs_Operators_QcOperatorId",
                table: "RetreadJobs");

            migrationBuilder.DropIndex(
                name: "IX_RetreadJobs_QcOperatorId",
                table: "RetreadJobs");

            migrationBuilder.DropColumn(
                name: "QcOperatorId",
                table: "RetreadJobs");

            migrationBuilder.DropColumn(
                name: "QcOperatorNameSnapshot",
                table: "RetreadJobs");
        }
    }
}
