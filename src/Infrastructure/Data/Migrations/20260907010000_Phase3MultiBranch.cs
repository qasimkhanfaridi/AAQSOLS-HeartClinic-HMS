using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeartClinicHms.Infrastructure.Data.Migrations
{
    public partial class Phase3MultiBranch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Branches",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsImaging",
                table: "CatalogServices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Branches SET Region = 'Rawalpindi' WHERE Code = 'THC-MAIN'");
            migrationBuilder.Sql("UPDATE CatalogServices SET IsImaging = 1 WHERE Code = 'HOLTER'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Region", table: "Branches");
            migrationBuilder.DropColumn(name: "IsImaging", table: "CatalogServices");
        }
    }
}
