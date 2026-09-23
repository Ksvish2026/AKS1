using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AksTyreProduction.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TyreId = table.Column<int>(type: "INTEGER", nullable: true),
                    RetreadJobId = table.Column<int>(type: "INTEGER", nullable: true),
                    User = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    OldValue = table.Column<string>(type: "TEXT", nullable: true),
                    NewValue = table.Column<string>(type: "TEXT", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AccountNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Site = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaterialId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentUnitCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ReorderLevel = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    BatchTrackingRequired = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeeId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    LabourRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tyres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AksTyreId = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Brand = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<string>(type: "TEXT", nullable: false),
                    SerialNumber = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalTreadPattern = table.Column<string>(type: "TEXT", nullable: false),
                    CasingCondition = table.Column<string>(type: "TEXT", nullable: false),
                    Application = table.Column<string>(type: "TEXT", nullable: false),
                    IsScrapped = table.Column<bool>(type: "INTEGER", nullable: false),
                    ScrapReason = table.Column<string>(type: "TEXT", nullable: true),
                    ScrappedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tyres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tyres_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    LotNumber = table.Column<string>(type: "TEXT", nullable: false),
                    QuantityRemaining = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialBatches_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RetreadJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TyreId = table.Column<int>(type: "INTEGER", nullable: false),
                    RetreadNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    JobNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentStage = table.Column<string>(type: "TEXT", nullable: false),
                    BaseCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MarkupPercent = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetreadJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetreadJobs_Tyres_TyreId",
                        column: x => x.TyreId,
                        principalTable: "Tyres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dispatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RetreadJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    DispatchDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Method = table.Column<string>(type: "TEXT", nullable: false),
                    Reference = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dispatches_RetreadJobs_RetreadJobId",
                        column: x => x.RetreadJobId,
                        principalTable: "RetreadJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhotoAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RetreadJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    StationTransactionId = table.Column<int>(type: "INTEGER", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhotoAttachments_RetreadJobs_RetreadJobId",
                        column: x => x.RetreadJobId,
                        principalTable: "RetreadJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Repairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RetreadJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Repairs_RetreadJobs_RetreadJobId",
                        column: x => x.RetreadJobId,
                        principalTable: "RetreadJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StationTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RetreadJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    Station = table.Column<string>(type: "TEXT", nullable: false),
                    OperatorId = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    LabourRateSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    LabourCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Result = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    FailureReason = table.Column<string>(type: "TEXT", nullable: true),
                    DetailsJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StationTransactions_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StationTransactions_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StationTransactions_RetreadJobs_RetreadJobId",
                        column: x => x.RetreadJobId,
                        principalTable: "RetreadJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RetreadJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    StationTransactionId = table.Column<int>(type: "INTEGER", nullable: true),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialBatchId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    UnitCostSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    UsedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialUsages_MaterialBatches_MaterialBatchId",
                        column: x => x.MaterialBatchId,
                        principalTable: "MaterialBatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaterialUsages_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialUsages_RetreadJobs_RetreadJobId",
                        column: x => x.RetreadJobId,
                        principalTable: "RetreadJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialUsages_StationTransactions_StationTransactionId",
                        column: x => x.StationTransactionId,
                        principalTable: "StationTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_RetreadJobId",
                table: "Dispatches",
                column: "RetreadJobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialBatches_LotNumber",
                table: "MaterialBatches",
                column: "LotNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialBatches_MaterialId",
                table: "MaterialBatches",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialUsages_MaterialBatchId",
                table: "MaterialUsages",
                column: "MaterialBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialUsages_MaterialId",
                table: "MaterialUsages",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialUsages_RetreadJobId",
                table: "MaterialUsages",
                column: "RetreadJobId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialUsages_StationTransactionId",
                table: "MaterialUsages",
                column: "StationTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoAttachments_RetreadJobId",
                table: "PhotoAttachments",
                column: "RetreadJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Repairs_RetreadJobId",
                table: "Repairs",
                column: "RetreadJobId");

            migrationBuilder.CreateIndex(
                name: "IX_RetreadJobs_TyreId_RetreadNumber",
                table: "RetreadJobs",
                columns: new[] { "TyreId", "RetreadNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StationTransactions_MachineId",
                table: "StationTransactions",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_StationTransactions_OperatorId",
                table: "StationTransactions",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_StationTransactions_RetreadJobId",
                table: "StationTransactions",
                column: "RetreadJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Tyres_AksTyreId",
                table: "Tyres",
                column: "AksTyreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tyres_CustomerId",
                table: "Tyres",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tyres_SerialNumber",
                table: "Tyres",
                column: "SerialNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "Dispatches");

            migrationBuilder.DropTable(
                name: "MaterialUsages");

            migrationBuilder.DropTable(
                name: "PhotoAttachments");

            migrationBuilder.DropTable(
                name: "Repairs");

            migrationBuilder.DropTable(
                name: "MaterialBatches");

            migrationBuilder.DropTable(
                name: "StationTransactions");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.DropTable(
                name: "RetreadJobs");

            migrationBuilder.DropTable(
                name: "Tyres");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
