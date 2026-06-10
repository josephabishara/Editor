using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddChildMediaClient2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientArticles_ClientArticles_ParentId",
                table: "ClientArticles");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientArticles_ClientArticles_ParentId",
                table: "ClientArticles",
                column: "ParentId",
                principalTable: "ClientArticles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientArticles_ClientArticles_ParentId",
                table: "ClientArticles");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientArticles_ClientArticles_ParentId",
                table: "ClientArticles",
                column: "ParentId",
                principalTable: "ClientArticles",
                principalColumn: "Id");
        }
    }
}
