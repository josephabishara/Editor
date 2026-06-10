using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddChildMediaClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "ClientArticles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientArticles_ParentId",
                table: "ClientArticles",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientArticles_ClientArticles_ParentId",
                table: "ClientArticles",
                column: "ParentId",
                principalTable: "ClientArticles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientArticles_ClientArticles_ParentId",
                table: "ClientArticles");

            migrationBuilder.DropIndex(
                name: "IX_ClientArticles_ParentId",
                table: "ClientArticles");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "ClientArticles");
        }
    }
}
