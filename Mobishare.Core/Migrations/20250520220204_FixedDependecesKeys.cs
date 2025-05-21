using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mobishare.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixedDependecesKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Positions_PositionEndId",
                table: "Rides");

            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Positions_PositionStartId",
                table: "Rides");

            migrationBuilder.AlterColumn<int>(
                name: "PositionStartId",
                table: "Rides",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "PositionEndId",
                table: "Rides",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Positions_PositionEndId",
                table: "Rides",
                column: "PositionEndId",
                principalTable: "Positions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Positions_PositionStartId",
                table: "Rides",
                column: "PositionStartId",
                principalTable: "Positions",
                principalColumn: "Id");
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

            migrationBuilder.AlterColumn<int>(
                name: "PositionStartId",
                table: "Rides",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PositionEndId",
                table: "Rides",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

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
    }
}
