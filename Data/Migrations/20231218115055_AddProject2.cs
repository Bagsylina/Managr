using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Managr.Data.Migrations
{
    public partial class AddProject2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectOrganizerId",
                table: "Tasks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectOrganizerId",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
