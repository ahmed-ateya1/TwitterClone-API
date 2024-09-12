using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixprimaryKeyinTweetFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TweetFiles",
                table: "TweetFiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TweetFiles",
                table: "TweetFiles",
                column: "TweetFilesID");

            migrationBuilder.CreateIndex(
                name: "IX_TweetFiles_TweetID",
                table: "TweetFiles",
                column: "TweetID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TweetFiles",
                table: "TweetFiles");

            migrationBuilder.DropIndex(
                name: "IX_TweetFiles_TweetID",
                table: "TweetFiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TweetFiles",
                table: "TweetFiles",
                column: "TweetID");
        }
    }
}
