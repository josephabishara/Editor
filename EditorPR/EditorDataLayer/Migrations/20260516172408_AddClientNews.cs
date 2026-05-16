using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddClientNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PRValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ADValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PROption = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ADOption = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ArticleBranding = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HeadlineBranding = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    pictureInArticle = table.Column<bool>(type: "bit", nullable: false),
                    Generation = table.Column<bool>(type: "bit", nullable: false),
                    Toning = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Translation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_News", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientNews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    publicationId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    WriterId = table.Column<int>(type: "int", nullable: false),
                    Pages = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PRValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ADValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ADOption = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PROption = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ArticleBranding = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HeadlineBranding = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    pictureInArticle = table.Column<bool>(type: "bit", nullable: false),
                    Generation = table.Column<bool>(type: "bit", nullable: false),
                    Toning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Translation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Publish = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ClientNews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientNews_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientNews_News_NewsId",
                        column: x => x.NewsId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientNews_Writers_WriterId",
                        column: x => x.WriterId,
                        principalTable: "Writers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientNews_ClientId",
                table: "ClientNews",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientNews_NewsId",
                table: "ClientNews",
                column: "NewsId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientNews_WriterId",
                table: "ClientNews",
                column: "WriterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientNews");

            migrationBuilder.DropTable(
                name: "News");
        }
    }
}
