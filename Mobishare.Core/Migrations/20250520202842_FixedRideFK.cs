using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mobishare.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixedRideFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_AspNetUsers_UserId1",
                table: "Rides");

            migrationBuilder.DropIndex(
                name: "IX_Rides_UserId1",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Rides");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Rides",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Rides_UserId",
                table: "Rides",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_AspNetUsers_UserId",
                table: "Rides",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_AspNetUsers_UserId",
                table: "Rides");

            migrationBuilder.DropIndex(
                name: "IX_Rides_UserId",
                table: "Rides");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Rides",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Rides",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rides_UserId1",
                table: "Rides",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_AspNetUsers_UserId1",
                table: "Rides",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
