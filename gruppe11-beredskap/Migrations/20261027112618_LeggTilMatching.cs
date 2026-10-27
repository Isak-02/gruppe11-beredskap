using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gruppe11_beredskap.Migrations
{
    /// <inheritdoc />
    public partial class LeggTilMatching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matchinger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BehovId = table.Column<int>(type: "int", nullable: false),
                    RessursId = table.Column<int>(type: "int", nullable: false),
                    OpprettetTidspunkt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matchinger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matchinger_Behov_BehovId",
                        column: x => x.BehovId,
                        principalTable: "Behov",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Matchinger_Ressurser_RessursId",
                        column: x => x.RessursId,
                        principalTable: "Ressurser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Matchinger_BehovId",
                table: "Matchinger",
                column: "BehovId");

            migrationBuilder.CreateIndex(
                name: "IX_Matchinger_RessursId",
                table: "Matchinger",
                column: "RessursId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matchinger");
        }
    }
}
