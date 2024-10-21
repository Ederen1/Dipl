using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dipl.Business.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Read = table.Column<bool>(type: "INTEGER", nullable: false),
                    Write = table.Column<bool>(type: "INTEGER", nullable: false),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                    table.ForeignKey(
                        name: "FK_Permissions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupUser",
                columns: table => new
                {
                    GroupsGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsersUserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupUser", x => new { x.GroupsGroupId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_GroupUser_Groups_GroupsGroupId",
                        column: x => x.GroupsGroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupUser_Users_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Links",
                columns: table => new
                {
                    LinkId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LinkType = table.Column<int>(type: "INT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAccessed = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: false),
                    PermissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    NotifyOnUpload = table.Column<bool>(type: "INTEGER", nullable: false),
                    LinkClosed = table.Column<bool>(type: "INTEGER", nullable: false),
                    LinkName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Links", x => x.LinkId);
                    table.ForeignKey(
                        name: "FK_Links_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Links_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionUser",
                columns: table => new
                {
                    PermissionsAssociatedWithThisUserPermissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsersUserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionUser", x => new { x.PermissionsAssociatedWithThisUserPermissionId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_PermissionUser_Permissions_PermissionsAssociatedWithThisUserPermissionId",
                        column: x => x.PermissionsAssociatedWithThisUserPermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionUser_Users_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Groups",
                columns: new[] { "GroupId", "Description", "Name" },
                values: new object[] { -1, "", "Guest" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "UserName" },
                values: new object[] { new Guid("f55aa676-775d-4312-b31c-e9d5848e06d7"), "guest@example.com", "Guest" });

            migrationBuilder.InsertData(
                table: "GroupUser",
                columns: new[] { "GroupsGroupId", "UsersUserId" },
                values: new object[] { -1, new Guid("f55aa676-775d-4312-b31c-e9d5848e06d7") });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "GroupId", "Read", "Write" },
                values: new object[] { -1, -1, true, false });

            migrationBuilder.CreateIndex(
                name: "IX_GroupUser_UsersUserId",
                table: "GroupUser",
                column: "UsersUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Links_CreatedById",
                table: "Links",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Links_PermissionId",
                table: "Links",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_GroupId",
                table: "Permissions",
                column: "GroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionUser_UsersUserId",
                table: "PermissionUser",
                column: "UsersUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupUser");

            migrationBuilder.DropTable(
                name: "Links");

            migrationBuilder.DropTable(
                name: "PermissionUser");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
