using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DedsiNative.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "DedsiNative");

            migrationBuilder.CreateTable(
                name: "LoginAudits",
                schema: "DedsiNative",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    LoginTimeUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Result = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Reason = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Account = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FailureDescription = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Systems",
                schema: "DedsiNative",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Sort = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatorName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Systems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "DedsiNative",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IdCardNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastLoginTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastLoginIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SoftDeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatorName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dictionaries",
                schema: "DedsiNative",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SystemId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatorName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dictionaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dictionaries_Systems_SystemId",
                        column: x => x.SystemId,
                        principalSchema: "DedsiNative",
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "DedsiNative",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SystemId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatorName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Systems_SystemId",
                        column: x => x.SystemId,
                        principalSchema: "DedsiNative",
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                schema: "DedsiNative",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SystemId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatorName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Positions_Systems_SystemId",
                        column: x => x.SystemId,
                        principalSchema: "DedsiNative",
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserLoginInfos",
                schema: "DedsiNative",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Account = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PasswordSalt = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLoginInfos", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserLoginInfos_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "DedsiNative",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPositions",
                schema: "DedsiNative",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    PositionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPositions", x => new { x.UserId, x.PositionId });
                    table.ForeignKey(
                        name: "FK_UserPositions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "DedsiNative",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DictionaryItems",
                schema: "DedsiNative",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    DictionaryId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    ParentId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DictionaryItems_Dictionaries_DictionaryId",
                        column: x => x.DictionaryId,
                        principalSchema: "DedsiNative",
                        principalTable: "Dictionaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DictionaryItems_DictionaryItems_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "DedsiNative",
                        principalTable: "DictionaryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                schema: "DedsiNative",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SystemId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RoutePath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Component = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Redirect = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PermissionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: true),
                    PermissionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    IsDisabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsExternal = table.Column<bool>(type: "boolean", nullable: false),
                    ExternalUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    KeepAlive = table.Column<bool>(type: "boolean", nullable: false),
                    IsAffix = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatorName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Menus_Menus_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "DedsiNative",
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Menus_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "DedsiNative",
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Menus_Systems_SystemId",
                        column: x => x.SystemId,
                        principalSchema: "DedsiNative",
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PositionOrganizations",
                schema: "DedsiNative",
                columns: table => new
                {
                    PositionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    OrganizationId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionOrganizations", x => new { x.PositionId, x.OrganizationId });
                    table.ForeignKey(
                        name: "FK_PositionOrganizations_Positions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "DedsiNative",
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PositionPermissions",
                schema: "DedsiNative",
                columns: table => new
                {
                    PositionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    PermissionId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    PermissionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SystemId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionPermissions", x => new { x.PositionId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_PositionPermissions_Positions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "DedsiNative",
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "DedsiNative",
                table: "Systems",
                columns: new[] { "Id", "ConcurrencyStamp", "CreationTime", "CreatorId", "CreatorName", "Description", "ExtraProperties", "Name" },
                values: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FAV", null, new DateTime(2026, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "DedsiNative 基础身份与授权管理。", "{}", "身份管理系统" });

            migrationBuilder.InsertData(
                schema: "DedsiNative",
                table: "Users",
                columns: new[] { "Id", "ConcurrencyStamp", "CreationTime", "CreatorId", "CreatorName", "Email", "ExtraProperties", "IdCardNumber", "LastLoginIp", "LastLoginTime", "LastUpdatedAt", "Name", "Phone", "SoftDeletedAt" },
                values: new object[] { new Guid("01951500-0000-7000-8000-000000000001"), null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "cohenwang@example.com", "{}", null, null, null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "CohenWang", null, null });

            migrationBuilder.InsertData(
                schema: "DedsiNative",
                table: "Permissions",
                columns: new[] { "Id", "ConcurrencyStamp", "CreationTime", "CreatorId", "CreatorName", "Description", "ExtraProperties", "IsEnabled", "Name", "SystemId", "SystemName" },
                values: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FB2", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "查看登录审计列表和详情。", "{}", true, "LoginAudits.View", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" });

            migrationBuilder.InsertData(
                schema: "DedsiNative",
                table: "Positions",
                columns: new[] { "Id", "ConcurrencyStamp", "CreationTime", "CreatorId", "CreatorName", "Description", "ExtraProperties", "IsEnabled", "Name", "SystemId", "SystemName" },
                values: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FB0", null, new DateTime(2026, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "拥有身份管理系统的基础管理权限。", "{}", true, "系统管理员", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" });

            migrationBuilder.InsertData(
                schema: "DedsiNative",
                table: "UserLoginInfos",
                columns: new[] { "UserId", "Account", "PasswordHash", "PasswordSalt", "Status" },
                values: new object[] { new Guid("01951500-0000-7000-8000-000000000001"), "CohenWang", "W/2szOheNj12boeq2Lb+T8mtJsWknqskgTxfEcbPV68=", "AQIDBAUGBwgJCgsMDQ4PEA==", "Normal" });

            migrationBuilder.InsertData(
                schema: "DedsiNative",
                table: "UserPositions",
                columns: new[] { "PositionId", "UserId", "PositionName" },
                values: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FB0", new Guid("01951500-0000-7000-8000-000000000001"), "系统管理员" });

            migrationBuilder.InsertData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                columns: new[] { "PermissionId", "PositionId", "PermissionName", "SystemId", "SystemName" },
                values: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FB2", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "LoginAudits.View", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" });

            migrationBuilder.CreateIndex(
                name: "IX_Dictionaries_SystemId_Name",
                schema: "DedsiNative",
                table: "Dictionaries",
                columns: new[] { "SystemId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryItems_DictionaryId_Code",
                schema: "DedsiNative",
                table: "DictionaryItems",
                columns: new[] { "DictionaryId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryItems_DictionaryId_ParentId",
                schema: "DedsiNative",
                table: "DictionaryItems",
                columns: new[] { "DictionaryId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryItems_ParentId",
                schema: "DedsiNative",
                table: "DictionaryItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAudits_Account_LoginTimeUtc",
                schema: "DedsiNative",
                table: "LoginAudits",
                columns: new[] { "Account", "LoginTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAudits_LoginTimeUtc",
                schema: "DedsiNative",
                table: "LoginAudits",
                column: "LoginTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAudits_UserId_LoginTimeUtc",
                schema: "DedsiNative",
                table: "LoginAudits",
                columns: new[] { "UserId", "LoginTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_ParentId",
                schema: "DedsiNative",
                table: "Menus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_PermissionId",
                schema: "DedsiNative",
                table: "Menus",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_SystemId_Code",
                schema: "DedsiNative",
                table: "Menus",
                columns: new[] { "SystemId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_SystemId_ParentId",
                schema: "DedsiNative",
                table: "Menus",
                columns: new[] { "SystemId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_SystemId",
                schema: "DedsiNative",
                table: "Permissions",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_SystemId",
                schema: "DedsiNative",
                table: "Positions",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLoginInfos_Account",
                schema: "DedsiNative",
                table: "UserLoginInfos",
                column: "Account",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DictionaryItems",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "LoginAudits",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "Menus",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "PositionOrganizations",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "PositionPermissions",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "UserLoginInfos",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "UserPositions",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "Dictionaries",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "Positions",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "DedsiNative");

            migrationBuilder.DropTable(
                name: "Systems",
                schema: "DedsiNative");
        }
    }
}
