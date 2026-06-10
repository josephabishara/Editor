using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EditorDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class EditMediaClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Distribution",
                table: "WebsiteCustomerCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "WebsiteCustomerCategories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "WebsiteCustomerCategories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reach",
                table: "WebsiteCustomerCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Circulation",
                table: "PublicationCustomerCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Distribution",
                table: "PublicationCustomerCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "PublicationCustomerCategories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "PublicationCustomerCategories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "PublicationCustomerCategories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Distribution",
                table: "ChannelCustomerCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "ChannelCustomerCategories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reach",
                table: "ChannelCustomerCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCurrency",
                table: "ChannelCustomerCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distribution",
                table: "WebsiteCustomerCategories");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "WebsiteCustomerCategories");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "WebsiteCustomerCategories");

            migrationBuilder.DropColumn(
                name: "Reach",
                table: "WebsiteCustomerCategories");

            migrationBuilder.DropColumn(
                name: "Circulation",
                table: "PublicationCustomerCategories");

            migrationBuilder.DropColumn(
                name: "Distribution",
                table: "PublicationCustomerCategories");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "PublicationCustomerCategories");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "PublicationCustomerCategories");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "PublicationCustomerCategories");

            migrationBuilder.DropColumn(
                name: "Distribution",
                table: "ChannelCustomerCategories");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "ChannelCustomerCategories");

            migrationBuilder.DropColumn(
                name: "Reach",
                table: "ChannelCustomerCategories");

            migrationBuilder.DropColumn(
                name: "UnitCurrency",
                table: "ChannelCustomerCategories");
        }
    }
}
