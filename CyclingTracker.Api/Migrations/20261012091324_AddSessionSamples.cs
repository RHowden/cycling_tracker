using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyclingTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionSamples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Samples",
                table: "Sessions",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Samples",
                table: "Sessions");
        }
    }
}
