using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mobishare.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddedPositionsToRide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PositionEndId",
                table: "Rides",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PositionStartId",
                table: "Rides",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Rides_PositionEndId",
                table: "Rides",
                column: "PositionEndId");

            migrationBuilder.CreateIndex(
                name: "IX_Rides_PositionStartId",
                table: "Rides",
                column: "PositionStartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Positions_PositionEndId",
                table: "Rides",
                column: "PositionEndId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Positions_PositionStartId",
                table: "Rides",
                column: "PositionStartId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Positions_PositionEndId",
                table: "Rides");

            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Positions_PositionStartId",
                table: "Rides");

            migrationBuilder.DropIndex(
                name: "IX_Rides_PositionEndId",
                table: "Rides");

            migrationBuilder.DropIndex(
                name: "IX_Rides_PositionStartId",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "PositionEndId",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "PositionStartId",
                table: "Rides");
        }
    }
}
