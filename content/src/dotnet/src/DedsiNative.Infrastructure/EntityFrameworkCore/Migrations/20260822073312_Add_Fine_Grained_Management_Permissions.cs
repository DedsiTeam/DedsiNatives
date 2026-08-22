using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DedsiNative.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_Fine_Grained_Management_Permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "DedsiNative",
                table: "Permissions",
                columns: new[] { "Id", "ConcurrencyStamp", "CreationTime", "CreatorId", "CreatorName", "Description", "ExtraProperties", "IsEnabled", "Name", "SystemId", "SystemName" },
                values: new object[,]
                {
                    { "01ARZ3NDEKTSV4RRFFQ69G5FB3", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "查看 SSO 客户端、作用域、授权与令牌。", "{}", true, "system:openiddict:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FB4", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "管理 SSO 客户端、作用域、授权与令牌。", "{}", true, "system:openiddict:manage", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC0", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "查看系统。", "{}", true, "system:systems:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC1", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "创建系统。", "{}", true, "system:systems:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC2", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "修改系统。", "{}", true, "system:systems:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC3", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "删除系统。", "{}", true, "system:systems:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC4", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "查看权限定义。", "{}", true, "system:permissions:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC5", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "创建权限定义。", "{}", true, "system:permissions:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC6", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "修改权限定义。", "{}", true, "system:permissions:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC7", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "删除权限定义。", "{}", true, "system:permissions:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC8", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "查看菜单。", "{}", true, "system:menus:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC9", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "创建菜单。", "{}", true, "system:menus:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCA", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "修改菜单。", "{}", true, "system:menus:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCB", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "删除菜单。", "{}", true, "system:menus:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCC", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "查看岗位。", "{}", true, "system:positions:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCD", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "创建岗位。", "{}", true, "system:positions:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCE", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "修改岗位。", "{}", true, "system:positions:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCF", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "删除岗位。", "{}", true, "system:positions:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCG", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "配置岗位权限与组织范围。", "{}", true, "system:positions:assign", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCH", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "查看组织机构。", "{}", true, "system:organizations:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCJ", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "创建组织机构。", "{}", true, "system:organizations:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCK", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "修改组织机构。", "{}", true, "system:organizations:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCM", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "删除组织机构。", "{}", true, "system:organizations:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCN", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "查看用户。", "{}", true, "system:users:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCP", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "创建用户。", "{}", true, "system:users:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCQ", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "修改用户。", "{}", true, "system:users:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCR", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "删除用户。", "{}", true, "system:users:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCS", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "重置用户密码。", "{}", true, "system:users:reset-password", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCT", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "分配用户岗位。", "{}", true, "system:users:assign-position", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCV", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "查看、预览和下载文件。", "{}", true, "system:storage:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCW", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "上传文件。", "{}", true, "system:storage:upload", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCX", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "删除文件。", "{}", true, "system:storage:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCY", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "查看字典及字典项。", "{}", true, "system:dictionaries:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCZ", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "创建字典及字典项。", "{}", true, "system:dictionaries:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FD0", null, new DateTime(2026, 8, 4, 10, 30, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "system", "修改字典及字典项。", "{}", true, "system:dictionaries:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" }
                });

            migrationBuilder.InsertData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                columns: new[] { "PermissionId", "PositionId", "PermissionName", "SystemId", "SystemName" },
                values: new object[,]
                {
                    { "01ARZ3NDEKTSV4RRFFQ69G5FB3", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:openiddict:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FB4", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:openiddict:manage", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC0", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:systems:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC1", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:systems:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC2", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:systems:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC3", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:systems:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC4", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:permissions:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC5", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:permissions:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC6", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:permissions:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC7", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:permissions:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC8", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:menus:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FC9", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:menus:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCA", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:menus:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCB", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:menus:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCC", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:positions:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCD", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:positions:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCE", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:positions:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCF", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:positions:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCG", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:positions:assign", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCH", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:organizations:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCJ", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:organizations:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCK", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:organizations:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCM", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:organizations:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCN", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:users:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCP", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:users:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCQ", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:users:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCR", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:users:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCS", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:users:reset-password", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCT", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:users:assign-position", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCV", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:storage:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCW", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:storage:upload", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCX", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:storage:delete", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCY", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:dictionaries:view", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FCZ", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:dictionaries:create", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" },
                    { "01ARZ3NDEKTSV4RRFFQ69G5FD0", "01ARZ3NDEKTSV4RRFFQ69G5FB0", "system:dictionaries:update", "01ARZ3NDEKTSV4RRFFQ69G5FAV", "身份管理系统" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FB3");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FB4");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FC0");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FC1");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FC2");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FC3");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FC4");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FC5");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FC6");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FC7");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FC8");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FC9");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCA");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCB");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCC");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCD");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCE");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCF");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCG");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCH");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCJ");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCK");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCM");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCN");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCP");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCQ");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCR");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCS");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCT");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCV");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCW");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCX");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCY");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FCZ");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "01ARZ3NDEKTSV4RRFFQ69G5FD0");

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FB3", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FB4", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FC0", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FC1", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FC2", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FC3", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FC4", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FC5", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FC6", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FC7", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FC8", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FC9", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCA", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCB", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCC", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCD", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCE", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCF", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCG", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCH", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCJ", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCK", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCM", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCN", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCP", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCQ", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCR", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCS", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCT", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCV", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCW", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCX", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCY", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FCZ", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });

            migrationBuilder.DeleteData(
                schema: "DedsiNative",
                table: "PositionPermissions",
                keyColumns: new[] { "PermissionId", "PositionId" },
                keyValues: new object[] { "01ARZ3NDEKTSV4RRFFQ69G5FD0", "01ARZ3NDEKTSV4RRFFQ69G5FB0" });
        }
    }
}
