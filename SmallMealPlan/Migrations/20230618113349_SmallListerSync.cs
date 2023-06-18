using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmallMealPlan.Migrations
{
    /// <inheritdoc />
    public partial class SmallListerSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmallListerSyncListId",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmallListerSyncListName",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmallListerSyncListId",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "SmallListerSyncListName",
                table: "UserAccounts");
        }
    }
}
