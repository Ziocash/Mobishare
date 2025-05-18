using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mobishare.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddedVehiclePositionTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Positions",
                newName: "GpsReceptionTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "GpsEmissionTime",
                table: "Positions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GpsEmissionTime",
                table: "Positions");

            migrationBuilder.RenameColumn(
                name: "GpsReceptionTime",
                table: "Positions",
                newName: "CreatedAt");
        }
    }
}
