using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mobishare.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddedParkingSlotType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ParkingSlots",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "ParkingSlots");
        }
    }
}
