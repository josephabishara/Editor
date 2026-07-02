using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralNewspaper2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportArticles_GeneralArticles_ArticleId",
                table: "ReportArticles");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportNewspapers_NewsPapers_NewspaperId",
                table: "ReportNewspapers");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportArticles_ClientArticles_ArticleId",
                table: "ReportArticles",
                column: "ArticleId",
                principalTable: "ClientArticles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportNewspapers_ClientNewsPapers_NewspaperId",
                table: "ReportNewspapers",
                column: "NewspaperId",
                principalTable: "ClientNewsPapers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportArticles_ClientArticles_ArticleId",
                table: "ReportArticles");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportNewspapers_ClientNewsPapers_NewspaperId",
                table: "ReportNewspapers");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportArticles_GeneralArticles_ArticleId",
                table: "ReportArticles",
                column: "ArticleId",
                principalTable: "GeneralArticles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportNewspapers_NewsPapers_NewspaperId",
                table: "ReportNewspapers",
                column: "NewspaperId",
                principalTable: "NewsPapers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
