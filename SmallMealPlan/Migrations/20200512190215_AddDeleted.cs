using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallMealPlan.Migrations
{
    public partial class AddDeleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTime",
                table: "UserAccounts",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTime",
                table: "Meals",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTime",
                table: "MealIngredients",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTime",
                table: "Ingredients",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedDateTime",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedDateTime",
                table: "Meals");

            migrationBuilder.DropColumn(
                name: "DeletedDateTime",
                table: "MealIngredients");

            migrationBuilder.DropColumn(
                name: "DeletedDateTime",
                table: "Ingredients");
        }
    }
}
