using Microsoft.EntityFrameworkCore.Migrations;

namespace Scv.Db.Migrations
{
    public partial class PartIdAgencyId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "agency_id",
                table: "request_file_access",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "part_id",
                table: "request_file_access",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "agency_id",
                table: "request_file_access");

            migrationBuilder.DropColumn(
                name: "part_id",
                table: "request_file_access");
        }
    }
}
