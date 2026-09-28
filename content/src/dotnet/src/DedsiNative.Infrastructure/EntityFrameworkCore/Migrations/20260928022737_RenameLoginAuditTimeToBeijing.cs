using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DedsiNative.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RenameLoginAuditTimeToBeijing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LoginTimeUtc",
                schema: "DedsiNative",
                table: "LoginAudits",
                newName: "LoginTime");

            migrationBuilder.RenameIndex(
                name: "IX_LoginAudits_UserId_LoginTimeUtc",
                schema: "DedsiNative",
                table: "LoginAudits",
                newName: "IX_LoginAudits_UserId_LoginTime");

            migrationBuilder.RenameIndex(
                name: "IX_LoginAudits_LoginTimeUtc",
                schema: "DedsiNative",
                table: "LoginAudits",
                newName: "IX_LoginAudits_LoginTime");

            migrationBuilder.RenameIndex(
                name: "IX_LoginAudits_Account_LoginTimeUtc",
                schema: "DedsiNative",
                table: "LoginAudits",
                newName: "IX_LoginAudits_Account_LoginTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LoginTime",
                schema: "DedsiNative",
                table: "LoginAudits",
                newName: "LoginTimeUtc");

            migrationBuilder.RenameIndex(
                name: "IX_LoginAudits_UserId_LoginTime",
                schema: "DedsiNative",
                table: "LoginAudits",
                newName: "IX_LoginAudits_UserId_LoginTimeUtc");

            migrationBuilder.RenameIndex(
                name: "IX_LoginAudits_LoginTime",
                schema: "DedsiNative",
                table: "LoginAudits",
                newName: "IX_LoginAudits_LoginTimeUtc");

            migrationBuilder.RenameIndex(
                name: "IX_LoginAudits_Account_LoginTime",
                schema: "DedsiNative",
                table: "LoginAudits",
                newName: "IX_LoginAudits_Account_LoginTimeUtc");
        }
    }
}
