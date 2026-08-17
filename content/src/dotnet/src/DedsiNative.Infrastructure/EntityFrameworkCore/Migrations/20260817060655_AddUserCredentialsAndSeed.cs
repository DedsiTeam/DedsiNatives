using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DedsiNative.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCredentialsAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Account",
                schema: "DedsiNative",
                table: "Users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                schema: "DedsiNative",
                table: "Users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordSalt",
                schema: "DedsiNative",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.InsertData(
                schema: "DedsiNative",
                table: "Users",
                columns: new[] { "Id", "Account", "ConcurrencyStamp", "CreationTime", "CreatorId", "CreatorName", "Email", "ExtraProperties", "Name", "PasswordHash", "PasswordSalt" },
                values: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FAV", "15833084138", null, new DateTime(2026, 8, 17, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "system", "admin@dedsinative.local", "{}", "超级管理员", "DqpyFntIjpkXAwEXsqcW5PDBfi27fXEnDcuC4v4f3/Q=", "XMTFCyq7q+8jOGe5ihk1eA==" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Account",
                schema: "DedsiNative",
                table: "Users",
                column: "Account",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Account",
                schema: "DedsiNative",
                table: "Users");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Users",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FAV");

            migrationBuilder.DropColumn(
                name: "Account",
                schema: "DedsiNative",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                schema: "DedsiNative",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                schema: "DedsiNative",
                table: "Users");
        }
    }
}
