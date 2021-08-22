using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallMealPlan.Migrations
{
    public partial class UserAccountAddLastListId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RememberTheMilkLastListId",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmallListerLastListId",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RememberTheMilkLastListId",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "SmallListerLastListId",
                table: "UserAccounts");
        }
    }
}
