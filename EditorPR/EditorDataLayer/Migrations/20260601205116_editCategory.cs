using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class editCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "WebsiteCustomerCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "PublicationCustomerCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "ChannelCustomerCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_GeneralArticles_WebsiteId",
                table: "GeneralArticles",
                column: "WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralArticles_WriterId",
                table: "GeneralArticles",
                column: "WriterId");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralArticles_Websites_WebsiteId",
                table: "GeneralArticles",
                column: "WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_GeneralArticles_Writers_WriterId",
            //    table: "GeneralArticles",
            //    column: "WriterId",
            //    principalTable: "Writers",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneralArticles_Websites_WebsiteId",
                table: "GeneralArticles");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_GeneralArticles_Writers_WriterId",
            //    table: "GeneralArticles");

            migrationBuilder.DropIndex(
                name: "IX_GeneralArticles_WebsiteId",
                table: "GeneralArticles");

            migrationBuilder.DropIndex(
                name: "IX_GeneralArticles_WriterId",
                table: "GeneralArticles");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "WebsiteCustomerCategories");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "PublicationCustomerCategories");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "ChannelCustomerCategories");
        }
    }
}
