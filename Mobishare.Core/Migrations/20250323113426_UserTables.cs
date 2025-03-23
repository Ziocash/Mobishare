using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mobishare.Core.Migrations
{
    /// <inheritdoc />
    public partial class UserTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Balances_AspNetUsers_UserId1",
                table: "Balances");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_AspNetUsers_UserId1",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_UserId1",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Balances_UserId1",
                table: "Balances");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Balances");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Feedback",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Balances",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_UserId",
                table: "Feedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_UserId",
                table: "Balances",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_AspNetUsers_UserId",
                table: "Balances",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_AspNetUsers_UserId",
                table: "Feedback",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Balances_AspNetUsers_UserId",
                table: "Balances");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_AspNetUsers_UserId",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_UserId",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Balances_UserId",
                table: "Balances");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Feedback",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Feedback",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Balances",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Balances",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_UserId1",
                table: "Feedback",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_UserId1",
                table: "Balances",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_AspNetUsers_UserId1",
                table: "Balances",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_AspNetUsers_UserId1",
                table: "Feedback",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
