using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeartClinicHms.Infrastructure.Data.Migrations
{
    public partial class PanelAndLab : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorporatePanels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LegacySourceSystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegacyExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsLegacyRecord = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporatePanels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporatePanels_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PanelServiceRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorporatePanelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PanelRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PanelServiceRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PanelServiceRates_CatalogServices_CatalogServiceId",
                        column: x => x.CatalogServiceId,
                        principalTable: "CatalogServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PanelServiceRates_CorporatePanels_CorporatePanelId",
                        column: x => x.CorporatePanelId,
                        principalTable: "CorporatePanels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "CorporatePanelId",
                table: "Patients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CorporatePanelId",
                table: "Challans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PanelBillingMode",
                table: "Challans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PanelPayable",
                table: "Challans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PatientPayable",
                table: "Challans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "LabWorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CheckInServiceLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentVerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SampleCollectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentVerifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SampleCollectedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabWorkItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabWorkItems_CheckInServiceLines_CheckInServiceLineId",
                        column: x => x.CheckInServiceLineId,
                        principalTable: "CheckInServiceLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabWorkItems_Users_PaymentVerifiedByUserId",
                        column: x => x.PaymentVerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabWorkItems_Users_SampleCollectedByUserId",
                        column: x => x.SampleCollectedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorporatePanels_BranchId",
                table: "CorporatePanels",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporatePanels_Code",
                table: "CorporatePanels",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PanelServiceRates_CatalogServiceId",
                table: "PanelServiceRates",
                column: "CatalogServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PanelServiceRates_CorporatePanelId_CatalogServiceId",
                table: "PanelServiceRates",
                columns: new[] { "CorporatePanelId", "CatalogServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_CorporatePanelId",
                table: "Patients",
                column: "CorporatePanelId");

            migrationBuilder.CreateIndex(
                name: "IX_Challans_CorporatePanelId",
                table: "Challans",
                column: "CorporatePanelId");

            migrationBuilder.CreateIndex(
                name: "IX_LabWorkItems_CheckInServiceLineId",
                table: "LabWorkItems",
                column: "CheckInServiceLineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabWorkItems_PaymentVerifiedByUserId",
                table: "LabWorkItems",
                column: "PaymentVerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LabWorkItems_SampleCollectedByUserId",
                table: "LabWorkItems",
                column: "SampleCollectedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Challans_CorporatePanels_CorporatePanelId",
                table: "Challans",
                column: "CorporatePanelId",
                principalTable: "CorporatePanels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_CorporatePanels_CorporatePanelId",
                table: "Patients",
                column: "CorporatePanelId",
                principalTable: "CorporatePanels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("UPDATE Challans SET PatientPayable = SubTotal WHERE PatientPayable = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Challans_CorporatePanels_CorporatePanelId", table: "Challans");
            migrationBuilder.DropForeignKey(name: "FK_Patients_CorporatePanels_CorporatePanelId", table: "Patients");
            migrationBuilder.DropTable(name: "LabWorkItems");
            migrationBuilder.DropTable(name: "PanelServiceRates");
            migrationBuilder.DropTable(name: "CorporatePanels");
            migrationBuilder.DropColumn(name: "CorporatePanelId", table: "Patients");
            migrationBuilder.DropColumn(name: "CorporatePanelId", table: "Challans");
            migrationBuilder.DropColumn(name: "PanelBillingMode", table: "Challans");
            migrationBuilder.DropColumn(name: "PanelPayable", table: "Challans");
            migrationBuilder.DropColumn(name: "PatientPayable", table: "Challans");
        }
    }
}
