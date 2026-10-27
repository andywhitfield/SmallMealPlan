using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmallMealPlan.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRememberTheMilk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RememberTheMilkLastListId",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "RememberTheMilkToken",
                table: "UserAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RememberTheMilkLastListId",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RememberTheMilkToken",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);
        }
    }
}
