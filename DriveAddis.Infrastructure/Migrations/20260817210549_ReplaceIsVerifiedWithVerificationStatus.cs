using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveAddis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIsVerifiedWithVerificationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Instructors");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Instructors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "Instructors",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Instructors");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Instructors",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
