using Microsoft.EntityFrameworkCore.Migrations;

namespace Scv.Db.Migrations
{
    public partial class IUserName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "user_name",
                table: "request_file_access",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_name",
                table: "request_file_access");
        }
    }
}
