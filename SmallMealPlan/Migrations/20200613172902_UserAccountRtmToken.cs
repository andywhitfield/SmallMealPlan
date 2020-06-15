using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallMealPlan.Migrations
{
    public partial class UserAccountRtmToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RememberTheMilkToken",
                table: "UserAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RememberTheMilkToken",
                table: "UserAccounts");
        }
    }
}
