using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class EditWebsitesClass2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Publish",
                table: "ClientArticles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Publish",
                table: "ClientArticles");
        }
    }
}
