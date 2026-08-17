using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveAddis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLicensePhotoToInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicensePhotoUrl",
                table: "Instructors",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicensePhotoUrl",
                table: "Instructors");
        }
    }
}
