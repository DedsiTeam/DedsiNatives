using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DedsiNative.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_User_Organizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserOrganizations",
                schema: "DedsiNative",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOrganizations", x => new { x.UserId, x.OrganizationId });
                    table.ForeignKey(
                        name: "FK_UserOrganizations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "DedsiNative",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOrganizations",
                schema: "DedsiNative");
        }
    }
}
