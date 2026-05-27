using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcApp.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUserAddedArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "UserAddedArticles");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "UserAddedArticles",
                type: "longblob",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "UserAddedArticles");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "UserAddedArticles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
