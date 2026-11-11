using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gruppe11_beredskap.Migrations
{
    /// <inheritdoc />
    public partial class FjernTidspunkt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tidspunkt",
                table: "Behov");

            migrationBuilder.DropColumn(
                name: "Tidspunkt",
                table: "Ressurser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Tidspunkt",
                table: "Behov",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Tidspunkt",
                table: "Ressurser",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
