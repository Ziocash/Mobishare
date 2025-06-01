using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mobishare.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddedMessagePair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessagePairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserMessageId = table.Column<int>(type: "INTEGER", nullable: false),
                    AiMessageId = table.Column<int>(type: "INTEGER", nullable: false),
                    Embedding = table.Column<string>(type: "TEXT", nullable: false),
                    IsForRag = table.Column<bool>(type: "INTEGER", nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", nullable: false),
                    Answered = table.Column<bool>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagePairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagePairs_ChatMessages_AiMessageId",
                        column: x => x.AiMessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessagePairs_ChatMessages_UserMessageId",
                        column: x => x.UserMessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessagePairs_AiMessageId",
                table: "MessagePairs",
                column: "AiMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagePairs_UserMessageId",
                table: "MessagePairs",
                column: "UserMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessagePairs");
        }
    }
}
