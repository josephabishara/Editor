using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ADOption",
                table: "News");

            migrationBuilder.DropColumn(
                name: "PROption",
                table: "News");

            migrationBuilder.DropColumn(
                name: "ADOption",
                table: "ClientNews");

            migrationBuilder.DropColumn(
                name: "PROption",
                table: "ClientNews");

            migrationBuilder.RenameColumn(
                name: "Reach",
                table: "Websites",
                newName: "Impression");

            migrationBuilder.CreateTable(
                name: "ClientArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WebsiteId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    WriterId = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Impression = table.Column<int>(type: "int", nullable: false),
                    Reach = table.Column<int>(type: "int", nullable: false),
                    ADValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PRValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Toning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaTier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Writer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArticleBranding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadlineBranding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureinArticle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Generation = table.Column<bool>(type: "bit", nullable: true),
                    ArticleURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_ClientArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientArticles_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientVideos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VideoId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VideoFileFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScreenshotFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Toning = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_ClientVideos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientVideos_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientVideos_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneralArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WebsiteId = table.Column<int>(type: "int", nullable: false),
                    WriterId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArticleBranding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadlineBranding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureinArticle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Generation = table.Column<bool>(type: "bit", nullable: true),
                    ArticleURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_GeneralArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneralVideos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VideoFileFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScreenshotFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Toning = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_GeneralVideos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsPapers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicationId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PRValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ADValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ArticleBranding = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HeadlineBranding = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    pictureInArticle = table.Column<bool>(type: "bit", nullable: false),
                    Generation = table.Column<bool>(type: "bit", nullable: false),
                    Toning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_NewsPapers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientNewsPapers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsPaperId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    PublicationId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    WriterId = table.Column<int>(type: "int", nullable: false),
                    Pages = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PRValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ADValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ArticleBranding = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HeadlineBranding = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    pictureInArticle = table.Column<bool>(type: "bit", nullable: false),
                    Generation = table.Column<bool>(type: "bit", nullable: false),
                    Toning = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_ClientNewsPapers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientNewsPapers_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientNewsPapers_NewsPapers_NewsPaperId",
                        column: x => x.NewsPaperId,
                        principalTable: "NewsPapers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientNewsPapers_Writers_WriterId",
                        column: x => x.WriterId,
                        principalTable: "Writers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientArticles_ClientId",
                table: "ClientArticles",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientNewsPapers_ClientId",
                table: "ClientNewsPapers",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientNewsPapers_NewsPaperId",
                table: "ClientNewsPapers",
                column: "NewsPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientNewsPapers_WriterId",
                table: "ClientNewsPapers",
                column: "WriterId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientVideos_ChannelId",
                table: "ClientVideos",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientVideos_ClientId",
                table: "ClientVideos",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientArticles");

            migrationBuilder.DropTable(
                name: "ClientNewsPapers");

            migrationBuilder.DropTable(
                name: "ClientVideos");

            migrationBuilder.DropTable(
                name: "GeneralArticles");

            migrationBuilder.DropTable(
                name: "GeneralVideos");

            migrationBuilder.DropTable(
                name: "NewsPapers");

            migrationBuilder.RenameColumn(
                name: "Impression",
                table: "Websites",
                newName: "Reach");

            migrationBuilder.AddColumn<string>(
                name: "ADOption",
                table: "News",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PROption",
                table: "News",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ADOption",
                table: "ClientNews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PROption",
                table: "ClientNews",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
