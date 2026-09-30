using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AksTyreProduction.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerVisibleRejectionNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerVisibleRejectionNote",
                table: "RetreadJobs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerVisibleRejectionNote",
                table: "RetreadJobs");
        }
    }
}
