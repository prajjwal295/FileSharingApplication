using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileSharing.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShareLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownloadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadAudits_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DownloadAudits_ShareLinks_ShareLinkId",
                        column: x => x.ShareLinkId,
                        principalTable: "ShareLinks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadAudits_DownloadedAtUtc",
                table: "DownloadAudits",
                column: "DownloadedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadAudits_FileId",
                table: "DownloadAudits",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadAudits_ShareLinkId",
                table: "DownloadAudits",
                column: "ShareLinkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadAudits");
        }
    }
}
