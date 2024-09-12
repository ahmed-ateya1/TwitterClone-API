using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTweetsProfileDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tweets_Profiles_ProfileID",
                table: "Tweets");

            migrationBuilder.AddForeignKey(
                name: "FK_Tweets_Profiles_ProfileID",
                table: "Tweets",
                column: "ProfileID",
                principalTable: "Profiles",
                principalColumn: "ProfileID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tweets_Profiles_ProfileID",
                table: "Tweets");

            migrationBuilder.AddForeignKey(
                name: "FK_Tweets_Profiles_ProfileID",
                table: "Tweets",
                column: "ProfileID",
                principalTable: "Profiles",
                principalColumn: "ProfileID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
