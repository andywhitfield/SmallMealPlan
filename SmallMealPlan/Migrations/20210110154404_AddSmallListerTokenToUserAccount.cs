using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallMealPlan.Migrations
{
    public partial class AddSmallListerTokenToUserAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmallListerToken",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmallListerToken",
                table: "UserAccounts");
        }
    }
}
