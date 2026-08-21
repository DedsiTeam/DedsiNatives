using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DedsiNative.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_Organizations_And_StorageFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizations",
                schema: "DedsiNative",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SystemId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name1 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Name2 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Name3 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Name4 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ParentId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: true),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizations_Organizations_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "DedsiNative",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organizations_Systems_SystemId",
                        column: x => x.SystemId,
                        principalSchema: "DedsiNative",
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorageFiles",
                schema: "DedsiNative",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StorageName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Extension = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageType = table.Column<int>(type: "integer", nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Md5Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageFiles", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FB2",
                column: "Name",
                value: "system:login-audits:view");

            migrationBuilder.UpdateData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FB2", "01ARZ3NDEKTSV4RRFFQ69G5FB0" },
                column: "PermissionName",
                value: "system:login-audits:view");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_ParentId",
                schema: "DedsiNative",
                table: "Organizations",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Sort",
                schema: "DedsiNative",
                table: "Organizations",
                column: "Sort");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_SystemId_Code",
                schema: "DedsiNative",
                table: "Organizations",
                columns: new[] { "SystemId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_SystemId_ParentId",
                schema: "DedsiNative",
                table: "Organizations",
                columns: new[] { "SystemId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_StorageFiles_Category",
                schema: "DedsiNative",
                table: "StorageFiles",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_StorageFiles_CreationTime",
                schema: "DedsiNative",
                table: "StorageFiles",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_StorageFiles_Md5Hash",
                schema: "DedsiNative",
                table: "StorageFiles",
                column: "Md5Hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organizations",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "StorageFiles",
                schema: "DedsiNative");

            migrationBuilder.UpdateData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FB2",
                column: "Name",
                value: "LoginAudits.View");

            migrationBuilder.UpdateData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FB2", "01ARZ3NDEKTSV4RRFFQ69G5FB0" },
                column: "PermissionName",
                value: "LoginAudits.View");
        }
    }
}
