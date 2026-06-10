using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddReportClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Publish = table.Column<bool>(type: "bit", nullable: false),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    Writed = table.Column<int>(type: "int", nullable: true),
                    Deleted = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteId = table.Column<int>(type: "int", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_Clients_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    Writed = table.Column<int>(type: "int", nullable: true),
                    Deleted = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteId = table.Column<int>(type: "int", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportArticles_GeneralArticles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "GeneralArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportArticles_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportNewspapers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    NewspaperId = table.Column<int>(type: "int", nullable: false),
                    Writed = table.Column<int>(type: "int", nullable: true),
                    Deleted = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteId = table.Column<int>(type: "int", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportNewspapers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportNewspapers_NewsPapers_NewspaperId",
                        column: x => x.NewspaperId,
                        principalTable: "NewsPapers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportNewspapers_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportArticles_ArticleId",
                table: "ReportArticles",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportArticles_ReportId",
                table: "ReportArticles",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportNewspapers_NewspaperId",
                table: "ReportNewspapers",
                column: "NewspaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportNewspapers_ReportId",
                table: "ReportNewspapers",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CustomerId",
                table: "Reports",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportArticles");

            migrationBuilder.DropTable(
                name: "ReportNewspapers");

            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
