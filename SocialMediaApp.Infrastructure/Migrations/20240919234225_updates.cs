using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectionProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "ProfileId",
                table: "UserConnections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserConnections_ProfileId",
                table: "UserConnections",
                column: "ProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserConnections_Profiles_ProfileId",
                table: "UserConnections",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "ProfileID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConnections_Profiles_ProfileId",
                table: "UserConnections");

            migrationBuilder.DropIndex(
                name: "IX_UserConnections_ProfileId",
                table: "UserConnections");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "UserConnections");

            migrationBuilder.CreateTable(
                name: "ConnectionProfiles",
                columns: table => new
                {
                    ProfileID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserConnectionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionProfiles", x => new { x.ProfileID, x.UserConnectionID });
                    table.ForeignKey(
                        name: "FK_ConnectionProfiles_Profiles_ProfileID",
                        column: x => x.ProfileID,
                        principalTable: "Profiles",
                        principalColumn: "ProfileID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConnectionProfiles_UserConnections_UserConnectionID",
                        column: x => x.UserConnectionID,
                        principalTable: "UserConnections",
                        principalColumn: "UserConnectionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionProfiles_UserConnectionID",
                table: "ConnectionProfiles",
                column: "UserConnectionID");
        }
    }
}
