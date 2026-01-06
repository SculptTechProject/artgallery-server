using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace artgallery_server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExhibitionCapacityAndTicketDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_UserId",
                table: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Tickets",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Exhibitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_UserId",
                table: "Tickets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_UserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Exhibitions");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_UserId",
                table: "Tickets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
