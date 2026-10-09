using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyclingTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAthleteTrainingZones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HrZone1Max",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 130);

            migrationBuilder.AddColumn<int>(
                name: "HrZone2Max",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 150);

            migrationBuilder.AddColumn<int>(
                name: "PowerZone1Max",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 150);

            migrationBuilder.AddColumn<int>(
                name: "PowerZone2Max",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 200);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HrZone1Max",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HrZone2Max",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PowerZone1Max",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PowerZone2Max",
                table: "AspNetUsers");
        }
    }
}
