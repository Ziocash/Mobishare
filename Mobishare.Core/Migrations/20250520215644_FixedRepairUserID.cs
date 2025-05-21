using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mobishare.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixedRepairUserID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Repairs_AspNetUsers_UserId1",
                table: "Repairs");

            migrationBuilder.DropIndex(
                name: "IX_Repairs_UserId1",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Repairs");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Repairs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Repairs_UserId",
                table: "Repairs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Repairs_AspNetUsers_UserId",
                table: "Repairs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Repairs_AspNetUsers_UserId",
                table: "Repairs");

            migrationBuilder.DropIndex(
                name: "IX_Repairs_UserId",
                table: "Repairs");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Repairs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Repairs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Repairs_UserId1",
                table: "Repairs",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Repairs_AspNetUsers_UserId1",
                table: "Repairs",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
