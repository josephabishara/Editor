using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class EditWebsitesClass3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebsiteType",
                table: "ClientArticles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebsiteType",
                table: "ClientArticles");
        }
    }
}
