using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gruppe11_beredskap.Migrations
{
    /// <inheritdoc />
    public partial class LeggTilRadius : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Radius",
                table: "Behov",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Radius",
                table: "Behov");
        }
    }
}
