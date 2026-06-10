using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class EditPublicationClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Images",
                table: "NewsPapers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Circulation",
                table: "ClientNewsPapers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "ClientNewsPapers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Images",
                table: "ClientNewsPapers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "ClientNewsPapers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaTier",
                table: "ClientNewsPapers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "ClientNewsPapers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "ClientNewsPapers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Reach",
                table: "ClientNewsPapers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ClientNewsPapers_ParentId",
                table: "ClientNewsPapers",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientNewsPapers_ClientNewsPapers_ParentId",
                table: "ClientNewsPapers",
                column: "ParentId",
                principalTable: "ClientNewsPapers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientNewsPapers_ClientNewsPapers_ParentId",
                table: "ClientNewsPapers");

            migrationBuilder.DropIndex(
                name: "IX_ClientNewsPapers_ParentId",
                table: "ClientNewsPapers");

            migrationBuilder.DropColumn(
                name: "Images",
                table: "NewsPapers");

            migrationBuilder.DropColumn(
                name: "Circulation",
                table: "ClientNewsPapers");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "ClientNewsPapers");

            migrationBuilder.DropColumn(
                name: "Images",
                table: "ClientNewsPapers");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "ClientNewsPapers");

            migrationBuilder.DropColumn(
                name: "MediaTier",
                table: "ClientNewsPapers");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "ClientNewsPapers");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "ClientNewsPapers");

            migrationBuilder.DropColumn(
                name: "Reach",
                table: "ClientNewsPapers");
        }
    }
}
