using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallMealPlan.Migrations
{
    public partial class AddNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    NoteId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserAccountId = table.Column<int>(nullable: false),
                    NoteText = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.NoteId);
                    table.ForeignKey(
                        name: "FK_Notes_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_UserAccountId",
                table: "Notes",
                column: "UserAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes");
        }
    }
}
