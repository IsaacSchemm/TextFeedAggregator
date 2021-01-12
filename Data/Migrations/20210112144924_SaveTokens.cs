using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TextFeedAggregator.Data.Migrations
{
    public partial class SaveTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDeviantArtTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    LastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDeviantArtTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserDeviantArtTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMastodonTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    LastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMastodonTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMastodonTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTwitterTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    AccessTokenSecret = table.Column<string>(type: "varchar(max)", nullable: true),
                    LastRead = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTwitterTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserTwitterTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMastodonTokens_UserId",
                table: "UserMastodonTokens",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDeviantArtTokens");

            migrationBuilder.DropTable(
                name: "UserMastodonTokens");

            migrationBuilder.DropTable(
                name: "UserTwitterTokens");
        }
    }
}
