using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmallMealPlan.Migrations
{
    /// <inheritdoc />
    public partial class Webauthn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AuthenticationUri",
                table: "UserAccounts",
                newName: "Email");

            migrationBuilder.CreateTable(
                name: "UserAccountCredentials",
                columns: table => new
                {
                    UserAccountCredentialId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    CredentialId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UserHandle = table.Column<byte[]>(type: "BLOB", nullable: false),
                    SignatureCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountCredentials", x => x.UserAccountCredentialId);
                    table.ForeignKey(
                        name: "FK_UserAccountCredentials_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountCredentials_UserAccountId",
                table: "UserAccountCredentials",
                column: "UserAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccountCredentials");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "UserAccounts",
                newName: "AuthenticationUri");
        }
    }
}
