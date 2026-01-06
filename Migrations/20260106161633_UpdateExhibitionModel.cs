using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace artgallery_server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExhibitionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Exhibitions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Exhibitions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Exhibitions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Exhibitions");
        }
    }
}
