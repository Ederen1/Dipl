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
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "RequestLinks",
                columns: table => new
                {
                    LinkId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NotifyOnUpload = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsProtected = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllFilesSizeLimit = table.Column<long>(type: "INTEGER", nullable: false),
                    AllowedExtensions = table.Column<string>(type: "TEXT", nullable: false),
                    LinkTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VerifierSalt = table.Column<byte[]>(type: "BLOB", nullable: true),
                    VerifierHash = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Salt = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLinks", x => x.LinkId);
                    table.ForeignKey(
                        name: "FK_RequestLinks_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadLinks",
                columns: table => new
                {
                    LinkId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Uploaded = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LinkTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VerifierSalt = table.Column<byte[]>(type: "BLOB", nullable: true),
                    VerifierHash = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Salt = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadLinks", x => x.LinkId);
                    table.ForeignKey(
                        name: "FK_UploadLinks_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestLinkUploadSlots",
                columns: table => new
                {
                    RequestLinkUploadSlotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false),
                    Uploaded = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RequestLinkId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLinkUploadSlots", x => x.RequestLinkUploadSlotId);
                    table.ForeignKey(
                        name: "FK_RequestLinkUploadSlots_RequestLinks_RequestLinkId",
                        column: x => x.RequestLinkId,
                        principalTable: "RequestLinks",
                        principalColumn: "LinkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "UserName" },
                values: new object[] { "f55aa676-775d-4312-b31c-e9d5848e06d7", "guest@example.com", "Guest" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestLinks_CreatedById",
                table: "RequestLinks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLinkUploadSlots_RequestLinkId",
                table: "RequestLinkUploadSlots",
                column: "RequestLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadLinks_CreatedById",
                table: "UploadLinks",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestLinkUploadSlots");

            migrationBuilder.DropTable(
                name: "UploadLinks");

            migrationBuilder.DropTable(
                name: "RequestLinks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
