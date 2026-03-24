using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmallMealPlan.Migrations
{
    /// <inheritdoc />
    public partial class NoteOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentArea",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoteSortOrdering",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrdering",
                table: "Notes",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentArea",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "NoteSortOrdering",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "SortOrdering",
                table: "Notes");
        }
    }
}
