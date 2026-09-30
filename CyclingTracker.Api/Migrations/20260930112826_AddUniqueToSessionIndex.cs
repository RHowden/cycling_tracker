using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyclingTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueToSessionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_UserId_StartTimeUtc",
                table: "Sessions");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId_StartTimeUtc",
                table: "Sessions",
                columns: new[] { "UserId", "StartTimeUtc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_UserId_StartTimeUtc",
                table: "Sessions");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId_StartTimeUtc",
                table: "Sessions",
                columns: new[] { "UserId", "StartTimeUtc" });
        }
    }
}
