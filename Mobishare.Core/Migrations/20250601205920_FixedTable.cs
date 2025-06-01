using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mobishare.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "MessagePairs");

            migrationBuilder.AddColumn<string>(
                name: "Embedding",
                table: "ChatMessages",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "ChatMessages");

            migrationBuilder.AddColumn<string>(
                name: "Embedding",
                table: "MessagePairs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
