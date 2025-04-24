using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mobishare.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixedVehicleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_ParkingSlots_ParkingSlotId1",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PricePerMinute",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "ParkingSlotId1",
                table: "Vehicles",
                newName: "VehicleTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_ParkingSlotId1",
                table: "Vehicles",
                newName: "IX_Vehicles_VehicleTypeId");

            migrationBuilder.AlterColumn<int>(
                name: "ParkingSlotId",
                table: "Vehicles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    PricePerMinute = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ParkingSlotId",
                table: "Vehicles",
                column: "ParkingSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_ParkingSlots_ParkingSlotId",
                table: "Vehicles",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleTypes_VehicleTypeId",
                table: "Vehicles",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_ParkingSlots_ParkingSlotId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleTypes_VehicleTypeId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "VehicleTypes");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_ParkingSlotId",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "VehicleTypeId",
                table: "Vehicles",
                newName: "ParkingSlotId1");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_VehicleTypeId",
                table: "Vehicles",
                newName: "IX_Vehicles_ParkingSlotId1");

            migrationBuilder.AlterColumn<string>(
                name: "ParkingSlotId",
                table: "Vehicles",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Vehicles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "PricePerMinute",
                table: "Vehicles",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Vehicles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_ParkingSlots_ParkingSlotId1",
                table: "Vehicles",
                column: "ParkingSlotId1",
                principalTable: "ParkingSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
